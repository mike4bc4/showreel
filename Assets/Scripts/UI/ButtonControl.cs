using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class ButtonControl : Control, IExtendedControl
    {
        public new class UxmlFactory : UxmlFactory<ButtonControl, UxmlTraits> { }

        public new class UxmlTraits : Control.UxmlTraits
        {
            public UxmlTraits()
            {
                focusable.defaultValue = true;
            }
        }

        Clickable m_Clickable;

        public event Action clicked
        {
            add
            {
                if (m_Clickable == null)
                {
                    clickable = new Clickable(value);
                }
                else
                {
                    m_Clickable.clicked += value;
                }
            }
            remove
            {
                if (m_Clickable != null)
                {
                    m_Clickable.clicked -= value;
                }
            }
        }

        public Clickable clickable
        {
            get
            {
                return m_Clickable;
            }
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                {
                    this.RemoveManipulator(m_Clickable);
                }

                m_Clickable = value;

                if (m_Clickable != null)
                {
                    this.AddManipulator(m_Clickable);
                }
            }
        }

        public ButtonControl() : this(default) { }

        public ButtonControl(Action clickEvent)
        {
            clickable = new Clickable(clickEvent);
            focusable = true;
            tabIndex = 0;
        }
    }
}
