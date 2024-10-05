using System;
using System.Collections;
using System.Collections.Generic;
// using CustomControls;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Experimental.Rendering;

public class TextureEditor : MonoBehaviour
{
    static TextureEditor s_Instance;

    [SerializeField] Texture2D m_Texture;
    [SerializeField] UIDocument m_TestUIDocument;
    [SerializeField] RenderTexture m_TestRT;

    Camera m_Camera;
    Material m_Material;

    void Awake()
    {
        if (s_Instance != null)
        {
            Destroy(this);
            return;
        }

        s_Instance = this;
        m_Camera = GetOrCreateCamera();
        m_Material = CreateMaterial();
    }

    Camera GetOrCreateCamera()
    {
        var camera = GetComponent<Camera>();
        if (camera == null)
        {
            camera = gameObject.AddComponent<Camera>();
        }

        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.Depth;
        return camera;
    }

    Material CreateMaterial()
    {
        var shader = Shader.Find("UI/Default");
        var material = new Material(shader);
        return material;
    }

    public static Texture2D Crop(Texture source, Rect rect, int element = 0, int mipmap = 0)
    {
        var texture = new Texture2D((int)rect.width, (int)rect.height, source.graphicsFormat, TextureCreationFlags.None);
        Graphics.CopyTexture(source, element, mipmap, (int)rect.x, source.height - (int)rect.yMax, (int)rect.width, (int)rect.height, texture, element, mipmap, 0, 0);
        return texture;
    }

    public static Texture2D CreateMask(Vector2 size, params Rect[] rects)
    {
        return CreateMask((int)size.x, (int)size.y, false, rects);
    }

    public static Texture2D CreateMask(Vector2 size, bool invert, params Rect[] rects)
    {
        return CreateMask((int)size.x, (int)size.y, invert, rects);
    }

    public static Texture2D CreateMask(int width, int height, bool invert, params Rect[] rects)
    {
        var renderTexture = RenderTexture.GetTemporary(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm);
        var camera = s_Instance.m_Camera;
        camera.targetTexture = renderTexture;

        Camera.CameraCallback onPostRender = null;
        onPostRender = (Camera cam) =>
        {
            if (cam == camera)
            {
                if (invert)
                {
                    AddQuadMesh(new Rect(0, 0, width, height), Color.white);
                }

                foreach (var rect in rects)
                {
                    AddQuadMesh(rect, invert ? Color.black : Color.white);
                }
            }

            Camera.onPostRender -= onPostRender;
        };

        Camera.onPostRender += onPostRender;
        camera.Render();

        var texture = new Texture2D(width, height, renderTexture.graphicsFormat, TextureCreationFlags.None);
        Graphics.CopyTexture(renderTexture, texture);
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture;
    }

    static void AddQuadMesh(Rect rect, Color color)
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        s_Instance.m_Material.SetPass(0);

        var camera = s_Instance.m_Camera;
        float w = (float)rect.width / camera.targetTexture.width;
        float h = (float)rect.height / camera.targetTexture.height;
        float x = (float)rect.x / camera.targetTexture.width;
        float y = 1f - (float)rect.yMax / camera.targetTexture.height;

        GL.Begin(GL.QUADS);
        GL.Color(color);
        GL.Vertex3(x, y, 0);
        GL.Vertex3(x + w, y, 0);
        GL.Vertex3(x + w, y + h, 0);
        GL.Vertex3(x, y + h, 0);
        GL.End();

        GL.PopMatrix();
    }

    // [ContextMenu("Test")]
    // void Test()
    // {
    //     Debug.Log(m_TestRT.graphicsFormat);

    //     var element = m_TestUIDocument.rootVisualElement.Q<ListElement>().Q("text-container");
    //     m_Texture = CreateMask(m_TestUIDocument.rootVisualElement.layout.size, element.worldBound);

    //     var listViewport = m_TestUIDocument.rootVisualElement.Q<VisualElement>("list-viewport");
    //     listViewport.style.backgroundImage = m_Texture;
    //     listViewport.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 0.5f);

    //     var rt = m_TestUIDocument.panelSettings.targetTexture;
    //     var tex = Crop(rt, element.worldBound);
    //     m_Texture = tex;
    // }

}
