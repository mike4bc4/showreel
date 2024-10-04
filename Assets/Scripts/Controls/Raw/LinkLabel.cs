using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Localization;
using TimerUtility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class LinkLabel : LocalizedElement
    {
        const string k_UssClassName = "link-label";
        const string k_LinkContainerUssClassName = k_UssClassName + "__link-container";
        const string k_TagName = "link";

        class LinkElement : VisualElement
        {
            public enum State
            {
                Default,
                Hover,
            }

            const string k_UssClassName = "link-element";
            const string k_UssClassNameHoverVariant = k_UssClassName + "--hover";

            Label m_Label;
            VisualElement m_Border;
            State m_State;
            Vector2 m_Position;

            public Vector2 position
            {
                get => m_Position;
                set
                {
                    m_Position = value;
                    style.left = position.x;
                    style.top = position.y;
                }
            }

            public State state
            {
                get => m_State;
                set
                {
                    m_State = value;
                    switch (state)
                    {
                        case State.Default:
                            RemoveFromClassList(k_UssClassNameHoverVariant);
                            break;
                        case State.Hover:
                            AddToClassList(k_UssClassNameHoverVariant);
                            break;
                    }
                }
            }

            public string text
            {
                get => m_Label.text;
                set => m_Label.text = value;
            }

            public LinkElement()
            {
                AddToClassList(k_UssClassName);

                m_Label = new Label();
                m_Label.name = "label";
                m_Label.pickingMode = PickingMode.Ignore;
                Add(m_Label);

                m_Border = new VisualElement();
                m_Border.name = "border";
                m_Border.pickingMode = PickingMode.Ignore;
                Add(m_Border);
            }
        }

        class Link
        {
            LinkInfo m_LinkEntry;
            List<LinkElement> m_Elements;
            VisualElement m_ElementContainer;

            public LinkInfo entry => m_LinkEntry;

            public LinkElement this[int index]
            {
                get
                {
                    if (0 <= index && index <= m_Elements.Count)
                    {
                        return m_Elements[index];
                    }

                    return null;
                }
            }

            public Link(VisualElement elementContainer, LinkInfo linkEntry)
            {
                m_LinkEntry = linkEntry;
                m_Elements = new List<LinkElement>();
                m_ElementContainer = elementContainer;
            }

            public void SetElementCount(int count)
            {
                while (m_Elements.Count < count)
                {
                    var element = CreateElement();
                    m_Elements.Add(element);
                    m_ElementContainer.Add(element);
                }

                while ((m_Elements.Count > count))
                {
                    var button = PopElement();
                    m_ElementContainer.Remove(button);
                }
            }

            LinkElement CreateElement()
            {
                var element = new LinkElement();
                element.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    foreach (var element in m_Elements)
                    {
                        element.state = LinkElement.State.Hover;
                    }
                });

                element.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    foreach (var element in m_Elements)
                    {
                        element.state = LinkElement.State.Default;
                    }
                });

                element.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.target == element)
                    {
                        Application.OpenURL(m_LinkEntry.url);
                    }
                });

                return element;
            }

            LinkElement PopElement()
            {
                if (m_Elements.Count > 0)
                {
                    var element = m_Elements[0];
                    m_Elements.RemoveAt(0);
                    return element;
                }

                return null;
            }
        }

        class LinkInfo
        {
            public int startIndex;
            public string url;
            public string text;

            public int length => text.Length;

            public LinkInfo(int startIndex, string url, string text)
            {
                this.startIndex = startIndex;
                this.url = url;
                this.text = text;
            }
        }

        class TextInfo
        {
            const string k_RootTag = "root";
            const string k_HiddenStartTag = "<alpha=#00>";
            const string k_HiddenEndTag = "<alpha=#FF>";

            string m_Text;
            List<LinkInfo> m_LinkInfos;

            public string text => m_Text;
            public List<LinkInfo> linkInfos => m_LinkInfos;

            public TextInfo(string rawText)
            {
                m_Text = string.Empty;
                m_LinkInfos = new List<LinkInfo>();

                // Unescape raw text to avoid taking escaped characters into account when calculating
                // link position in text.
                string unescapedText = Regex.Unescape(rawText);
                
                var xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.LoadXml($"<{k_RootTag}>{unescapedText}</{k_RootTag}>");
                }
                catch (Exception)
                {
                    m_Text = "PARSE ERROR";
                    return;
                }

                var rootNode = xmlDocument.ChildNodes[0];
                foreach (XmlNode node in rootNode)
                {
                    if (node.Name.Equals(k_TagName, StringComparison.OrdinalIgnoreCase))
                    {
                        m_LinkInfos.Add(new LinkInfo(m_Text.Length, node.Attributes["href"]?.Value, node.InnerText));
                        m_Text += node.InnerText;
                    }
                    else if (node is XmlText)
                    {
                        m_Text += node.Value;
                    }
                    else
                    {
                        m_Text += node.InnerText;
                    }
                }
            }

            public string GetTextWithHiddenLinks()
            {
                var text = m_Text;
                var offset = 0;
                foreach (var info in m_LinkInfos)
                {
                    text = text.Insert(info.startIndex + offset, k_HiddenStartTag);
                    offset += k_HiddenStartTag.Length;
                    text = text.Insert(info.startIndex + info.length + offset, k_HiddenEndTag);
                    offset += k_HiddenEndTag.Length;
                }

                return text;
            }
        }

        public new class UxmlFactory : UxmlFactory<LinkLabel, UxmlTraits> { }

        public new class UxmlTraits : LocalizedElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription() { name = "text", defaultValue = "Lorem ipsum <link href=\"\">Link.com</link> dolor <link href=\"\">Link.com</link> sit amet." };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var hyperlinkLabel = (LinkLabel)ve;
                hyperlinkLabel.text = m_Text.GetValueFromBag(bag, cc);
            }
        }

        LocalizedLabel m_Label;
        VisualElement m_LinkContainer;
        List<Link> m_Links;
        TextInfo m_TextInfo;
        ITimerHandle m_TimerHandle1;
        ITimerHandle m_TimerHandle2;
        string m_RawText;

        event Action m_OnVisualContentGenerated;

        public string text
        {
            get => m_RawText;
            set
            {
                m_RawText = value;
                m_TextInfo = new TextInfo(m_RawText);
                m_Label.text = m_TextInfo.text;

                m_LinkContainer.Clear();
                m_Links.Clear();
                foreach (var info in m_TextInfo.linkInfos)
                {
                    m_Links.Add(new Link(m_LinkContainer, info));
                }

                // Check OnLabelGeometryChanged to see the explanation of what's happening here.
                Action action = null;
                m_OnVisualContentGenerated += action = () =>
                {
                    m_TimerHandle1?.Cancel();
                    m_TimerHandle1 = Timer.NextFrame(() =>
                    {
                        m_Label.text = m_TextInfo.GetTextWithHiddenLinks();
                        UpdateLinkElements();
                    });

                    m_OnVisualContentGenerated -= action;
                };
            }
        }

        protected override ILocalizedElement localizedElement => m_Label;

        public LinkLabel()
        {
            m_Links = new List<Link>();

            m_Label = new LocalizedLabel();
            m_Label.name = "label";
            m_Label.generateVisualContent += mgc => m_OnVisualContentGenerated?.Invoke();
            m_Label.selection.isSelectable = true;
            m_Label.SetEnabled(false);
            m_Label.onLocalized += translation => text = translation;
            Add(m_Label);

            m_LinkContainer = new VisualElement();
            m_LinkContainer.name = "link-container";
            m_LinkContainer.AddToClassList(k_LinkContainerUssClassName);
            Add(m_LinkContainer);

            m_Label.RegisterCallback<GeometryChangedEvent>(OnLabelGeometryChanged);
        }

        void UpdateLinkElements()
        {
            foreach (var link in m_Links)
            {
                // Using cursor index, traverse link text to detect line-breaks and check into how
                // many substrings link is divided.
                m_Label.selection.cursorIndex = link.entry.startIndex;
                var initialPosition = m_Label.selection.cursorPosition;
                var currentPosition = initialPosition;
                var currentSplitIndex = 0;
                var splits = new List<int> { 0 };
                for (int i = 0; i < link.entry.length; i++)
                {
                    m_Label.selection.cursorIndex++;
                    if (m_Label.selection.cursorPosition.y == currentPosition.y)
                    {
                        splits[currentSplitIndex]++;
                    }
                    else
                    {
                        splits[currentSplitIndex]++;
                        currentPosition = m_Label.selection.cursorPosition;
                        currentSplitIndex++;

                        // Only add another split if there will be next loop, otherwise there will
                        // be an empty split if link is at the end of line.
                        if (i + 1 < link.entry.length)
                        {
                            splits.Add(0);
                        }
                    }
                }

                link.SetElementCount(splits.Count);
                var fontSize = m_Label.resolvedStyle.fontSize;
                if (splits.Count > 0)
                {
                    link[0].position = new Vector2(initialPosition.x, initialPosition.y - fontSize);
                    link[0].text = link.entry.text.Substring(0, splits[0]);
                    int offset = splits[0];

                    for (int i = 1; i < splits.Count; i++)
                    {
                        int length = splits[i];
                        var element = link[i];
                        element.position = new Vector2(0f, initialPosition.y + fontSize * (i - 1));
                        element.text = link.entry.text.Substring(offset, length);
                        offset += length;
                    }
                }
            }
        }

        void OnLabelGeometryChanged(GeometryChangedEvent evt)
        {
            // When label element's geometry changes its text still remains intact until visual content
            // is generated. That's why we have to wait until such event is invoked after geometry changes.
            Action action = null;
            m_OnVisualContentGenerated += action = () =>
            {
                // At this stage text is ready and we could try to update link elements, yet right now
                // it would result in exception as label is in 'read-only' state, we are using a timer
                // to wait until next frame and then execute the update.
                m_TimerHandle2?.Cancel();
                m_TimerHandle2 = Timer.NextFrame(UpdateLinkElements);
                m_OnVisualContentGenerated -= action;
            };
        }
    }

}
