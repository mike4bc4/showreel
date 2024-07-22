using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_EmptyVta;

    [ContextMenu("Test")]
    void A()
    {
        var layer = GetComponent<Layer>();
        layer.Init();
        // var maskFiler = new MaskFilter();
        // layer.filter = maskFiler;

        var title = layer.rootVisualElement.Q<DiamondTitle>("title");
        var maskTexture = TextureEditor.CreateMask(layer.rootVisualElement.layout.size, invert: true, title.worldBound);
        // maskFiler.alphaTexture = maskTexture;

        var snapshotLayer = LayerManager.CreateLayer(m_EmptyVta, "SnapshotLayer");
        // var snapshot = layer.CreateSnapshot(title);
        // snapshotLayer.rootVisualElement.Add(snapshot);
        // snapshotLayer.filter = new BlurFilter();
    }
}
