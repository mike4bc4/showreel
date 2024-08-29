using System;
using System.Collections;
using System.Collections.Generic;
using Templates;
using UnityEngine;
using UnityEngine.Video;

namespace Utility
{
    public class MoviePlayer : DisposableObject
    {
        VideoPlayer m_Player;
        RenderTexture m_RenderTexture;

        public VideoClip videoClip
        {
            get => m_Player.clip;
        }

        public RenderTexture renderTexture
        {
            get => m_RenderTexture;
        }

        public MoviePlayer()
        {
            m_Player = Scheduler.AddComponent<VideoPlayer>();
            m_Player.playOnAwake = false;
            m_Player.renderMode = VideoRenderMode.RenderTexture;
            m_Player.audioOutputMode = VideoAudioOutputMode.None;
            m_Player.isLooping = true;

            m_RenderTexture = RenderTexture.GetTemporary(1, 1);
            m_Player.targetTexture = m_RenderTexture;
        }

        public void PlayClip(VideoClip videoClip)
        {
            if (videoClip == null)
            {
                throw new ArgumentNullException("Video cannot be null.");
            }

            // Although render texture width and height properties can be set, changing size of 
            // already created render texture is unsupported and will throw an exception, that's why
            // we have to recreate render texture when video clip size changes.
            if (m_RenderTexture.GetSize() != videoClip.GetSize())
            {
                RenderTexture.ReleaseTemporary(m_RenderTexture);
                m_RenderTexture = RenderTexture.GetTemporary((int)videoClip.width, (int)videoClip.height);
                m_Player.targetTexture = m_RenderTexture;
            }

            m_Player.clip = videoClip;
            m_Player.Play();
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            GameObject.Destroy(m_Player);
            RenderTexture.ReleaseTemporary(m_RenderTexture);

            m_Disposed = true;
        }
    }
}
