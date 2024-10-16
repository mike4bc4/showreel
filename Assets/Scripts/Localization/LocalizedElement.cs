using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Localization
{
    public abstract class LocalizedElement : Control
    {
        public new class UxmlTraits : Control.UxmlTraits
        {
            UxmlStringAttributeDescription m_LocalizationAddress = new UxmlStringAttributeDescription() { name = "localization-address", defaultValue = null };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = ve as LocalizedElement;
                container.localizationAddress = m_LocalizationAddress.GetValueFromBag(bag, cc);
            }
        }

        LocalizationAddress m_LocalizationAddress;

        protected abstract ILocalizedElement localizedElement { get; }

        public LocalizationAddress localizationAddress
        {
            get => m_LocalizationAddress;
            set
            {
                m_LocalizationAddress = value;
                localizedElement.localizationAddress = m_LocalizationAddress;
            }
        }
    }

    public abstract class MultiLocalizedElement : Control
    {
        public new class UxmlTraits : Control.UxmlTraits
        {
            UxmlStringAttributeDescription m_LocalizationAddresses = new UxmlStringAttributeDescription() { name = "localization-addresses", defaultValue = null };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = ve as MultiLocalizedElement;
                container.localizationAddresses = m_LocalizationAddresses.GetValueFromBag(bag, cc);
            }
        }

        LocalizationAddressCollection m_LocalizationAddresses;

        protected abstract List<ILocalizedElement> localizedElements { get; }

        public LocalizationAddressCollection localizationAddresses
        {
            get => m_LocalizationAddresses;
            set
            {
                m_LocalizationAddresses = value;
                for (int i = 0; i < value.addresses.Count; i++)
                {
                    if (i <= localizedElements.Count)
                    {
                        localizedElements[i].localizationAddress = value.addresses.ElementAt(i);
                    }
                }
            }
        }
    }
}
