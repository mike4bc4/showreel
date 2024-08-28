using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Utility
{
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

        public static VideoPlayer CreatePlayer(VideoClip videoClip, string name = "VideoPlayer")
        {
            var videoPlayer = s_Instance.gameObject.AddComponent<VideoPlayer>();
            videoPlayer.name = name;
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.clip = videoClip;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.isLooping = true;

            var renderTexture = RenderTexture.GetTemporary(s_Instance.m_TemplateRenderTexture.descriptor);
            renderTexture.height = (int)videoClip.height;
            renderTexture.width = (int)videoClip.width;
            videoPlayer.targetTexture = renderTexture;

            return videoPlayer;
        }

        public static VideoPlayer GetPlayer(string name)
        {
            var videoPlayers = s_Instance.gameObject.GetComponents<VideoPlayer>();
            foreach (var player in videoPlayers)
            {
                if (player.name == name)
                {
                    return player;
                }
            }

            return null;
        }

        public static void RemovePlayer(VideoPlayer videoPlayer)
        {
            if (videoPlayer.texture is RenderTexture renderTexture)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            Destroy(videoPlayer);
        }

        public static void RemovePlayer(string name)
        {
            var player = GetPlayer(name);
            if (player != null)
            {
                RemovePlayer(player);
            }
        }
    }
}
