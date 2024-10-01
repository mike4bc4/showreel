using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Localization
{
    public abstract class LocalizedElementContainer : VisualElement
    {
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_LocalizationAddress = new UxmlStringAttributeDescription() { name = "localization-address", defaultValue = "Table:Key" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = ve as LocalizedElementContainer;
                container.localizationAddress = m_LocalizationAddress.GetValueFromBag(bag, cc);
            }
        }

        string m_LocalizationAddress;

        protected abstract ILocalizedElement localizedElement { get; }

        protected string localizationAddress
        {
            get => m_LocalizationAddress;
            set
            {
                m_LocalizationAddress = value;
                var words = m_LocalizationAddress.Split(":", System.StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 2)
                {
                    localizedElement.table = words[0];
                    localizedElement.key = words[1];
                }
            }
        }
    }

    public abstract class TwoLocalizedElementsContainer : LocalizedElementContainer
    {
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_LocalizationAddress1 = new UxmlStringAttributeDescription() { name = "localization-address-1", defaultValue = "Table:Key" };
            UxmlStringAttributeDescription m_LocalizationAddress2 = new UxmlStringAttributeDescription() { name = "localization-address-2", defaultValue = "Table:Key" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var localizedElementContainer = ve as TwoLocalizedElementsContainer;
                localizedElementContainer.localizationAddress1 = m_LocalizationAddress1.GetValueFromBag(bag, cc);
                localizedElementContainer.localizationAddress2 = m_LocalizationAddress2.GetValueFromBag(bag, cc);
            }
        }

        protected List<string> m_LocalizationAddresses;
        protected abstract List<ILocalizedElement> localizedElements { get; }

        protected string localizationAddress1
        {
            get => m_LocalizationAddresses[0];
            set => SetLocalizationAddress(value, 0);
        }

        protected string localizationAddress2
        {
            get => m_LocalizationAddresses[1];
            set => SetLocalizationAddress(value, 1);
        }

        public TwoLocalizedElementsContainer()
        {
            m_LocalizationAddresses = new List<string>(new string[2]);
        }

        protected void SetLocalizationAddress(string value, int index)
        {
            m_LocalizationAddresses[index] = value;
            var words = m_LocalizationAddresses[index].Split(":", System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 2 && index < localizedElements.Count)
            {
                localizedElements[index].table = words[0];
                localizedElements[index].key = words[1];
            }
        }
    }
}
