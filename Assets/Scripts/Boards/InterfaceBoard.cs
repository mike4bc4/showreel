using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;
// using Utils;

namespace UI.Boards
{
    public class InterfaceBoard : Board, IBoard
    {
        public const int InputSortOrder = 1000;   // Sorting order affects UI element picking.
        public const int DisplaySortOrder = 1000;   // Display order affects Layer sorting

        // [SerializeField] VisualTreeAsset m_ControlsVta;

        // Layer m_ControlsLayer;
        // KeyframeTrackPlayer m_Player;

        // public void Init()
        // {
        //     m_Player = new KeyframeTrackPlayer();
        //     m_Player.sampling = 60;

        //     m_Player.AddEvent(0, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             m_ControlsLayer = LayerManager.CreateLayer(m_ControlsVta, displaySortOrder: DisplaySortOrder);
        //             m_ControlsLayer.inputSortOrder = InputSortOrder;
        //             m_ControlsLayer.interactable = false;
        //             m_ControlsLayer.alpha = 0f;
        //             m_ControlsLayer.blurSize = Layer.DefaultBlurSize;
        //         }
        //         else
        //         {
        //             LayerManager.RemoveLayer(m_ControlsLayer);
        //         }
        //     });

        //     var t1 = m_Player.AddKeyframeTrack((float alpha) =>
        //     {
        //         if (m_ControlsLayer != null)
        //         {
        //             m_ControlsLayer.alpha = alpha;
        //         }
        //     });
        //     t1.AddKeyframe(0, 0f);
        //     t1.AddKeyframe(20, 1f);

        //     var t2 = m_Player.AddKeyframeTrack((float blurSize) =>
        //     {
        //         if (m_ControlsLayer != null)
        //         {
        //             m_ControlsLayer.blurSize = blurSize;
        //         }
        //     });
        //     t2.AddKeyframe(10, Layer.DefaultBlurSize);
        //     t2.AddKeyframe(30, 0f);

        //     m_Player.AddEvent(30, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             m_ControlsLayer.interactable = true;
        //         }
        //         else
        //         {
        //             m_ControlsLayer.interactable = false;
        //         }
        //     });
        // }


        // public void ShowImmediate()
        // {

        // }

        // public void HideImmediate()
        // {

        // }

        // public UniTask Show(CancellationToken cancellationToken = default)
        // {
        //     return UniTask.CompletedTask;
        // }

        // public UniTask Hide(CancellationToken cancellationToken = default)
        // {
        //     return UniTask.CompletedTask;
        // }

        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.A))
        //     {
        //         m_Player.playbackSpeed = 1f;
        //         m_Player.Play();
        //     }
        //     else if (Input.GetKeyDown(KeyCode.D))
        //     {
        //         m_Player.playbackSpeed = -1f;
        //         m_Player.Play();
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Q))
        //     {
        //         ShowImmediate();
        //     }
        //     else if (Input.GetKeyDown(KeyCode.E))
        //     {
        //         HideImmediate();
        //     }
        // }
        public void Init()
        {
            // throw new System.NotImplementedException();
        }
    }
}
