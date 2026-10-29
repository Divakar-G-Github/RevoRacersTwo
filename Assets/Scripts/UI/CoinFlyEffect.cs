using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays a "coins fly to the counter" effect. Visual only: it never changes the coin amount.
public class CoinFlyEffect : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private RectTransform _coinPrefab;   // a small coin Image (Raycast Target off)
    [SerializeField] private RectTransform _target;       // where the coins land (the coin icon)

    [Header("Look")]
    [SerializeField] private int _coinCount = 8;
    [SerializeField] private float _flyTime = 0.7f;
    [SerializeField] private float _delayBetweenCoins = 0.06f;
    [SerializeField] private float _arcHeight = 120f;     // how high the curved path bends
    [SerializeField] private float _startSpread = 40f;    // random offset around the start point
    [SerializeField] private float _punchScale = 1.2f;    // how much the counter grows when coins land
    [SerializeField] private float _punchTime = 0.15f;

    private readonly List<GameObject> _activeCoins = new List<GameObject>();
    private Vector3 _targetBaseScale;
    private Coroutine _punchRoutine;

    private void Awake()
    {
        _targetBaseScale = _target.localScale;
    }

    private void OnDisable()
    {
        // The screen was closed mid-effect: remove leftover coins and restore the counter.
        StopAllCoroutines();
        _punchRoutine = null;

        foreach (GameObject coin in _activeCoins)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        _activeCoins.Clear();

        if (_target != null)
        {
            _target.localScale = _targetBaseScale;
        }
    }

    // Call this when a purchase succeeds. "from" is where the coins start (the Buy button).
    public void Play(RectTransform from)
    {
        // Read the position now: the Buy button may be hidden a moment later.
        Vector3 startPosition = from.position;
        StartCoroutine(SpawnCoins(startPosition));
    }

    private IEnumerator SpawnCoins(Vector3 startPosition)
    {
        for (int i = 0; i < _coinCount; i++)
        {
            StartCoroutine(FlyCoin(startPosition));
            yield return new WaitForSeconds(_delayBetweenCoins);
        }
    }

    private IEnumerator FlyCoin(Vector3 startPosition)
    {
        // Positions are in screen units, sizes in canvas units. This scale converts between them.
        float canvasScale = transform.lossyScale.x;

        RectTransform coin = Instantiate(_coinPrefab, transform);
        _activeCoins.Add(coin.gameObject);

        Vector2 spread = Random.insideUnitCircle * _startSpread * canvasScale;
        Vector3 start = startPosition + new Vector3(spread.x, spread.y, 0f);
        coin.position = start;

        float elapsed = 0f;
        while (elapsed < _flyTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _flyTime);

            float eased = t * t;   // starts slow, speeds up near the end
            Vector3 position = Vector3.Lerp(start, _target.position, eased);
            position.y += Mathf.Sin(t * Mathf.PI) * _arcHeight * canvasScale;   // curved path

            coin.position = position;
            coin.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, t);   // shrinks while flying

            yield return null;   // wait one frame
        }

        _activeCoins.Remove(coin.gameObject);
        Destroy(coin.gameObject);

        if (_punchRoutine != null)
        {
            StopCoroutine(_punchRoutine);
        }
        _punchRoutine = StartCoroutine(PunchTarget());
    }

    // Makes the counter grow a little and settle back.
    private IEnumerator PunchTarget()
    {
        float elapsed = 0f;
        while (elapsed < _punchTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _punchTime);
            _target.localScale = _targetBaseScale * Mathf.Lerp(_punchScale, 1f, t);
            yield return null;
        }

        _target.localScale = _targetBaseScale;
        _punchRoutine = null;
    }
}