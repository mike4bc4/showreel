using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

public class AnimationManager : MonoBehaviour
{
    static AnimationManager s_Instance;
    List<Animation> m_Animations;

    public static AnimationManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && s_Instance == null)
            {
                s_Instance = GameObject.FindAnyObjectByType<AnimationManager>();
                Assert.IsNotNull(s_Instance, "No Animation Manager on scene.");
            }
#endif
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Destroy(this);
            return;
        }

        s_Instance = this;
        m_Animations = new List<Animation>();
    }

    static Animation GetAnimation(object obj, string property)
    {
        foreach (var animation in s_Instance.m_Animations)
        {
            if (animation.obj == obj && animation.property == property)
            {
                return animation;
            }
        }

        return null;
    }

    static void StopAnimation(Animation animation)
    {
        s_Instance.StopCoroutine(animation.coroutine);
        s_Instance.m_Animations.Remove(animation);
    }

    static T LerpUnclamped<T>(T a, T b, float t)
    {

        switch (a, b)
        {
            case (float lhs, float rhs):
                return (T)(object)Mathf.LerpUnclamped(lhs, rhs, t);
            case (Vector2 lhs, Vector2 rhs):
                return (T)(object)Vector2.LerpUnclamped(lhs, rhs, t);
            case (Vector3 lhs, Vector3 rhs):
                return (T)(object)Vector3.LerpUnclamped(lhs, rhs, t);
            case (Color lhs, Color rhs):
                return (T)(object)Color.LerpUnclamped(lhs, rhs, t);
            case (Quaternion lhs, Quaternion rhs):
                return (T)(object)Quaternion.LerpUnclamped(lhs, rhs, t);
            default:
                throw new NotSupportedException($"Cannot interpolate function of type {typeof(T)}");
        }
    }

    static PropertyInfo GetAndValidateAnimationProperty<T>(object obj, string property, T targetValue)
    {
        var propertyInfo = obj.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (propertyInfo == null)
        {
            Debug.LogErrorFormat("Cannot access property '{0}' in object of type '{1}'.", propertyInfo.Name, obj.GetType());
            return null;
        }

        if (!propertyInfo.CanRead)
        {
            Debug.LogErrorFormat("Cannot read property '{0}' in object of type '{1}'.", propertyInfo.Name, obj.GetType());
            return null;
        }

        if (!propertyInfo.CanWrite)
        {
            Debug.LogErrorFormat("Cannot write to property '{0}' in object of type '{1}'.", propertyInfo.Name, obj.GetType());
            return null;
        }

        if (propertyInfo.PropertyType != typeof(T))
        {
            Debug.LogErrorFormat("Property of type '{0}' does not match type of target value '{1}'.", propertyInfo.PropertyType, typeof(T));
            return null;
        }

        return propertyInfo;
    }

    public static Animation Animate<T>(object obj, string property, T targetValue)
    {
        if (obj == null)
        {
            Debug.LogError("Cannot animate property of null object.");
            return null;
        }

        var animation = GetAnimation(obj, property);
        if (animation != null)
        {
            StopAnimation(animation);
        }

        var propertyInfo = GetAndValidateAnimationProperty(obj, property, targetValue);
        if (propertyInfo == null)
        {
            return null;
        }

        float elapsedTime = 0f;
        T initialValue = (T)propertyInfo.GetValue(obj);

        IEnumerator Coroutine()
        {
            yield return null;
            while (true)
            {
                if (obj == null)
                {
                    StopAnimation(animation);
                    break;
                }

                if (elapsedTime > animation.time)
                {
                    StopAnimation(animation);
                    animation.InvokeFinished();
                    break;
                }

                float curveValue = Curve.Evaluate(animation.timingFunction, Mathf.Min(elapsedTime / animation.time, 1f));
                T value = LerpUnclamped(initialValue, targetValue, curveValue);
                propertyInfo.SetValue(obj, value);

                elapsedTime += Time.deltaTime;
                animation.elapsedTime = elapsedTime;
                yield return null;
            }
        }

        animation = new Animation(obj, property, s_Instance.StartCoroutine(Coroutine()));
        s_Instance.m_Animations.Add(animation);
        return animation;
    }
}
