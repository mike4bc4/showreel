using System.Collections;
using System.Collections.Generic;
using KeyframeSystem;
using UnityEngine;

public static class AnimationPlayerStatusUtils
{
    public static bool IsPlaying(this AnimationPlayer.Status status)
    {
        return status == AnimationPlayer.Status.Playing;
    }
}
