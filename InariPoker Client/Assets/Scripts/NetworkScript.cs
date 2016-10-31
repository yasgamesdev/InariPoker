using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using YasGameLib;

namespace InariPoker
{
    public class NetworkScript : MonoBehaviour
    {
        //UI
        [SerializeField]
        GameObject menu, start, lobby, room;
        [SerializeField]
        Text debugText;

        [SerializeField]
        InputField username;

        string host;
        int port;

        float frameSpan = 0;

        List<GameObject> roomSummaryUIs = new List<GameObject>();
        List<GameObject> roomStateUIs = new List<GameObject>();

        [SerializeField]
        GameObject roarPanel;

        void Start()
        {
            if (!ReadFile(Application.dataPath + "/../Setting/setting.txt"))
            {
                return;
            }

            menu.SetActive(true);
            start.SetActive(false);
            lobby.SetActive(false);
            room.SetActive(false);

            GCli.Init();
            GCli.SetConnectPacketHandler(ConnectHandler);
            GCli.SetDebugPacketHandler(DebugHandler);

            GCli.Connect("InariPoker0.1", host, port);

            //DummyStart();
            CreateRoarPanel();
        }

        void CreateRoarPanel()
        {
            for(int i=0; i<Roars.GetMaxNum(); i++)
            {
                GameObject button = (GameObject)Instantiate(Resources.Load("RoarButton"), roarPanel.transform);
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(100 * (i / 13), -30 * (i % 13));
                button.GetComponent<RoarButtonScript>().Init(i, Roars.GetText(i));
            }
        }

        void DummyStart()
        {
            PlayerInitDatas inits = CreateDummyInit();

            Players.AddPlayer(inits);
        }

        PlayerInitDatas CreateDummyInit()
        {
            int playerNum = 6;
            List<List<int>> cards = Env.CreateRandomCards(playerNum);

            PlayerInitDatas ret = new PlayerInitDatas();

            PlayerInitData player = new PlayerInitData { userid = 0, username = "Yas"};
            player.cards.AddRange(cards[0]);
            ret.player = player;

            for(int i=1; i<playerNum; i++)
            {
                OtherPlayerInitData other = new OtherPlayerInitData { userid = i, username = "user" + i };
                other.cardsum = cards[i].Count;
                ret.others.Add(other);
            }

            return ret;
        }

        bool ReadFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            try
            {
                using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
                {
                    while (true)
                    {
                        string line = sr.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        if (line.StartsWith("Host="))
                        {
                            string sub = line.Substring("Host=".Length);
                            host = sub;
                        }
                        else if (line.StartsWith("Port="))
                        {
                            string sub = line.Substring("Port=".Length);
                            if (!int.TryParse(sub, out port))
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                debugText.text = e.Message;
                return false;
            }

            return true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            GCli.Receive();

            /*frameSpan += Time.deltaTime;
            if (frameSpan >= 0.1f)
            {
                frameSpan = 0;
            }*/

            Players.Update(Time.deltaTime);

            /*if (Input.GetMouseButtonDown(1))
            {
                //Players.RemovePlayerCard(0);
                //Players.RemoveOtherPlayerCard(1);
                //Players.AddCard(1, 2);
                TurnData turn = new TurnData { turnuserid = 1 };
                turn.directions.Add(new TurnDirection { srcuserid = 3, destuserid = 1, cardtype = 3 });
                turn.directions.Add(new TurnDirection { srcuserid = 1, destuserid = 3, cardtype = 4 });
                turn.directions.Add(new TurnDirection { srcuserid = 3, destuserid = 4, cardtype = 2 });
                turn.directions.Add(new TurnDirection { srcuserid = 4, destuserid = 0, cardtype = 1 });
                Players.ProcessTurn(turn);
            }
            if (Input.GetMouseButtonDown(2))
            {
                TrueOrLieData data = new TrueOrLieData { declare = true, cardtype = 3 };
                Players.SetTrueOrLieData(data);
            }*/
        }

        void FixedUpdate()
        {
            //Players.FixedUpdate(Time.deltaTime);
        }

        void OnDestroy()
        {
            GCli.Shutdown();
        }

        public void PressTrueButton()
        {
            //TrueOrLieData data = new TrueOrLieData { declare = true, cardtype = 0 };
            //Players.SetTrueOrLieData(data);
            GCli.Send(MessageType.RequestTrueOrLie, 1, NetDeliveryMethod.ReliableOrdered);
        }

        public void PressLieButton()
        {
            //TrueOrLieData data = new TrueOrLieData { declare = false, cardtype = 0 };
            //Players.SetTrueOrLieData(data);
            GCli.Send(MessageType.RequestTrueOrLie, 0, NetDeliveryMethod.ReliableOrdered);
        }

        public void PressPassButton()
        {
            //Players.SetPassCard(3);
            GCli.Send(MessageType.RequestPass, NetDeliveryMethod.ReliableOrdered);
        }       

        public void ConnectHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();
            debugText.text = "";
            start.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyUserID, DataType.Int32, ReplyUserIDHandler);            
        }

