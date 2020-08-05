using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Custom Variables")]
    public InputField playerNameField;
    public InputField roomNameField;

    string gameVersion = "1";

    private string playerName;
    private string roomName;

    public Text connectionStatus;
    public Text playStatus;
    public Text playerStatus;
    public Text playerCount;

    public Toggle redTeamToggle;
    public Toggle blueTeamToggle;

    public GameObject infoInputUI;
    public GameObject roomJoinUI;
    public GameObject buttonLoadArena;

    public Text redPlayerList;
    public Text bluePlayerList;
    public Text playerList;

    public Image spinner;
    private float spinnerSpeed = 50;

    // Start is called before the first frame update
    void Start()
    {
        //1
        PlayerPrefs.DeleteAll();

        Debug.Log("Connecting to Photon Network");

        //2
        infoInputUI.SetActive(false);
        roomJoinUI.SetActive(false);

        spinner.enabled = true;

        //3
        if (PhotonNetwork.IsConnected)
        {
            OnConnected();
        }
        else
        {
            ConnectToPhoton();
        }
    }

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            int players = PhotonNetwork.CurrentRoom.PlayerCount;
            playerCount.text = "Current players: " + players.ToString() + "/8";

            playerList.text = "";
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                playerList.text += player.NickName + "\n";
            }
        }
        if (spinner.IsActive())
        {
            spinner.transform.Rotate(0, 0, spinnerSpeed * Time.deltaTime);
        }
    }

    // Helper Methods
    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SetRoomName(string name)
    {
        roomName = name;
    }

    public void SetTeamRed()
    {
        if (redTeamToggle.isOn)
        {
            gameObject.GetComponent<TeamSettings>().photonView.RPC("TeamJoinRequest", RpcTarget.MasterClient, TeamSettings.Team.red);
        }
        else
        {
            gameObject.GetComponent<TeamSettings>().photonView.RPC("RemovePlayerFromTeam", RpcTarget.All, TeamSettings.Team.red, PhotonNetwork.LocalPlayer);
        }
    }

    public void SetTeamBlue()
    {
        if (blueTeamToggle.isOn)
        {
            gameObject.GetComponent<TeamSettings>().photonView.RPC("TeamJoinRequest", RpcTarget.MasterClient, TeamSettings.Team.blue);
        }
        else
        {
            gameObject.GetComponent<TeamSettings>().photonView.RPC("RemovePlayerFromTeam", RpcTarget.All, TeamSettings.Team.blue, PhotonNetwork.LocalPlayer);
        }
    }

    [PunRPC]
    public void TeamAccessDenied(TeamSettings.Team team)
    {
        playStatus.text = team.ToString() + " team has reached maximum number of players";
        redTeamToggle.isOn = false;
        blueTeamToggle.isOn = false;
    }

    public void UpdateTeamUI()
    {
        redPlayerList.text = "";
        bluePlayerList.text = "";
        foreach (Player player in TeamSettings.redTeamPlayers)
        {
            redPlayerList.text = redPlayerList.text + player.NickName + "\n";
        }

        foreach (Player player in TeamSettings.blueTeamPlayers)
        {
            bluePlayerList.text = bluePlayerList.text + player.NickName + "\n";
        }
    }

    public void JoinRoom()
    {
        SetPlayerName(playerNameField.text);
        SetRoomName(roomNameField.text);
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = playerName; //1
            Debug.Log("PhotonNetwork.IsConnected! | Trying to Create/Join Room " + roomNameField.text);
            RoomOptions roomOptions = new RoomOptions(); //2
            roomOptions.MaxPlayers = 8;
            TypedLobby typedLobby = new TypedLobby(roomName, LobbyType.Default); //3
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby); //4
            infoInputUI.SetActive(false);
            spinner.enabled = true;
        }
    }

    public void LeaveRoom()
    {
        gameObject.GetComponent<TeamSettings>().photonView.RPC("RemovePlayerFromTeam", RpcTarget.All, TeamSettings.myTeam, PhotonNetwork.LocalPlayer);
        redTeamToggle.isOn = false;
        blueTeamToggle.isOn = false;
        PhotonNetwork.LeaveRoom();
    }

    public void LoadGame()
    {
        //When load button is pressed, load the level and close the room to new entrants
        if (TeamSettings.redTeamPlayers.Count + TeamSettings.blueTeamPlayers.Count < PhotonNetwork.PlayerList.Length)
        {
            playStatus.text = "Not all players have joined a team";
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            PhotonNetwork.LoadLevel("Game");
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        else
        {
            playStatus.text = "Minimum 2 players required to start game";
        }
    }

    void ConnectToPhoton()
    {
        connectionStatus.text = "Connecting...";
        PhotonNetwork.GameVersion = gameVersion; //1
        PhotonNetwork.ConnectUsingSettings(); //2
    }

    // Photon Methods
    public override void OnConnected()
    {
        base.OnConnected();

        connectionStatus.text = "Connected to Photon!";
        connectionStatus.color = Color.green;
        infoInputUI.SetActive(true);
        spinner.enabled = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        roomJoinUI.SetActive(false);
        connectionStatus.text = "Disconnected";
        connectionStatus.color = Color.red;
        Debug.LogError("Disconnected. Please check your internet connection.");
    }

    public override void OnJoinedRoom()
    {
        connectionStatus.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        roomJoinUI.SetActive(true);
        spinner.enabled = false;
        if (PhotonNetwork.IsMasterClient)
        {
            buttonLoadArena.SetActive(true);
            playerStatus.text = "You are Lobby Leader";
            TeamSettings.UpdateTeams();
        }
        else
        {
            buttonLoadArena.SetActive(false);
            playerStatus.text = "Connected to Lobby";
            TeamSettings.redTeamPlayers = new List<Player>((Player[])PhotonNetwork.MasterClient.CustomProperties[TeamSettings.Team.red]);
            TeamSettings.blueTeamPlayers = new List<Player>((Player[])PhotonNetwork.MasterClient.CustomProperties[TeamSettings.Team.blue]);
            UpdateTeamUI();
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        roomJoinUI.SetActive(false);
        infoInputUI.SetActive(true);
    }

}
