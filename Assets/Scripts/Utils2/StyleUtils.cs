using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils2
{
    public class StyleTransition
    {
        public StylePropertyName property;
        public float duration;
        public EasingMode timingFunction;
        public float delay;
    }


    public static class StyleUtils
    {
        public static void SetSize(this IStyle style, StyleLength width, StyleLength height)
        {
            style.width = width;
            style.height = height;
        }

        public static void SetSize(this IStyle style, Vector2 size)
        {
            style.width = size.x;
            style.height = size.y;
        }

        public static void SetSize(this IStyle style, StyleKeyword keyword)
        {
            style.width = keyword;
            style.height = keyword;
        }

        public static async UniTask SetPropertyAsync<T>(this IStyle style, string propertyName, T value, CancellationToken ct = default)
        {
            var property = typeof(IStyle).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (!typeof(T).IsAssignableFrom(property.PropertyType))
            {
                throw new ArgumentException($"'{typeof(T)}' is not assignable from '{property.PropertyType}'.");
            }

            var previousValue = property.GetValue(style);
            property.SetValue(style, value);

            try
            {
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                property.SetValue(style, previousValue);
                await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                throw;
            }
        }

        // public static async UniTask AddTransitionAsync(this IStyle style, StylePropertyName property, float duration, EasingMode timingFunction = EasingMode.Ease, float delay = 0f, CancellationToken cancellationToken = default)
        // {
        //     await UniTask.WaitForEndOfFrame(CoroutineRunner.instance, cancellationToken);
        //     style.AddTransition(property, duration, timingFunction, delay);
        // }

        public static void SetPosition(this IStyle style, StylePosition stylePosition)
        {
            style.position = stylePosition.position;
            style.top = stylePosition.top;
            style.right = stylePosition.right;
            style.left = stylePosition.left;
            style.bottom = stylePosition.bottom;
        }

        public static void AddTransition(this IStyle style, StyleTransition transition)
        {
            if (transition != null)
            {
                style.AddTransition(transition.property, transition.duration, transition.timingFunction, transition.delay);
            }
        }

        public static void AddTransition(this IStyle style, StylePropertyName property, float duration, EasingMode timingFunction = EasingMode.Ease, float delay = 0f)
        {
            var transitionProperties = style.transitionProperty.ToList();
            int index = transitionProperties.IndexOf(property);
            if (index < 0)
            {
                transitionProperties.Add(property);
                index = transitionProperties.Count - 1;
                style.transitionProperty = transitionProperties;
            }

            var transitionDurations = style.transitionDuration.ToList();
            if (transitionDurations.Count > index)
            {
                transitionDurations[index] = duration;
            }
            else
            {
                transitionDurations.Add(duration);
            }

            style.transitionDuration = transitionDurations;

            var transitionTimingFunctions = style.transitionTimingFunction.ToList();
            if (transitionTimingFunctions.Count > index)
            {
                transitionTimingFunctions[index] = timingFunction;
            }
            else
            {
                transitionTimingFunctions.Add(timingFunction);
            }

            style.transitionTimingFunction = transitionTimingFunctions;

            var transitionDelays = style.transitionDelay.ToList();
            if (transitionDelays.Count > index)
            {
                transitionDelays[index] = delay;
            }
            else
            {
                transitionDelays.Add(delay);
            }

            style.transitionDelay = transitionDelays;
        }

        public static List<T> ToList<T>(this StyleList<T> styleList)
        {
            return styleList.value != null ? new List<T>(styleList.value) : new List<T>();
        }


        public static StyleTransition GetTransition(this IStyle style, StylePropertyName property)
        {
            var transitionProperties = style.transitionProperty.ToList();
            int index = transitionProperties.IndexOf(property);
            if (index < 0)
            {
                return null;
            }

            var transition = new StyleTransition() { property = property };

            var transitionDurations = style.transitionDuration.ToList();
            if (transitionDurations.Count > index)
            {
                transition.duration = transitionDurations[index].value;
            }

            var transitionTimingFunctions = style.transitionTimingFunction.ToList();
            if (transitionTimingFunctions.Count > index)
            {
                transition.timingFunction = transitionTimingFunctions[index].mode;
            }

            var transitionDelays = style.transitionDelay.ToList();
            if (transitionDelays.Count > index)
            {
                transition.delay = transitionDelays[index].value;
            }

            return transition;
        }

        public static void RemoveTransition(this IStyle style, StylePropertyName property)
        {
            var transitionProperties = style.transitionProperty.ToList();
            if (!transitionProperties.Any())
            {
                return;
            }

            int index = transitionProperties.IndexOf(property);
            if (index < 0)
            {
                return;
            }

            transitionProperties.RemoveAt(index);
            style.transitionProperty = transitionProperties.Count > 0 ? transitionProperties : StyleKeyword.Null;

            var transitionDurations = style.transitionDuration.ToList();
            if (transitionDurations.Count > index)
            {
                transitionDurations.RemoveAt(index);
                style.transitionDuration = transitionDurations.Count > 0 ? transitionDurations : StyleKeyword.Null;
            }

            var transitionTimingFunctions = style.transitionTimingFunction.ToList();
            if (transitionTimingFunctions.Count > index)
            {
                transitionTimingFunctions.RemoveAt(index);
                style.transitionTimingFunction = transitionTimingFunctions.Count > 0 ? transitionTimingFunctions : StyleKeyword.Null;
            }

            var transitionDelays = style.transitionDelay.ToList();
            if (transitionDelays.Count > index)
            {
                transitionDelays.RemoveAt(index);
                style.transitionDelay = transitionDelays.Count > 0 ? transitionDelays : StyleKeyword.Null;
            }
        }
    }
}