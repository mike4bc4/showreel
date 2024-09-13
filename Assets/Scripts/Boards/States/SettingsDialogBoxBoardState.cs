using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

namespace Boards.States
{
    public class SettingsDialogBoxState : BoardState
    {
        const int k_DefaultDisplaySortOrder = 2000;
        DialogBox m_DialogBox;

        public SettingsDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_DialogBox = DialogBox.CreateSettingsDialogBox();
            m_DialogBox.displaySortOrder = k_DefaultDisplaySortOrder;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, Confirm);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Right, Cancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, Cancel);
            m_DialogBox.Show();
        }

        void Close()
        {
            m_DialogBox.Hide(() =>
            {
                m_DialogBox.Dispose();
                switch (context.previousState)
                {
                    case PoliticoListBoardState:
                        context.state = new PoliticoListBoardState(context);
                        break;
                    case LayoutSystemListBoardState:
                        context.state = new LayoutSystemListBoardState(context);
                        break;
                    case LocalizationListBoardState:
                        context.state = new LocalizationListBoardState(context);
                        break;
                    case OtherListBoardState:
                        context.state = new OtherListBoardState(context);
                        break;
                }
            });
        }

        public override void Confirm()
        {
            // TODO: Apply settings.
            Close();
        }

        public override void Cancel()
        {
            Close();
        }
    }
}
