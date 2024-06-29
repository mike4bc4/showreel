using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class VideoPlayerManager : MonoBehaviour
{
    static VideoPlayerManager s_Instance;

    [SerializeField] RenderTexture m_TemplateRenderTexture;

    void Awake()
    {
        if (s_Instance != null)
        {
            Destroy(this);
            return;
        }

        s_Instance = this;
    }

    public static VideoPlayer CreatePlayer(VideoClip videoClip)
    {
        var videoPlayer = s_Instance.gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.clip = videoClip;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.isLooping = true;

        var renderTexture = new RenderTexture(s_Instance.m_TemplateRenderTexture);
        renderTexture.height = (int)videoClip.height;
        renderTexture.width = (int)videoClip.width;
        videoPlayer.targetTexture = renderTexture;

        return videoPlayer;
    }

    public static void RemovePlayer(VideoPlayer videoPlayer)
    {
        if (videoPlayer.texture is RenderTexture renderTexture)
        {
            renderTexture.Release();
        }

        Destroy(videoPlayer);
    }
}
