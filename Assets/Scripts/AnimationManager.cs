using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AnimationManager
{
    static List<AnimationEntry> m_AnimationRegistry = new List<AnimationEntry>();

    class AnimationEntry
    {
        public Animation2 animation;
        public Reference<CancellationTokenSource> cst;
    }

    static AnimationEntry GetAnimationEntry(object obj, string property)
    {
        foreach (var entry in m_AnimationRegistry)
        {
            if (entry.animation.obj == obj && entry.animation.property == property)
            {
                return entry;
            }
        }

        return null;
    }

    static void StopAnimation(AnimationEntry entry)
    {
        var cst = (CancellationTokenSource)entry.cst;
        if (cst != null)
        {
            cst.Cancel();
            cst.Dispose();
            cst = null;
        }

        m_AnimationRegistry.Remove(entry);
    }

    public static void StopAnimation(object obj, string property)
    {
        var entry = GetAnimationEntry(obj, property);
        if (entry != null)
        {
            StopAnimation(entry);
        }
    }

    public static void StopAnimation(Animation2 animation)
    {
        StopAnimation(animation.obj, animation.property);
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
            Debug.LogErrorFormat("Cannot access property '{0}' in object of type '{1}'.", property, obj.GetType());
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

    public static Animation2 Animate<T>(object obj, AnimationDescriptor<T> animationDescriptor)
    {
        var animation = Animate(obj, animationDescriptor.property, animationDescriptor.targetValue);
        animation.time = animationDescriptor.time;
        return animation;
    }


    public static Animation2 Animate<T>(object obj, string property, T targetValue)
    {
        if (obj == null)
        {
            Debug.LogError("Cannot animate property of null object.");
            return null;
        }

        StopAnimation(obj, property);

        var propertyInfo = GetAndValidateAnimationProperty(obj, property, targetValue);
        if (propertyInfo == null)
        {
            return null;
        }

        var cst = (Reference<CancellationTokenSource>)new CancellationTokenSource();
        var finished = (Reference<bool>)false;

        AnimationEntry animationEntry = new AnimationEntry() { cst = cst };
        Animation2 animation = null;
        
        float elapsedTime = 0f;
        T initialValue = (T)propertyInfo.GetValue(obj);
        
        async UniTask Action()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cst.Value.Token);
            while (true)
            {
                if (obj == null)
                {
                    StopAnimation(animationEntry);
                    return;
                }

                float curveValue = Curve.Evaluate(animation.timingFunction, Mathf.Min(elapsedTime / animation.time, 1f));
                T value = LerpUnclamped(initialValue, targetValue, curveValue);
                propertyInfo.SetValue(obj, value);

                if (elapsedTime > animation.time)
                {
                    StopAnimation(animationEntry);
                    animation.InvokeFinished();
                    finished.Value = true;
                    return;
                }

                elapsedTime += Time.deltaTime;
                animation.elapsedTime = elapsedTime;
                await UniTask.NextFrame(cst.Value.Token);
            }
        }

        animation = new Animation2(obj, property, Action(), cst, finished);
        animationEntry.animation = animation;
        m_AnimationRegistry.Add(animationEntry);

        return animation;
    }
}
