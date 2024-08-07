using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomControls;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace UI.Boards
{
    public class InitialBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        const string k_TitleSnapshotLayerName = "TitleSnapshotLayer";
        const string k_TitleLabelSnapshotLayerName = "TitleLabelSnapshotLayer";
        const string k_SubtitleSnapshotLayerName = "SubtitleSnapshotLayer";

        [SerializeField] VisualTreeAsset m_InitialBoardVta;

        KeyframeTrackPlayer m_Player;
        KeyframeTrackPlayer m_SubtitleAnimationPlayer;

        Layer m_InitialBoardLayer;
        DiamondTitle m_Title;
        Subtitle m_Subtitle;

        public void Init()
        {
            m_Player = new KeyframeTrackPlayer();
            m_Player.sampling = 60;

            m_SubtitleAnimationPlayer = new KeyframeTrackPlayer();
            m_SubtitleAnimationPlayer.sampling = 60;
            m_SubtitleAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;

            m_Player.AddEvent(0, () =>
            {
                m_InitialBoardLayer = LayerManager.CreateLayer(m_InitialBoardVta);
                m_InitialBoardLayer.alpha = 0f;

                m_Title = m_InitialBoardLayer.rootVisualElement.Q<DiamondTitle>("title");
                m_Title.label.visible = false;
                m_Title.animationProgress = 0f;

                m_Subtitle = m_InitialBoardLayer.rootVisualElement.Q<Subtitle>("subtitle");
                m_Subtitle.animationProgress = 0f;

            }, EventInvokeFlags.Forward);
            m_Player.AddEvent(0, () =>
            {
                m_InitialBoardLayer.MaskElements(m_Title, m_Subtitle);
                m_InitialBoardLayer.alpha = 1f;
            }, EventInvokeFlags.Forward, 1);
            m_Player.AddEvent(0, () =>
            {
                var layer = m_InitialBoardLayer.CreateSnapshotLayer(m_Title, k_TitleSnapshotLayerName);
                layer.alpha = 0f;
                layer.blur = Layer.DefaultBlur;
            }, EventInvokeFlags.Forward, 2);

            m_Player.AddEvent(0, () =>
            {
                LayerManager.RemoveLayer(m_InitialBoardLayer);
                LayerManager.RemoveLayer(k_TitleSnapshotLayerName);
            }, EventInvokeFlags.Backward);

            var t1 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(k_TitleSnapshotLayerName)?.SetAlpha(alpha));
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var t2 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(k_TitleSnapshotLayerName)?.SetBlur(blur));
            t2.AddKeyframe(10, Layer.DefaultBlur);
            t2.AddKeyframe(30, 0f);

            m_Player.AddEvent(30, () =>
            {
                LayerManager.RemoveLayer(k_TitleSnapshotLayerName);
                m_InitialBoardLayer.UnmaskElements(m_Title);
            }, EventInvokeFlags.Forward);

            m_Player.AddEvent(30, () =>
            {
                m_InitialBoardLayer.MaskElements(m_Title);
            }, EventInvokeFlags.Backward, 1);
            m_Player.AddEvent(30, () =>
            {
                m_InitialBoardLayer.CreateSnapshotLayer(m_Title, k_TitleSnapshotLayerName);
            }, EventInvokeFlags.Backward);

            var t3 = m_Player.AddKeyframeTrack((float progress) => m_Title?.SetAnimationProgress(progress));
            t3.AddKeyframe(30, 0f);
            t3.AddKeyframe(90, 1f);

            m_Player.AddEvent(90, () =>
            {
                m_InitialBoardLayer.MaskElements(m_Title.label);
                m_Title.label.visible = true;
            }, EventInvokeFlags.Forward);
            m_Player.AddEvent(90, () =>
            {
                var layer = m_InitialBoardLayer.CreateSnapshotLayer(m_Title.label, k_TitleLabelSnapshotLayerName);
                layer.alpha = 0f;
                layer.blur = Layer.DefaultBlur;
            }, EventInvokeFlags.Forward, 1);

            m_Player.AddEvent(90, () =>
            {
                m_InitialBoardLayer.UnmaskElements(m_Title.label);
                LayerManager.RemoveLayer(k_TitleLabelSnapshotLayerName);
            }, EventInvokeFlags.Backward, 1);
            m_Player.AddEvent(90, () =>
            {
                m_Title.label.visible = false;
            }, EventInvokeFlags.Backward);

            var t4 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(k_TitleLabelSnapshotLayerName)?.SetAlpha(alpha));
            t4.AddKeyframe(90, 0f);
            t4.AddKeyframe(110, 1f);

            var t5 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(k_TitleLabelSnapshotLayerName)?.SetBlur(blur));
            t5.AddKeyframe(100, Layer.DefaultBlur);
            t5.AddKeyframe(120, 0f);

            m_Player.AddEvent(120, () =>
            {
                LayerManager.RemoveLayer(k_TitleLabelSnapshotLayerName);
                m_InitialBoardLayer.UnmaskElements(m_Title.label);
                m_Subtitle.visible = true;

            }, EventInvokeFlags.Forward);
            m_Player.AddEvent(120, () =>
            {
                var layer = m_InitialBoardLayer.CreateSnapshotLayer(m_Subtitle, k_SubtitleSnapshotLayerName);
                layer.alpha = 0f;
                layer.blur = Layer.DefaultBlur;
            }, EventInvokeFlags.Forward, 1);

            m_Player.AddEvent(120, () =>
            {
                m_InitialBoardLayer.MaskElements(m_Title.label);
            }, EventInvokeFlags.Backward, 1);
            m_Player.AddEvent(120, () =>
            {
                m_InitialBoardLayer.CreateSnapshotLayer(m_Title.label, k_TitleLabelSnapshotLayerName);
                LayerManager.RemoveLayer(k_SubtitleSnapshotLayerName);
                m_Subtitle.visible = false;
            }, EventInvokeFlags.Backward);

            var t6 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(k_SubtitleSnapshotLayerName)?.SetAlpha(alpha));
            t6.AddKeyframe(120, 0f);
            t6.AddKeyframe(140, 1f);

            var t7 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(k_SubtitleSnapshotLayerName)?.SetBlur(blur));
            t7.AddKeyframe(130, Layer.DefaultBlur);
            t7.AddKeyframe(150, 0f);

            m_Player.AddEvent(150, () =>
            {
                LayerManager.RemoveLayer(k_SubtitleSnapshotLayerName);
                m_InitialBoardLayer.UnmaskElements(m_Subtitle);
                m_SubtitleAnimationPlayer.Play();
            }, EventInvokeFlags.Forward);

            m_Player.AddEvent(150, () =>
            {
                m_InitialBoardLayer.MaskElements(m_Subtitle);
            }, EventInvokeFlags.Backward, 1);
            m_Player.AddEvent(150, () =>
            {
                m_InitialBoardLayer.CreateSnapshotLayer(m_Subtitle, k_SubtitleSnapshotLayerName);
                m_SubtitleAnimationPlayer.Stop();
            }, EventInvokeFlags.Backward);

            var t8 = m_SubtitleAnimationPlayer.AddKeyframeTrack((float progress) => m_Subtitle?.SetAnimationProgress(progress));
            t8.AddKeyframe(0, 0f);
            t8.AddKeyframe(120, 1f);
            t8.AddKeyframe(180, 1f);
        }

        public void ShowImmediate()
        {

        }

        public void HideImmediate()
        {

        }

        public UniTask Show(CancellationToken ct = default)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Hide(CancellationToken ct = default)
        {
            return UniTask.CompletedTask;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                m_Player.playbackSpeed = 1f;
                m_Player.Play();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                m_Player.playbackSpeed = -1;
                m_Player.Play();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
            }
        }
    }
}
