using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class StyleUtils
{
    public static void SetPosition(this IStyle style, StylePosition stylePosition)
    {
        style.position = stylePosition.position;
        style.top = stylePosition.top;
        style.right = stylePosition.right;
        style.left = stylePosition.left;
        style.bottom = stylePosition.bottom;
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
