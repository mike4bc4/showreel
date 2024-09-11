using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Boards.States
{
    public class OtherListBoardState : BaseListBoardState
    {
        List<VideoClip> m_VideoClips;

        public OtherListBoardState(BoardStateContext context) : base(context) { }

        protected override void Init()
        {
            interactable = true;

            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("OtherListBoard");

            if (!listBoard.isVisible)
            {
                listBoard.initialVideoClip = ListBoardResources.GetVideoClip("ArcaneLands");
                listBoard.Show();
            }

            m_VideoClips = new List<VideoClip>()
            {
                ListBoardResources.GetVideoClip("ArcaneLands"),
                ListBoardResources.GetVideoClip("PlaneGame"),
                ListBoardResources.GetVideoClip("Other"),
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
                    context.state = new LocalizationListBoardState(context);
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
                context.state = new OtherListBoardQuitDialogBoxState(context);
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