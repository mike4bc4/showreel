using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Localization
{
    public interface ILocalizedElement
    {
        public string text { get; set; }
        public LocalizationAddress localizationAddress { get; set; }
    }

    public class LocalizedLabel : Label, ILocalizedElement
    {
        public new class UxmlFactory : UxmlFactory<LocalizedLabel, UxmlTraits> { }

        public new class UxmlTraits : Label.UxmlTraits
        {
            UxmlStringAttributeDescription m_LocalizationAddress = new UxmlStringAttributeDescription() { name = "localization-address", defaultValue = null };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var localizedLabel = ve as LocalizedLabel;
                localizedLabel.localizationAddress = m_LocalizationAddress.GetValueFromBag(bag, cc);
            }
        }

        LocalizationAddress m_LocalizationAddress;
        public LocalizationAddress localizationAddress
        {
            get => m_LocalizationAddress;
            set => m_LocalizationAddress = value;
        }

        public LocalizedLabel() : base()
        {
            RegisterElement();
        }

        public LocalizedLabel(string text) : base(text)
        {
            RegisterElement();
        }

        public void Localize(string locale = null)
        {
            LocalizationManager.Localize(this, locale);
        }

        void RegisterElement()
        {
            if (Application.isPlaying)
            {
                LocalizationManager.RegisterElement(this);
            }
        }
    }
}
