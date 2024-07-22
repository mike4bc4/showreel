using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CustomControls;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace UI.Boards
{
    public class TestBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        [SerializeField] VisualTreeAsset m_BackgroundVta;
        [SerializeField] VisualTreeAsset m_DiamondFrameVta;
        [SerializeField] VisualTreeAsset m_EmptyVta;
        [SerializeField] VideoClip m_VideoClip;

        public UniTask Hide(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void HideImmediate()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            LayerManager.CreateLayer(m_DiamondFrameVta, "DiamondFrameLayer");
        }

        public UniTask Show(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void ShowImmediate()
        {
            throw new NotImplementedException();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                var layer = (Layer)LayerManager.GetLayer("DiamondFrameLayer");
                var diamondFrameVertical = layer.rootVisualElement.Q<DiamondFrameVertical>();
                diamondFrameVertical.Unfold();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                var layer = (Layer)LayerManager.GetLayer("DiamondFrameLayer");
                var diamondFrameVertical = layer.rootVisualElement.Q<DiamondFrameVertical>();
                diamondFrameVertical.Fold();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                var layer = (Layer)LayerManager.GetLayer("DiamondFrameLayer");
                var diamondFrameVertical = layer.rootVisualElement.Q<DiamondFrameVertical>();
                diamondFrameVertical.UnfoldImmediate();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                var layer = (Layer)LayerManager.GetLayer("DiamondFrameLayer");
                var diamondFrameVertical = layer.rootVisualElement.Q<DiamondFrameVertical>();
                diamondFrameVertical.FoldImmediate();
            }
        }
    }
}