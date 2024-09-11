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
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("PoliticoListBoard");

            listBoard.blocksRaycasts = true;
            if (!listBoard.isVisible)
            {
                allowShowSkip = true;
                listBoard.initialVideoClip = ListBoardResources.GetVideoClip("Building");
                listBoard.Show(() =>
                {
                    allowShowSkip = false;
                    listBoard.interactable = true;
                });
            }
            else
            {
                listBoard.interactable = true;
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
                    
                    var interfaceBoard = BoardManager.GetBoard<InterfaceBoard>();
                    interfaceBoard.interactable = false;
                    interfaceBoard.Hide(() =>
                    {
                        interfaceBoard.blocksRaycasts = true;
                    });

                    BoardManager.GetBoard<DiamondBarBoard>().Hide(() =>
                    {
                        context.state = new InitialBoardState(context);
                    });
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