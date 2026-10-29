using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Goes on LeaderBoardBackground. Fills the rows, and adds Player5, Player6... when needed.
public class LeaderboardController : MonoBehaviour
{
    private static readonly string[] DummyNames =
        { "Arun", "Priya", "Karthik", "Meena", "Ravi", "Divya", "Sanjay", "Nisha" };

    [Header("Rows already in the scene")]
    [SerializeField] private LeaderboardRowView[] _fixedRows;      // Player1, Player2, Player3, Player4

    [Header("Extra rows")]
    [SerializeField] private LeaderboardRowView _extraRowPrefab;   // the Player4 PREFAB (from Project)
    [SerializeField] private RectTransform _content;
    [SerializeField] private ScrollRect _scrollRect;

    [Header("Data")]
    [SerializeField] private bool _useDummyData = true;            // untick when the database is connected

    private readonly List<LeaderboardRecord> _records = new List<LeaderboardRecord>();
    private readonly List<LeaderboardRowView> _extraRows = new List<LeaderboardRowView>();

    private void OnEnable()
    {
        if (_useDummyData && _records.Count == 0)
        {
            _records.AddRange(CreateDummyData());
        }

        Refresh();
    }

    // The database script will call this later with the real data.
    public void SetRecords(IEnumerable<LeaderboardRecord> records)
    {
        _records.Clear();
        _records.AddRange(records);
        Refresh();
    }

    private void Refresh()
    {
        // Fastest time first. The rank is simply the position in this order.
        _records.Sort((a, b) => a.TimeSeconds.CompareTo(b.TimeSeconds));

        for (int i = 0; i < _records.Count; i++)
        {
            LeaderboardRowView row = GetRow(i);
            row.gameObject.SetActive(true);
            row.gameObject.name = $"Player{i + 1}";
            row.Bind(i + 1, _records[i]);
        }

        HideUnusedRows(_records.Count);

        // Recalculate the Content height now, then start at the top.
        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        _scrollRect.StopMovement();
        _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, 0f);
    }

    // Rows 1-4 come from the scene. From the 5th on, a copy of the Player4 prefab is created.
    private LeaderboardRowView GetRow(int index)
    {
        if (index < _fixedRows.Length)
        {
            return _fixedRows[index];
        }

        int extraIndex = index - _fixedRows.Length;
        while (_extraRows.Count <= extraIndex)
        {
            _extraRows.Add(Instantiate(_extraRowPrefab, _content));
        }

        return _extraRows[extraIndex];
    }

    // Rows without data are hidden (not destroyed), so they can be reused later.
    private void HideUnusedRows(int usedCount)
    {
        for (int i = usedCount; i < _fixedRows.Length; i++)
        {
            _fixedRows[i].gameObject.SetActive(false);
        }

        int usedExtra = Mathf.Max(0, usedCount - _fixedRows.Length);
        for (int i = usedExtra; i < _extraRows.Count; i++)
        {
            _extraRows[i].gameObject.SetActive(false);
        }
    }

    private static List<LeaderboardRecord> CreateDummyData()
    {
        return new List<LeaderboardRecord>
        {
            new LeaderboardRecord("Arun Kumar", 62.418f),
            new LeaderboardRecord("Priya S", 64.052f),
            new LeaderboardRecord("Karthik", 65.731f),
            new LeaderboardRecord("Meena", 67.204f),
            new LeaderboardRecord("Ravi", 69.880f),
            new LeaderboardRecord("Divya", 71.326f),
            new LeaderboardRecord("Sanjay", 73.915f)
        };
    }

    // ---------- Test helpers: right-click the component header in Play mode ----------

    [ContextMenu("Add Dummy Player")]
    private void AddDummyPlayer()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        string playerName = DummyNames[Random.Range(0, DummyNames.Length)];
        _records.Add(new LeaderboardRecord(playerName, Random.Range(60f, 120f)));
        Refresh();
    }

    [ContextMenu("Remove Last Player")]
    private void RemoveLastPlayer()
    {
        if (!Application.isPlaying || _records.Count == 0)
        {
            return;
        }

        _records.RemoveAt(_records.Count - 1);
        Refresh();
    }
}