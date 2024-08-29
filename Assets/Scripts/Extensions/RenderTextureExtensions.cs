using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderTextureExtensions
{
    public static Vector2Int GetSize(this RenderTexture renderTexture)
    {
        return new Vector2Int(renderTexture.width, renderTexture.height);
    }
}
