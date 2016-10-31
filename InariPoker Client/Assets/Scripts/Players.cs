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
    public static class Players
    {
        public static int userid;

        static Player player;
        static Dictionary<int, OtherPlayer> otherPlayers = new Dictionary<int, OtherPlayer>();

        static Text nav;
        static List<GameObject> prefabs = new List<GameObject>();

        public enum PlayersState
        {
            None,
            SelectBot,
            SelectOther,
            SelectType,
            PassOther,
            PassType,
        };
        static PlayersState state = PlayersState.None;
        static int selectCardIndex = -1;
        static int selectUserID = -1;
        static GameObject selectPanel;

        static GameObject centerCard;

        static GameObject trueButton, lieButton, passButton;
        static TurnData turn;

        static TrueOrLieData tolData;
        static PlayerBase tolPlayer;
        static float tolTimer;

        public static void Update(float delta)
        {
            if(tolTimer > 0)
            {
                tolTimer -= delta;
                if(tolTimer <= 0)
                {
                    SetCenterCard("0" + tolData.cardtype);
                    if (tolData.declare)
                    {
                        if(tolData.cardtype == turn.directions.Last<TurnDirection>().cardtype)
                        {
                            tolPlayer.SetCorrect();
                            AddCard(turn.directions.Last<TurnDirection>().srcuserid, tolData.cardtype);
                            CheckGameEnd(turn.directions.Last<TurnDirection>().srcuserid);
                        }
                        else
                        {
                            tolPlayer.SetWrong();
                            AddCard(turn.turnuserid, tolData.cardtype);
                            CheckGameEnd(turn.turnuserid);
                        }
                    }
                    else
                    {
                        if (tolData.cardtype != turn.directions.Last<TurnDirection>().cardtype)
                        {
                            tolPlayer.SetCorrect();
                            AddCard(turn.directions.Last<TurnDirection>().srcuserid, tolData.cardtype);
                            CheckGameEnd(turn.directions.Last<TurnDirection>().srcuserid);
                        }
                        else
                        {
                            tolPlayer.SetWrong();
                            AddCard(turn.turnuserid, tolData.cardtype);
                            CheckGameEnd(turn.turnuserid);
                        }
                    }
                    
                }
            }
        }

        static void CheckGameEnd(int userid)
        {
            if(Players.userid == userid)
            {
                if(player.init.cards.Count == 0 || player.CardOver4())
                {
                    player.SetDefeat();
                }
                return;
            }

            if(otherPlayers[userid].init.cardsum == 0 || otherPlayers[userid].CardOver4())
            {
                otherPlayers[userid].SetDefeat();
            }
        }

        public static void AddPlayer(PlayerInitDatas inits)
        {           
            player = new Player(inits.player);

            int playerNum = 1 + inits.others.Count;
            float f = Mathf.PI * 2 / playerNum;
            for(int i=0; i<inits.others.Count; i++)
            {
                otherPlayers.Add(inits.others[i].userid, new OtherPlayer(inits.others[i], f * (i + 1)));
            }

            nav = GameObject.Find("NavText").GetComponent<Text>();
            centerCard = GameObject.Find("CenterCard");
            centerCard.SetActive(false);

            trueButton = GameObject.Find("TrueButton");
            trueButton.SetActive(false);
            lieButton = GameObject.Find("LieButton");
            lieButton.SetActive(false);
            passButton = GameObject.Find("PassButton");
            passButton.SetActive(false);
        }

        public static void RemovePlayerCard(int cardtype)
        {
            player.RemoveCard(cardtype);
        }

        public static void RemoveOtherPlayerCard()
        {
            otherPlayers[turn.turnuserid].RemoveCard();
        }

        static void AddCard(int userid, int cardtype)
        {
            if(Players.userid == userid)
            {
                player.AddCard(cardtype);
            }
            else
            {
                otherPlayers[userid].AddCard(cardtype);
            }
        }

        public static void ProcessTurn(TurnData turn)
        {
            Players.turn = turn;

            ClearPrefab();
            nav.text = "";
            centerCard.SetActive(false);
            trueButton.SetActive(false);
            lieButton.SetActive(false);
            passButton.SetActive(false);
            state = PlayersState.None;

            SetTurnCoin(turn.turnuserid);
            SetDirectionArrows(turn.directions);
            SetBalloon(turn.directions);

            if (turn.directions.Count == 0)
            {
                if(turn.turnuserid == userid)
                {
                    nav.text = "カードを選択してください。";
                    state = PlayersState.SelectBot;
                }
                else
                {
                    nav.text = GetUserName(turn.turnuserid) + "がカードを選択しています。";
                }
            }
            else
            {
                SetCenterCard("back");

                if (turn.turnuserid == userid)
                {
                    int srcuserid = turn.directions.Last<TurnDirection>().srcuserid;
                    int cardtype = turn.directions.Last<TurnDirection>().cardtype;
                    nav.text = GetUserName(srcuserid) + "は"+ Env.ConvertCardName(cardtype) + "と宣言しました。どうしますか？";

                    if(turn.directions.Count != otherPlayers.Count)
                    {
                        trueButton.SetActive(true);
                        lieButton.SetActive(true);
                        passButton.SetActive(true);
                    }
                    else
                    {
                        trueButton.SetActive(true);
                        lieButton.SetActive(true);
                    }
                }
                else
                {
                    nav.text = GetUserName(turn.turnuserid) + "がプレイしています。";
                }
            }
        }

        static void SetCenterCard(string name)
        {
            centerCard.SetActive(true);

            centerCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(name);
        }

        static string GetUserName(int userid)
        {
            if(Players.userid == userid)
            {
                return player.init.username;
            }
            else
            {
                return otherPlayers[userid].init.username;
            }
        }

        static void ClearPrefab()
        {
            foreach(GameObject prefab in prefabs)
            {
                GameObject.Destroy(prefab);
            }
            prefabs.Clear();
        }

        static void SetDirectionArrows(List<TurnDirection> directions)
        {
            foreach(TurnDirection dir in directions)
            {
                CreateDirectionArror(dir.srcuserid, dir.destuserid);
            }
        }

        static void CreateDirectionArror(int srcuserid, int destuserid)
        {
            Vector2 src = GetAnchorPosition(srcuserid);
            Vector2 dest = GetAnchorPosition(destuserid);

            GameObject arrow = (GameObject)GameObject.Instantiate(Resources.Load("ArrowImage"), GameObject.Find("Arrows").transform);

            arrow.GetComponent<RectTransform>().anchoredPosition = src;
            float rad = Mathf.Atan2(dest.y - src.y, dest.x - src.x);
            arrow.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 180 * (rad / Mathf.PI));

            arrow.GetComponent<RectTransform>().anchoredPosition = (src + dest) / 2;
            arrow.GetComponent<RectTransform>().sizeDelta = new Vector2(Vector2.Distance(src, dest) - 75, 50);

            prefabs.Add(arrow);
        }

        static Vector2 GetAnchorPosition(int userid)
        {
            if (Players.userid == userid)
            {
                return player.anchorPos;
            }
            else
            {
                return otherPlayers[userid].anchorPos;
            }
        }

        static void SetTurnCoin(int userid)
        {
            if (Players.userid == userid)
            {
                player.SetTurnCoin(true);
            }
            else
            {
                player.SetTurnCoin(false);
            }

            foreach(OtherPlayer other in otherPlayers.Values)
            {
                if(other.init.userid == userid)
                {
                    other.SetTurnCoin(true);
                }
                else
                {
                    other.SetTurnCoin(false);
                }
            }
        }

        static void SetBalloon(List<TurnDirection> directions)
        {
            //clear
            player.ClearBalloon();
            foreach(OtherPlayer other in otherPlayers.Values)
            {
                other.ClearBalloon();
            }

            foreach(TurnDirection dir in directions)
            {
                if (dir.srcuserid == Players.userid)
                {
                    player.CreateBalloon(Env.ConvertCardName(dir.cardtype));
                    continue;
                }

                foreach (OtherPlayer other in otherPlayers.Values)
                {
                    if (dir.srcuserid == other.init.userid)
                    {
                        other.CreateBalloon(Env.ConvertCardName(dir.cardtype));
                        break;
                    }
                }
            }            
        }

        static public void SelectBigCard(int index)
        {
            if (state == PlayersState.SelectBot)
            {
                selectCardIndex = index;
                player.SelectBottomCard(index);
                state = PlayersState.SelectOther;
                nav.text = "対象のプレイヤーを選択してください。";
                return;
            }
            else if (state == PlayersState.SelectOther)
            {
                if (selectCardIndex == index)
                {
                    selectCardIndex = -1;
                    player.ResetBottomCard();
                    state = PlayersState.SelectBot;
                    nav.text = "カードを選択してください。";
                    return;
                }
                else
                {
                    selectCardIndex = index;
                    player.ResetBottomCard();
                    player.SelectBottomCard(index);
                    return;
                }
            }
            else if (state == PlayersState.SelectType)
            {
                if (selectCardIndex == index)
                {
                    GameObject.Destroy(selectPanel);
                    selectUserID = -1;
                    selectCardIndex = -1;
                    player.ResetBottomCard();
                    state = PlayersState.SelectBot;
                    nav.text = "カードを選択してください。";
                    return;
                }
                else
                {
                    selectCardIndex = index;
                    player.ResetBottomCard();
                    player.SelectBottomCard(index);
                }
            }
        }

        static public void SelectOther(int userid)
        {
            if (state == PlayersState.SelectOther)
            {
                selectUserID = userid;
                state = PlayersState.SelectType;
                nav.text = "なんと宣言しますか？";
                CreateSelectPanel(userid);
                return;
            }
            else if(state == PlayersState.SelectType)
            {
                GameObject.Destroy(selectPanel);
                selectUserID = userid;
                CreateSelectPanel(userid);
                return;
            }

            if(state == PlayersState.PassOther || state == PlayersState.PassType)
            {
                foreach(TurnDirection direction in turn.directions)
                {
                    if(direction.srcuserid == userid)
                    {
                        return;
                    }
                }
            }

            if(state == PlayersState.PassOther)
            {
                selectUserID = userid;
                state = PlayersState.PassType;
                nav.text = "なんと宣言しますか？";
                CreateSelectPanel(userid);
                return;
            }
            else if(state == PlayersState.PassType)
            {
                GameObject.Destroy(selectPanel);
                selectUserID = userid;
                CreateSelectPanel(userid);
                return;
            }
        }

        static void CreateSelectPanel(int userid)
        {
            selectPanel = (GameObject)GameObject.Instantiate(Resources.Load("SelectPanel"), GameObject.Find("Select").transform);
            foreach(OtherPlayer other in otherPlayers.Values)
            {
                if(other.init.userid == userid)
                {
                    selectPanel.GetComponent<RectTransform>().anchoredPosition = other.anchorPos;
                    break;
                }
            }

            float f = Mathf.PI * 2 / 8;
            for (int i = 0; i < 8; i++)
            {
                GameObject card = (GameObject)GameObject.Instantiate(Resources.Load("SelectCard"), selectPanel.transform);
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Cos(f * i), Mathf.Sin(f * i)) * 75f;
                card.GetComponent<SelectCardScript>().Init(i);
            }
        }

        public static void DecideCard(int cardtype)
        {
            if (state == PlayersState.SelectType)
            {
                int selectType = player.init.cards[selectCardIndex];
                //Debug.Log("Select:" + selectType + "UserID:" + selectUserID + "Declare:" + cardtype);
                DeclareCardData dec = new DeclareCardData { cardtype = selectType, userid = selectUserID, deccardtype = cardtype };
                GCli.Send(MessageType.DeclareCard, GCli.Serialize<DeclareCardData>(dec), NetDeliveryMethod.ReliableOrdered);
            }
            else if(state == PlayersState.PassType)
            {
                //Debug.Log("UserID:" + selectUserID + "Declare:" + cardtype);
                DeclareCardData dec = new DeclareCardData { cardtype = -1, userid = selectUserID, deccardtype = cardtype };
                GCli.Send(MessageType.DeclareCard, GCli.Serialize<DeclareCardData>(dec), NetDeliveryMethod.ReliableOrdered);
            }

            GameObject.Destroy(selectPanel);
            selectUserID = -1;
            selectCardIndex = -1;
            player.ResetBottomCard();
            state = PlayersState.None;
            nav.text = "";
        }

        public static void SetPassCard(int cardtype)
        {
            trueButton.SetActive(false);
            lieButton.SetActive(false);
            passButton.SetActive(false);

            SetCenterCard("0" + cardtype);
            nav.text = "対象のプレイヤーを選択してください。";
            state = PlayersState.PassOther;
        }

        public static void SetTrueOrLieData(TrueOrLieData data)
        {
            tolData = data;
            tolTimer = 2.0f;

            trueButton.SetActive(false);
            lieButton.SetActive(false);
            passButton.SetActive(false);
            nav.text = "";


            if (turn.turnuserid == Players.userid)
            {
                player.CreateBalloon(data.declare ? "本当" : "嘘");
                tolPlayer = player;
                return;
            }

            foreach (OtherPlayer other in otherPlayers.Values)
            {
                if (turn.turnuserid == other.init.userid)
                {
                    other.CreateBalloon(data.declare ? "本当" : "嘘");
                    tolPlayer = other;
                    return;
                }
            }
        }

        public static void LeaveUser(int userid)
        {
            nav.text = GetUserName(userid) + "が抜けました。部屋に戻ります。";
        }

        public static void Destroy()
        {
            player.Destroy();
            foreach(OtherPlayer other in otherPlayers.Values)
            {
                other.Destroy();
            }
            otherPlayers.Clear();

            nav.text = "";
            centerCard.SetActive(true);
            trueButton.SetActive(true);
            lieButton.SetActive(true);
            passButton.SetActive(true);
        }

        public static void Roar(RoarData roar)
        {
            if(Players.userid == roar.userid)
            {
                player.SetRoar(roar.roartype);
                return;
            }

            otherPlayers[roar.userid].SetRoar(roar.roartype);
        }

        /*public static void AddOtherPlayer(PlayerInitData init)
        {
            otherPlayers.Add(init.sync.userid, new OtherPlayer(init));
        }

        public static Player GetPlayer()
        {
            return player;
        }        

        public static PushData GetPushData()
        {
            return player.GetPushData();
        }

        public static void UpdatePlayerSyncData(List<PlayerSyncData> syncs)
        {
            foreach (OtherPlayer other in otherPlayers.Values)
            {
                other.ResetUpdateFlag();
            }

            foreach (PlayerSyncData sync in syncs)
            {
                if (sync.userid == userid)
                {
                    player.ReceiveLatestData(sync);
                }
                else if (otherPlayers.ContainsKey(sync.userid))
                {
                    otherPlayers[sync.userid].ReceiveLatestData(sync);
                }
                else
                {
                    
                }
            }

            var removes = otherPlayers.Where(f => f.Value.updated == false).ToArray();
            foreach (var remove in removes)
            {
                remove.Value.Destroy();
                otherPlayers.Remove(remove.Key);
            }
        }

        public static void FixedUpdate(float delta)
        {
            foreach (OtherPlayer other in otherPlayers.Values)
            {
                other.Update(delta);
            }
        }

        public static void Destroy()
        {
            player.Destroy();
            player = null;

            foreach (OtherPlayer other in otherPlayers.Values)
            {
                other.Destroy();
            }
            otherPlayers.Clear();
        }

        public static void ReplyFire(int userid)
        {
            KeyValuePair<int, OtherPlayer> other = otherPlayers.FirstOrDefault(x => x.Value.GetUserID() == userid);

            if(!other.Equals(default(KeyValuePair<int, OtherPlayer>)))
            {
                other.Value.ReplyFire();
            }
        }*/
    }
}
