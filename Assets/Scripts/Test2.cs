using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    [ContextMenu("Test")]
    void TestMethod()
    {
        var rawImage = GetComponent<RawImage>();
        rawImage.material.EnableKeyword("BLUR_ON");
    }

    [ContextMenu("Test2")]
    void TestMethod2()
    {
        var rawImage = GetComponent<RawImage>();
        rawImage.material.DisableKeyword("BLUR_ON");
    }
}
