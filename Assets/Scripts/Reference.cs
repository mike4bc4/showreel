using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reference<T>
{
    public T Value;

    public Reference() { }

    public Reference(T value)
    {
        Value = value;
    }

    public static implicit operator T(Reference<T> box) => box.Value;
    public static explicit operator Reference<T>(T value) => new Reference<T>() { Value = value };
}
