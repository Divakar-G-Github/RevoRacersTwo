using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Records a racing line while you drive. Press F8 to start and F8 again to stop.
// A label on the Game view shows what it is doing.
public class RacingLineRecorder : MonoBehaviour
{
    [SerializeField] private RacingLine _line;
    [SerializeField] private Transform _car;
    [SerializeField] private float _spacing = 4f;   // meters between points

    private bool _recording;
    private Vector3 _lastPoint;
    private GUIStyle _style;

    private void Start()
    {
        if (_line == null || _car == null)
        {
            Debug.LogError("RacingLineRecorder: fill in both Line and Car in the Inspector.", this);
        }
    }

    private void Update()
    {
        if (_line == null || _car == null)
        {
            return;
        }

        if (TogglePressed())
        {
            _recording = !_recording;

            if (_recording)
            {
                _line.Clear();
                _lastPoint = _car.position;
                _line.AddPoint(_lastPoint);
                Debug.Log("Recording started. Drive one clean lap, then press F8 again.");
            }
            else
            {
                Debug.Log($"Recording stopped: {_line.Count} points.");
            }
        }

        if (_recording && (_car.position - _lastPoint).sqrMagnitude >= _spacing * _spacing)
        {
            _lastPoint = _car.position;
            _line.AddPoint(_lastPoint);
        }
    }

    // A simple label on the Game view, so you can see what the recorder is doing.
    private void OnGUI()
    {
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
        }

        string text;
        if (_line == null || _car == null)
        {
            text = "RECORDER ERROR: assign Line and Car";
            GUI.color = Color.yellow;
        }
        else if (_recording)
        {
            text = $"RECORDING   {_line.Count} points   (F8 = stop)";
            GUI.color = Color.red;
        }
        else
        {
            text = $"Recorder ready   ({_line.Count} points saved)   F8 = start";
            GUI.color = Color.white;
        }

        GUI.Label(new Rect(20f, 20f, 1200f, 50f), text, _style);
    }

    private static bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F8);
#endif
    }
}