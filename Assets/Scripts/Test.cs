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

    [ContextMenu("Test")]
    void T()
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(0.5f);
            var element = GetComponentInChildren<UIDocument>().rootVisualElement.Q<DiamondTitle>();
            element.Unfold();
        }

        StartCoroutine(Coroutine());
    }

    [ContextMenu("Test2")]
    void T2()
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(0.5f);
            var element = GetComponentInChildren<UIDocument>().rootVisualElement.Q<DiamondTitle>();
            element.Fold();
        }

        StartCoroutine(Coroutine());
    }

    // [ContextMenu("Test")]
    // void C()
    // {
    //     IEnumerator Coroutine()
    //     {
    //         yield return new WaitForSeconds(0.5f);
    //         var random = new System.Random();
    //         var diamondBar = GetComponentInChildren<UIDocument>().rootVisualElement.Q<DiamondBar>();

    //         int newIndex = random.Next(0, 3);
    //         while (diamondBar.activeIndex == newIndex)
    //         {
    //             newIndex = random.Next(0, 3);
    //         }

    //         diamondBar.SetActiveIndex(newIndex);
    //     }

    //     StartCoroutine(Coroutine());
    // }

    // [ContextMenu("Test2")]
    // void D()
    // {
    //     IEnumerator Coroutine()
    //     {
    //         yield return new WaitForSeconds(0.5f);
    //         var random = new System.Random();
    //         var diamondBar = GetComponentInChildren<UIDocument>().rootVisualElement.Q<DiamondBar>();

    //         int newIndex = random.Next(0, 3);
    //         while (diamondBar.activeIndex == newIndex)
    //         {
    //             newIndex = random.Next(0, 3);
    //         }

    //         diamondBar.SetActiveIndex(newIndex, immediate: true);
    //     }

    //     StartCoroutine(Coroutine());
    // }
}
