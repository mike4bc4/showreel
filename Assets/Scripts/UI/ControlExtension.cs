using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    // This element extends functionality of custom ui elements. Normally it would be easier to use
    // simple inheritance, but reconstructing implementation of e.g. TextElement class to provide
    // 'bridge' between base element and label is very difficult and would require usage of reflection.
    // That is why it's easier to apply composition over inheritance pattern and 'attach' extension
    // component exposed via interface.
    public class ControlExtension
    {
        VisualElement m_Element;
        PickingModeExtended m_PickingModeExtended;

        public PickingModeExtended pickingModeExtended
        {
            get => m_PickingModeExtended;
            set
            {
                m_PickingModeExtended = value;
                m_Element.pickingMode = isPickableInHierarchy ? PickingMode.Position : PickingMode.Ignore;
                UpdateDescendantsPickingMode();
            }
        }

        public bool isPickableInHierarchy
        {
            get
            {
                if (pickingModeExtended == PickingModeExtended.Ignore || pickingModeExtended == PickingModeExtended.IgnoreSelf)
                {
                    return false;
                }

                var parent = m_Element.hierarchy.parent;
                while (parent != null)
                {
                    if (parent is IExtendedControl element && element.extension.pickingModeExtended == PickingModeExtended.Ignore)
                    {
                        return false;
                    }

                    parent = parent.hierarchy.parent;
                }

                return true;
            }
        }

        public ControlExtension(VisualElement element)
        {
            m_Element = element;

            // if (!Application.isPlaying)
            // {
            // m_Element.schedule.Execute(() =>
            // {
            //     if (m_Element.pickingMode == PickingMode.Position)
            //     {
            //         element.style.backgroundColor = new Color(0f, 1f, 0f, 0.1f);
            //     }
            //     else
            //     {
            //         element.style.backgroundColor = new Color(1f, 0f, 0f, 0.1f);
            //     }
            // }).Every(0);
            // }

            m_Element.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_Element.pickingMode = isPickableInHierarchy ? PickingMode.Position : PickingMode.Ignore;
            UpdateDescendantsPickingMode();
        }

        List<IExtendedControl> GetDescendants()
        {
            var descendants = new List<IExtendedControl>();
            GetDescendantsRecursive(m_Element, descendants);
            return descendants;
        }

        void GetDescendantsRecursive(VisualElement descendant, List<IExtendedControl> descendants)
        {
            for (int i = 0; i < descendant.hierarchy.childCount; i++)
            {
                if (descendant.hierarchy[i] is IExtendedControl uiElement)
                {
                    descendants.Add(uiElement);
                }

                GetDescendantsRecursive(descendant.hierarchy[i], descendants);
            }
        }

        void UpdateDescendantsPickingMode()
        {
            var descendants = GetDescendants();
            if (pickingModeExtended == PickingModeExtended.Ignore)
            {
                foreach (var descendant in descendants)
                {
                    ((VisualElement)descendant).pickingMode = PickingMode.Ignore;
                }
            }
            else
            {
                foreach (var descendant in descendants)
                {
                    if (descendant.extension.isPickableInHierarchy)
                    {
                        ((VisualElement)descendant).pickingMode = PickingMode.Position;
                    }
                    else
                    {
                        ((VisualElement)descendant).pickingMode = PickingMode.Ignore;
                    }
                }
            }
        }
    }
}
