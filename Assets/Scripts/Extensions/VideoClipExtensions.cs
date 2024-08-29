using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public static class VideoClipExtensions
{
    public static Vector2Int GetSize(this VideoClip videoClip)
    {
        return new Vector2Int((int)videoClip.width, (int)videoClip.height);
    }
}
