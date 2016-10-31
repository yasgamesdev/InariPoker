using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InariPoker
{
    class OtherPlayer : PlayerBase
    {
        public OtherPlayerInitData init { get; private set; }
        float f;

        public OtherPlayer(OtherPlayerInitData init, float f)
        {
            this.init = init;
            this.f = f;

            prefab = (GameObject)GameObject.Instantiate(Resources.Load("Player"), GameObject.Find("Players").transform);
            anchorPos = new Vector2(Mathf.Cos(Mathf.PI * 2f * 0.75f - f), Mathf.Sin(Mathf.PI * 2f * 0.75f - f)) * 200;
            prefab.GetComponent<RectTransform>().anchoredPosition = anchorPos;

            prefab.GetComponent<PlayerPrefabScript>().Init(init.userid, init.username);
            prefab.GetComponent<PlayerPrefabScript>().SetMediumCard(init.cardsum);

            prefab.GetComponent<PlayerPrefabScript>().SetSmallCard(smallCards);
        }

        public void RemoveCard()
        {
            init.cardsum--;
            prefab.GetComponent<PlayerPrefabScript>().SetMediumCard(init.cardsum);
        }
        /*PrefabManager prefab, name;

        public OtherPlayer(PlayerInitData init) : base(init)
        {
            prefab = new PrefabManager();
            prefab.LoadPrefab("OtherPlayer");
            prefab.GetInstance().GetComponent<OtherPlayerPrefabScript>().Init(init);

            if(Players.GetPlayer().GetFaction() == init.faction)
            {
                name = new PrefabManager();
                name.LoadPrefab("NamePlate", GameObject.Find("Canvas").transform);
                name.GetInstance().GetComponent<NamePlateScript>().Init(prefab.GetInstance().transform, init.username);
            }
        }

        public void Update(float delta)
        {
            if (init.sync.hp != 0)
            {
                prefab.GetInstance().GetComponent<OtherPlayerPrefabScript>().Interpolation(init.sync, delta);

                if(name != null)
                {
                    Vector3 dir = prefab.GetInstance().transform.position - Players.GetPlayer().GetPosition();
                    if (Vector3.Dot(Players.GetPlayer().GetForward(), dir) >= 0)
                    {
                        name.GetInstance().SetActive(true);
                    }
                    else
                    {
                        name.GetInstance().SetActive(false);
                    }                    
                }
            }
        }

        public void Destroy()
        {
            prefab.Destroy();
            if (name != null)
            {
                name.Destroy();
            }
        }

        public override void ManageDiff()
        {
            if(init.sync.hp == 0 && previous.hp != 0)
            {
                prefab.ReloadPrefab("OtherPlayerDead");
                prefab.GetInstance().transform.position = new Vector3(init.sync.xpos, init.sync.ypos, init.sync.zpos);
                prefab.GetInstance().transform.localEulerAngles = new Vector3(0, init.sync.yrot, 0);

                if (name != null)
                {
                    name.Destroy();
                }
            }
            else if(init.sync.hp != 0 && previous.hp == 0)
            {
                prefab = new PrefabManager();
                prefab.LoadPrefab("OtherPlayer");
                prefab.GetInstance().GetComponent<OtherPlayerPrefabScript>().Init(init);

                if (Players.GetPlayer().GetFaction() == init.faction)
                {
                    name = new PrefabManager();
                    name.LoadPrefab("NamePlate", GameObject.Find("Canvas").transform);
                    name.GetInstance().GetComponent<NamePlateScript>().Init(prefab.GetInstance().transform, init.username);
                }
            }
        }

        public int GetUserID()
        {
            return init.sync.userid;
        }

        public void ReplyFire()
        {
            if(init.sync.hp != 0)
            {
                prefab.GetInstance().GetComponent<OtherPlayerPrefabScript>().Fire();
            }
        }*/
    }
}
