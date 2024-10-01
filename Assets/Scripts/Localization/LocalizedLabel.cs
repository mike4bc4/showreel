using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Localization
{
    public interface ILocalizable
    {
        public string key { get; set; }
        public string table { get; set; }
    }

    public interface ILocalizedElement : ILocalizable
    {
        public string text { get; set; }
    }

    public class LocalizedLabel : Label, ILocalizedElement
    {
        public new class UxmlFactory : UxmlFactory<LocalizedLabel, UxmlTraits> { }

        public new class UxmlTraits : Label.UxmlTraits
        {
            UxmlStringAttributeDescription m_Table = new UxmlStringAttributeDescription() { name = "table" };
            UxmlStringAttributeDescription m_Key = new UxmlStringAttributeDescription() { name = "key" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }

        string m_Table;
        string m_Key;

        public string table
        {
            get => m_Table;
            set => m_Table = value;
        }

        public string key
        {
            get => m_Key;
            set => m_Key = value;
        }

        public LocalizedLabel() : base()
        {
            RegisterElement();
        }

        public LocalizedLabel(string text) : base(text)
        {
            RegisterElement();
        }

        public void Localize(string locale)
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
