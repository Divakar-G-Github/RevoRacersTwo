using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Reads the keyboard / gamepad and smooths pedals and steering. It does not move the car.
public class CarInput : MonoBehaviour
{
    [Header("Pedals")]
    [SerializeField] private float _pedalRiseSpeed = 5f;    // pedal 0 -> 1 takes 0.2 s
    [SerializeField] private float _pedalFallSpeed = 10f;   // released faster
    [SerializeField] private float _brakeRiseSpeed = 12f;   // the brake bites quickly

    [Header("Steering")]
    [SerializeField] private float _steerSpeed = 4f;         // how fast the wheel turns toward the key
    [SerializeField] private float _steerReturnSpeed = 7f;   // how fast it returns to center

    private bool _resetRequested;

    public float Throttle { get; private set; }   // +1 = accelerate, -1 = brake / reverse
    public float Steer { get; private set; }      // -1 = left, +1 = right
    public float Brake { get; private set; }      // 0..1 (Space)
    public bool InputEnabled { get; set; } = true;

    // True once per press. The controller calls this from FixedUpdate.
    public bool ConsumeReset()
    {
        bool requested = _resetRequested;
        _resetRequested = false;
        return requested;
    }

    private void Update()
    {
        ReadRawInput(out float rawThrottle, out float rawSteer, out bool rawBrake, out bool reset);
        if (!InputEnabled)
        {
            rawThrottle = 0f;
            rawSteer = 0f;
            rawBrake = true;    // holds the car still at the start, brings it to a stop at the finish
            reset = false;
        }
        float deltaTime = Time.deltaTime;

        // The pedals move gradually instead of jumping, like a real foot.
        float throttleRate = Mathf.Abs(rawThrottle) > Mathf.Abs(Throttle) ? _pedalRiseSpeed : _pedalFallSpeed;
        Throttle = Mathf.MoveTowards(Throttle, rawThrottle, throttleRate * deltaTime);

        float brakeRate = rawBrake ? _brakeRiseSpeed : _pedalFallSpeed;
        Brake = Mathf.MoveTowards(Brake, rawBrake ? 1f : 0f, brakeRate * deltaTime);

        // Return to center (or reverse direction) faster than turning in.
        bool returning = Mathf.Approximately(rawSteer, 0f) || rawSteer * Steer < 0f;
        float steerRate = returning ? _steerReturnSpeed : _steerSpeed;
        Steer = Mathf.MoveTowards(Steer, rawSteer, steerRate * deltaTime);

        if (reset)
        {
            _resetRequested = true;   // kept until the physics step uses it
        }
    }

    private static void ReadRawInput(out float throttle, out float steer, out bool brake, out bool reset)
    {
        throttle = 0f;
        steer = 0f;
        brake = false;
        reset = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            throttle += (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                      - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            steer += (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                   - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            brake |= keyboard.spaceKey.isPressed;
            reset |= keyboard.rKey.wasPressedThisFrame;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            throttle += gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue();
            steer += gamepad.leftStick.x.ReadValue();
            brake |= gamepad.buttonSouth.isPressed;
            reset |= gamepad.buttonNorth.wasPressedThisFrame;
        }
#else
        throttle = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");
        brake = Input.GetKey(KeyCode.Space);
        reset = Input.GetKeyDown(KeyCode.R);
#endif

        throttle = Mathf.Clamp(throttle, -1f, 1f);
        steer = Mathf.Clamp(steer, -1f, 1f);
    }
}