using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace StyleUtility
{
    public static class StyleSheetExtensions
    {
        static readonly Type s_StyleSheetType = typeof(StyleSheet);
        static readonly MethodInfo s_ReadEnumMethod = s_StyleSheetType.GetMethod("ReadEnum", BindingFlags.Instance | BindingFlags.NonPublic);

        public static string ReadEnum(this StyleSheet styleSheet, StyleValueHandle handle)
        {
            return (string)s_ReadEnumMethod.Invoke(styleSheet, new object[] { handle.obj });
        }
    }

    public enum StyleValueType
    {
        Invalid,
        Keyword,
        Float,
        Dimension,
        Color,
        ResourcePath, // When using resource("...")
        AssetReference,
        Enum, // A literal value that is not quoted
        Variable, // A literal value starting with "--"
        String, // A quoted value or any other value that is not recognized as a primitive
        Function,
        CommaSeparator,
        ScalableImage,
        MissingAssetReference,
    }

    public class StyleValueHandle
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_StyleValueHandleType = s_Assembly.GetType("UnityEngine.UIElements.StyleValueHandle");
        static readonly PropertyInfo s_ValueTypeProperty = s_StyleValueHandleType.GetProperty("valueType");

        object m_StyleValueHandle;
        StyleValueType m_ValueType;

        internal object obj
        {
            get => m_StyleValueHandle;
        }

        public StyleValueType valueType
        {
            get => m_ValueType;
        }

        public StyleValueHandle(object styleValueHandle)
        {
            m_StyleValueHandle = styleValueHandle;
            m_ValueType = (StyleValueType)(int)s_ValueTypeProperty.GetValue(styleValueHandle);
        }
    }

    public class StyleProperty
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_StylePropertyType = s_Assembly.GetType("UnityEngine.UIElements.StyleProperty");
        static readonly PropertyInfo s_NameProperty = s_StylePropertyType.GetProperty("name");
        static readonly PropertyInfo s_ValuesProperty = s_StylePropertyType.GetProperty("values");

        object m_StyleProperty;
        string m_Name;
        List<StyleValueHandle> m_Values;

        public string name
        {
            get => m_Name;
        }

        public List<StyleValueHandle> values
        {
            get => m_Values;
        }

        public StyleProperty(object styleProperty)
        {
            m_StyleProperty = styleProperty;
            m_Name = (string)s_NameProperty.GetValue(styleProperty);

            m_Values = new List<StyleValueHandle>();
            foreach (var val in (Array)s_ValuesProperty.GetValue(styleProperty))
            {
                m_Values.Add(new StyleValueHandle(val));
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " + name;
        }
    }

    public class InlineRule
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_InlineRuleType = s_Assembly.GetType("UnityEngine.UIElements.InlineStyleAccess+InlineRule");
        static readonly FieldInfo s_SheetField = s_InlineRuleType.GetField("sheet");
        static readonly FieldInfo s_RuleField = s_InlineRuleType.GetField("rule");
        static readonly PropertyInfo s_PropertiesProperty = s_InlineRuleType.GetProperty("properties");

        object m_InlineRule;
        StyleSheet m_Sheet;
        object m_Rule;
        List<StyleProperty> m_Properties;

        public StyleSheet sheet
        {
            get => (StyleSheet)m_Sheet;
        }

        public object rule
        {
            get => m_Rule;
        }

        public IReadOnlyList<StyleProperty> properties
        {
            get => m_Properties.AsReadOnly();
        }

        public InlineRule(object inlineRule)
        {
            m_InlineRule = inlineRule;
            m_Sheet = (StyleSheet)s_SheetField.GetValue(inlineRule);
            m_Rule = s_RuleField.GetValue(inlineRule);

            if (m_Rule != null)
            {
                m_Properties = new List<StyleProperty>();
                foreach (var property in (Array)s_PropertiesProperty.GetValue(inlineRule))
                {
                    m_Properties.Add(new StyleProperty(property));
                }
            }
        }
    }

    public class InlineStyleAccess
    {
        static readonly Assembly s_Assembly = Assembly.Load("UnityEngine");
        static readonly Type s_InlineStyleAccessType = s_Assembly.GetType("UnityEngine.UIElements.InlineStyleAccess");
        static readonly PropertyInfo s_InlineRuleProperty = s_InlineStyleAccessType.GetProperty("inlineRule");

        IStyle m_Style;
        InlineRule m_InlineRule;

        public InlineRule inlineRule
        {
            get => m_InlineRule;
        }

        public InlineStyleAccess(IStyle style)
        {
            m_Style = style;
            m_InlineRule = new InlineRule(s_InlineRuleProperty.GetValue(style));
        }

        public bool TryReadEnumProperty<T>(string propertyName, out T result) where T : struct
        {
            if (inlineRule.rule == null || inlineRule.sheet == null)
            {
                result = default;
                return false;
            }

            foreach (var property in inlineRule.properties)
            {
                if (property.name == propertyName && property.values.Count > 0 && property.values[0].valueType == StyleValueType.Enum)
                {
                    var enumName = inlineRule.sheet.ReadEnum(property.values[0]);
                    if (Enum.TryParse<T>(enumName, true, out var enumResult))
                    {
                        result = enumResult;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            result = default;
            return false;
        }

        public bool TryReadEnumProperty(string propertyName, out string result)
        {
            if (inlineRule.rule == null || inlineRule.sheet == null)
            {
                result = null;
                return false;
            }

            foreach (var property in inlineRule.properties)
            {
                if (property.name == propertyName && property.values.Count > 0 && property.values[0].valueType == StyleValueType.Enum)
                {
                    result = inlineRule.sheet.ReadEnum(property.values[0]);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
