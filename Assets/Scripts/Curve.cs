using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// https://easings.net/
/// </summary>
public class Curve
{
    static float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1f) / 2f;
    }

    static float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    public static float Evaluate(TimingFunction timingFunction, float x)
    {
        switch (timingFunction)
        {
            case TimingFunction.EaseInOutSine:
                return EaseInOutSine(x);
            case TimingFunction.EaseOutCubic:
                return EaseOutCubic(x);
            default:
                return float.NaN;
        }
    }
}
