using UnityEngine;

// The fixed data of one map. Shown inside TrackList in the Inspector.
[System.Serializable]
public class TrackDefinition
{
    public string DisplayName = "MAP";
    public Sprite Preview;
    public string TimeTrialSceneName = "Race";
    public string HotLapSceneName = "Hotlap";
    [Min(10f)] public float UnlockNextTime = 360f;   // finish a Time Trial under this many seconds to unlock the next map
}