using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "DialogBoxResources", menuName = "Scriptable Objects/Dialog Box Resources")]
public class DialogBoxResources : ScriptableSingleton<DialogBoxResources>
{
    static DialogBoxResources s_Instance;

    [SerializeField] List<VisualTreeAsset> m_ContentVisualTreeAsset;

    public static VisualTreeAsset GetContentVisualTreeAsset(string name)
    {
        foreach (var vta in Instance.m_ContentVisualTreeAsset)
        {
            if (vta.name == name)
            {
                return vta;
            }
        }

        return null;
    }
}
