using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomEditor(typeof(RefresherSettings))]
    public class RefresherSettingsInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var enabledProperty = new PropertyField(serializedObject.FindProperty("m_IsEnabled"));
            enabledProperty.label = "Enabled";
            enabledProperty.RegisterValueChangeCallback(evt =>
            {
                Refresher.Enabled = evt.changedProperty.boolValue;
            });
            root.Add(enabledProperty);

            var intervalProperty = new PropertyField(serializedObject.FindProperty("m_Interval"));
            intervalProperty.RegisterValueChangeCallback(evt =>
            {
                float interval = Mathf.Max(0f, evt.changedProperty.floatValue);
                evt.changedProperty.floatValue = interval;
                serializedObject.ApplyModifiedProperties();
                Refresher.Interval = interval;
            });
            root.Add(intervalProperty);

            return root;
        }
    }
}
