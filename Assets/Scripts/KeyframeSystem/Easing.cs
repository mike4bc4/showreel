using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public enum Easing
    {
        Linear,
        Ease,
        EaseInSine,
        EaseInOutSine,
        EaseInCubic,
        EaseInOutCubic,
    }

    public static class EasingExtensions
    {
        static float EaseInOutSine(float x)
        {
            return -(Mathf.Cos(Mathf.PI * x) - 1f) * 0.5f;
        }

        static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) * 0.5f;
        }

        static float EaseInSine(float x)
        {
            return 1f - Mathf.Cos((x * Mathf.PI) * 0.5f);
        }

        static float EaseInCubic(float x)
        {
            return x * x * x;
        }

        // https://developer.mozilla.org/en-US/docs/Web/CSS/easing-function
        static float EaseInOut(float x)
        {
            return CubicBezier(0.42f, 0f, 0.58f, 1f, x).y;
        }

        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve Explicit for of cubic bezier curve.
        static Vector2 CubicBezier(float x1, float y1, float x2, float y2, float t)
        {
            // Skipping first segment of equation because P0 is (0,0).
            return 3f * (1f - t) * (1f - t) * t * new Vector2(x1, y1) + 3 * (1f - t) * t * t * new Vector2(x2, y2) + t * t * t * Vector2.one;
        }

        public static float Evaluate(this Easing handle, float x)
        {
            switch (handle)
            {
                case Easing.Ease:
                    return EaseInOut(x);
                case Easing.EaseInSine:
                    return EaseInSine(x);
                case Easing.EaseInOutSine:
                    return EaseInOutSine(x);
                case Easing.EaseInCubic:
                    return EaseInCubic(x);
                case Easing.EaseInOutCubic:
                    return EaseInOutCubic(x);
                default:
                    return x;
            }
        }
    }
}

