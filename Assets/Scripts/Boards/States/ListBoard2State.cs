using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Boards.States
{
    public class ListBoard2State : BaseListBoardState
    {
        List<VideoClip> m_VideoClips;

        public ListBoard2State(BoardStateContext context) : base(context) { }

        protected override void Init()
        {
            interactable = true;

            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.Instance.visualTreeAssets[1];

            if (!listBoard.isVisible)
            {
                listBoard.initialVideoClip = ListBoardResources.Instance.videoClips[1];
                listBoard.Show();
            }

            m_VideoClips = new List<VideoClip>()
            {
                ListBoardResources.Instance.videoClips[1],
                ListBoardResources.Instance.videoClips[0],
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
                    context.state = new ListBoard1State(context);
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
                context.state = new ListBoard2QuitDialogBoxState(context);
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