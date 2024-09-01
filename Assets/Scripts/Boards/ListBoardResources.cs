using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "ListBoardResources", menuName = "Scriptable Objects/List Board Resources")]
public class ListBoardResources : ScriptableSingleton<ListBoardResources>
{
    [SerializeField] List<VideoClip> m_VideoClips;
    [SerializeField] List<VisualTreeAsset> m_VisualTreeAssets;

    public IReadOnlyList<VideoClip> videoClips => m_VideoClips.AsReadOnly();
    public IReadOnlyList<VisualTreeAsset> visualTreeAssets => m_VisualTreeAssets.AsReadOnly();
}
