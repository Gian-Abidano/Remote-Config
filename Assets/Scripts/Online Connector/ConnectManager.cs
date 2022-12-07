using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_Text feedbackText;

    private void Start()
    {
        usernameInput.text = PlayerPrefs.GetString(PropertyNames.Player.NickName, "");
    }

    public void ClickConnect()
    {
        feedbackText.text = "";
        if(usernameInput.text.Length <= 3)
        {
            feedbackText.text = "Username min. 4 characters";
            return;
        }

        PlayerPrefs.SetString(PropertyNames.Player.NickName, usernameInput.text);
        PhotonNetwork.NickName = usernameInput.text;
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.ConnectUsingSettings();
        feedbackText.text = "Connecting...";

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        feedbackText.text = "Connected to Master";
        StartCoroutine(LoadLevelAfterConnectedAndReady());
        // SceneManager.LoadScene("Lobby");
    }

    IEnumerator LoadLevelAfterConnectedAndReady() {
        while (PhotonNetwork.IsConnectedAndReady == false)
        {
            yield return null;
        }
        PhotonNetwork.LoadLevel("Lobby");
    }
}
