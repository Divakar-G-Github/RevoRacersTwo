using UnityEngine;

// Drives a car around the racing line by pressing the "pedals" of the car's CarInput.
[RequireComponent(typeof(CarController), typeof(CarInput), typeof(Rigidbody))]
public class AiDriver : MonoBehaviour
{
    [SerializeField] private RacingLine _line;

    [Header("Driver")]
    [SerializeField, Range(0.7f, 1.05f)] private float _speedFactor = 0.95f;   // 1 = the planned speed
    [SerializeField] private float _lateralOffset = 0f;    // meters left (-) or right (+) of the line
    [SerializeField] private float _startDelay = 0f;       // seconds to wait at the start (until the race manager does it)

    [Header("Steering")]
    [SerializeField] private float _lookAheadMin = 6f;
    [SerializeField] private float _lookAheadPerMs = 0.4f;   // extra look-ahead meters per m/s of speed
    [SerializeField] private float _maxSteerAngle = 30f;     // degrees of error that give full steering
    [SerializeField] private float _steerGain = 1.3f;

    [Header("Speed")]
    [SerializeField] private float _throttleRange = 4f;      // m/s under the target for full throttle
    [SerializeField] private float _brakeMargin = 2f;        // m/s over the target before braking starts
    [SerializeField] private int _speedLookAheadPoints = 3;

    [Header("Other cars")]
    [SerializeField] private float _carCheckDistance = 14f;
    [SerializeField] private float _tooCloseDistance = 5f;

    [Header("Recovery")]
    [SerializeField] private float _stuckSeconds = 3f;

    private CarController _car;
    private CarInput _input;
    private Rigidbody _body;
    private int _index;
    private float _stuckTimer;

    private void Awake()
    {
        _car = GetComponent<CarController>();
        _input = GetComponent<CarInput>();
        _body = GetComponent<Rigidbody>();
        _input.UseExternalInput = true;   // this car is driven by the AI, not the keyboard
    }

    private void Start()
    {
        if (_line == null || _line.Count < 8)
        {
            Debug.LogWarning("AiDriver: the Racing Line has no recorded points yet.", this);
            return;
        }

        _index = _line.FindNearest(transform.position);
    }

    private void FixedUpdate()
    {
        if (_line == null || _line.Count < 8)
        {
            return;
        }

        if (Time.timeSinceLevelLoad < _startDelay)
        {
            _input.SetExternalInput(0f, 0f, 1f);
            return;
        }

        _index = _line.Advance(_index, transform.position);

        float speed = _car.ForwardSpeed;
        float steer = ComputeSteer(speed);
        ComputeSpeedControl(speed, out float throttle, out float brake);

        _input.SetExternalInput(throttle, steer, brake);
        HandleStuck();
    }

    // Aims at a point ahead on the line (the faster we go, the farther ahead).
    private float ComputeSteer(float speed)
    {
        float lookAhead = _lookAheadMin + _lookAheadPerMs * Mathf.Max(speed, 0f);
        Vector3 target = _line.GetPointAhead(_index, lookAhead, _lateralOffset);

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;

        float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);   // positive = target is to the right
        return Mathf.Clamp(angle / _maxSteerAngle * _steerGain, -1f, 1f);
    }

    private void ComputeSpeedControl(float speed, out float throttle, out float brake)
    {
        // The slowest planned speed over the next few points, so the car brakes in time.
        float planned = float.MaxValue;
        for (int i = 0; i <= _speedLookAheadPoints; i++)
        {
            planned = Mathf.Min(planned, _line.GetSpeedLimit(_index + i));
        }

        float target = planned * _speedFactor;
        brake = 0f;

        // Do not drive into the car in front.
        float distanceAhead = FindCarAhead(out float aheadSpeed);
        if (distanceAhead > 0f)
        {
            target = Mathf.Min(target, aheadSpeed);
            if (distanceAhead < _tooCloseDistance)
            {
                brake = 0.5f;
            }
        }

        float error = target - speed;
        throttle = Mathf.Clamp01(error / _throttleRange);

        if (error < -_brakeMargin)
        {
            brake = Mathf.Max(brake, Mathf.Clamp01((-error - _brakeMargin) / 5f));
        }
    }

    // Returns the distance to a car straight ahead, or -1 if there is none.
    private float FindCarAhead(out float aheadSpeed)
    {
        aheadSpeed = float.MaxValue;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, _carCheckDistance,
                            ~0, QueryTriggerInteraction.Ignore))
        {
            CarController other = hit.collider.GetComponentInParent<CarController>();
            if (other != null && other != _car)
            {
                aheadSpeed = Mathf.Max(other.ForwardSpeed, 0f);
                return hit.distance;
            }
        }

        return -1f;
    }

    // If the car hardly moves for a few seconds while it should be racing, put it back on the line.
    private void HandleStuck()
    {
        bool stuck = _input.InputEnabled && _car.SpeedKmh < 5f;
        _stuckTimer = stuck ? _stuckTimer + Time.fixedDeltaTime : 0f;

        if (_stuckTimer < _stuckSeconds)
        {
            return;
        }

        _stuckTimer = 0f;
        Vector3 point = _line.GetPointAhead(_index, 0f, 0f);
        Vector3 direction = _line.GetPointAhead(_index, 5f, 0f) - point;
        direction.y = 0f;

        _body.linearVelocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
        _body.position = point + Vector3.up * 1f;
        _body.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}