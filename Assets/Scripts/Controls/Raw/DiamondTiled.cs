using System.Collections;
using System.Collections.Generic;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class DiamondTiled : VisualElement
    {
        public const float DefaultTargetTileScale = 0.66f;

        const string k_UssClassName = "diamond-tiled";
        const string k_TileUssClassName = k_UssClassName + "__tile";
        const string k_TileTopUssClassName = k_TileUssClassName + "--top";
        const string k_TileRightUssClassName = k_TileUssClassName + "--right";
        const string k_TileBottomUssClassName = k_TileUssClassName + "--bottom";
        const string k_TileLeftUssClassName = k_TileUssClassName + "--left";
        const string k_QuarterInvertedUssClassName = k_UssClassName + "__quarter-inverted";
        const string k_FullUssClassName = k_UssClassName + "__full";
        const string k_AnimationName = "Animation";

        public new class UxmlFactory : UxmlFactory<DiamondTiled, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };
            UxmlFloatAttributeDescription m_TargetTileScale = new UxmlFloatAttributeDescription() { name = "target-tile-scale", defaultValue = DefaultTargetTileScale };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondTiled diamondTiled = (DiamondTiled)ve;
                diamondTiled.targetTileScale = m_TargetTileScale.GetValueFromBag(bag, cc);
                diamondTiled.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_TileTop;
        VisualElement m_TileRight;
        VisualElement m_TileBottom;
        VisualElement m_TileLeft;
        VisualElement m_DiamondQuarterInverted;
        VisualElement m_DiamondFull;
        AnimationPlayer m_Player;
        float m_TargetTileScale;

        List<VisualElement> tiles
        {
            get => new List<VisualElement>() { m_TileTop, m_TileRight, m_TileBottom, m_TileLeft };
        }

        public float targetTileScale
        {
            get => m_TargetTileScale;
            set
            {
                m_TargetTileScale = value;
                m_Player.Sample();
            }
        }

        public float animationProgress
        {
            get => m_Player.animationTime / m_Player.duration;
            set
            {
                var previousFrameIndex = m_Player.frameIndex;
                m_Player.animationTime = m_Player.duration * Mathf.Clamp01(value);
                if (m_Player.frameIndex != previousFrameIndex)
                {
                    m_Player.Sample();
                }
            }
        }

        public DiamondTiled()
        {
            m_Player = new AnimationPlayer();
            m_Player.AddAnimation(CreateAnimation(), k_AnimationName);
            m_Player.animation = m_Player[k_AnimationName];

            AddToClassList(k_UssClassName);

            m_TileTop = new VisualElement() { name = "tile-top" };
            m_TileTop.AddToClassList(k_TileUssClassName);
            m_TileTop.AddToClassList(k_TileTopUssClassName);
            Add(m_TileTop);

            m_TileRight = new VisualElement() { name = "tile-right" };
            m_TileRight.AddToClassList(k_TileUssClassName);
            m_TileRight.AddToClassList(k_TileRightUssClassName);
            Add(m_TileRight);

            m_TileBottom = new VisualElement() { name = "tile-bottom" };
            m_TileBottom.AddToClassList(k_TileUssClassName);
            m_TileBottom.AddToClassList(k_TileBottomUssClassName);
            Add(m_TileBottom);

            m_TileLeft = new VisualElement() { name = "tile-left" };
            m_TileLeft.AddToClassList(k_TileUssClassName);
            m_TileLeft.AddToClassList(k_TileLeftUssClassName);
            Add(m_TileLeft);

            m_DiamondQuarterInverted = new VisualElement() { name = "diamond-quarter-inverted" };
            m_DiamondQuarterInverted.AddToClassList(k_QuarterInvertedUssClassName);
            Add(m_DiamondQuarterInverted);

            m_DiamondFull = new VisualElement() { name = "diamond-full" };
            m_DiamondFull.AddToClassList(k_FullUssClassName);
            Add(m_DiamondFull);

            targetTileScale = DefaultTargetTileScale;
        }

        KeyframeAnimation CreateAnimation()
        {
            var animation = new KeyframeAnimation();
            int interval = 60;
            for (int i = 0; i < tiles.Count; i++)
            {
                var index = i;
                var track = animation.AddTrack(t => tiles[index].style.scale = Vector2.one * Mathf.Lerp(targetTileScale, 1f, t));

                int startFrame = i * interval;
                track.AddKeyframe(startFrame, 1f);
                track.AddKeyframe(startFrame + (int)(interval * 0.2f), 0f);
                track.AddKeyframe(startFrame + (int)(interval * 0.8f), 0f);
                track.AddKeyframe(startFrame + interval, 1f);
            }

            var t1 = animation.AddTrack(t => m_DiamondQuarterInverted.transform.rotation = Quaternion.Euler(0f, 0f, t * 360f));
            t1.AddKeyframe(0, 0f, Easing.StepOut);
            t1.AddKeyframe(interval, 0.25f, Easing.StepOut);
            t1.AddKeyframe(interval * 2, 0.5f, Easing.StepOut);
            t1.AddKeyframe(interval * 3, 0.75f, Easing.StepOut);

            var t2 = animation.AddTrack(opacity =>
            {
                m_DiamondFull.style.opacity = opacity;
                m_DiamondQuarterInverted.style.opacity = 1 - opacity;
            });
            t2.AddKeyframe(0, 1f);
            t2.AddKeyframe(1, 0f, Easing.StepOut);
            t2.AddKeyframe(interval, 1f);
            t2.AddKeyframe(interval + 1, 0f, Easing.StepOut);
            t2.AddKeyframe(interval * 2, 1f);
            t2.AddKeyframe(interval * 2 + 1, 0f, Easing.StepOut);
            t2.AddKeyframe(interval * 3, 1f);
            t2.AddKeyframe(interval * 3 + 1, 0f, Easing.StepOut);
            t2.AddKeyframe(interval * 4, 1f);

            var t3 = animation.AddTrack(index =>
            {
                foreach (var tile in tiles)
                {
                    tile.style.opacity = 0f;
                }

                tiles[(int)index].style.opacity = 1f;
            });
            t3.AddKeyframe(0, 0f, Easing.StepOut);
            t3.AddKeyframe(interval, 1f, Easing.StepOut);
            t3.AddKeyframe(interval * 2, 2f, Easing.StepOut);
            t3.AddKeyframe(interval * 3, 3f, Easing.StepOut);

            return animation;
        }
    }
}
