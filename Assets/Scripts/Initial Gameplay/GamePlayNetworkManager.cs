using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayNetworkManager : MonoBehaviourPunCallbacks
{
    public void BackToMenu()
    {
        StartCoroutine(BackToMenuCR());
    }

    IEnumerator BackToMenuCR()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
            yield return null;
        
        SceneManager.LoadScene("Main Menu");
    }
    
    public void BackToLobby()
    {
        StartCoroutine(BackToLobbyCR());
    }

    IEnumerator BackToLobbyCR()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom || PhotonNetwork.IsConnectedAndReady==false)
            yield return null;
        
        SceneManager.LoadScene("Lobby");
    }
    
    public void Replay()
    {
        if(PhotonNetwork.IsMasterClient) {
            var scene = SceneManager.GetActiveScene();
            PhotonNetwork.LoadLevel(scene.name);
        }
    }
    
    public void Quit()
    {
        StartCoroutine(QuitCR());
    }

    IEnumerator QuitCR()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
            yield return null;
        
        Application.Quit();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) 
    {
        Debug.Log("PhotonNetwork.CurrentRoom.PlayerCount = " + PhotonNetwork.CurrentRoom.PlayerCount);
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("Tes jalan (Masuk) ");
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            BackToLobby();
            Debug.Log("Lewat ga?");
        }
    }
}
