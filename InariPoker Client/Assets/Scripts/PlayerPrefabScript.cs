using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace InariPoker
{
    public class PlayerPrefabScript : MonoBehaviour
    {
        [SerializeField]
        Text username;
        int userid;
        List<GameObject> bottomCards = new List<GameObject>();
        List<GameObject> smallTexts = new List<GameObject>();

        [SerializeField]
        GameObject coin, speak;
        [SerializeField]
        AudioSource source;

        [SerializeField]
        GameObject balloon, circle, cross;
        [SerializeField]
        Text balloonText;
        [SerializeField]
        AudioSource balloonSource;

        [SerializeField]
        GameObject defeat;

        public void Init(int userid, string username)
        {
            this.userid = userid;
            this.username.text = username;
        }

        void ClearBigOrMediumCard()
        {
            foreach(GameObject card in bottomCards)
            {
                Destroy(card);
            }
            bottomCards.Clear();
        }

        public void SetBigCard(List<int> cards)
        {
            ClearBigOrMediumCard();

            for (int i = 0; i < cards.Count; i++)
            {
                GameObject card = (GameObject)Instantiate(Resources.Load("BigCard"), gameObject.transform);
                card.GetComponent<Image>().sprite = Resources.Load<Sprite>("0" + cards[i].ToString());
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2((cards.Count / 2f) * -40f + 20f + i * 40f, -120f);
                card.GetComponent<BigCardScript>().Init(i);
                this.bottomCards.Add(card);
            }
        }

        public void SetMediumCard(int cardsum)
        {
            ClearBigOrMediumCard();

            for (int i = 0; i < cardsum; i++)
            {
                GameObject card = (GameObject)Instantiate(Resources.Load("MediumCard"), gameObject.transform);
                card.GetComponent<Image>().sprite = Resources.Load<Sprite>("back");
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(((cardsum / 2f) * -20f + 10f + i * 20f) * 0.25f, -100f);
                this.bottomCards.Add(card);
            }
        }

        void CreateTextUI()
        {
            for(int x=0; x<2; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    GameObject card = (GameObject)Instantiate(Resources.Load("SmallCard"), gameObject.transform);
                    card.GetComponent<Image>().sprite = Resources.Load<Sprite>("0" + (x*4 + y).ToString());
                    card.GetComponent<RectTransform>().anchoredPosition = new Vector2(60 + x * 30, 60 + y * -24);

                    GameObject text = (GameObject)Instantiate(Resources.Load("SmallText"), gameObject.transform);
                    text.GetComponent<RectTransform>().anchoredPosition = new Vector2(75 + x * 30, 60 + y * -24);
                    smallTexts.Add(text);
                }
            }
        }

        public void SetSmallCard(int[] smallCards)
        {
            if(smallTexts.Count == 0)
            {
                CreateTextUI();
            }

            for(int i=0; i<smallCards.Length; i++)
            {
                smallTexts[i].GetComponent<Text>().text = "×" + smallCards[i].ToString();
            }
        }

        public void AvatorClick()
        {
            if(userid == Players.userid)
            {
                return;
            }

            Players.SelectOther(userid);
        }

        void Update()
        {
            if(source.isPlaying)
            {
                speak.SetActive(true);
            }
            else
            {
                speak.SetActive(false);
            }
        }

        public void SetTurnCoin(bool active)
        {
            coin.SetActive(active);
        }

        public void CreateBalloon(string text)
        {
            balloon.SetActive(true);
            balloonText.text = text;
        }

        public void ClearBalloon()
        {
            circle.SetActive(false);
            cross.SetActive(false);
            balloon.SetActive(false);
        }

        public void SetCorrect()
        {
            circle.SetActive(true);
            balloonSource.clip = Resources.Load<AudioClip>("Correct");
            balloonSource.Play();
        }

        public void SetWrong()
        {
            cross.SetActive(true);
            balloonSource.clip = Resources.Load<AudioClip>("Wrong");
            balloonSource.Play();
        }

        public void SelectBottomCard(int index)
        {
            bottomCards[index].GetComponent<RectTransform>().anchoredPosition = bottomCards[index].GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 20);
        }

        public void ResetBottomCard()
        {
            foreach(GameObject card in bottomCards)
            {
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(card.GetComponent<RectTransform>().anchoredPosition.x, -120f);
            }
        }

        public void SetDefeat()
        {
            defeat.SetActive(true);
        }

        public void SetRoar(int index)
        {
            source.Stop();
            source.clip = Resources.Load<AudioClip>(Roars.GetText(index));
            source.Play();
        }
    }
}
