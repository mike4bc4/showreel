using System;
using System.Collections;
using System.Collections.Generic;
using Controls.Raw;

// using CustomControls;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    [SerializeField] UIDocument document;


    [ContextMenu("TEST")]
    void Foo()
    {
        // DiamondBar diamondBar = document.rootVisualElement.Q<DiamondBar>();
        // var random = new System.Random();
        // diamondBar.size = random.Next(3, 6);
    }

    [ContextMenu("T2")]
    void F2()
    {
        // DiamondBar diamondBar = document.rootVisualElement.Q<DiamondBar>();
        // diamondBar.style.width = 1060;
    }

    [ContextMenu("T3")]
    void F3()
    {
        // DiamondBar diamondBar = document.rootVisualElement.Q<DiamondBar>();
        // diamondBar.style.width = 960;
    }
}
