using System.Collections;
using System.Collections.Generic;
// using CustomControls;
using UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] RenderTexture renderTexture2;
    [SerializeField] Material postProcessingMaterial;
    [SerializeField] Material postProcessingMaterial2;
    [SerializeField] Material postProcessingMaterial3;


    [ContextMenu("TEST")]
    void Foo()
    {
        var camera = GetComponent<Camera>();
        camera.RemoveAllCommandBuffers();

        var commandBuffer = new CommandBuffer();
        commandBuffer.ClearRenderTarget(true, true, Color.clear);

        var rt = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight);
        commandBuffer.Blit(renderTexture, rt, postProcessingMaterial);
        commandBuffer.Blit(renderTexture2, rt, postProcessingMaterial2);



        commandBuffer.Blit(rt, Camera.main.targetTexture);


        camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

        RenderTexture.ReleaseTemporary(rt);
    }
}
