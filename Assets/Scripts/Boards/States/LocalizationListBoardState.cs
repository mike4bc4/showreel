using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Boards.States
{
    public class LocalizationListBoardState : BaseListBoardState
    {
        List<VideoClip> m_VideoClips;

        public LocalizationListBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("LocalizationListBoard");

            listBoard.blocksRaycasts = true;
            switch (context.previousState)
            {
                case LayoutSystemListBoardState:
                case OtherListBoardState:
                    allowShowSkip = true;
                    listBoard.initialVideoClip = ListBoardResources.GetVideoClip("LocalizationWindow");
                    diamondBarBoard.activeIndex = 2;
                    listBoard.Show(() =>
                    {
                        allowShowSkip = false;
                        listBoard.interactable = true;
                    });
                    break;
                case QuitDialogBoxState:
                    listBoard.interactable = true;
                    break;
            }

            m_VideoClips = new List<VideoClip>()
            {
                ListBoardResources.GetVideoClip("LocalizationWindow"),
                ListBoardResources.GetVideoClip("TableCreation"),
                ListBoardResources.GetVideoClip("LabelLocalization"),
                ListBoardResources.GetVideoClip("LocalizationSettings"),
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
                    context.state = new LayoutSystemListBoardState(context);
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
                    context.state = new OtherListBoardState(context);
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