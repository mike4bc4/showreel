using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Boards.States
{
    public class PoliticoListBoardState : BaseListBoardState
    {
        List<VideoClip> m_VideoClips;

        public PoliticoListBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            base.Init();
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("PoliticoListBoard");

            switch (context.previousState)
            {
                case DiamondBarBoardState:
                case WelcomeDialogBoxState:
                case LayoutSystemListBoardState:
                    listBoard.initialVideoClip = ListBoardResources.GetVideoClip("Building");
                    diamondBarBoard.activeIndex = 0;
                    ShowBoard();
                    break;
                case SettingsDialogBoxState:
                case QuitDialogBoxState:
                case InfoDialogBoxState:
                    showCompleted = true;
                    listBoard.interactable = true;
                    break;
            }

            m_VideoClips = new List<VideoClip>()
            {
                ListBoardResources.GetVideoClip("Building"),
                ListBoardResources.GetVideoClip("Navigation"),
                ListBoardResources.GetVideoClip("Production"),
                ListBoardResources.GetVideoClip("Equipment"),
                ListBoardResources.GetVideoClip("EditModeWorker"),
            };
        }

        protected override void OnLeft()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            listBoard.Hide(() =>
            {
                listBoard.blocksRaycasts = false;
                BoardManager.InterfaceBoard.interactable = false;
                context.state = new DiamondBarBoardState(context);
            });
        }

        protected override void OnRight()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            listBoard.Hide(() =>
            {
                listBoard.blocksRaycasts = false;
                context.state = new LayoutSystemListBoardState(context);
            });
        }

        protected override void OnCancel()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            context.state = new QuitDialogBoxState(context);
        }

        protected override void OnSettings()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            context.state = new SettingsDialogBoxState(context);
        }

        protected override void OnInfo()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            context.state = new InfoDialogBoxState(context);
        }

        void OnListElementClicked(int index)
        {
            var videoClip = m_VideoClips.ElementAtOrDefault(index);
            if (videoClip != null)
            {
                listBoard.SwapVideo(videoClip);
            }
        }
    }
}