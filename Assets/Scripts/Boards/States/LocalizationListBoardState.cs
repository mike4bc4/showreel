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

        protected override void Init()
        {
            interactable = true;

            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("LocalizationListBoard");

            if (!listBoard.isVisible)
            {
                listBoard.initialVideoClip = ListBoardResources.GetVideoClip("LocalizationWindow");
                listBoard.Show();
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
            if (listBoard.isVisible && interactable)
            {
                interactable = false;
                listBoard.Hide(() =>
                {
                    listBoard.onListElementClicked -= OnListElementClicked;
                    context.state = new LayoutSystemListBoardState(context);
                });
            }
        }

        public override void Right()
        {
            if (listBoard.isVisible && interactable)
            {
                interactable = false;
                listBoard.Hide(() =>
                {
                    listBoard.onListElementClicked -= OnListElementClicked;
                    context.state = new OtherListBoardState(context);
                });
            }
        }

        public override void Cancel()
        {
            if (listBoard.isVisible && interactable)
            {
                listBoard.onListElementClicked -= OnListElementClicked;
                context.state = new LocalizationListBoardQuitDialogBoxState(context);
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