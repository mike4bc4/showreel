using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "ListBoardResources", menuName = "Scriptable Objects/List Board Resources")]
public class ListBoardResources : ScriptableSingleton<ListBoardResources>
{
    [SerializeField] List<VideoClip> m_VideoClips;

    public IReadOnlyList<VideoClip> videoClips => m_VideoClips.AsReadOnly();
}
