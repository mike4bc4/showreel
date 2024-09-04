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
            interactable = true;

            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("LayoutSystemListBoard");

            if (!listBoard.isVisible)
            {
                listBoard.initialVideoClip = ListBoardResources.GetVideoClip("Component");
                listBoard.Show();
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
            if (listBoard.isVisible && interactable)
            {
                interactable = false;
                listBoard.Hide(() =>
                {
                    listBoard.onListElementClicked -= OnListElementClicked;
                    context.state = new PoliticoListBoardState(context);
                });
            }
        }

        public override void Right()
        {
            if (listBoard.isVisible && interactable)
            {
                Debug.Log("Next board not implemented yet.");
            }
        }

        public override void Cancel()
        {
            if (listBoard.isVisible && interactable)
            {
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