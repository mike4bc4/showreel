using System.Collections;
using System.Collections.Generic;
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
            property = nameof(Layer.alpha),
            targetValue = 1f,
            time = 1f,
        };
    }

    public static AnimationDescriptor<float> AlphaZero
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(Layer.alpha),
            targetValue = 0f,
            time = 1f,
        };
    }

    public static AnimationDescriptor<float> BlurZero
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(Layer.blur),
            targetValue = 0f,
            time = 1f,
        };
    }

    public static AnimationDescriptor<float> BlurDefault
    {
        get => new AnimationDescriptor<float>()
        {
            property = nameof(Layer.blur),
            targetValue = Layer.DefaultBlur,
            time = 1f,
        };
    }
}

public class AnimationDescriptor<T> : AnimationDescriptor
{
    public T targetValue;
}
