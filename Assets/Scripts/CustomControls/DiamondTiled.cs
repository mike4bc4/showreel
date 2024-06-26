using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class DiamondTiled : VisualElement
    {
        const string k_UssClassName = "diamond-tiled";
        const string k_TileUssClassName = k_UssClassName + "__tile";
        const string k_TileTransitionUssClassName = k_TileUssClassName + "--transition";
        const string k_TileTopUssClassName = k_TileUssClassName + "--top";
        const string k_TileRightUssClassName = k_TileUssClassName + "--right";
        const string k_TileBottomUssClassName = k_TileUssClassName + "--bottom";
        const string k_TileLeftUssClassName = k_TileUssClassName + "--left";

        const float k_DefaultAnimationScale = 0.66f;
        const float k_DefaultRestTime = 0.5f;

        public new class UxmlFactory : UxmlFactory<DiamondTiled, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationScale = new UxmlFloatAttributeDescription() { name = "animation-scale", defaultValue = k_DefaultAnimationScale };
            UxmlFloatAttributeDescription m_RestTime = new UxmlFloatAttributeDescription() { name = "rest-time", defaultValue = k_DefaultRestTime };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondTiled diamondTiled = (DiamondTiled)ve;
                diamondTiled.animationScale = m_AnimationScale.GetValueFromBag(bag, cc);
                diamondTiled.restTime = m_RestTime.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_TileTop;
        VisualElement m_TileRight;
        VisualElement m_TileBottom;
        VisualElement m_TileLeft;
        Coroutine m_AnimationCoroutine;

        List<VisualElement> tiles
        {
            get => new List<VisualElement>() { m_TileTop, m_TileRight, m_TileBottom, m_TileLeft };
        }

        public float animationScale { get; set; }

        public float restTime { get; set; }

        public DiamondTiled()
        {
            AddToClassList(k_UssClassName);

            m_TileTop = new VisualElement() { name = "tile-top" };
            m_TileTop.AddToClassList(k_TileUssClassName);
            m_TileTop.AddToClassList(k_TileTopUssClassName);
            m_TileTop.AddToClassList(k_TileTransitionUssClassName);
            Add(m_TileTop);

            m_TileRight = new VisualElement() { name = "tile-right" };
            m_TileRight.AddToClassList(k_TileUssClassName);
            m_TileRight.AddToClassList(k_TileRightUssClassName);
            m_TileRight.AddToClassList(k_TileTransitionUssClassName);
            Add(m_TileRight);

            m_TileBottom = new VisualElement() { name = "tile-bottom" };
            m_TileBottom.AddToClassList(k_TileUssClassName);
            m_TileBottom.AddToClassList(k_TileBottomUssClassName);
            m_TileBottom.AddToClassList(k_TileTransitionUssClassName);
            Add(m_TileBottom);

            m_TileLeft = new VisualElement() { name = "tile-left" };
            m_TileLeft.AddToClassList(k_TileUssClassName);
            m_TileLeft.AddToClassList(k_TileLeftUssClassName);
            m_TileLeft.AddToClassList(k_TileTransitionUssClassName);
            Add(m_TileLeft);

            // UXMLTraits.Init will not be invoked when control is created in constructor of other 
            // custom control, so default values should be set here instead.
            animationScale = k_DefaultAnimationScale;
            restTime = k_DefaultRestTime;
        }

        public void StopAnimation()
        {
            if (m_AnimationCoroutine != null)
            {
                AnimationManager.Instance.StopCoroutine(m_AnimationCoroutine);
            }

            foreach (var tile in tiles)
            {
                tile.style.scale = Vector2.one;
            }
        }

        public void StartAnimation()
        {
            IEnumerator Coroutine()
            {
                var s = new Vector2(animationScale, animationScale);
                var wait = new WaitForSeconds(restTime);
                while (true)
                {
                    foreach (var tile in tiles)
                    {
                        // Debug.Log(tile);
                        tile.style.scale = tile.resolvedStyle.scale;
                        yield return null;

                        tile.style.scale = s;
                        while (tile.resolvedStyle.scale != s)
                        {
                            yield return null;
                        }


                        yield return wait;

                        tile.style.scale = Vector2.one;
                        while (tile.resolvedStyle.scale != Vector2.one)
                        {
                            yield return null;
                        }
                    }
                }
            }

            if (m_AnimationCoroutine != null)
            {
                AnimationManager.Instance.StopCoroutine(m_AnimationCoroutine);
            }

            m_AnimationCoroutine = AnimationManager.Instance.StartCoroutine(Coroutine());
        }
    }
}
