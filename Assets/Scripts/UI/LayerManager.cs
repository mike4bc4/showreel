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
        List<LayerBase> m_Layers;

        static List<LayerBase> layers => s_Instance.m_Layers;
        static List<Layer> layerPool => s_Instance.m_LayerPool;
        static Canvas canvas => s_Instance.m_Canvas;

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
            m_Layers = canvas.GetComponentsInChildren<LayerBase>(true).ToList();
            foreach (var layer in m_Layers)
            {
                layer.Init();
            }

            SortLayers();
        }

        public static void RemoveLayer(LayerBase layer)
        {
            if (layer == null)
            {
                return;
            }

            if (layer is EffectLayer || layerPool.Count >= k_LayerPoolSize)
            {
                if (layer.texture is RenderTexture renderTexture)
                {
                    renderTexture.Release();
                }

                Destroy(layer.gameObject);
                layers.Remove(layer);
            }
            else if (!layerPool.Contains(layer))
            {
                layer.active = false;
                layer.name += "(Inactive)";
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
            layers.Add(layer);
            layer.Init();
            SortLayers();
            return layer;
        }

        public static Layer CreateLayer(string name = "Layer")
        {
            Layer layer = null;
            if (layerPool.Count > 0)
            {
                layer = layerPool[0];
                layerPool.RemoveAt(0);
                layer.ResetLayer();
                layer.name = name;
                layer.active = true;
            }
            else
            {
                var gameObject = CreateLayerGameObject(name);
                layer = gameObject.AddComponent<Layer>();
                layers.Add(layer);
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
            layers.Sort(Layer.Comparer);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].transform.SetSiblingIndex(i);
            }
        }

        public static LayerBase GetLayer(string name)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] == null)
                {
                    // Clear reference in case of layer not being removed via RemoveLayer().
                    layers.RemoveAt(i);
                }
                else if (layers[i].name == name)
                {
                    return layers[i];
                }
            }

            return null;
        }
    }
}
