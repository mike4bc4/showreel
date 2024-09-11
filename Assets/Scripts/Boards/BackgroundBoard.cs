using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Layers;

namespace Boards
{
    public class BackgroundBoard : Board
    {
        public const int DisplaySortOrder = -100;

        [SerializeField] VisualTreeAsset m_BackgroundBoardVisualTreeAsset;

        Layer m_BackgroundBoardLayer;

        public override bool interactable
        {
            get => m_BackgroundBoardLayer.interactable;
            set => m_BackgroundBoardLayer.interactable = value;
        }
        public override bool blocksRaycasts
        {
            get => m_BackgroundBoardLayer.blocksRaycasts;
            set => m_BackgroundBoardLayer.blocksRaycasts = value;
        }

        public override void ShowImmediate()
        {
            m_BackgroundBoardLayer = LayerManager.CreateLayer("Background");
            m_BackgroundBoardLayer.displaySortOrder = DisplaySortOrder;
            m_BackgroundBoardLayer.AddTemplateFromVisualTreeAsset(m_BackgroundBoardVisualTreeAsset);
            interactable = false;
            blocksRaycasts = false;
            m_IsVisible = true;
        }

        public override void HideImmediate()
        {
            LayerManager.RemoveLayer(m_BackgroundBoardLayer);
            m_IsVisible = false;
        }
    }
}
