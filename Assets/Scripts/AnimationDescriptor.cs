using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimationDescriptor<T>
{
    public string property;
    public T targetValue;
    public float time;
}
