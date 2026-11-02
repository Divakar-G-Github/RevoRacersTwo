using UnityEngine;

// Draws skid marks and tire smoke while the car skids. Put it on the Car root.
public class CarSkidEffects : MonoBehaviour
{
    [SerializeField] private CarController _car;

    [Header("Skid marks (one trail per rear wheel)")]
    [SerializeField] private TrailRenderer[] _skidTrails;
    [SerializeField] private float _skidThreshold = 0.25f;

    [Header("Tire smoke (optional)")]
    [SerializeField] private ParticleSystem[] _smoke;
    [SerializeField] private float _maxSmokeRate = 40f;

    [Header("Debug")]
    [SerializeField] private bool _logSmoke;   // tick to print what the smoke systems are doing

    private bool _marking;

    private void Start()
    {
        SetTrails(false);

        if (_smoke.Length == 0)
        {
            Debug.LogWarning("CarSkidEffects: the Smoke list is empty. Drag Smoke_L and Smoke_R into it.", this);
        }
    }

    private void Update()
    {
        float skid = _car.IsGrounded ? _car.SkidAmount : 0f;
        bool skidding = skid > _skidThreshold;

        if (skidding != _marking)
        {
            SetTrails(skidding);
        }

        UpdateSmoke(skid, skidding);
    }

    private void UpdateSmoke(float skid, bool skidding)
    {
        foreach (ParticleSystem smoke in _smoke)
        {
            if (smoke == null)
            {
                continue;
            }

            ParticleSystem.EmissionModule emission = smoke.emission;
            emission.enabled = true;   // the module's checkbox can't be left off
            emission.rateOverTime = skidding ? skid * _maxSmokeRate : 0f;

            if (!smoke.isPlaying)
            {
                smoke.Play();   // a system that stopped never emits again
            }

            if (_logSmoke && skidding)
            {
                Debug.Log($"{smoke.name}: rate={emission.rateOverTime.constant:0}, " +
                          $"particles={smoke.particleCount}, playing={smoke.isPlaying}", smoke);
            }
        }
    }

    private void SetTrails(bool emitting)
    {
        _marking = emitting;
        foreach (TrailRenderer trail in _skidTrails)
        {
            trail.emitting = emitting;
        }
    }
}