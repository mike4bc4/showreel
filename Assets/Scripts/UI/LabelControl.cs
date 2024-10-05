using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class LabelControl : UnityEngine.UIElements.Label, IExtendedControl
    {
        public new class UxmlFactory : UxmlFactory<LabelControl, UxmlTraits> { }

        public new class UxmlTraits : Control.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text", defaultValue = "Label" };
            UxmlBoolAttributeDescription m_EnableRichText = new UxmlBoolAttributeDescription { name = "enable-rich-text", defaultValue = true };
            UxmlBoolAttributeDescription m_ParseEscapeSequences = new UxmlBoolAttributeDescription { name = "parse-escape-sequences" };
            UxmlBoolAttributeDescription m_DisplayTooltipWhenElided = new UxmlBoolAttributeDescription { name = "display-tooltip-when-elided" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var textElement = (TextElement)ve;
                textElement.text = m_Text.GetValueFromBag(bag, cc);
                textElement.enableRichText = m_EnableRichText.GetValueFromBag(bag, cc);
                textElement.parseEscapeSequences = m_ParseEscapeSequences.GetValueFromBag(bag, cc);
                textElement.displayTooltipWhenElided = m_DisplayTooltipWhenElided.GetValueFromBag(bag, cc);
            }
        }

        ControlExtension m_Extension;

        public ControlExtension extension => m_Extension;

        [Obsolete("Use pickingModeExtended instead.")]
        public new PickingMode pickingMode { get; set; }

        public LabelControl() : this(String.Empty) { }

        public LabelControl(string text)
        {
            m_Extension = new ControlExtension(this);
            this.text = text;
        }
    }
}

