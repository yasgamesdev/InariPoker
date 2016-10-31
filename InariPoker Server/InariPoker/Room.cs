using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YasGameLib;

namespace InariPoker
{
    public class Room
    {
        int roomid;
        Dictionary<NetConnection, Player> players = new Dictionary<NetConnection, Player>();
        int roomstate = 0;

        //game
        float wonTimer = 0;
        float leaveTimer = 0;
        float turnTimer = 0;

        TurnData turn;

        public Room(int roomid)
        {
            this.roomid = roomid;
        }

        public RoomSummary GetRoomSummary()
        {
            return new RoomSummary { roomid = roomid, playernum = players.Count, roomstate = roomstate };
        }

        public int GetPlayerNum()
        {
            return players.Count;
        }

        public void EnterRoom(Player player)
        {
            player.ready = false;

            players.Add(player.connection, player);
        }

        public void SendRoomStateToAll()
        {
            RoomState state = new RoomState();
            state.roomid = roomid;
            foreach(Player player in players.Values)
            {
                state.players.Add(new PlayerState { userid = player.userid, username = player.username, ready = player.ready });
            }

            foreach(Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyRoomState, GSrv.Serialize<RoomState>(state), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public int GetRoomState()
        {
            return roomstate;
        }

        public Player LeaveRoom(NetConnection connection)
        {
            Player player = players[connection];
            players.Remove(connection);
            return player;
        }

        public void ChangeReady(NetConnection connection)
        {
            players[connection].ready = !players[connection].ready;
        }

        public bool CheckBattleStart()
        {
            if (players.Count <= 1)
            {
                return false;
            }

            Player not_ready_player = players.FirstOrDefault(x => x.Value.ready == false).Value;
            if (not_ready_player != null)
            {
                return false;
            }

            List<List<int>> randoms = Env.CreateRandomCards(players.Count);
            int index = 0;
            foreach(Player player in players.Values)
            {
                player.cards.Clear();
                player.cards.AddRange(randoms[index++]);
                player.ResetPenalties();
            }

            int randNum = new Random().Next(players.Count);
            int foreachCount = 0;
            foreach(Player player in players.Values)
            {
                if(foreachCount++ == randNum)
                {
                    turn = new TurnData { turnuserid = player.userid };
                    break;
                }
            }

            foreach (Player player in players.Values)
            {
                PlayerInitDatas datas = new PlayerInitDatas();

                foreach (Player p in players.Values)
                {
                    if (p == player)
                    {
                        datas.player = new PlayerInitData { userid = p.userid, username = p.username };
                        datas.player.cards.AddRange(p.cards);
                    }
                    else
                    {
                        datas.others.Add(new OtherPlayerInitData { userid = p.userid, username = p.username, cardsum = p.cards.Count });
                    }
                }

                GSrv.Send(MessageType.BattleStart, GSrv.Serialize<PlayerInitDatas>(datas), player.connection, NetDeliveryMethod.ReliableOrdered);
                GSrv.Send(MessageType.SendTrunData, GSrv.Serialize<TurnData>(turn), player.connection, NetDeliveryMethod.ReliableOrdered);
            }

            wonTimer = 0;
            leaveTimer = 0;
            turnTimer = 0;

            roomstate = 1;

            return true;
        }

        public void Update(float delta)
        {
            if (roomstate == 0)
            {
                return;
            }

            if(leaveTimer > 0)
            {
                leaveTimer -= delta;

                if(leaveTimer <= 0)
                {
                    BattleEnd();
                }
            }

            if(wonTimer > 0)
            {
                wonTimer -= delta;

                if(wonTimer <= 0)
                {
                    BattleEnd();
                }
            }

            if (turnTimer > 0)
            {
                turnTimer -= delta;

                if (turnTimer <= 0)
                {
                    SendTurnDataToAll();
                }
            }
        }

        /*public void TouchCore(NetConnection connection)
        {
            if (wonTimer > 0)
            {
                return;
            }

            wonTimer = 5.0f;

            int faction = players[connection].faction;
            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyWon, faction, player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }*/

        void BattleEnd()
        {
            wonTimer = 0;
            leaveTimer = 0;
            turnTimer = 0;

            roomstate = 0;
            foreach (Player player in players.Values)
            {
                player.ready = false;
            }

            RoomState state = new RoomState();
            state.roomid = roomid;
            foreach (Player player in players.Values)
            {
                state.players.Add(new PlayerState { userid = player.userid, username = player.username, ready = player.ready });
            }

            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.BattleEnd, player.connection, NetDeliveryMethod.ReliableOrdered);
                GSrv.Send(MessageType.ReplyRoomState, GSrv.Serialize<RoomState>(state), player.connection, NetDeliveryMethod.ReliableOrdered);
            }

            Program.SendLobbyStateToAll();
        }

