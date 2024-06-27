using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class StyleUtils
{
    public static void AddTransition(this IStyle style, StylePropertyName property, float duration, EasingMode timingFunction = EasingMode.Ease, float delay = 0f)
    {
        var transitionProperties = style.transitionProperty.value ?? new List<StylePropertyName>();
        int index = transitionProperties.IndexOf(property);
        if (index < 0)
        {
            transitionProperties.Add(property);
            index = transitionProperties.Count - 1;
            style.transitionProperty = transitionProperties;
        }

        var transitionDurations = style.transitionDuration.value ?? new List<TimeValue>(new TimeValue[1]);
        if (transitionDurations.Count > index)
        {
            transitionDurations[index] = duration;
            style.transitionDuration = transitionDurations;
        }

        var transitionTimingFunctions = style.transitionTimingFunction.value ?? new List<EasingFunction>(new EasingFunction[1]);
        if (transitionTimingFunctions.Count > index)
        {
            transitionTimingFunctions[index] = timingFunction;
            style.transitionTimingFunction = transitionTimingFunctions;
        }

        var transitionDelays = style.transitionDelay.value ?? new List<TimeValue>(new TimeValue[1]);
        if (transitionDelays.Count > index)
        {
            transitionDelays[index] = delay;
            style.transitionDelay = transitionDelays;
        }
    }

    public static void RemoveTransition(this IStyle style, StylePropertyName property)
    {
        var transitionProperties = style.transitionProperty.value;
        if (transitionProperties == null)
        {
            return;
        }

        int index = transitionProperties.IndexOf(property);
        if (index < 0)
        {
            return;
        }

        transitionProperties.RemoveAt(index);
        style.transitionProperty = transitionProperties.Count > 0 ? transitionProperties : new StyleList<StylePropertyName>();

        var transitionDurations = style.transitionDuration.value ?? new List<TimeValue>(new TimeValue[1]);
        if (transitionDurations.Count > index)
        {
            transitionDurations.RemoveAt(index);
            style.transitionDuration = transitionDurations.Count > 0 ? transitionDurations : new StyleList<TimeValue>();
        }

        var transitionTimingFunctions = style.transitionTimingFunction.value ?? new List<EasingFunction>(new EasingFunction[1]);
        if (transitionTimingFunctions.Count > index)
        {
            transitionTimingFunctions.RemoveAt(index);
            style.transitionTimingFunction = transitionTimingFunctions.Count > 0 ? transitionTimingFunctions : new StyleList<EasingFunction>();
        }

        var transitionDelays = style.transitionDelay.value ?? new List<TimeValue>(new TimeValue[1]);
        if (transitionDelays.Count > index)
        {
            transitionDelays.RemoveAt(index);
            style.transitionDelay = transitionDelays.Count > 0 ? transitionDelays : new StyleList<TimeValue>();
        }
    }
}
