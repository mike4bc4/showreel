using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extensions
{
    public static class VisualElementUtils
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_RuntimePanelType = s_Assembly.GetType("UnityEngine.UIElements.RuntimePanel");
        static readonly PropertyInfo s_SelectableGameObjectProperty = s_RuntimePanelType.GetProperty("selectableGameObject");

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
    }
}
