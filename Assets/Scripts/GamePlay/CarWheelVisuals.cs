using System.Collections.Generic;
using UnityEngine;

// Goes on the Car root. Spins every wheel with the car's speed and steers the front ones.
// At start it builds two invisible pivots around each wheel (one for steering, one for
// spinning), so it works even if the wheel's own pivot or rotation is wrong.
public class CarWheelVisuals : MonoBehaviour
{
    private class Wheel
    {
        public Transform SteerPivot;
        public Transform SpinPivot;
        public bool IsFront;
        public float Radius;
        public float SpinAngle;
    }

    [SerializeField] private CarController _car;
    [SerializeField] private Transform[] _wheels;           // the four wheel parts, in any order

    [Header("Look")]
    [SerializeField] private float _steerRate = 300f;       // degrees per second the front wheels can turn
    [SerializeField] private float _steerMultiplier = 1f;   // above 1 exaggerates the steering angle
    [SerializeField] private float _radiusOverride = 0f;    // 0 = measure from the model (meters)

    private readonly List<Wheel> _wheelList = new List<Wheel>();
    private float _steerAngle;

    private void Awake()
    {
        BuildWheels();
    }

    private void LateUpdate()
    {
        if (_car == null || _wheelList.Count == 0)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        float forwardSpeed = _car.ForwardSpeed;

        _steerAngle = Mathf.MoveTowards(_steerAngle, _car.SteerAngle * _steerMultiplier, _steerRate * deltaTime);

        foreach (Wheel wheel in _wheelList)
        {
            // distance traveled / radius = angle turned (in radians)
            float spin = forwardSpeed / wheel.Radius * Mathf.Rad2Deg * deltaTime;
            wheel.SpinAngle = Mathf.Repeat(wheel.SpinAngle + spin, 360f);
            wheel.SpinPivot.localRotation = Quaternion.Euler(wheel.SpinAngle, 0f, 0f);

            if (wheel.IsFront)
            {
                wheel.SteerPivot.localRotation = Quaternion.Euler(0f, _steerAngle, 0f);
            }
        }
    }

    private void BuildWheels()
    {
        if (_wheels == null || _wheels.Length == 0)
        {
            Debug.LogWarning("CarWheelVisuals: no wheels assigned.", this);
            return;
        }

        // 1. Measure every wheel in the car's own space (X right, Y up, Z forward).
        Bounds[] bounds = new Bounds[_wheels.Length];
        bool[] measured = new bool[_wheels.Length];
        float averageZ = 0f;
        int count = 0;

        for (int i = 0; i < _wheels.Length; i++)
        {
            if (_wheels[i] == null)
            {
                continue;
            }

            measured[i] = TryMeasure(_wheels[i], out bounds[i]);
            if (!measured[i])
            {
                Debug.LogWarning($"CarWheelVisuals: '{_wheels[i].name}' has no mesh, skipped.", this);
                continue;
            }

            if (bounds[i].size.x > bounds[i].size.y * 1.3f)
            {
                Debug.LogWarning($"CarWheelVisuals: '{_wheels[i].name}' is wider than a wheel. " +
                                 "It may contain more than one wheel.", this);
            }

            averageZ += bounds[i].center.z;
            count++;
        }

        if (count == 0)
        {
            return;
        }

        averageZ /= count;

        // 2. Wheels in front of the middle are the front wheels. Build the pivots.
        for (int i = 0; i < _wheels.Length; i++)
        {
            if (!measured[i])
            {
                continue;
            }

            Wheel wheel = new Wheel
            {
                IsFront = bounds[i].center.z > averageZ,
                Radius = _radiusOverride > 0f ? _radiusOverride : Mathf.Max(bounds[i].size.y * 0.5f, 0.1f)
            };

            wheel.SteerPivot = CreatePivot(_wheels[i].name + "_Steer", transform, bounds[i].center);
            wheel.SpinPivot = CreatePivot(_wheels[i].name + "_Spin", wheel.SteerPivot, Vector3.zero);
            _wheels[i].SetParent(wheel.SpinPivot, true);   // true = keep the wheel exactly where it is

            _wheelList.Add(wheel);
        }
    }

    // The size and center of a wheel measured in the car's own space, from the exact
    // corners of each mesh's local bounds. The vertical size is the wheel's diameter.
    private bool TryMeasure(Transform part, out Bounds result)
    {
        Renderer[] renderers = part.GetComponentsInChildren<Renderer>();
        result = new Bounds();
        bool first = true;

        foreach (Renderer meshRenderer in renderers)
        {
            Bounds local = meshRenderer.localBounds;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = local.center + Vector3.Scale(local.extents, new Vector3(x, y, z));
                        Vector3 inCarSpace = transform.InverseTransformPoint(
                            meshRenderer.transform.TransformPoint(corner));

                        if (first)
                        {
                            result = new Bounds(inCarSpace, Vector3.zero);
                            first = false;
                        }
                        else
                        {
                            result.Encapsulate(inCarSpace);
                        }
                    }
                }
            }
        }

        return !first;
    }

    private static Transform CreatePivot(string pivotName, Transform parent, Vector3 localPosition)
    {
        Transform pivot = new GameObject(pivotName).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = localPosition;
        pivot.localRotation = Quaternion.identity;
        return pivot;
    }
}