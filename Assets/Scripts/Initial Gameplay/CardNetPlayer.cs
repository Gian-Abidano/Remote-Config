using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

public class CardNetPlayer : MonoBehaviourPun
{
    public static List<CardNetPlayer> NetPlayers = new List<CardNetPlayer>(2);
    private CardPlayer cardPlayer;
    private Card[] cards;

    public void Set(CardPlayer player)
    {
        player.NickName.text = photonView.Owner.NickName;
        cardPlayer = player;
        cards = player.GetComponentsInChildren<Card>();
        foreach (var card in cards)
        {
            var button = card.GetComponent<Button>();
            button.onClick.AddListener(()=>RemoteClickButton(card.attackValue));
            
            if(photonView.IsMine==false) 
            {
                button.interactable=false;
            }
        }
    }

    private void OnDestroy()
    {
        
    }

    private void RemoteClickButton(Attack value)
    {
        if(photonView.IsMine)
            photonView.RPC("RemoteClickButtonRPC",RpcTarget.Others, (int) value);
    }

    [PunRPC]
    private void RemoteClickButtonRPC(int value)
    {
        foreach (var card in cards)
        {
            if(card.attackValue == (Attack) value)
            {
                var button = card.GetComponent<Button>();
                button.onClick.Invoke();
                break;
            }
        }
    }
    private void OnEnable() 
    {
        NetPlayers.Add(this);
    }

    private void OnDisable() 
    {
        NetPlayers.Remove(this);
        foreach (var card in cards)
        {
            var button = card.GetComponent<Button>();
            button.onClick.RemoveListener(()=>RemoteClickButton(card.attackValue));
        }
    }
}
