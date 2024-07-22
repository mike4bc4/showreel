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

    static float EaseInOutCubic(float x)
    {
        return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }

    static float EaseInCubic(float x)
    {
        return x * x * x;
    }

    static float EaseOutSine(float x)
    {
        return Mathf.Sin((x * Mathf.PI) / 2);
    }

    public static float Evaluate(TimingFunction timingFunction, float x)
    {
        switch (timingFunction)
        {
            case TimingFunction.EaseInOutSine:
                return EaseInOutSine(x);
            case TimingFunction.EaseOutCubic:
                return EaseOutCubic(x);
            case TimingFunction.EaseInOutCubic:
                return EaseInOutCubic(x);
            case TimingFunction.EaseInCubic:
                return EaseInCubic(x);
            case TimingFunction.EaseOutSine:
                return EaseOutSine(x);
            default:
                return float.NaN;
        }
    }
}
