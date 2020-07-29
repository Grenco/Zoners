using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Pun;


public class GameController : MonoBehaviourPunCallbacks
{
    [Header("Game Settings")]
    public float gameTime; // seconds
    private float timeRemaining; // seconds

    private GameObject player1;
    private MultiplayerControls playerControls;

    [Header("Team Settings")]
    public GameObject redTeam;
    public GameObject blueTeam;
    private GameObject team;
    private GameObject enemyTeam;

    private List<GameObject> spawnPositions;
    public List<GameObject> redSpawnPositions;
    public List<GameObject> blueSpawnPositions;

    private TeamController teamController;
    private TeamController enemyTeamController;

    [Header("UI Elements")]
    public Text hpText;
    public Text timerText;
    public Text redScoreText;
    public Text blueScoreText;
    public Text gameEndText;
    public Text cooldownText;
    public GameObject gameEndPanel;
    public GameObject damageTakenPanel;
    public GameObject RestartButton;

    public Camera minimapCamera;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameEndPanel.SetActive(false);

        if (!PhotonNetwork.IsConnected) // 1
        {
            SceneManager.LoadScene("LoadScreen");
            return;
        }

        timeRemaining = gameTime;
        string player;
        int playerNum = PhotonNetwork.LocalPlayer.ActorNumber;
        int[] teamList;

        if (Launcher.team == "blue")
        {
            player = "BluePlayer";
            team = blueTeam;
            enemyTeam = redTeam;
            spawnPositions = blueSpawnPositions;
            teamList = (int[])PhotonNetwork.PlayerList[0].CustomProperties["blueTeam"];
            minimapCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("RedTeamMinimap"));
        }
        else
        {
            player = "RedPlayer";
            team = redTeam;
            enemyTeam = blueTeam;
            spawnPositions = redSpawnPositions;
            teamList = (int[])PhotonNetwork.PlayerList[0].CustomProperties["redTeam"];
            minimapCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("BlueTeamMinimap"));
        }

        int teamNumber = Array.IndexOf(teamList, PhotonNetwork.LocalPlayer.ActorNumber);
        Transform mySpawn = spawnPositions[teamNumber].transform;
        player1 = PhotonNetwork.Instantiate(player, mySpawn.position, mySpawn.rotation, 0);
        playerControls = player1.GetComponent<MultiplayerControls>();
        teamController = team.GetComponent<TeamController>();
        enemyTeamController = enemyTeam.GetComponent<TeamController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Exit Sample  
        if (Input.GetKey(KeyCode.Escape))
        {
            QuitGame();
        }

        // Unlock Pointer
        if (Input.GetKey(KeyCode.P))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // Lock Pointer
        if (Input.GetKey(KeyCode.O))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Check for damage on player
        if (playerControls.movementEnabled)
        {
            DamageCheck();
            hpText.text = "HP: " + playerControls.hitPoints.ToString();
        }

        CooldownCheck();

        UpdateScores();
        UpdateTimer();
    }

    private void DamageCheck()
    {
        // Check if zone is active first
        if (enemyTeamController.IsAround(player1.transform.position))
        {
            playerControls.TakeDamage();
            damageTakenPanel.SetActive(true);
            damageTakenPanel.GetComponent<Image>().color = new Color(1, 0, 0, 0.2f * (Mathf.Sin(playerControls.hitPoints * playerControls.damageSpeed * 5) + 1));
            if (playerControls.hitPoints <= 0)
            {
                playerControls.KillPlayer();
            }
        }
        else
        {
            damageTakenPanel.SetActive(false);
        }
        // If no zone is active, perform different damage check
    }

    private void CooldownCheck()
    {
        if (playerControls.coolDowmTime > 0.0f)
        {
            int coolDownTimeInt = (int)playerControls.coolDowmTime;
            cooldownText.text = "Respawn in: " + coolDownTimeInt.ToString();
        }
        else if (cooldownText.text != "")
        {
            cooldownText.text = "";
        }
    }

    private void UpdateTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            int minutes = (int)timeRemaining / 60;
            string minuteString = minutes.ToString();
            if (minutes < 10)
            {
                minuteString = "0" + minuteString;
            }
            int seconds = (int)timeRemaining % 60;
            string secondString = seconds.ToString();
            if (seconds < 10)
            {
                secondString = "0" + secondString;
            }
            timerText.text = minuteString + ":" + secondString;
        }
        else
        {
            EndGame();
        }
    }

    private void UpdateScores()
    {
        redScoreText.text = redTeam.GetComponent<TeamController>().score.ToString();
        blueScoreText.text = blueTeam.GetComponent<TeamController>().score.ToString();
    }

    private void EndGame()
    {
        playerControls.DisableControls();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameEndPanel.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            RestartButton.SetActive(true);
        }
        else
        {
            RestartButton.SetActive(false);
        }

        if (teamController.score > enemyTeamController.score)
        {
            gameEndText.text = "You Win!";
        }
        else if (teamController.score == enemyTeamController.score)
        {
            gameEndText.text = "Draw";
        }
        else
        {
            gameEndText.text = "You Lose";
        }
    }

    public void RestartGame()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    public void ReturnToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.LoadLevel("LoadScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
