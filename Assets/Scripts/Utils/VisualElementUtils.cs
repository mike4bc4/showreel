using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public static class VisualElementUtils
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_RuntimePanelType = s_Assembly.GetType("UnityEngine.UIElements.RuntimePanel");
        static readonly PropertyInfo s_SelectableGameObjectProperty = s_RuntimePanelType.GetProperty("selectableGameObject");

        /// <returns>
        /// GameObject which has PanelEventHandler and PanelRaycaster components. This game object is usually
        /// a child of EventSystem and will be created for each active PanelSettings object attached to UIDocuments on scene.
        /// </returns> 
        public static GameObject GetSelectableGameObject(this VisualElement ve)
        {
            return ve?.panel != null ? (GameObject)s_SelectableGameObjectProperty.GetValue(ve.panel) : null;
        }

        public static List<VisualElement> GetDescendants(this VisualElement ve, bool includeHierarchy = true)
        {
            List<VisualElement> visualElements = new List<VisualElement>();

            void RecursiveFunction(VisualElement visualElement)
            {
                var children = includeHierarchy ? visualElement.hierarchy.Children() : visualElement.Children();
                visualElements.AddRange(children);
                foreach (var child in children)
                {
                    RecursiveFunction(child);
                }
            }

            RecursiveFunction(ve);

            return visualElements;
        }

        public static async UniTask RemoveAsync(this VisualElement ve, VisualElement element, CancellationToken ct = default)
        {
            ve.Remove(element);
            try
            {
                await UniTask.NextFrame(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                ve.Add(element);
                throw;
            }
        }

        public static async UniTask AddAsync(this VisualElement ve, VisualElement child, CancellationToken ct = default)
        {
            ve.Add(child);
            try
            {
                await UniTask.NextFrame(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                ve.Remove(child);
                throw;
            }
        }

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
    }
}
