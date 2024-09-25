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

        public override void Init()
        {
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("OtherListBoard");

            listBoard.blocksRaycasts = true;
            switch (context.previousState)
            {
                case LocalizationListBoardState:
                    allowShowSkip = true;
                    listBoard.initialVideoClip = ListBoardResources.GetVideoClip("ArcaneLands");
                    diamondBarBoard.activeIndex = 3;
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
                ListBoardResources.GetVideoClip("ArcaneLands"),
                ListBoardResources.GetVideoClip("PlaneGame"),
                ListBoardResources.GetVideoClip("Other"),
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
                    context.state = new LocalizationListBoardState(context);
                });
            }
        }

        public override void Right()
        {
            if (listBoard.interactable)
            {
                Debug.Log("Next board not implemented yet.");
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