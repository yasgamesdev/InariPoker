using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YasGameLib;

namespace InariPoker
{
    class RoomSummaryScript : MonoBehaviour
    {
        [SerializeField]
        Text roomName, roomState, playerNum;
        [SerializeField]
        Image roomStateImage;
        [SerializeField]
        GameObject enterButton;

        RoomSummary summary;

        public void Init(RoomSummary summary)
        {
            this.summary = summary;

            roomName.text = "部屋" + summary.roomid.ToString();
            if(summary.roomstate == 0)
            {
                roomState.text = "ゲーム開始前";
                roomStateImage.color = Color.green;
            }
            else
            {
                roomState.text = "ゲーム中";
                roomStateImage.color = Color.red;
                enterButton.SetActive(false);
            }
            playerNum.text = summary.playernum.ToString() + "人";

            if(summary.playernum >= 6)
            {
                enterButton.SetActive(false);
            }
        }

        public void EnterRoom()
        {
            GCli.Send(MessageType.EnterRoom, summary.roomid, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
