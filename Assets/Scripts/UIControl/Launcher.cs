using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Custom Variables")]
    public InputField playerNameField;
    public InputField roomNameField;

    string gameVersion = "1";

    private string playerName;
    private string roomName;

    public Text connectionStatus;
    public Text roomStatus;
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

    public PhotonTeamsManager teamManager;

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

        DontDestroyOnLoad(teamManager.gameObject);

        PhotonTeamsManager.PlayerLeftTeam += OnPlayerLeftTeam;
        PhotonTeamsManager.PlayerJoinedTeam += OnPlayerJoinedTeam;
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
            //UpdateTeamUI();
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

    public void SetTeam(string team)
    {
        if (PhotonNetwork.LocalPlayer.GetPhotonTeam() != null && !redTeamToggle.isOn && !blueTeamToggle.isOn)
        {
            PhotonNetwork.LocalPlayer.LeaveCurrentTeam();
            return;
        }
        
        if (team == TeamSettings.redTeam && !redTeamToggle.isOn)
        {
            return;
        }
        else if (team == TeamSettings.blueTeam && !blueTeamToggle.isOn)
        {
            return;
        }
        else if (!blueTeamToggle.isOn && !redTeamToggle.isOn)
        {
            return;
        }

        if (teamManager.GetTeamMembersCount(team) < TeamSettings.maxPlayers)
        {
            if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)
            {
                PhotonNetwork.LocalPlayer.JoinTeam(team);
            }
            else
            {
                PhotonNetwork.LocalPlayer.SwitchTeam(team);
            }
        }
        else
        {
            TeamAccessDenied(team);
        }
    }

    public void TeamAccessDenied(string team)
    {
        playStatus.text = team + " team has reached maximum number of players";
        redTeamToggle.isOn = false;
        blueTeamToggle.isOn = false;
    }

    public void UpdateTeamUI()
    {
        redPlayerList.text = "";
        bluePlayerList.text = "";

        teamManager.TryGetTeamMembers(TeamSettings.redTeam, out Player[] redTeam);
        teamManager.TryGetTeamMembers(TeamSettings.blueTeam, out Player[] blueTeam);

        foreach (Player player in redTeam)
        {
            redPlayerList.text = redPlayerList.text + player.NickName + "\n";
        }

        foreach (Player player in blueTeam)
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
            roomStatus.text = "";
        }
    }

    public void LeaveRoom()
    {
        //gameObject.GetComponent<TeamSettings>().photonView.RPC("RemovePlayerFromTeam", RpcTarget.All, TeamSettings.myTeam, PhotonNetwork.LocalPlayer);
        redTeamToggle.isOn = false;
        blueTeamToggle.isOn = false;
        PhotonNetwork.LeaveRoom();
    }

    public void LoadGame()
    {
        //When load button is pressed, load the level and close the room to new entrants
        if (teamManager.GetTeamMembersCount(TeamSettings.redTeam) + teamManager.GetTeamMembersCount(TeamSettings.blueTeam) < PhotonNetwork.PlayerList.Length)
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


        if (PhotonNetwork.LocalPlayer.GetPhotonTeam() != null)
        {
            PhotonNetwork.LocalPlayer.LeaveCurrentTeam();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            buttonLoadArena.SetActive(true);
            playerStatus.text = "You are Lobby Leader";
        }
        else
        {
            buttonLoadArena.SetActive(false);
            playerStatus.text = "Connected to Lobby";
            UpdateTeamUI();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        spinner.enabled = false;
        infoInputUI.SetActive(true);
        roomStatus.text = "This room is not available to join at the minute.";
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        roomJoinUI.SetActive(false);
        infoInputUI.SetActive(true);
    }

    private void OnPlayerLeftTeam(Player player, PhotonTeam team)
    {
        UpdateTeamUI();
    }

    private void OnPlayerJoinedTeam(Player player, PhotonTeam team)
    {
        UpdateTeamUI();
    }
}
