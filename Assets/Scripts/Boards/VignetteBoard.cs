using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Layers;
using System;
using Settings;

namespace Boards
{
    public class VignetteBoard : Board
    {
        [Serializable]
        struct VignetteDescriptor
        {
            [SerializeField] Theme m_Theme;
            [SerializeField] Color m_Color;
            [SerializeField] float m_Dither;
            [SerializeField] float m_PowerFactor;

            public Theme theme => m_Theme;
            public Color color => m_Color;
            public float dither => m_Dither;
            public float powerFactor => m_PowerFactor;
        }

        const string k_LayerName = "Vignette";
        const int k_DisplaySortOrder = 5000;
        const string k_ColorPropertyName = "_Color";
        const string k_DitherPropertyName = "_Dither";
        const string k_PowerFactorPropertyName = "_PowerFactor";

        [SerializeField] Material m_VignetteMaterial;
        [SerializeField] List<VignetteDescriptor> m_VignetteDescriptors;

        CustomLayer m_Layer;

        public override bool interactable { get; set; }
        public override bool blocksRaycasts { get; set; }

        public override void Init()
        {
            m_Layer = LayerManager.CreateCustomLayer(Instantiate(m_VignetteMaterial), k_LayerName);
            m_Layer.displaySortOrder = k_DisplaySortOrder;
            HideImmediate();

            SettingsManager.OnSettingsApplied += OnSettingsApplied;
            if (TryGetVignetteDescriptor(SettingsManager.Theme, out var descriptor))
            {
                SetVignetteDescriptor(descriptor);
            }
        }

        public override void ShowImmediate()
        {
            m_Layer.visible = true;
        }

        public override void HideImmediate()
        {
            m_Layer.visible = false;
        }

        public override void Show(Action onCompleted = null)
        {
            base.Show(onCompleted);
            ShowImmediate();
            m_ShowCompletedCallback?.Invoke();
        }

        public override void Hide(Action onCompleted = null)
        {
            base.Hide(onCompleted);
            HideImmediate();
            m_HideCompletedCallback?.Invoke();
        }

        void OnSettingsApplied()
        {
            if (TryGetVignetteDescriptor(SettingsManager.Theme, out var descriptor))
            {
                SetVignetteDescriptor(descriptor);
            }
        }

        void SetVignetteDescriptor(VignetteDescriptor descriptor)
        {
            m_Layer.material.SetColor(k_ColorPropertyName, descriptor.color);
            m_Layer.material.SetFloat(k_DitherPropertyName, descriptor.dither);
            m_Layer.material.SetFloat(k_PowerFactorPropertyName, descriptor.powerFactor);
        }

        bool TryGetVignetteDescriptor(Theme theme, out VignetteDescriptor descriptor)
        {
            foreach (var vignetteDescriptor in m_VignetteDescriptors)
            {
                if (vignetteDescriptor.theme == theme)
                {
                    descriptor = vignetteDescriptor;
                    return true;
                }
            }

            descriptor = default;
            return false;
        }
    }
}
