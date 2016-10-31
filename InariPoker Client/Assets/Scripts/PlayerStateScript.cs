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
    public class PlayerStateScript : MonoBehaviour
    {
        [SerializeField]
        Text userName;
        [SerializeField]
        GameObject readyButton, ready;

        PlayerState state;

        public void Init(PlayerState state)
        {
            this.state = state;

            userName.text = state.username;

            if(!state.ready)
            {
                ready.SetActive(false);
            }

            if (state.userid == Players.userid)
            {
                if (state.ready)
                {
                    readyButton.GetComponentInChildren<Text>().text = "キャンセル";
                }
                else
                {
                    readyButton.GetComponentInChildren<Text>().text = "準備完了";
                }
            }
            else
            {
                readyButton.SetActive(false);
            }
        }

        public void ChangeReady()
        {
            GCli.Send(MessageType.ChangeReady, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
