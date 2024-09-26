using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using StyleUtility;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extensions
{
    public static class VisualElementUtils
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_RuntimePanelType = s_Assembly.GetType("UnityEngine.UIElements.RuntimePanel");
        static readonly PropertyInfo s_SelectableGameObjectProperty = s_RuntimePanelType.GetProperty("selectableGameObject");

        public static bool IsVisibleInHierarchy(this VisualElement ve)
        {
            var inheritedVisibility = ve.GetInheritedVisibility();
            return inheritedVisibility == StyleKeyword.Null || inheritedVisibility == Visibility.Visible;
        }

        public static StyleEnum<Visibility> GetInheritedVisibility(this VisualElement ve)
        {
            var parent = ve.hierarchy.parent;
            while (parent != null)
            {
                var inlineStyleAccess = new InlineStyleAccess(parent.style);
                if (inlineStyleAccess.TryReadEnumProperty<Visibility>("visibility", out var visibility))
                {
                    return visibility;
                }
                else if (parent.style.visibility != StyleKeyword.Null)
                {
                    return parent.style.visibility;
                }

                parent = parent.hierarchy.parent;
            }

            return StyleKeyword.Null;
        }

        public static bool IsMyDescendant(this VisualElement ve, VisualElement descendant)
        {
            var parent = descendant.hierarchy.parent;
            while (parent != null && parent != ve)
            {
                parent = parent.hierarchy.parent;
            }

            return parent == ve;
        }

        /// <returns>
        /// GameObject which has PanelEventHandler and PanelRaycaster components. This game object is usually
        /// a child of EventSystem and will be created for each active PanelSettings object attached to UIDocuments on scene.
        /// </returns> 
        public static GameObject GetSelectableGameObject(this VisualElement ve)
        {
            return ve?.panel != null ? (GameObject)s_SelectableGameObjectProperty.GetValue(ve.panel) : null;
        }

        public static VisualElement GetRootVisualElement(this VisualElement ve)
        {
            if (ve.panel == null || ve.hierarchy.parent == null)
            {
                return null;
            }

            if (ve.panel.contextType == ContextType.Player)
            {
                var parent = ve.hierarchy.parent;
                while (true)
                {
                    if (parent.hierarchy.parent == null)
                    {
                        return parent;
                    }

                    parent = parent.hierarchy.parent;
                }
            }
            else
            {
                var parent = ve.hierarchy.parent;
                while (true)
                {
                    if (parent == null)
                    {
                        return null;
                    }
                    else if (parent.name == "document")
                    {
                        return parent;
                    }

                    parent = parent.hierarchy.parent;
                }
            }
        }
    }
}
