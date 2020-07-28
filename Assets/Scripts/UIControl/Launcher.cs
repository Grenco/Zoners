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

    private List<int> redTeam;
    private List<int> blueTeam;
    public Text redPlayerList;
    public Text bluePlayerList;
    public Text playerList;
    public static string team;

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


        redTeam = new List<int>();
        blueTeam = new List<int>();

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
        //4 
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
        spinner.transform.Rotate(0, 0, spinnerSpeed * Time.deltaTime);
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
        //If toggle is off, player will be removed from team
        if (redTeam.Count < 4 || !redTeamToggle.isOn)
        {
            if (redTeamToggle.isOn)
            {
                team = "red";
            }
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("AddToRedTeam", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else
        {
            playStatus.text = "Red team has reached maximum number of players";
            //redTeamToggle.isOn = false;
        }
    }

    public void SetTeamBlue()
    {
        //If toggle is off, player will be removed from team
        if (blueTeam.Count < 4 || !blueTeamToggle.isOn)
        {
            if (blueTeamToggle.isOn)
            {
                team = "blue";
            }
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("AddToBlueTeam", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else
        {
            playStatus.text = "Blue team has reached maximum number of players";
            //blueTeamToggle.isOn = false;
        }
    }

    public void UpdateTeamUI()
    {
        redPlayerList.text = "";
        bluePlayerList.text = "";
        foreach (int playerID in redTeam)
        {
            redPlayerList.text = redPlayerList.text + PhotonNetwork.PlayerList[playerID - 1].NickName + "\n";
        }

        foreach (int playerID in blueTeam)
        {
            bluePlayerList.text = bluePlayerList.text + PhotonNetwork.PlayerList[playerID - 1].NickName + "\n";
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
        if (team == "blue")
        {
            blueTeamToggle.isOn = false;
        }
        else if(team == "red")
        {
            redTeamToggle.isOn = false;
        }
        PhotonNetwork.LeaveRoom();
    }

    public void LoadGame()
    {
        //When load button is pressed, load the level and close the room to new entrants
        if (redTeam.Count + blueTeam.Count < PhotonNetwork.PlayerList.Length)
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
        // 1
        base.OnConnected();
        // 2
        connectionStatus.text = "Connected to Photon!";
        connectionStatus.color = Color.green;
        infoInputUI.SetActive(true);
        spinner.enabled = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // 3
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
            UpdateTeams();
        }
        else
        {
            buttonLoadArena.SetActive(false);
            playerStatus.text = "Connected to Lobby";
            redTeam = new List<int>((int[])PhotonNetwork.PlayerListOthers[0].CustomProperties["redTeam"]);
            blueTeam = new List<int>((int[])PhotonNetwork.PlayerListOthers[0].CustomProperties["blueTeam"]);
            UpdateTeamUI();
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        roomJoinUI.SetActive(false);
        infoInputUI.SetActive(true);
    }

    [PunRPC]
    private void AddToRedTeam(int playerID)
    {
        if (!redTeam.Contains(playerID))
        {
            redTeam.Add(playerID);
        }
        else
        {
            redTeam.Remove(playerID);
        }

        if (blueTeam.Contains(playerID))
        {
            blueTeam.Remove(playerID);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateTeams();
        }

        UpdateTeamUI();
    }

    [PunRPC]
    private void AddToBlueTeam(int playerID)
    {
        if (!blueTeam.Contains(playerID))
        {
            blueTeam.Add(playerID);
        }
        else
        {
            blueTeam.Remove(playerID);
        }

        if (redTeam.Contains(playerID))
        {
            redTeam.Remove(playerID);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateTeams();
        }

        UpdateTeamUI();
    }

    private void UpdateTeams()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("redTeam", redTeam.ToArray());
        properties.Add("blueTeam", blueTeam.ToArray());
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }
}
