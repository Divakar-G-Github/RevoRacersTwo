using System.Collections.Generic;
using UnityEngine;

// The path the AI cars follow. It is recorded from a lap you drive (see RacingLineRecorder).
// At start it smooths the path and works out a safe speed for every point, including braking zones.
public class RacingLine : MonoBehaviour
{
    private const int SmoothRadius = 2;      // points on each side that are averaged
    private const int CurvatureStep = 2;     // points apart used to measure a corner

    [Header("Recorded points (world positions, in driving order)")]
    [SerializeField] private List<Vector3> _points = new List<Vector3>();

    [Header("Planned speed")]
    [SerializeField] private float _cornerGrip = 13f;          // m/s² the AI trusts in corners
    [SerializeField] private float _brakeDeceleration = 16f;   // m/s² the AI plans to brake with
    [SerializeField] private float _maxSpeedKmh = 170f;

    private Vector3[] _path;
    private float[] _segments;   // flat distance from each point to the next
    private float[] _limits;     // planned speed at each point (m/s)

    public int Count => _points.Count;

    private void Awake()
    {
        Build();
    }

    // ---------- Recording ----------

    public void AddPoint(Vector3 point)
    {
        _points.Add(point);
        _path = null;
    }

    public void Clear()
    {
        _points.Clear();
        _path = null;
    }

    // ---------- Queries used by the AI ----------

    public int FindNearest(Vector3 position)
    {
        EnsureBuilt();
        int best = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < _path.Length; i++)
        {
            float distance = FlatSqrDistance(position, _path[i]);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    // Moves the index forward while the next point is closer than the current one.
    public int Advance(int index, Vector3 position)
    {
        EnsureBuilt();
        index = Wrap(index);

        for (int step = 0; step < 20; step++)
        {
            int next = Wrap(index + 1);
            if (FlatSqrDistance(position, _path[next]) < FlatSqrDistance(position, _path[index]))
            {
                index = next;
            }
            else
            {
                break;
            }
        }

        return index;
    }

    public float GetSpeedLimit(int index)
    {
        EnsureBuilt();
        return _limits[Wrap(index)];
    }

    // A point 'distance' meters ahead along the line, moved sideways by lateralOffset meters.
    public Vector3 GetPointAhead(int fromIndex, float distance, float lateralOffset)
    {
        EnsureBuilt();
        int index = Wrap(fromIndex);
        float remaining = Mathf.Max(distance, 0f);

        int guard = 0;
        while (remaining > _segments[index] && guard < _path.Length)
        {
            remaining -= _segments[index];
            index = Wrap(index + 1);
            guard++;
        }

        int next = Wrap(index + 1);
        float t = _segments[index] > 0.001f ? remaining / _segments[index] : 0f;
        Vector3 point = Vector3.Lerp(_path[index], _path[next], t);

        Vector3 direction = _path[next] - _path[index];
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
        {
            point += Vector3.Cross(Vector3.up, direction.normalized) * lateralOffset;   // right of the line
        }

        return point;
    }

    // ---------- Building the path ----------

    private void EnsureBuilt()
    {
        if (_path == null)
        {
            Build();
        }
    }

    private void Build()
    {
        int n = _points.Count;
        if (n < 8)
        {
            _path = null;
            return;
        }

        // 1. Smooth the recorded points, so small steering wobbles don't look like corners.
        _path = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 sum = Vector3.zero;
            for (int k = -SmoothRadius; k <= SmoothRadius; k++)
            {
                sum += _points[WrapIndex(i + k, n)];
            }

            _path[i] = sum / (SmoothRadius * 2 + 1);
        }

        // 2. Distance between neighbours.
        _segments = new float[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 gap = _path[WrapIndex(i + 1, n)] - _path[i];
            gap.y = 0f;
            _segments[i] = gap.magnitude;
        }

        // 3. A safe speed for every point, from how sharp the corner is.
        float maxSpeed = _maxSpeedKmh / 3.6f;
        _limits = new float[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 a = _path[WrapIndex(i - CurvatureStep, n)];
            Vector3 b = _path[i];
            Vector3 c = _path[WrapIndex(i + CurvatureStep, n)];

            float curvature = Curvature(a, b, c);
            float limit = curvature > 0.0001f ? Mathf.Sqrt(_cornerGrip / curvature) : maxSpeed;
            _limits[i] = Mathf.Min(limit, maxSpeed);
        }

        // 4. Braking zones: a point may not be faster than what the car can brake down to
        //    before the next, slower point. Two passes close the loop.
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = n - 1; i >= 0; i--)
            {
                float nextLimit = _limits[WrapIndex(i + 1, n)];
                float reachable = Mathf.Sqrt(nextLimit * nextLimit + 2f * _brakeDeceleration * _segments[i]);
                if (reachable < _limits[i])
                {
                    _limits[i] = reachable;
                }
            }
        }
    }

    // 1 / radius of the circle through three points (flat). Larger = sharper corner.
    private static float Curvature(Vector3 a, Vector3 b, Vector3 c)
    {
        float ab = FlatDistance(a, b);
        float bc = FlatDistance(b, c);
        float ac = FlatDistance(a, c);
        float product = ab * bc * ac;
        if (product < 0.001f)
        {
            return 0f;
        }

        float cross = Mathf.Abs((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x));
        return 2f * cross / product;
    }

    private int Wrap(int index)
    {
        return WrapIndex(index, _path.Length);
    }

    private static int WrapIndex(int index, int count)
    {
        return ((index % count) + count) % count;
    }

    private static float FlatSqrDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(FlatSqrDistance(a, b));
    }

    // Shows the path in the Scene view: cyan = recorded, green to red = planned speed (in Play mode).
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _points.Count; i++)
        {
            Gizmos.DrawLine(_points[i], _points[(i + 1) % _points.Count]);
        }

        if (_path == null || _limits == null)
        {
            return;
        }

        float maxSpeed = _maxSpeedKmh / 3.6f;
        for (int i = 0; i < _path.Length; i++)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.green, _limits[i] / maxSpeed);
            Gizmos.DrawSphere(_path[i] + Vector3.up * 0.3f, 0.4f);
        }
    }
}