using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementUtils
{
    public static void UnregisterPreviewModeChangedCallback(this VisualElement ve, EventCallback<ChangeEvent<bool>> callback)
    {
        if (ve.panel != null && ve.panel.contextType == ContextType.Editor)
        {
            var previewButton = ve.panel.visualTree.Q<Toggle>("preview-button");
            if (previewButton != null)
            {
                previewButton.UnregisterCallback(callback);
            }
        }
    }

    public static void RegisterPreviewModeChangedCallback(this VisualElement ve, EventCallback<ChangeEvent<bool>> callback)
    {
        if (ve.panel != null && ve.panel.contextType == ContextType.Editor)
        {
            var previewButton = ve.panel.visualTree.Q<Toggle>("preview-button");
            if (previewButton != null)
            {
                previewButton.RegisterValueChangedCallback(callback);
            }
        }
    }

    public static VisualElementCoroutine StartCoroutine(this VisualElement ve, IEnumerator enumerator)
    {
        var enumeratorQueue = new List<IEnumerator>() { enumerator };
        IVisualElementScheduledItem item = null;
        item = ve.schedule.Execute(() =>
        {
            IEnumerator e = null;
            if (enumeratorQueue.Count > 0)
            {
                e = enumeratorQueue[enumeratorQueue.Count - 1];
            }
            else
            {
                item.Pause();
                item = null;
            }

            if (e.Current is IEnumerator ne)
            {
                enumeratorQueue.Add(ne);
            }
            else if (!e.MoveNext())
            {
                enumeratorQueue.RemoveAt(enumeratorQueue.Count - 1);
                if (enumeratorQueue.Count > 0)
                {
                    enumeratorQueue[enumeratorQueue.Count - 1].MoveNext();
                }
            }
        }).Every(0);

        return new VisualElementCoroutine(item);
    }

    public static VisualElement CreateSnapshot(this VisualElement ve, SnapshotMode snapshotMode = SnapshotMode.Default)
    {
        var snapshot = (VisualElement)Activator.CreateInstance(ve.GetType());
        snapshot.name = (string.IsNullOrEmpty(ve.name) ? "unnamed" : ve.name) + "-snapshot";
        CopyStyle(ve, snapshot);

        // TODO: Copy more control-specific parameters if necessary.
        switch (ve)
        {
            case Label label:
                ((Label)snapshot).text = label.text;
                break;
        }

        if (snapshotMode.HasFlag(SnapshotMode.SetAbsoluteWorldPosition))
        {
            var rect = ve.worldBound;
            snapshot.style.SetPosition(new StylePosition()
            {
                position = Position.Absolute,
                top = rect.y - ve.resolvedStyle.marginTop,
                left = rect.x - ve.resolvedStyle.marginLeft,
            });

            snapshot.style.width = rect.width;
            snapshot.style.height = rect.height;
        }

        if (snapshotMode.HasFlag(SnapshotMode.ClearTransitions))
        {
            snapshot.style.transitionDelay = StyleKeyword.Initial;
            snapshot.style.transitionDuration = StyleKeyword.Initial;
            snapshot.style.transitionProperty = StyleKeyword.Initial;
            snapshot.style.transitionTimingFunction = StyleKeyword.Initial;
        }

        return snapshot;
    }

    static void CopyStyle(VisualElement source, VisualElement target)
    {
        target.style.alignContent = source.resolvedStyle.alignContent;
        target.style.alignItems = source.resolvedStyle.alignItems;
        target.style.alignSelf = source.resolvedStyle.alignSelf;
        target.style.backgroundColor = source.resolvedStyle.backgroundColor;
        target.style.backgroundImage = source.resolvedStyle.backgroundImage;
        target.style.backgroundPositionX = source.resolvedStyle.backgroundPositionX;
        target.style.backgroundPositionY = source.resolvedStyle.backgroundPositionY;
        target.style.backgroundRepeat = source.resolvedStyle.backgroundRepeat;
        target.style.backgroundSize = source.resolvedStyle.backgroundSize;
        target.style.borderBottomColor = source.resolvedStyle.borderBottomColor;
        target.style.borderBottomLeftRadius = source.resolvedStyle.borderBottomLeftRadius;
        target.style.borderBottomRightRadius = source.resolvedStyle.borderBottomRightRadius;
        target.style.borderBottomWidth = source.resolvedStyle.borderBottomWidth;
        target.style.borderLeftColor = source.resolvedStyle.borderLeftColor;
        target.style.borderLeftWidth = source.resolvedStyle.borderLeftWidth;
        target.style.borderRightColor = source.resolvedStyle.borderRightColor;
        target.style.borderRightWidth = source.resolvedStyle.borderRightWidth;
        target.style.borderTopColor = source.resolvedStyle.borderTopColor;
        target.style.borderTopLeftRadius = source.resolvedStyle.borderTopLeftRadius;
        target.style.borderTopRightRadius = source.resolvedStyle.borderTopRightRadius;
        target.style.borderTopWidth = source.resolvedStyle.borderTopWidth;
        target.style.bottom = source.resolvedStyle.bottom;
        target.style.color = source.resolvedStyle.color;
        target.style.display = source.resolvedStyle.display;
        target.style.flexBasis = source.resolvedStyle.flexBasis.value;
        target.style.flexDirection = source.resolvedStyle.flexDirection;
        target.style.flexGrow = source.resolvedStyle.flexGrow;
        target.style.flexShrink = source.resolvedStyle.flexShrink;
        target.style.flexWrap = source.resolvedStyle.flexWrap;
        target.style.fontSize = source.resolvedStyle.fontSize;
        target.style.height = source.resolvedStyle.height;
        target.style.justifyContent = source.resolvedStyle.justifyContent;
        target.style.left = source.resolvedStyle.left;
        target.style.letterSpacing = source.resolvedStyle.letterSpacing;
        target.style.marginBottom = source.resolvedStyle.marginBottom;
        target.style.marginLeft = source.resolvedStyle.marginLeft;
        target.style.marginRight = source.resolvedStyle.marginRight;
        target.style.marginTop = source.resolvedStyle.marginTop;
        target.style.maxHeight = source.resolvedStyle.maxHeight == StyleKeyword.None ? StyleKeyword.Initial : source.resolvedStyle.maxHeight.value;
        target.style.maxWidth = source.resolvedStyle.maxWidth == StyleKeyword.None ? StyleKeyword.Initial : source.resolvedStyle.maxWidth.value;
        target.style.minHeight = source.resolvedStyle.minHeight == StyleKeyword.None ? StyleKeyword.Initial : source.resolvedStyle.minHeight.value;
        target.style.minWidth = source.resolvedStyle.minWidth == StyleKeyword.None ? StyleKeyword.Initial : source.resolvedStyle.minWidth.value;
        target.style.opacity = source.resolvedStyle.opacity;
        target.style.paddingBottom = source.resolvedStyle.paddingBottom;
        target.style.paddingLeft = source.resolvedStyle.paddingLeft;
        target.style.paddingRight = source.resolvedStyle.paddingRight;
        target.style.paddingTop = source.resolvedStyle.paddingTop;
        target.style.position = source.resolvedStyle.position;
        target.style.right = source.resolvedStyle.right;
        target.style.rotate = source.resolvedStyle.rotate;
        target.style.scale = source.resolvedStyle.scale;
        target.style.textOverflow = source.resolvedStyle.textOverflow;
        target.style.top = source.resolvedStyle.top;
        target.style.transformOrigin = Vec3ToTransformOrigin(source.resolvedStyle.transformOrigin);
        target.style.transitionDelay = new List<TimeValue>(source.resolvedStyle.transitionDelay);
        target.style.transitionDuration = new List<TimeValue>(source.resolvedStyle.transitionDuration);
        target.style.transitionProperty = new List<StylePropertyName>(source.resolvedStyle.transitionProperty);
        target.style.transitionTimingFunction = new List<EasingFunction>(source.resolvedStyle.transitionTimingFunction);
        target.style.translate = Vec3ToTranslate(source.resolvedStyle.translate);
        target.style.unityBackgroundImageTintColor = source.resolvedStyle.unityBackgroundImageTintColor;
        target.style.unityFont = source.resolvedStyle.unityFont;
        target.style.unityFontDefinition = source.resolvedStyle.unityFontDefinition;
        target.style.unityFontStyleAndWeight = source.resolvedStyle.unityFontStyleAndWeight;
        target.style.unityParagraphSpacing = source.resolvedStyle.unityParagraphSpacing;
        target.style.unitySliceBottom = source.resolvedStyle.unitySliceBottom;
        target.style.unitySliceLeft = source.resolvedStyle.unitySliceLeft;
        target.style.unitySliceRight = source.resolvedStyle.unitySliceRight;
        target.style.unitySliceScale = source.resolvedStyle.unitySliceScale;
        target.style.unitySliceTop = source.resolvedStyle.unitySliceTop;
        target.style.unityTextAlign = source.resolvedStyle.unityTextAlign;
        target.style.unityTextOutlineColor = source.resolvedStyle.unityTextOutlineColor;
        target.style.unityTextOutlineWidth = source.resolvedStyle.unityTextOutlineWidth;
        target.style.unityTextOverflowPosition = source.resolvedStyle.unityTextOverflowPosition;
        target.style.visibility = source.resolvedStyle.visibility;
        target.style.whiteSpace = source.resolvedStyle.whiteSpace;
        target.style.width = source.resolvedStyle.width;
        target.style.wordSpacing = source.resolvedStyle.wordSpacing;
    }

    static TransformOrigin Vec3ToTransformOrigin(Vector3 vec3)
    {
        return new TransformOrigin(vec3.x, vec3.y, vec3.z);
    }

    static Translate Vec3ToTranslate(Vector3 vec3)
    {
        return new Translate(vec3.x, vec3.y, vec3.z);
    }
}
