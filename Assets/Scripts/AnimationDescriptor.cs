using System.Collections;
using System.Collections.Generic;
using Layers;
using UI;
using UnityEngine;


public abstract class AnimationDescriptor
{
    public string property;
    public float time;

    public static AnimationDescriptor<float> AlphaOne
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(UILayer.alpha),
            targetValue = 1f,
            time = 1f,
        };
    }

    public static AnimationDescriptor<float> AlphaZero
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(UILayer.alpha),
            targetValue = 0f,
            time = 1f,
        };
    }

    public static AnimationDescriptor<float> BlurZero
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(UILayer.blurSize),
            targetValue = 0f,
            time = 1f,
        };
    }

    public static AnimationDescriptor<float> BlurDefault
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(UILayer.blurSize),
            targetValue = UILayer.DefaultBlurSize,
            time = 1f,
        };
    }
}

public class AnimationDescriptor<T> : AnimationDescriptor
{
    public T targetValue;
}
