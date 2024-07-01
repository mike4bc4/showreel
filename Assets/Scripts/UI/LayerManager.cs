using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomControls;
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

        List<Layer> m_LayerPool;

        [SerializeField] VisualTreeAsset test;

        public static Material BlurMaterial
        {
            get => s_Instance.m_BlurMaterial;
        }

        public static Material ShineMaterial
        {
            get => s_Instance.m_ShineMaterial;
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

        void Start()
        {

        }

        public static void RemoveLayer(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException();
            }

            var layerPool = s_Instance.m_LayerPool;
            if (layerPool.Count < k_LayerPoolSize)
            {
                layer.gameObject.SetActive(false);
                layer.transform.SetAsFirstSibling();
                layer.Clear();
                layerPool.Add(layer);
                SortLayers();
            }
            else
            {
                if (layer.texture is RenderTexture renderTexture)
                {
                    renderTexture.Release();
                }

                Destroy(layer.gameObject);
            }
        }

        public static Layer AddNewLayer(string name = "Layer")
        {
            Layer layer = null;
            var layerPool = s_Instance.m_LayerPool;
            if (layerPool.Count > 0)
            {
                layer = layerPool.Last();
                layerPool.RemoveAt(layerPool.Count - 1);
                layer.name = name;
                layer.transform.SetAsLastSibling();
                layer.gameObject.SetActive(true);
            }
            else
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

                layer = gameObject.AddComponent<Layer>();

                var rawImage = gameObject.AddComponent<RawImage>();
                rawImage.texture = new RenderTexture(s_Instance.m_TemplateRenderTexture);

                var uiDocument = gameObject.AddComponent<UIDocument>();
                uiDocument.panelSettings = Instantiate(s_Instance.m_TemplatePanelSettings);
                uiDocument.panelSettings.targetTexture = (RenderTexture)rawImage.texture;

                layer.Init();
            }

            SortLayers();
            return layer;
        }

        public static Layer AddNewLayer(VisualTreeAsset vta, string name = null)
        {
            var layer = AddNewLayer(string.IsNullOrEmpty(name) ? vta.name : name);
            layer.visualTreeAsset = vta;
            return layer;
        }

        public static void SortLayers()
        {
            var layers = s_Instance.m_Canvas.GetComponentsInChildren<Layer>(includeInactive: true).ToList();
            layers.Sort(Layer.Comparer);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].transform.SetSiblingIndex(i);
            }
        }
    }
}