        public void SendUserName()
        {
            string name = username.text;
            GCli.Send(MessageType.SendUserName, name, NetDeliveryMethod.ReliableOrdered);
        }

        public void DebugHandler(NetConnection connection, object data)
        {
            string message = (string)data;
            debugText.text = message;
        }

        public void ReplyUserIDHandler(NetConnection connection, object data)
        {
            Players.userid = (int)data;

            GCli.ClearPacketHandler();
            start.SetActive(false);
            lobby.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyLobbyState, DataType.Bytes, ReplyLobbyStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessEnterRoom, DataType.Null, ReplySuccessEnterRoomHandler);
        }

        public void ReplyLobbyStateHandler(NetConnection connection, object data)
        {
            LobbyState state = GCli.Deserialize<LobbyState>((byte[])data);

            RefreshLobbyUI(state);
        }
        
        void RefreshLobbyUI(LobbyState state)
        {
            //delete
            foreach(GameObject room in roomSummaryUIs)
            {
                Destroy(room);
            }
            roomSummaryUIs.Clear();

            //UI
            for (int i = 0; i < state.summaries.Count; i++)
            {
                GameObject room = GameObject.Instantiate(Resources.Load("RoomSummary"), lobby.transform) as GameObject;
                room.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 142 + -34 * i);
                room.GetComponent<RoomSummaryScript>().Init(state.summaries[i]);
                roomSummaryUIs.Add(room);
            }
        }

        /*public void RequestLobbyState()
        {
            GCli.Send(MessageType.RequestLobbyState, NetDeliveryMethod.ReliableOrdered);
        }*/

        public void ReplySuccessEnterRoomHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();
            lobby.SetActive(false);
            room.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyRoomState, DataType.Bytes, ReplyRoomStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessLeaveRoom, DataType.Null, ReplySuccessLeaveRoomHandler);
            GCli.SetPacketHandler(MessageType.BattleStart, DataType.Bytes, BattleStartHandler);
        }

        public void ReplyRoomStateHandler(NetConnection connection, object data)
        {
            RoomState state = GCli.Deserialize<RoomState>((byte[])data);

            RefreshRoomUI(state);
        }

        void RefreshRoomUI(RoomState state)
        {
            //delete
            foreach (GameObject room in roomStateUIs)
            {
                Destroy(room);
            }
            roomStateUIs.Clear();

            //UI
            for (int i = 0; i < state.players.Count; i++)
            {
                GameObject player = GameObject.Instantiate(Resources.Load("PlayerState"), room.transform) as GameObject;
                player.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 142 + -34 * i);
                player.GetComponent<PlayerStateScript>().Init(state.players[i]);
                roomStateUIs.Add(player);
            }
        }

        public void LeaveRoom()
        {
            GCli.Send(MessageType.LeaveRoom, NetDeliveryMethod.ReliableOrdered);
        }

        public void ReplySuccessLeaveRoomHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();
            room.SetActive(false);
            lobby.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyLobbyState, DataType.Bytes, ReplyLobbyStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessEnterRoom, DataType.Null, ReplySuccessEnterRoomHandler);
        }

        public void BattleStartHandler(NetConnection connection, object data)
        {
            PlayerInitDatas inits = GCli.Deserialize<PlayerInitDatas>((byte[])data);

            GCli.ClearPacketHandler();
            room.SetActive(false);
            menu.SetActive(false);

            Players.AddPlayer(inits);

            GCli.SetPacketHandler(MessageType.SendTrunData, DataType.Bytes, SendTrunDataHandler);
            GCli.SetPacketHandler(MessageType.RemoveCardType, DataType.Int32, RemoveCardTypeHandler);
            GCli.SetPacketHandler(MessageType.RemoveCard, DataType.Null, RemoveCardHandler);
            GCli.SetPacketHandler(MessageType.ReplyPass, DataType.Int32, ReplyPassHandler);

            //GCli.SetPacketHandler(MessageType.ReplyWon, DataType.Int32, ReplyWonHandler);
            GCli.SetPacketHandler(MessageType.ReplyTrueOrLie, DataType.Bytes, ReplyTrueOrLieHandler);
            GCli.SetPacketHandler(MessageType.BattleEnd, DataType.Null, BattleEndHandler);
            GCli.SetPacketHandler(MessageType.LeavePlayer, DataType.Int32, LeavePlayerHandler);
            GCli.SetPacketHandler(MessageType.BroadcastMessage, DataType.Bytes, BroadcastMessageHandler);
        }

        public void SendTrunDataHandler(NetConnection connection, object data)
        {
            TurnData turn = GCli.Deserialize<TurnData>((byte[])data);

            Players.ProcessTurn(turn);
        }

        public void RemoveCardTypeHandler(NetConnection connection, object data)
        {
            int cardtype = (int)data;
            Players.RemovePlayerCard(cardtype);
        }

        public void RemoveCardHandler(NetConnection connection, object data)
        {
            Players.RemoveOtherPlayerCard();
        }

        public void ReplyPassHandler(NetConnection connection, object data)
        {
            int cardtype = (int)data;
            Players.SetPassCard(cardtype);
        }

        /*public void ReplyWonHandler(NetConnection connection, object data)
        {
            int faction = (int)data;

            GameObject won = Instantiate(Resources.Load("WonText"), GameObject.Find("Canvas").transform) as GameObject;
            won.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            won.GetComponent<WonTextScript>().Init(faction);
        }*/

        public void ReplyTrueOrLieHandler(NetConnection connection, object data)
        {
            TrueOrLieData result = GCli.Deserialize<TrueOrLieData>((byte[])data);
            Players.SetTrueOrLieData(result);
        }

        public void BattleEndHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();

            Players.Destroy();

            if(roarPanel.activeSelf)
            {
                RoarPanelOpen();
            }

            menu.SetActive(true);
            room.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyRoomState, DataType.Bytes, ReplyRoomStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessLeaveRoom, DataType.Null, ReplySuccessLeaveRoomHandler);
            GCli.SetPacketHandler(MessageType.BattleStart, DataType.Bytes, BattleStartHandler);
        }

        public void LeavePlayerHandler(NetConnection connection, object data)
        {
            int userid = (int)data;
            Players.LeaveUser(userid);
        }

        public void RoarPanelOpen()
        {
            roarPanel.SetActive(!roarPanel.activeSelf);
        }

        public  void RoarButtonClick(int index)
        {
            roarPanel.SetActive(false);

            GCli.Send(MessageType.SendMessage, index, NetDeliveryMethod.ReliableOrdered);
        }

        public void BroadcastMessageHandler(NetConnection connection, object data)
        {
            RoarData roar = GCli.Deserialize<RoarData>((byte[])data);

            Players.Roar(roar);
        }
    }
}