        public void Disconnect(NetConnection connection)
        {
            Console.WriteLine("Leave:" + players[connection].username);
            int userid = players[connection].userid;
            players.Remove(connection);

            if (roomstate == 0)
            {
                SendRoomStateToAll();
                CheckBattleStart();
                Program.SendLobbyStateToAll();
            }
            else
            {
                //roomstate = 0;
                leaveTimer = 5.0f;
                turnTimer = 0;
                foreach (Player player in players.Values)
                {
                    GSrv.Send(MessageType.LeavePlayer, userid, player.connection, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void DeclareCard(NetConnection connection, DeclareCardData dec)
        {
            if(IsTimerOn())
            {
                return;
            }

            if (players[connection].userid != turn.turnuserid)
            {
                return;
            }

            if (players.Values.FirstOrDefault(x => x.userid == dec.userid) == null || dec.userid == turn.turnuserid)
            {
                return;
            }

            if(dec.deccardtype < 0 || 8 <= dec.deccardtype)
            {
                return;
            }

            if(turn.directions.Count == 0)
            {
                if (!players[connection].cards.Contains(dec.cardtype))
                {
                    return;
                }

                turn.selectCard = dec.cardtype;
                players[connection].cards.Remove(dec.cardtype);
                foreach(Player player in players.Values)
                {
                    if(player.connection == connection)
                    {
                        GSrv.Send(MessageType.RemoveCardType, dec.cardtype, player.connection, NetDeliveryMethod.ReliableOrdered);
                    }
                    else
                    {
                        GSrv.Send(MessageType.RemoveCard, player.connection, NetDeliveryMethod.ReliableOrdered);
                    }
                }
                turn.turnuserid = dec.userid;
                turn.directions.Add(new TurnDirection { srcuserid = players[connection].userid, destuserid = dec.userid, cardtype = dec.deccardtype });

                SendTurnDataToAll();
            }
            else
            {
                if(turn.directions.Count == players.Count - 1)
                {
                    return;
                }
                if(!turn.pass)
                {
                    return;
                }

                turn.pass = false;

                turn.turnuserid = dec.userid;
                turn.directions.Add(new TurnDirection { srcuserid = players[connection].userid, destuserid = dec.userid, cardtype = dec.deccardtype });

                SendTurnDataToAll();
            }
        }

        bool IsTimerOn()
        {
            return (wonTimer > 0 || leaveTimer > 0 || turnTimer > 0);
        }

        void SendTurnDataToAll()
        {
            foreach(Player player in players.Values)
            {
                GSrv.Send(MessageType.SendTrunData, GSrv.Serialize<TurnData>(turn), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void RequestPass(NetConnection connection)
        {
            if (IsTimerOn())
            {
                return;
            }

            if (players[connection].userid != turn.turnuserid)
            {
                return;
            }

            if (turn.directions.Count == 0)
            {
                return;
            }            

            if (turn.directions.Count == players.Count - 1)
            {
                return;
            }

            if (turn.pass)
            {
                return;
            }

            turn.pass = true;
            GSrv.Send(MessageType.ReplyPass, turn.selectCard, connection, NetDeliveryMethod.ReliableOrdered);
        }

        public void RequestTrueOrLie(NetConnection connection, bool declare)
        {
            if (IsTimerOn())
            {
                return;
            }

            if (players[connection].userid != turn.turnuserid)
            {
                return;
            }

            if (turn.directions.Count == 0)
            {
                return;
            }

            if (turn.pass)
            {
                return;
            }

            TrueOrLieData data = new TrueOrLieData { declare = declare, cardtype = turn.selectCard };
            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyTrueOrLie, GSrv.Serialize<TrueOrLieData>(data), player.connection, NetDeliveryMethod.ReliableOrdered);
            }

            Player penPlayer;
            if (declare)
            {
                if(turn.directions.Last<TurnDirection>().cardtype == turn.selectCard)
                {
                    int userid = turn.directions.Last<TurnDirection>().srcuserid;
                    penPlayer = players.Values.FirstOrDefault(x => x.userid == userid);
                    penPlayer.penalties[turn.selectCard]++;
                    turn.turnuserid = penPlayer.userid;
                }
                else
                {
                    penPlayer = players[connection];
                    penPlayer.penalties[turn.selectCard]++;
                }
            }
            else
            {
                if (turn.directions.Last<TurnDirection>().cardtype != turn.selectCard)
                {
                    int userid = turn.directions.Last<TurnDirection>().srcuserid;
                    penPlayer = players.Values.FirstOrDefault(x => x.userid == userid);
                    penPlayer.penalties[turn.selectCard]++;
                    turn.turnuserid = penPlayer.userid;
                }
                else
                {
                    penPlayer = players[connection];
                    penPlayer.penalties[turn.selectCard]++;
                }
            }

            turn.selectCard = -1;
            turn.directions.Clear();

            if(CheckGameEnd(penPlayer))
            {
                wonTimer = 5.0f;
            }
            else
            {
                turnTimer = 5.0f;
            }
        }

        bool CheckGameEnd(Player player)
        {
            if(player.cards.Count == 0)
            {
                return true;
            }

            foreach(int num in player.penalties)
            {
                if(num >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        public void SendMessage(NetConnection connection, int index)
        {
            RoarData roar = new RoarData { userid = players[connection].userid, roartype = index };

            foreach(Player player in players.Values)
            {
                GSrv.Send(MessageType.BroadcastMessage, GSrv.Serialize<RoarData>(roar), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
