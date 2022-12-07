using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CardGameManager : MonoBehaviour, IOnEventCallback
{
    public GameState State, NextState = GameState.ChooseAttackState;
    public GameObject netPlayerPrefab;
    public CardPlayer Player1;
    public CardPlayer Player2;
    public PlayerStats defaultPlayerStats = new PlayerStats()
    {
        MaxHealth=100,
        RestoreValue=5,
        DamageValue=10
    };
    public float restoreValue = 5;
    public float damageValue = 10;
    private CardPlayer damagedPlayer;
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public TMP_Text pingText;
    public bool Online = true;

    // public List <int> syncReadyPlayers = new List<int>(2);
    HashSet<int> syncReadyPlayers= new HashSet<int>();
    
    public enum GameState
    {
        SyncState,
        NetPlayersInitialization,
        ChooseAttackState,
        AttackingState,
        SuccessAttackState,
        DrawState,
        GameOver,
    }

    // Start is called before the first frame update
    void Start()
    {
        if(Online)
        {
            gameOverPanel.SetActive(false);
            PhotonNetwork.Instantiate(netPlayerPrefab.name, Vector3.zero, Quaternion.identity);
            StartCoroutine(PingCoroutine());
            State = GameState.NetPlayersInitialization;
            NextState = GameState.NetPlayersInitialization;

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PropertyNames.Room.RestoreValue, out var restoreValue)) {
                defaultPlayerStats.RestoreValue = (float)restoreValue;
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PropertyNames.Room.DamageValue, out var damageValue)) {
                defaultPlayerStats.DamageValue = (float)damageValue;
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PropertyNames.Room.MaxHealth, out var MaxHealth)) {
                Player1.stats.MaxHealth = (float)MaxHealth;
                Player2.stats.MaxHealth = (float)MaxHealth;
            }
        }
        else
        {
            State = GameState.ChooseAttackState;
        }

        
        Debug.Log("Debug 3");
        Player1.SetStats(defaultPlayerStats, true);
        Player2.SetStats(defaultPlayerStats, true);
        Player1.IsReady = true;
        Player2.IsReady = true;
        // Debug.Log(State);
    }

    IEnumerator PingCoroutine() {
        var wait = new WaitForSeconds(0.5f);
        while(true) {
            pingText.text = "Ping : " + PhotonNetwork.GetPing() + " ms";
            yield return wait;   
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case GameState.SyncState:
                if(syncReadyPlayers.Count == 2) 
                {
                    syncReadyPlayers.Clear();
                    State = NextState;
                }
                break;
            
            case GameState.NetPlayersInitialization:
                if(CardNetPlayer.NetPlayers.Count == 2)
                {
                    foreach (var netPlayer in CardNetPlayer.NetPlayers)
                    {
                        if(netPlayer.photonView.IsMine)
                            netPlayer.Set(Player1);
                        else
                            netPlayer.Set(Player2);
                    }
                    ChangeState(GameState.ChooseAttackState);
                }
                break;
            
            case GameState.ChooseAttackState:
                if(Player1.AttackValue != null && Player2.AttackValue != null)
                {
                    Player1.AnimateAttack();
                    Player2.AnimateAttack();
                    Player1.isClickable(false);
                    Player2.isClickable(false);
                    ChangeState(GameState.AttackingState);
                }
                break;

            case GameState.AttackingState:
                if(Player1.InAnimation() == false && Player2.InAnimation() == false)
                {
                    damagedPlayer = GetDamagePlayer();
                    if(damagedPlayer != null)
                    {
                        damagedPlayer.DamageAnimation();
                        ChangeState(GameState.SuccessAttackState);
                    }
                    else
                    {
                        Player1.DrawAnimation();
                        Player2.DrawAnimation();
                        ChangeState(GameState.DrawState);
                    }
                }
                break;

            case GameState.SuccessAttackState:
                if(Player1.InAnimation() == false && Player2.InAnimation() == false)
                {
                    if(damagedPlayer == Player1)
                    {
                        Player1.ChangingHealth(-Player2.stats.DamageValue);
                        Player2.ChangingHealth(Player2.stats.RestoreValue);
                    }
                    else
                    {
                        Player2.ChangingHealth(-Player1.stats.DamageValue);
                        Player1.ChangingHealth(Player1.stats.RestoreValue);
                    }
                

                    var winner = GetWinner();
                    
                    if(winner==null)
                    {
                        ResetPlayers();
                        Player1.isClickable(true);
                        Player2.isClickable(true);
                        ChangeState(GameState.ChooseAttackState);
                    }
                    else
                    {
                        gameOverPanel.SetActive(true);
                        winnerText.text = winner == Player1 ?
                             $"{Player1.NickName.text} Wins" : $"{Player2.NickName.text} Wins";
                        Debug.Log("The Winner is" + winner);
                        ChangeState(GameState.GameOver);
                        
                    }
                }
                break;

            case GameState.DrawState:
                if(Player1.InAnimation() == false && Player2.InAnimation() == false)
                {
                    ResetPlayers();
                    Player1.isClickable(true);
                    Player2.isClickable(true);
                    ChangeState(GameState.ChooseAttackState);
                }
                break;

        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private const byte playerChangeState=1;

    private void ChangeState(GameState newState)
    {
        if(Online == false)
        {
            State = newState;
            return;
        }

        if(this.NextState == newState)
            return;
        
        //kirim message bahwa player ready
        var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        var raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(1, actorNum, raiseEventOptions, SendOptions.SendReliable);
        
        //masuk ke state sync sebagai transisi sebelum state baru
        this.State =  GameState.SyncState;
        this.NextState = newState;
    }
    
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case playerChangeState:
                var actorNum = (int)photonEvent.CustomData;
                
                //kalau pake hashset ga perlu cek lagi
                syncReadyPlayers.Add(actorNum);

                break;

            default:
                break;
        }
    }

    private void ResetPlayers()
    {
        damagedPlayer = null;
        Player1.Reset();
        Player2.Reset();
    }

    private CardPlayer GetDamagePlayer()
    {
        Attack? Player1Atk = Player1.AttackValue;
        Attack? Player2Atk = Player2.AttackValue;

        if(Player1Atk == Attack.Rock && Player2Atk == Attack.Paper)
        {
            return Player1;
        }
        else if(Player1Atk == Attack.Rock && Player2Atk == Attack.Scissor)
        {
            return Player2;
        }
        else if(Player1Atk == Attack.Paper && Player2Atk == Attack.Scissor)
        {
            return Player1;
        }
        else if(Player1Atk == Attack.Paper && Player2Atk == Attack.Rock)
        {
            return Player2;
        }
        else if(Player1Atk == Attack.Scissor && Player2Atk == Attack.Rock)
        {
            return Player1;
        }
        else if(Player1Atk == Attack.Scissor && Player2Atk == Attack.Paper)
        {
            return Player2;
        }
        
        return null;
    }

    private CardPlayer GetWinner()
    {
        if(Player1.health==0)
        {
            return Player2;
        }
        else if(Player2.health==0)
        {
            return Player1;
        }
        else
            return null;
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting");
        Application.Quit();
    }
}
