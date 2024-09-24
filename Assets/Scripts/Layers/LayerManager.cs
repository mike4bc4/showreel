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
        const string k_DestinationTexturePropertyName = "_DestTex";
        const CameraEvent k_CameraEvent = CameraEvent.AfterEverything;
        const int m_IntervalDelayMs = 500;
        static LayerManager s_Instance;

        [SerializeField] Shader m_LayerShader;
        [SerializeField] PanelSettings m_PanelSettings;
        [SerializeField] Material m_BlitCopyMaterial;
        [SerializeField] Material m_BlitOverMaterial;

        CommandBuffer m_CommandBuffer;
        bool m_CommandBufferDirty;
        Vector2Int m_PreviousScreenSize;
        GroupLayer m_RootGroupLayer;
        int m_PropertyNameIndex;

        static PanelSettings panelSettings => s_Instance.m_PanelSettings;
        static Shader layerShader => s_Instance.m_LayerShader;
        static new Transform transform => ((Component)s_Instance).transform;
        static Material blitCopyMaterial => s_Instance.m_BlitCopyMaterial;
        static GroupLayer rootGroupLayer => s_Instance.m_RootGroupLayer;
        static Material blitOverMaterial => s_Instance.m_BlitOverMaterial;

        public static LayerManager Instance
        {
            get => s_Instance;
        }

        static int propertyNameIndex
        {
            get => s_Instance.m_PropertyNameIndex;
            set => s_Instance.m_PropertyNameIndex = value;
        }

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
            m_RootGroupLayer = CreateGroupLayerInternal("Root");
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
            foreach (var layer in m_RootGroupLayer.descendants)
            {
                if (layer is UILayer uiLayer)
                {
                    RenderTexture.ReleaseTemporary(uiLayer.uiDocument.panelSettings.targetTexture);
                    var renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
                    uiLayer.uiDocument.panelSettings.targetTexture = renderTexture;
                    MarkCommandBufferDirty();
                }
            }
        }

        public static void MarkCommandBufferDirty()
        {
            commandBufferDirty = true;
        }

        static GroupLayer CreateGroupLayerInternal(string name = "Unnamed")
        {
            var gameObject = new GameObject();
            gameObject.transform.SetParent(transform);

            var layer = gameObject.AddComponent<GroupLayer>();
            layer.name = name;
            layer.Init();

            MarkCommandBufferDirty();
            return layer;
        }

        public static GroupLayer CreateGroupLayer(string name = "Unnamed")
        {
            var layer = CreateGroupLayerInternal(name);
            rootGroupLayer.Add(layer);
            return layer;
        }

        public static PostProcessingLayer CreatePostProcessingLayer(string name = "Unnamed")
        {
            var gameObject = new GameObject();
            gameObject.transform.SetParent(transform);

            var layer = gameObject.AddComponent<PostProcessingLayer>();
            layer.name = name;
            layer.Init(new Material(layerShader));
            rootGroupLayer.Add(layer);

            MarkCommandBufferDirty();
            return layer;
        }

        public static UILayer CreateUILayer(string name = "Unnamed")
        {
            var renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);

            var ps = Instantiate(panelSettings);
            ps.targetTexture = renderTexture;

            var gameObject = new GameObject();
            gameObject.transform.SetParent(transform);

            var uiDocument = gameObject.AddComponent<UIDocument>();
            uiDocument.panelSettings = ps;

            var layer = gameObject.AddComponent<UILayer>();
            layer.name = name;
            layer.Init(new Material(layerShader), uiDocument);
            rootGroupLayer.Add(layer);

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
            rootGroupLayer.Remove(baseLayer);
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
            foreach (var layer in rootGroupLayer.descendants)
            {
                if (layer.name == name)
                {
                    return layer;
                }
            }

            return null;
        }

        static string CreateUniquePropertyName(string propertyName)
        {
            return propertyName + "_" + propertyNameIndex++;
        }

        static void RebuildCommandBuffer()
        {
            propertyNameIndex = 0;
            commandBufferDirty = false;
            if (commandBuffer != null)
            {
                Camera.main.RemoveCommandBuffer(k_CameraEvent, commandBuffer);
            }

            if (rootGroupLayer.descendants.Count == 0)
            {
                return;
            }

            commandBuffer = new CommandBuffer();
            commandBuffer.name = k_CommandBufferName;

            var outputTexID = Shader.PropertyToID(CreateUniquePropertyName("Camera"));
            commandBuffer.GetTemporaryRT(outputTexID, -1, -1);  // -1, -1 For camera pixel width and height.

            // Here we are copying camera texture into output texture to preserve originally generated image.
            // Because of 'reasons' Unity may flip camera target render texture upside down, that's why
            // instead of default 'Hidden/BlitCopy' shader, we are using modified version which flips texture 
            // coords if UNITY_UV_STARTS_AT_TOP keyword is enabled.
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, outputTexID, blitCopyMaterial);

            void RenderGroupLayer(GroupLayer groupLayer, int parentTexID)
            {
                var groupOutputTexId = Shader.PropertyToID(CreateUniquePropertyName(groupLayer.name));
                commandBuffer.GetTemporaryRT(groupOutputTexId, -1, -1);
                commandBuffer.SetRenderTarget(groupOutputTexId);
                commandBuffer.ClearRenderTarget(true, true, Color.clear);

                if (groupLayer.dirty)
                {
                    groupLayer.Sort();
                }

                for (int i = 0; i < groupLayer.count; i++)
                {
                    var layer = groupLayer[i];
                    if (!layer.visible)
                    {
                        continue;
                    }

                    if (layer is UILayer uiLayer)
                    {
                        // First blit to fresh render texture.
                        var tempRT1 = Shader.PropertyToID(CreateUniquePropertyName(layer.name));
                        commandBuffer.GetTemporaryRT(tempRT1, -1, -1);
                        commandBuffer.SetRenderTarget(tempRT1);
                        commandBuffer.ClearRenderTarget(true, true, Color.clear);
                        commandBuffer.Blit(uiLayer.uiDocument.panelSettings.targetTexture, tempRT1, uiLayer.material);

                        // Because Blit basically redraws source into destination using given material,
                        // entire group texture is being overwritten and lost. We want to preserve it, 
                        // so let's copy said texture into another temporary RT. 
                        var tempRT2 = Shader.PropertyToID(CreateUniquePropertyName("GroupCopy"));
                        commandBuffer.GetTemporaryRT(tempRT2, -1, -1);
                        commandBuffer.CopyTexture(groupOutputTexId, tempRT2);

                        // We are setting our 'backup' group texture as global property, so blit over
                        // material can make use of it.
                        commandBuffer.SetGlobalTexture(k_DestinationTexturePropertyName, tempRT2);

                        // Now 'blit over' new layer.
                        commandBuffer.Blit(tempRT1, groupOutputTexId, blitOverMaterial);
                    }
                    else if (layer is PostProcessingLayer postProcessingLayer)
                    {
                        // Since blit source has to be different from destination, first let's make
                        // a copy of group texture.
                        var tempRT = Shader.PropertyToID(CreateUniquePropertyName(layer.name));
                        commandBuffer.GetTemporaryRT(tempRT, -1, -1);
                        commandBuffer.CopyTexture(groupOutputTexId, tempRT);
                        commandBuffer.Blit(tempRT, groupOutputTexId, postProcessingLayer.material);
                    }
                    else if (layer is GroupLayer gLayer)
                    {
                        RenderGroupLayer(gLayer, groupOutputTexId);
                    }
                }

                // Finally 'blit over' entire group, this process is similar to UILayer blit.
                var tempRT3 = Shader.PropertyToID(CreateUniquePropertyName("GroupParentCopy"));
                commandBuffer.GetTemporaryRT(tempRT3, -1, -1);
                commandBuffer.CopyTexture(parentTexID, tempRT3);
                commandBuffer.SetGlobalTexture(k_DestinationTexturePropertyName, tempRT3);
                commandBuffer.Blit(groupOutputTexId, parentTexID, blitOverMaterial);
            }

            RenderGroupLayer(rootGroupLayer, outputTexID);

            commandBuffer.Blit(outputTexID, Camera.main.targetTexture);
            Camera.main.AddCommandBuffer(k_CameraEvent, commandBuffer);
        }
    }
}

