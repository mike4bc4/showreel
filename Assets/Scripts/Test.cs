using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEngine;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    DiamondFrameVertical m_DiamondFrame;

    void Start()
    {
        var uiDocument = GetComponentInChildren<UIDocument>();
        m_DiamondFrame = uiDocument.rootVisualElement.Q<DiamondFrameVertical>();
        m_DiamondFrame.Fold(immediate: true);


    }

    [ContextMenu("Unfold")]
    void A()
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(0.1f);
            m_DiamondFrame.Unfold();
        }

        StartCoroutine(Coroutine());
    }

    [ContextMenu("Fold")]
    void B()
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(0.1f);
            m_DiamondFrame.Fold();
        }

        StartCoroutine(Coroutine());
    }
}
