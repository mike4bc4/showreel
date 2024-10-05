using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public enum PickingModeExtended
    {
        Position,
        Ignore,
        IgnoreSelf,
    }

    public interface IExtendedControl
    {
        public ControlExtension extension { get; }
    }

    public class Control : VisualElement, IExtendedControl
    {
        public new class UxmlFactory : UxmlFactory<Control, UxmlTraits> { }


        // This class is actually almost exact copy of default visual element UxmlTraits, the only 
        // difference is picking mode uxml attribute having type of custom picking mode.
        public new class UxmlTraits : UnityEngine.UIElements.UxmlTraits
        {
            UxmlStringAttributeDescription test = new UxmlStringAttributeDescription { name = "test" };
            UxmlStringAttributeDescription m_Name = new UxmlStringAttributeDescription { name = "name" };
            UxmlStringAttributeDescription m_ViewDataKey = new UxmlStringAttributeDescription { name = "view-data-key" };
            UxmlEnumAttributeDescription<PickingModeExtended> m_PickingMode = new UxmlEnumAttributeDescription<PickingModeExtended> { name = "picking-mode" };
            UxmlStringAttributeDescription m_Tooltip = new UxmlStringAttributeDescription { name = "tooltip" };
            UxmlEnumAttributeDescription<UsageHints> m_UsageHints = new UxmlEnumAttributeDescription<UsageHints> { name = "usage-hints" };
            UxmlIntAttributeDescription m_TabIndex = new UxmlIntAttributeDescription { name = "tabindex", defaultValue = 0 };
            UxmlStringAttributeDescription m_Class = new UxmlStringAttributeDescription { name = "class" };
            UxmlStringAttributeDescription m_ContentContainer = new UxmlStringAttributeDescription { name = "content-container", obsoleteNames = new string[1] { "contentContainer" } };
            UxmlStringAttributeDescription m_Style = new UxmlStringAttributeDescription { name = "style" };

            protected UxmlIntAttributeDescription focusIndex { get; set; } = new UxmlIntAttributeDescription
            {
                name = null,
                obsoleteNames = new string[2] { "focus-index", "focusIndex" },
                defaultValue = -1
            };

            protected UxmlBoolAttributeDescription focusable { get; set; } = new UxmlBoolAttributeDescription
            {
                name = "focusable",
                defaultValue = false
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                if (ve.name == "test")
                {
                    Debug.Log("ASD");
                }

                base.Init(ve, bag, cc);
                if (ve == null)
                {
                    throw new ArgumentNullException("ve");
                }

                ve.name = m_Name.GetValueFromBag(bag, cc);
                ve.viewDataKey = m_ViewDataKey.GetValueFromBag(bag, cc);
                ve.usageHints = m_UsageHints.GetValueFromBag(bag, cc);
                ve.tooltip = m_Tooltip.GetValueFromBag(bag, cc);
                int value = 0;
                if (focusIndex.TryGetValueFromBag(bag, cc, ref value))
                {
                    ve.tabIndex = ((value >= 0) ? value : 0);
                    ve.focusable = value >= 0;
                }

                ve.tabIndex = m_TabIndex.GetValueFromBag(bag, cc);
                ve.focusable = focusable.GetValueFromBag(bag, cc);

                var uiElement = (IExtendedControl)ve;
                uiElement.extension.pickingModeExtended = m_PickingMode.GetValueFromBag(bag, cc);
            }
        }

        ControlExtension m_Extension;

        public ControlExtension extension => m_Extension;

        [Obsolete("Use pickingModeExtended instead.")]
        public new PickingMode pickingMode { get; set; }

        public Control()
        {
            m_Extension = new ControlExtension(this);
        }
    }
}
