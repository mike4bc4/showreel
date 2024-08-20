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
    public class BackgroundBoard : Board, IBoard
    {
        const int k_DisplaySortOrder = 0;

        [SerializeField] VisualTreeAsset m_BackgroundBoardVisualTreeAsset;

        Layer m_BackgroundBoardLayer;

        public void ShowImmediate()
        {
            m_BackgroundBoardLayer = LayerManager.CreateLayer(m_BackgroundBoardVisualTreeAsset, displaySortOrder: k_DisplaySortOrder);
            m_BackgroundBoardLayer.interactable = false;
            m_BackgroundBoardLayer.blocksRaycasts = false;
        }

        public void HideImmediate()
        {
            LayerManager.RemoveLayer(m_BackgroundBoardLayer);
        }
    }
}
