using System.Collections.Generic;
using UnityEngine;

// One asset holding every map, in order: Create > Game > Track List.
[CreateAssetMenu(fileName = "TrackList", menuName = "Game/Track List")]
public class TrackList : ScriptableObject
{
    [SerializeField] private List<TrackDefinition> _tracks = new List<TrackDefinition>();

    public int Count => _tracks.Count;

    public TrackDefinition GetTrack(int index)
    {
        return _tracks[index];
    }
}