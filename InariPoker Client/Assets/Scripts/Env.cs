using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YasGameLib
{
    public static class Env
    {
        static Random rand;
        static List<int> cards = new List<int>();

        static void Init()
        {
            cards.Clear();

            for(int i=0; i<8; i++)
            {
                for(int j=0; j<8; j++)
                {
                    cards.Add(i);
                }
            }

            rand = new Random();
        }

        static int GetRandomCard()
        {
            int index = rand.Next(cards.Count);
            int ret = cards[index];
            cards.RemoveAt(index);
            return ret;
        }

        public static List<List<int>> CreateRandomCards(int playerNum)
        {
            Init();

            List<List<int>> ret = new List<List<int>>();

            int cardNum = 64 / playerNum;

            for(int i=0; i<playerNum; i++)
            {
                List<int> cards = new List<int>();
                for(int j=0; j<cardNum; j++)
                {
                    cards.Add(GetRandomCard());
                }
                cards.Sort();
                ret.Add(cards);
            }

            return ret;
        }

        public static string ConvertCardName(int cardtype)
        {
            switch (cardtype)
            {
                case 0:
                    return "いなり";
                case 1:
                    return "ビール";
                case 2:
                    return "ステーキ";
                case 3:
                    return "ラーメン";
                case 4:
                    return "アイスティー";
                case 5:
                    return "カメラ";
                case 6:
                    return "免許";
                default:
                    return "車";
            }

        }
    }
}
