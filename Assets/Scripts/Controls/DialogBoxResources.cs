using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "DialogBoxResources", menuName = "Scriptable Objects/Dialog Box Resources")]
public class DialogBoxResources : ScriptableSingleton<DialogBoxResources>
{
    static DialogBoxResources s_Instance;

    [SerializeField] VisualTreeAsset m_InfoDialogBoxContentVta;
    [SerializeField] VisualTreeAsset m_WelcomeDialogBoxContentVta;

    public VisualTreeAsset infoDialogBoxContentVta => m_InfoDialogBoxContentVta;
    public VisualTreeAsset welcomeDialogBoxContentVta => m_WelcomeDialogBoxContentVta;
}
