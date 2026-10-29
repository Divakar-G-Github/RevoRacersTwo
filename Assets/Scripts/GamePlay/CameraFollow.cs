using UnityEngine;

// Chase camera. Rigid horizontal follow (the gap to the car never changes with speed),
// with smoothed heading and smoothed height only.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;   // the Car

    [Header("Position")]
    [SerializeField] private float _distance = 8f;
    [SerializeField] private float _height = 2.6f;
    [SerializeField] private float _lookHeight = 1f;

    [Header("Smoothing (seconds, higher = softer)")]
    [SerializeField] private float _rotationSmoothTime = 0.2f;
    [SerializeField] private float _heightSmoothTime = 0.15f;

    [Header("Field of view")]
    [SerializeField] private float _baseFov = 60f;
    [SerializeField] private float _maxFov = 65f;
    [SerializeField] private float _fovSmoothTime = 0.5f;

    private Camera _camera;
    private CarController _car;

    private float _yaw;
    private float _yawVelocity;
    private float _anchorY;
    private float _anchorYVelocity;
    private float _fov;
    private float _fovVelocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_target != null)
        {
            SetTarget(_target);
        }
    }

    // Also used by a spawner script later.
    public void SetTarget(Transform target)
    {
        _target = target;
        if (_target == null)
        {
            return;
        }

        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        _car = _target.GetComponent<CarController>();

        _yaw = GetHeading(_target, 0f);
        _yawVelocity = 0f;
        _anchorY = _target.position.y;
        _anchorYVelocity = 0f;
        _fov = _baseFov;
        _fovVelocity = 0f;

        Place(_target.position);   // start right behind the car instead of swooping in
        _camera.fieldOfView = _fov;
    }

    private void LateUpdate()   // after all movement
    {
        if (_target == null || Time.deltaTime <= 0f)
        {
            return;
        }

        // 1. Heading: the car's flat forward direction, smoothed.
        _yaw = Mathf.SmoothDampAngle(_yaw, GetHeading(_target, _yaw), ref _yawVelocity, _rotationSmoothTime);

        // 2. Only the height is smoothed, so road bumps never reach the screen.
        Vector3 position = _target.position;
        _anchorY = Mathf.SmoothDamp(_anchorY, position.y, ref _anchorYVelocity, _heightSmoothTime);
        Place(new Vector3(position.x, _anchorY, position.z));

        // 3. A slightly wider view at speed, changed slowly.
        float speedRatio = _car != null ? _car.SpeedRatio : 0f;
        float targetFov = Mathf.Lerp(_baseFov, _maxFov, speedRatio);
        _fov = Mathf.SmoothDamp(_fov, targetFov, ref _fovVelocity, _fovSmoothTime);
        _camera.fieldOfView = _fov;
    }

    // Puts the camera at a fixed offset behind the anchor and looks at the car.
    private void Place(Vector3 anchor)
    {
        Vector3 offset = Quaternion.Euler(0f, _yaw, 0f) * new Vector3(0f, _height, -_distance);
        Vector3 cameraPosition = anchor + offset;
        Vector3 lookPoint = anchor + Vector3.up * _lookHeight;

        transform.SetPositionAndRotation(
            cameraPosition,
            Quaternion.LookRotation(lookPoint - cameraPosition, Vector3.up));
    }

    // The direction the car points, as an angle around the vertical axis.
    private static float GetHeading(Transform car, float fallback)
    {
        Vector3 flat = Vector3.ProjectOnPlane(car.forward, Vector3.up);
        if (flat.sqrMagnitude < 0.001f)
        {
            return fallback;
        }

        return Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
    }
}