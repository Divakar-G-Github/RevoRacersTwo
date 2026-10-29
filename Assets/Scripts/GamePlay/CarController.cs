using UnityEngine;

// Car physics with realistic numbers: engine curve, real brakes, bicycle-model steering,
// a tire grip limit, air drag and downforce.
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(CarInput))]
public class CarController : MonoBehaviour
{
    private static readonly Vector2[] GroundCorners =
    {
        new Vector2(-1f, -1f), new Vector2(1f, -1f),
        new Vector2(-1f, 1f),  new Vector2(1f, 1f)
    };

    private const float MsToKmh = 3.6f;
    private const float RayStartInset = 0.1f;   // ground rays start this far above the bottom of the box

    [Header("Engine")]
    [SerializeField] private float _topSpeedKmh = 180f;
    [SerializeField] private float _acceleration = 8f;       // m/s² from a standstill
    // X = speed as a fraction of top speed (0..1), Y = how much engine power is left (1..0).
    [SerializeField]
    private AnimationCurve _powerBySpeed = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(0.6f, 0.7f), new Keyframe(1f, 0f));
    [SerializeField] private float _reverseSpeedKmh = 30f;
    [SerializeField] private float _engineBraking = 2.5f;    // slowdown when you lift off the pedal (m/s²)

    [Header("Brakes")]
    [SerializeField] private float _brakePower = 18f;        // m/s² (Space; S while moving forward)

    [Header("Resistance")]
    [SerializeField] private float _rollingResistance = 0.3f;   // m/s²
    [SerializeField] private float _airDrag = 0.0012f;          // grows with speed squared

    [Header("Steering")]
    [SerializeField] private float _wheelBase = 3.4f;        // meters, front axle to rear axle
    [SerializeField] private float _maxSteerAngle = 30f;     // degrees of front-wheel angle at low speed
    [SerializeField] private float _steerResponse = 12f;     // how fast the car reaches the wanted turn rate
    [SerializeField] private float _cornerLimitFactor = 1.2f;   // above 1 lets you push the tires a little

    [Header("Grip")]
    [SerializeField] private float _maxLateralAcceleration = 16f;   // m/s² (about 1.6 g)
    [SerializeField] private float _grip = 10f;

    [Header("Stability")]
    [SerializeField] private float _downforce = 0.001f;      // extra push down, grows with speed squared
    [SerializeField] private float _airGravityMultiplier = 1.5f;
    [SerializeField] private Vector3 _centerOfMass = new Vector3(0f, 0.25f, 0f);

    [Header("Ground check")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private float _groundCheckDistance = 0.5f;
    [SerializeField] private float _groundGraceTime = 0.15f;   // stays "grounded" this long after the last hit

    private Rigidbody _body;
    private BoxCollider _box;
    private CarInput _input;
    private float _lastGroundedTime = -100f;

    public bool IsGrounded { get; private set; }
    public Vector3 Velocity => _body.linearVelocity;
    public float SpeedKmh => _body.linearVelocity.magnitude * MsToKmh;
    public float SpeedRatio => Mathf.Clamp01(SpeedKmh / _topSpeedKmh);
    public float ForwardSpeed => Vector3.Dot(_body.linearVelocity, transform.forward);
    public float SteerAngle { get; private set; }   // front wheel angle in degrees (used by the wheel visuals)

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _box = GetComponent<BoxCollider>();
        _input = GetComponent<CarInput>();

        if (_input == null)
        {
            _input = gameObject.AddComponent<CarInput>();
        }

        if (_box == null)
        {
            Debug.LogError("CarController needs a Box Collider on the same object.", this);
            enabled = false;
            return;
        }

        _body.centerOfMass = _centerOfMass;
        _body.interpolation = RigidbodyInterpolation.Interpolate;
        _body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _body.angularDamping = 1f;
        _body.maxDepenetrationVelocity = 3f;   // the car is pushed out of the road gently, no popping
    }

    private void FixedUpdate()
    {
        UpdateGroundState();

        if (_input.ConsumeReset())
        {
            ResetCar();
            return;
        }

        if (!IsGrounded)
        {
            ApplyAirGravity();
            return;
        }

        Vector3 velocity = _body.linearVelocity;
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);

        ApplyLongitudinalForces(velocity, forwardSpeed);
        ApplySteering(forwardSpeed);
        ApplyGrip();
        ApplyDownforce(velocity.sqrMagnitude);
    }

    // ---------- Engine, brakes, resistance ----------

    private void ApplyLongitudinalForces(Vector3 velocity, float forwardSpeed)
    {
        float throttle = _input.Throttle;
        Vector3 flatVelocity = Vector3.ProjectOnPlane(velocity, transform.up);   // along the road
        float speed = flatVelocity.magnitude;

        // How hard the brakes are pressed (0..1): Space, S while rolling forward,
        // or W while rolling backward.
        float brakeAmount = _input.Brake;
        if (throttle < -0.01f && forwardSpeed > 0.5f)
        {
            brakeAmount = Mathf.Max(brakeAmount, -throttle);
        }
        else if (throttle > 0.01f && forwardSpeed < -0.5f)
        {
            brakeAmount = Mathf.Max(brakeAmount, throttle);
        }

        float slowdown = _rollingResistance + _airDrag * speed * speed;   // always present
        float drive = 0f;                                                  // signed, along forward

        if (brakeAmount > 0.01f)
        {
            slowdown += _brakePower * brakeAmount;
        }
        else if (throttle > 0.01f)
        {
            float speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / (_topSpeedKmh / MsToKmh));
            drive = _acceleration * _powerBySpeed.Evaluate(speedRatio) * throttle;
        }
        else if (throttle < -0.01f)
        {
            if (forwardSpeed > -_reverseSpeedKmh / MsToKmh)   // stopped: S reverses
            {
                drive = _acceleration * 0.5f * throttle;
            }
        }
        else
        {
            slowdown += _engineBraking;   // lifted off: the engine slows the car
        }

        if (drive != 0f)
        {
            _body.AddForce(transform.forward * drive, ForceMode.Acceleration);
        }

        Decelerate(flatVelocity, slowdown);
    }

    // Slows the car against its direction of travel, and never pushes it backwards.
    private void Decelerate(Vector3 flatVelocity, float amount)
    {
        float speed = flatVelocity.magnitude;
        if (speed < 0.05f || amount <= 0f)
        {
            return;
        }

        float limited = Mathf.Min(amount, speed / Time.fixedDeltaTime);
        _body.AddForce(-flatVelocity / speed * limited, ForceMode.Acceleration);
    }

    // ---------- Steering ----------

    // Bicycle model: turn rate = speed * tan(steering angle) / wheelbase.
    // The steering angle is limited to what the tires can grip at the current speed.
    private void ApplySteering(float forwardSpeed)
    {
        float speed = Mathf.Abs(forwardSpeed);
        float maxAngle = _maxSteerAngle * Mathf.Deg2Rad;
        float gripLimitedAngle = Mathf.Atan(
            _wheelBase * _maxLateralAcceleration * _cornerLimitFactor / Mathf.Max(speed * speed, 1f));

        float steerAngle = Mathf.Min(maxAngle, gripLimitedAngle) * _input.Steer;
        SteerAngle = steerAngle * Mathf.Rad2Deg;

        // forwardSpeed is signed, so reversing flips the turn automatically.
        float targetYawRate = forwardSpeed * Mathf.Tan(steerAngle) / _wheelBase;   // radians per second
        float currentYawRate = Vector3.Dot(_body.angularVelocity, transform.up);
        float change = (targetYawRate - currentYawRate) * Mathf.Clamp01(_steerResponse * Time.fixedDeltaTime);

        _body.AddTorque(transform.up * change, ForceMode.VelocityChange);
    }

    // ---------- Grip and stability ----------

    // Pulls the car's sideways speed to zero, up to a limit. Beyond the limit the car slides.
    private void ApplyGrip()
    {
        float sideSpeed = Vector3.Dot(_body.linearVelocity, transform.right);
        float lateral = Mathf.Clamp(-sideSpeed * _grip, -_maxLateralAcceleration, _maxLateralAcceleration);
        _body.AddForce(transform.right * lateral, ForceMode.Acceleration);
    }

    private void ApplyDownforce(float speedSquared)
    {
        _body.AddForce(-transform.up * (_downforce * speedSquared), ForceMode.Acceleration);
    }

    private void ApplyAirGravity()
    {
        _body.AddForce(Physics.gravity * (_airGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    // ---------- Ground check and reset ----------

    // Four rays near the corners of the Box Collider. One hit is enough, and the car stays
    // "grounded" for a short grace time so a flicker of the rays never cuts the engine.
    private void UpdateGroundState()
    {
        Vector3 half = _box.size * 0.5f;
        float rayY = _box.center.y - half.y + RayStartInset;
        float rayLength = RayStartInset + _groundCheckDistance;

        foreach (Vector2 corner in GroundCorners)
        {
            Vector3 local = new Vector3(
                _box.center.x + corner.x * half.x * 0.8f,
                rayY,
                _box.center.z + corner.y * half.z * 0.8f);

            Vector3 origin = transform.TransformPoint(local);
            if (Physics.Raycast(origin, -transform.up, rayLength, _groundMask, QueryTriggerInteraction.Ignore))
            {
                _lastGroundedTime = Time.fixedTime;
                break;
            }
        }

        IsGrounded = Time.fixedTime - _lastGroundedTime <= _groundGraceTime;
    }

    // Puts the car upright where it is, a little higher, and stops it.
    private void ResetCar()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        _body.linearVelocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
        _body.position = transform.position + Vector3.up * 1.5f;
        _body.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
    }
}