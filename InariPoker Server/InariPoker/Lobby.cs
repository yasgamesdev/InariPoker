using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YasGameLib;

namespace InariPoker
{
    public static class Lobby
    {
        static Dictionary<NetConnection, Player> caches = new Dictionary<NetConnection, Player>();
        static Dictionary<NetConnection, Player> players = new Dictionary<NetConnection, Player>();
        static List<Room> rooms = new List<Room>();

        public static void Init()
        {
            for(int i=0; i<Env.RoomNum; i++)
            {
                rooms.Add(new Room(i));
            }
        }

        public static bool AuthDone(NetConnection connection)
        {
            return caches.ContainsKey(connection);
        }

        public static int AddPlayer(NetConnection connection, string username)
        {
            Player player = new Player(connection, username);

            players.Add(connection, player);
            caches.Add(connection, player);

            return player.userid;
        }

        public static LobbyState GetLobbyState()
        {
            LobbyState state = new LobbyState();
            foreach(Room room in rooms)
            {
                state.summaries.Add(room.GetRoomSummary());
            }

            return state;
        }

        public static bool InLobby(NetConnection connection)
        {
            return caches[connection].inLobby;
        }

        public static int GetPlayerNum(int roomid)
        {
            return rooms[roomid].GetPlayerNum();
        }

        public static void EnterRoom(NetConnection connection, int roomid)
        {
            Player player = players[connection];
            players.Remove(connection);

            player.inLobby = false;
            player.roomID = roomid;

            rooms[roomid].EnterRoom(player);
        }

        public static void SendRoomStateToAll(int roomid)
        {
            rooms[roomid].SendRoomStateToAll();
        }

        public static int GetRoomState(int roomid)
        {
            return rooms[roomid].GetRoomState();
        }

        public static void SendLobbyStateToAll(LobbyState state)
        {
            foreach(Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public static int GetRoomID(NetConnection connection)
        {
            return caches[connection].roomID;
        }

        public static void LeaveRoom(NetConnection connection, int roomid)
        {
            Player player = rooms[roomid].LeaveRoom(connection);
            player.inLobby = true;
            players.Add(connection, player);
        }

        public static void ChangeReady(NetConnection connection, int roomid)
        {
            rooms[roomid].ChangeReady(connection);
        }

        public static bool CheckBattleStart(int roomid)
        {
            return rooms[roomid].CheckBattleStart();
        }

        public static void Update(float delta)
        {
            foreach (Room room in rooms)
            {
                room.Update(delta);
            }
        }

        public static void Disconnect(NetConnection connection)
        {
            if (Lobby.InLobby(connection))
            {
                Console.WriteLine("Leave:" + players[connection].username);
                players.Remove(connection);
            }
            else
            {
                int roomid = Lobby.GetRoomID(connection);
                rooms[roomid].Disconnect(connection);
            }

            caches.Remove(connection);
        }

        public static void DeclareCard(NetConnection connection, int roomid, DeclareCardData dec)
        {
            rooms[roomid].DeclareCard(connection, dec);
        }

        public static void RequestPass(NetConnection connection, int roomid)
        {
            rooms[roomid].RequestPass(connection);
        }

        public static void RequestTrueOrLie(NetConnection connection, int roomid, bool declare)
        {
            rooms[roomid].RequestTrueOrLie(connection, declare);
        }

        public static void SendMessage(NetConnection connection, int roomid, int index)
        {
            rooms[roomid].SendMessage(connection, index);
        }
    }
}
