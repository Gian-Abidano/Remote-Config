using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] Image avatarImage;
    [SerializeField] Sprite[] avatarSprites;

    public void Set(Photon.Realtime.Player player)
    {
        if(player.CustomProperties.TryGetValue(PropertyNames.Player.AvatarIndex, out var value))
        {
            avatarImage.sprite = avatarSprites[(int)value];
        }
        playerNameText.text = player.NickName;

        if(player == PhotonNetwork.MasterClient)
        {
            playerNameText.text = player.NickName + " <b> (Master) </b>";
        }

        if (player.IsLocal && !player.IsMasterClient)
        {
            playerNameText.text += " <b> (You) </b>";
        }
    }
}
