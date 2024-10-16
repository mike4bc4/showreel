using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Localization.Editor
{
    [CustomEditor(typeof(Table))]
    public class TableInspector : UnityEditor.Editor
    {
        Table m_Table;

        void OnEnable()
        {
            m_Table = target as Table;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            var csvFilePropertyField = container.Q<PropertyField>("PropertyField:m_CsvFile");
            csvFilePropertyField.RegisterValueChangeCallback(OnCsvFilePropertyChanged);

            var entriesPropertyField = container.Q<PropertyField>("PropertyField:m_Entries");
            entriesPropertyField.SetEnabled(false);
            entriesPropertyField.RegisterCallback<GeometryChangedEvent>(OnEntriesPropertyFieldGeometryChanged);

            var scrollView = new ScrollView();
            scrollView.style.maxHeight = 520;
            container.Insert(container.IndexOf(entriesPropertyField), scrollView);

            scrollView.Add(entriesPropertyField);

            var buttonContainer = new VisualElement() { name = "button-container" };
            buttonContainer.style.alignItems = Align.FlexEnd;
            container.Insert(container.IndexOf(csvFilePropertyField) + 1, buttonContainer);

            var readFileButton = new Button() { name = "read-file-button" };
            readFileButton.text = "Read File";
            readFileButton.clicked += OnReadFileButtonClicked;
            buttonContainer.Add(readFileButton);

            return container;
        }

        void OnEntriesPropertyFieldGeometryChanged(GeometryChangedEvent evt)
        {
            var propertyField = evt.target as PropertyField;
            propertyField[0].style.maxHeight = StyleKeyword.None;
            propertyField.UnregisterCallback<GeometryChangedEvent>(OnEntriesPropertyFieldGeometryChanged);
        }

        void OnCsvFilePropertyChanged(SerializedPropertyChangeEvent evt)
        {
            if (m_Table != null)
            {
                m_Table.ReadCsv();
            }
        }

        void OnReadFileButtonClicked()
        {
            if (m_Table != null)
            {
                m_Table.ReadCsv();
            }
        }
    }
}
