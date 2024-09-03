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

    public static VisualTreeAsset GetVisualTreeAsset(string name)
    {
        foreach (var vta in Instance.m_VisualTreeAssets)
        {
            Debug.Log(vta.name);

            if (vta.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return vta;
            }
        }

        return null;
    }

    public static VideoClip GetVideoClip(string name)
    {
        foreach (var videoClip in Instance.m_VideoClips)
        {
            if (videoClip.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return videoClip;
            }
        }

        return null;
    }
}
