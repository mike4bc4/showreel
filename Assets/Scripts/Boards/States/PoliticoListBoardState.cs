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

        protected override void Init()
        {
            interactable = true;

            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("PoliticoListBoard");

            if (!listBoard.isVisible)
            {
                listBoard.initialVideoClip = ListBoardResources.GetVideoClip("Building");
                listBoard.Show();
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
            if (listBoard.isVisible && interactable)
            {
                interactable = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                listBoard.Hide(() =>
                {
                    BoardManager.GetBoard<InterfaceBoard>().Hide();
                    BoardManager.GetBoard<DiamondBarBoard>().Hide(() =>
                    {
                        context.state = new InitialBoardState(context);
                    });
                });
            }
        }

        public override void Right()
        {
            if (listBoard.isVisible && interactable)
            {
                interactable = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                listBoard.Hide(() =>
                {
                    context.state = new ListBoard2State(context);
                });
            }
        }

        public override void Cancel()
        {
            if (listBoard.isVisible && interactable)
            {
                listBoard.onListElementClicked -= OnListElementClicked;
                context.state = new PoliticoListBoardQuitDialogBoxState(context);
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