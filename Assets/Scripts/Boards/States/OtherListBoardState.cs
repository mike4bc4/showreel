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
            base.Init();
            listBoard.onListElementClicked += OnListElementClicked;
            listBoard.visualTreeAsset = ListBoardResources.GetVisualTreeAsset("OtherListBoard");

            listBoard.blocksRaycasts = true;
            switch (context.previousState)
            {
                case LocalizationListBoardState:
                    listBoard.initialVideoClip = ListBoardResources.GetVideoClip("ArcaneLands");
                    diamondBarBoard.activeIndex = 3;
                    ShowBoard();
                    break;
                case SettingsDialogBoxState:
                case QuitDialogBoxState:
                case InfoDialogBoxState:
                    showCompleted = true;
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

        protected override void OnLeft()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            listBoard.Hide(() =>
            {
                listBoard.blocksRaycasts = false;
                context.state = new LocalizationListBoardState(context);
            });
        }

        protected override void OnRight()
        {
            // enabled = false;
            Debug.Log("Next board not implemented yet.");
        }

        protected override void OnCancel()
        {
            enabled = false; listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            context.state = new QuitDialogBoxState(context);
        }

        protected override void OnSettings()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            context.state = new SettingsDialogBoxState(context);
        }

        protected override void OnInfo()
        {
            enabled = false;
            listBoard.interactable = false;
            listBoard.onListElementClicked -= OnListElementClicked;
            context.state = new InfoDialogBoxState(context);
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