using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CustomControls;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class LayerManager : MonoBehaviour
    {
        const int k_LayerPoolSize = 10;

        static LayerManager s_Instance;

        [SerializeField] Canvas m_Canvas;
        [SerializeField] PanelSettings m_TemplatePanelSettings;
        [SerializeField] RenderTexture m_TemplateRenderTexture;
        [SerializeField] Material m_BlurMaterial;
        [SerializeField] Material m_ShineMaterial;
        [SerializeField] Material m_MaskMaterial;
        [SerializeField] Material m_BlurEffectMaterial;

        List<Layer> m_LayerPool;

        public static PanelSettings TemplatePanelSettings
        {
            get => s_Instance.m_TemplatePanelSettings;
        }

        public static RenderTexture TemplateRenderTexture
        {
            get => s_Instance.m_TemplateRenderTexture;
        }

        public static Material BlurMaterial
        {
            get => s_Instance.m_BlurMaterial;
        }

        public static Material ShineMaterial
        {
            get => s_Instance.m_ShineMaterial;
        }

        public static Material MaskMaterial
        {
            get => s_Instance.m_MaskMaterial;
        }

        public static Material BlurEffectMaterial
        {
            get => s_Instance.m_BlurEffectMaterial;
        }

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
            m_LayerPool = new List<Layer>();
        }

        public static void RemoveLayer(LayerBase layer)
        {
            if (layer == null)
            {
                return;
            }

            var layerPool = s_Instance.m_LayerPool;
            if (layer is EffectLayer || layerPool.Count >= k_LayerPoolSize)
            {
                if (layer.texture is RenderTexture renderTexture)
                {
                    renderTexture.Release();
                }

                Destroy(layer.gameObject);
            }
            else if (!layerPool.Contains(layer))
            {
                layer.gameObject.SetActive(false);
                layer.transform.SetAsFirstSibling();
                layer.name += "(Removed)";
                layerPool.Add((Layer)layer);
                SortLayers();
            }
        }

        public static void RemoveLayer(string name)
        {
            var layer = GetLayer(name);
            if (layer != null)
            {
                RemoveLayer(layer);
            }
        }

        static GameObject CreateLayerGameObject(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(s_Instance.m_Canvas.transform);

            // Fill entire canvas.
            var rectTransform = (RectTransform)gameObject.transform;
            rectTransform.localScale = Vector3.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return gameObject;
        }

        public static EffectLayer CreateEffectLayer(string name = "EffectLayer")
        {
            var gameObject = CreateLayerGameObject(name);
            var layer = gameObject.AddComponent<EffectLayer>();
            layer.Init();
            SortLayers();
            return layer;
        }

        public static Layer CreateLayer(string name = "Layer")
        {
            Layer layer = null;
            var layerPool = s_Instance.m_LayerPool;
            if (layerPool.Count > 0)
            {
                layer = layerPool.Last();
                layerPool.RemoveAt(layerPool.Count - 1);
                layer.ResetLayer();
                layer.name = name;
                layer.transform.SetAsLastSibling();
                layer.gameObject.SetActive(true);
            }
            else
            {
                var gameObject = CreateLayerGameObject(name);
                layer = gameObject.AddComponent<Layer>();
                layer.Init();
            }

            SortLayers();
            return layer;
        }

        public static Layer CreateLayer(VisualTreeAsset vta, string name = null)
        {
            var layer = CreateLayer(string.IsNullOrEmpty(name) ? vta.name : name);
            layer.visualTreeAsset = vta;
            return layer;
        }

        public static void SortLayers()
        {
            var layers = s_Instance.m_Canvas.GetComponentsInChildren<LayerBase>(includeInactive: true).ToList();
            layers.Sort(Layer.Comparer);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].transform.SetSiblingIndex(i);
            }
        }

        public static LayerBase GetLayer(string name)
        {
            var layers = s_Instance.m_Canvas.GetComponentsInChildren<LayerBase>(true);
            foreach (var layer in layers)
            {
                if (layer.name == name)
                {
                    return layer;
                }
            }

            return null;
        }
    }
}
