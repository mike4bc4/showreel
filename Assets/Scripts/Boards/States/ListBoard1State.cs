using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Boards.States
{
    // public class ListBoard1State : BaseListBoardState
    // {
    //     List<VideoClip> m_VideoClips;

    //     public ListBoard1State(BoardStateContext context) : base(context) { }

    //     protected override void Init()
    //     {
    //         interactable = true;

    //         listBoard.onListElementClicked += OnListElementClicked;
    //         listBoard.visualTreeAsset = ListBoardResources.Instance.visualTreeAssets[0];

    //         if (!listBoard.isVisible)
    //         {
    //             listBoard.initialVideoClip = ListBoardResources.Instance.videoClips[0];
    //             listBoard.Show();
    //         }

    //         m_VideoClips = new List<VideoClip>()
    //         {
    //             ListBoardResources.Instance.videoClips[0],
    //             ListBoardResources.Instance.videoClips[1],
    //         };
    //     }

    //     public override void Left()
    //     {
    //         if (listBoard.isVisible && interactable)
    //         {
    //             interactable = false;
    //             listBoard.onListElementClicked -= OnListElementClicked;
    //             listBoard.Hide(() =>
    //             {
    //                 BoardManager.GetBoard<InterfaceBoard>().Hide();
    //                 BoardManager.GetBoard<DiamondBarBoard>().Hide(() =>
    //                 {
    //                     context.state = new InitialBoardState(context);
    //                 });
    //             });
    //         }
    //     }

    //     public override void Right()
    //     {
    //         if (listBoard.isVisible && interactable)
    //         {
    //             interactable = false;
    //             listBoard.onListElementClicked -= OnListElementClicked;
    //             listBoard.Hide(() =>
    //             {
    //                 context.state = new ListBoard2State(context);
    //             });
    //         }
    //     }

    //     public override void Cancel()
    //     {
    //         if (listBoard.isVisible && interactable)
    //         {
    //             listBoard.onListElementClicked -= OnListElementClicked;
    //             context.state = new ListBoardQuitDialogBoxState(context);
    //         }
    //     }

    //     void OnListElementClicked(int index)
    //     {
    //         var videoClip = m_VideoClips.ElementAtOrDefault(index);
    //         if (videoClip != null)
    //         {
    //             listBoard.SwapVideo(videoClip);
    //         }
    //     }
    // }
}
