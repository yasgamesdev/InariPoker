using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YasGameLib;

namespace InariPoker
{
    class Program
    {
        static Task task;
        static bool exit = false;
        const float frameSpan = 1.0f / 30.0f;
        //static int sendCount = 0;

        static int port;
        static string sqlpath;

        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory() + "/Setting/setting.txt";
            if (!ReadFile(path))
            {
                return;
            }

            task = Task.Run(() => {
                Server();
            });

            while (true)
            {
                Console.WriteLine(@"type exit for close server");
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }
            }
            exit = true;

            task.Wait();
        }

        static void Server()
        {
            Lobby.Init();

            //GSQLite.Open(sqlpath);            

            GSrv.Init();
            GSrv.SetConnectPacketHandler(ConnectHandler);
            GSrv.SetDisconnectPacketHandler(DisconnectHandler);
            GSrv.SetDebugPacketHandler(DebugHandler);
            GSrv.SetPacketHandler(MessageType.SendUserName, DataType.String, SendUserNameHandler);
            //GSrv.SetPacketHandler(MessageType.RequestLobbyState, DataType.Null, RequestLobbyStateHandler);
            GSrv.SetPacketHandler(MessageType.EnterRoom, DataType.Int32, EnterRoomHandler);
            GSrv.SetPacketHandler(MessageType.LeaveRoom, DataType.Null, LeaveRoomHandler);
            GSrv.SetPacketHandler(MessageType.ChangeReady, DataType.Null, ChangeReadyHandler);
            GSrv.SetPacketHandler(MessageType.DeclareCard, DataType.Bytes, DeclareCardHandler);
            GSrv.SetPacketHandler(MessageType.RequestPass, DataType.Null, RequestPassHandler);
            GSrv.SetPacketHandler(MessageType.RequestTrueOrLie, DataType.Int32, RequestTrueOrLieHandler);
            GSrv.SetPacketHandler(MessageType.SendMessage, DataType.Int32, SendMessageHandler);

            GSrv.Listen("InariPoker0.1", port);

            while (!exit)
            {
                DateTime startTime = DateTime.Now;

                GSrv.Receive();

                /*sendCount++;
                
                if (sendCount == 3)
                {
                    sendCount = 0;

                    SendSnapshot();
                }*/

                Update(frameSpan);

                TimeSpan span = DateTime.Now - startTime;
                if (span.TotalMilliseconds < frameSpan * 1000)
                {
                    Thread.Sleep((int)(frameSpan * 1000) - (int)span.TotalMilliseconds);
                }
            }

            GSrv.Shutdown();
            /*Players.SaveAllPlayer();
            GSQLite.Close();*/
        }

        /*static void SendSnapshot()
        {
            Lobby.SendSnapShot();
        }*/

        static void Update(float delta)
        {
            Lobby.Update(delta);
        }

        static bool ReadFile(string path)
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

                        if (line.StartsWith("Port="))
                        {
                            string sub = line.Substring("Port=".Length);
                            if (!int.TryParse(sub, out port))
                            {
                                throw new Exception();
                            }
                        }
                        else if (line.StartsWith("SQLite="))
                        {
                            string sub = line.Substring("SQLite=".Length);
                            sqlpath = Directory.GetCurrentDirectory() + "/SQLite/" + sub;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        static public void ConnectHandler(NetConnection connection, object data)
        {
            
        }

        static public void DisconnectHandler(NetConnection connection, object data)
        {
            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            Lobby.Disconnect(connection);
        }

        static public void DebugHandler(NetConnection connection, object data)
        {
            Console.WriteLine((string)data);
        }

        static public void SendUserNameHandler(NetConnection connection, object data)
        {
            string username = (string)data;
            if(username == "")
            {
                username = "野獣先輩";
            }

            if(Lobby.AuthDone(connection))
            {
                return;
            }

            Console.WriteLine("Enter:" + username);

            int userid = Lobby.AddPlayer(connection, username);
            GSrv.Send(MessageType.ReplyUserID, userid, connection, NetDeliveryMethod.ReliableOrdered);
            LobbyState state = Lobby.GetLobbyState();
            GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), connection, NetDeliveryMethod.ReliableOrdered);
        }

        /*static public void RequestLobbyStateHandler(NetConnection connection, object data)
        {
            LobbyState state = Lobby.GetLobbyState();
            GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), connection, NetDeliveryMethod.ReliableOrdered);
        }*/

        static public void EnterRoomHandler(NetConnection connection, object data)
        {
            int roomid = (int)data;
            if(roomid < 0 || Env.RoomNum <= roomid)
            {
                return;
            }

            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (!Lobby.InLobby(connection))
            {
                return;
            }

            if (Lobby.GetRoomState(roomid) == 1 || Lobby.GetPlayerNum(roomid) >= 6)
            {
                return;
            }

            Lobby.EnterRoom(connection, roomid);

            GSrv.Send(MessageType.ReplySuccessEnterRoom, connection, NetDeliveryMethod.ReliableOrdered);

            Lobby.SendRoomStateToAll(roomid);

            SendLobbyStateToAll();
        }

        public static void SendLobbyStateToAll()
        {
            LobbyState state = Lobby.GetLobbyState();

            Lobby.SendLobbyStateToAll(state);
        }

        static public void LeaveRoomHandler(NetConnection connection, object data)
        {
            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (Lobby.InLobby(connection))
            {
                return;
            }

            int roomid = Lobby.GetRoomID(connection);

            if(Lobby.GetRoomState(roomid) == 1)
            {
                return;
            }

            Lobby.LeaveRoom(connection, roomid);
            Lobby.SendRoomStateToAll(roomid);

            Lobby.CheckBattleStart(roomid);

            GSrv.Send(MessageType.ReplySuccessLeaveRoom, connection, NetDeliveryMethod.ReliableOrdered);
            //LobbyState state = Lobby.GetLobbyState();
            //GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), connection, NetDeliveryMethod.ReliableOrdered);

            SendLobbyStateToAll();
        }

        static public void ChangeReadyHandler(NetConnection connection, object data)
        {
            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (Lobby.InLobby(connection))
            {
                return;
            }

            int roomid = Lobby.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 1)
            {
                return;
            }

            Lobby.ChangeReady(connection, roomid);
            Lobby.SendRoomStateToAll(roomid);

            if(Lobby.CheckBattleStart(roomid))
            {
                SendLobbyStateToAll();
            }
        }

        static public void DeclareCardHandler(NetConnection connection, object data)
        {
            DeclareCardData dec = null;
            try
            {
                dec = GSrv.Deserialize<DeclareCardData>((byte[])data);
            }
            catch
            {
                return;
            }

            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (Lobby.InLobby(connection))
            {
                return;
            }

            int roomid = Lobby.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.DeclareCard(connection, roomid, dec);
        }

        static public void RequestPassHandler(NetConnection connection, object data)
        {
            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (Lobby.InLobby(connection))
            {
                return;
            }

            int roomid = Lobby.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.RequestPass(connection, roomid);
        }

        static public void RequestTrueOrLieHandler(NetConnection connection, object data)
        {
            int value = (int)data;

            bool declare = value != 0 ? true : false;

            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (Lobby.InLobby(connection))
            {
                return;
            }

            int roomid = Lobby.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.RequestTrueOrLie(connection, roomid, declare);
        }

        static public void SendMessageHandler(NetConnection connection, object data)
        {
            int index = (int)data;

            if(index < 0 || 51 <= index)
            {
                return;
            }

            if (!Lobby.AuthDone(connection))
            {
                return;
            }

            if (Lobby.InLobby(connection))
            {
                return;
            }

            int roomid = Lobby.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.SendMessage(connection, roomid, index);
        }
    }
}
