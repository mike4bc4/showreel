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
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("PoliticoListBoard");

            listBoard.blocksRaycasts = true;
            switch (context.previousState)
            {
                case WelcomeDialogBoxState:
                case LayoutSystemListBoardState:
                    allowShowSkip = true;
                    listBoard.initialVideoClip = ListBoardResources.GetVideoClip("Building");
                    diamondBarBoard.activeIndex = 0;
                    listBoard.Show(() =>
                    {
                        allowShowSkip = false;
                        listBoard.interactable = true;
                    });
                    break;
                case SettingsDialogBoxState:
                case QuitDialogBoxState:
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

        public override void Left()
        {
            if (listBoard.interactable)
            {
                listBoard.interactable = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                listBoard.Hide(() =>
                {
                    listBoard.blocksRaycasts = false;
                    BoardManager.GetBoard<InterfaceBoard>().interactable = false;
                    context.state = new DiamondBarBoardState(context);
                });
            }
        }

        public override void Right()
        {
            if (listBoard.interactable)
            {
                listBoard.interactable = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                listBoard.Hide(() =>
                {
                    listBoard.blocksRaycasts = false;
                    context.state = new LayoutSystemListBoardState(context);
                });
            }
        }

        public override void Cancel()
        {
            if (listBoard.interactable)
            {
                listBoard.interactable = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                context.state = new QuitDialogBoxState(context);
            }
        }

        public override void Settings()
        {
            if (listBoard.interactable)
            {
                listBoard.interactable = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                context.state = new SettingsDialogBoxState(context);
            }
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