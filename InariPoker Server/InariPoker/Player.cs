using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace InariPoker
{
    public class Player
    {
        static int userCount = 0;
        public int userid { get; private set; }
        public string username { get; private set; }
        public NetConnection connection { get; private set; }

        //lobby
        public bool inLobby = true;
        public int roomID;

        //room
        public bool ready;

        //game
        public List<int> cards = new List<int>();
        public int[] penalties = new int[8];

        public Player(NetConnection connection, string username)
        {
            userid = userCount++;
            this.username = username;
            this.connection = connection;
        }

        public void ResetPenalties()
        {
            for(int i=0; i<penalties.Length; i++)
            {
                penalties[i] = 0;
            }
        }
    }
}
