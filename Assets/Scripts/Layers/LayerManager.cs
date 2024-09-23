using System;
using System.Collections;
using System.Collections.Generic;
using Settings;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Utility;

namespace Layers
{
    public partial class LayerManager : MonoBehaviour
    {
        const string k_CommandBufferName = "UIDocumentCommandBuffer";
        const CameraEvent k_CameraEvent = CameraEvent.AfterEverything;
        const int m_IntervalDelayMs = 500;
        static LayerManager s_Instance;

        [SerializeField] Shader m_LayerShader;
        [SerializeField] PanelSettings m_PanelSettings;
        [SerializeField] Material m_BlitCopyMaterial;

        List<BaseLayer> m_Layers;
        CommandBuffer m_CommandBuffer;
        bool m_CommandBufferDirty;
        Vector2Int m_PreviousScreenSize;

        public static int layerCount => layers.Count;

        static PanelSettings panelSettings => s_Instance.m_PanelSettings;
        static Shader layerShader => s_Instance.m_LayerShader;
        static List<BaseLayer> layers => s_Instance.m_Layers;
        static new Transform transform => ((Component)s_Instance).transform;
        static Material blitCopyMaterial => s_Instance.m_BlitCopyMaterial;

        static bool commandBufferDirty
        {
            get => s_Instance.m_CommandBufferDirty;
            set => s_Instance.m_CommandBufferDirty = value;
        }

        static CommandBuffer commandBuffer
        {
            get => s_Instance.m_CommandBuffer;
            set => s_Instance.m_CommandBuffer = value;
        }

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(this);
            }

            s_Instance = this;
            m_Layers = new List<BaseLayer>();
            m_PreviousScreenSize = new Vector2Int(Screen.width, Screen.height);
            Scheduler.SetInterval(TrackScreenSizeChanges, m_IntervalDelayMs);
        }

        void LateUpdate()
        {
            if (m_CommandBufferDirty)
            {
                RebuildCommandBuffer();
            }
        }

        void TrackScreenSizeChanges()
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (m_PreviousScreenSize != screenSize)
            {
                m_PreviousScreenSize = screenSize;
                OnScreenSizeChanged();
            }
        }

        void OnScreenSizeChanged()
        {
            foreach (var baseLayer in layers)
            {
                if (baseLayer is Layer layer)
                {
                    RenderTexture.ReleaseTemporary(layer.uiDocument.panelSettings.targetTexture);
                    var renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
                    layer.uiDocument.panelSettings.targetTexture = renderTexture;
                    MarkCommandBufferDirty();
                }
            }
        }

        public static void MarkCommandBufferDirty()
        {
            commandBufferDirty = true;
        }

        public static PostProcessingLayer CreatePostProcessingLayer(string name = "Unnamed")
        {
            var gameObject = new GameObject();
            gameObject.transform.SetParent(transform);

            var layer = gameObject.AddComponent<PostProcessingLayer>();
            layer.name = name;
            layer.Init(new Material(layerShader));
            layers.Add(layer);

            MarkCommandBufferDirty();

            return layer;
        }

        public static Layer CreateLayer(string name = "Unnamed")
        {
            var renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);

            var ps = Instantiate(panelSettings);
            ps.targetTexture = renderTexture;

            var gameObject = new GameObject();
            gameObject.transform.SetParent(transform);

            var uiDocument = gameObject.AddComponent<UIDocument>();
            uiDocument.panelSettings = ps;

            var layer = gameObject.AddComponent<Layer>();
            layer.name = name;
            layer.Init(new Material(layerShader), uiDocument);
            layers.Add(layer);

            MarkCommandBufferDirty();

            return layer;
        }

        public static void RemoveLayer(BaseLayer layer)
        {
            if (layer == null)
            {
                return;
            }

            var baseLayer = (BaseLayer)layer;
            layers.Remove(baseLayer);
            DestroyImmediate(baseLayer.gameObject);

            MarkCommandBufferDirty();
        }

        public static void RemoveLayer(string name)
        {
            var layer = GetLayer(name);
            if (layer != null)
            {
                RemoveLayer(layer);
            }
        }

        public static BaseLayer GetLayer(string name)
        {
            foreach (var layer in layers)
            {
                if (layer.name == name)
                {
                    return layer;
                }
            }

            return null;
        }

        static void RebuildCommandBuffer()
        {
            commandBufferDirty = false;
            if (commandBuffer != null)
            {
                Camera.main.RemoveCommandBuffer(k_CameraEvent, commandBuffer);
            }

            if (layers.Count == 0)
            {
                return;
            }

            commandBuffer = new CommandBuffer();
            commandBuffer.name = k_CommandBufferName;

            var outputTexID = Shader.PropertyToID("_OutputRT");
            commandBuffer.GetTemporaryRT(outputTexID, -1, -1);  // -1, -1 For camera pixel width and height.

            // Here we are copying camera texture into output texture to preserve originally generated image.
            // Because of 'reasons' Unity may flip camera target render texture upside down, that's why
            // instead of default 'Hidden/BlitCopy' shader, we are using modified version which flips texture 
            // coords if UNITY_UV_STARTS_AT_TOP keyword is enabled.
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, outputTexID, blitCopyMaterial);

            layers.Sort(BaseLayer.Comparer);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].transform.SetSiblingIndex(i);

                if (!layers[i].visible)
                {
                    continue;
                }

                if (layers[i] is Layer layer)
                {
                    commandBuffer.Blit(layer.uiDocument.panelSettings.targetTexture, outputTexID, layer.material);
                }
                else if (layers[i] is PostProcessingLayer postProcessingLayer)
                {
                    var tmpTexID = Shader.PropertyToID("_Temp1" + i);
                    commandBuffer.GetTemporaryRT(tmpTexID, -1, -1);
                    commandBuffer.Blit(outputTexID, tmpTexID);
                    commandBuffer.Blit(tmpTexID, outputTexID, postProcessingLayer.material);
                }
            }

            commandBuffer.Blit(outputTexID, Camera.main.targetTexture);
            Camera.main.AddCommandBuffer(k_CameraEvent, commandBuffer);
        }
    }
}

