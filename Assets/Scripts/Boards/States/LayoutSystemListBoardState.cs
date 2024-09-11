using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Boards.States
{
    public class LayoutSystemListBoardState : BaseListBoardState
    {
        List<VideoClip> m_VideoClips;

        public LayoutSystemListBoardState(BoardStateContext context) : base(context) { }

        protected override void Init()
        {
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("LayoutSystemListBoard");

            listBoard.blocksRaycasts = true;
            if (!listBoard.isVisible)
            {
                allowShowSkip = true;
                listBoard.initialVideoClip = ListBoardResources.GetVideoClip("Component");
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
                ListBoardResources.GetVideoClip("Component"),
                ListBoardResources.GetVideoClip("FlowAndAlignment"),
                ListBoardResources.GetVideoClip("SizingAndAnimation"),
                ListBoardResources.GetVideoClip("PaddingAndSpacing"),
                ListBoardResources.GetVideoClip("Error"),
                ListBoardResources.GetVideoClip("TextSupport"),
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
                    context.state = new PoliticoListBoardState(context);
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
                    context.state = new LocalizationListBoardState(context);
                });
            }
        }

        public override void Cancel()
        {
            if (listBoard.interactable)
            {
                listBoard.blocksRaycasts = false;
                listBoard.onListElementClicked -= OnListElementClicked;
                context.state = new LayoutSystemListBoardQuitDialogBoxState(context);
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