using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviourPunCallbacks
{
    [Header("Game Settings")]
    public float timeRemaining; // seconds
    private double startTime;

    private bool gameStarted = false;
    private int countdown = 4;

    private GameObject player1;
    private MultiplayerControls playerControls;

    [Header("Team Settings")]
    private List<GameObject> spawnPositions;

    public List<GameObject> redSpawnPositions;
    public List<GameObject> blueSpawnPositions;

    public ZoneController redTeamController;
    public ZoneController blueTeamController;
    private ZoneController teamController;
    private ZoneController enemyTeamController;

    [Header("UI Elements")]
    public Text hpText;

    public Text timerText;
    public Text redScoreText;
    public Text blueScoreText;
    public Text gameEndText;
    public Text centreScreenText;
    public GameObject gameEndPanel;
    public GameObject damageTakenPanel;
    public GameObject RestartButton;
    public HealthBarControl zoneStrengthBar;
    public GameObject minimap;

    public Camera minimapCamera;

    [Header("Maze")]
    public MazeConstructor mazeConstructor;

    public GameObject redSpawnPlatform;
    public GameObject blueSpawnPlatform;

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameEndPanel.SetActive(false);

        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("LoadScreen");
            return;
        }

        timeRemaining = GameSettings.GameTime;

        minimapCamera.orthographicSize = Math.Max(mazeConstructor.MapLength(), mazeConstructor.MapWidth()) / 2;

        float spawnPos = mazeConstructor.MapLength() / 2;
        redSpawnPlatform.transform.position = new Vector3(0, -3.5f, -spawnPos);
        blueSpawnPlatform.transform.position = new Vector3(0, -3.5f, spawnPos);

        string myTeam = TeamSettings.MyTeam;

        if (myTeam == TeamSettings.blueTeam)
        {
            spawnPositions = blueSpawnPositions;
            minimapCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("RedTeamMinimap"));
            minimap.transform.Rotate(new Vector3(0, 0, 1), 180);
            teamController = blueTeamController;
            enemyTeamController = redTeamController;
        }
        else
        {
            spawnPositions = redSpawnPositions;
            minimapCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("BlueTeamMinimap"));
            teamController = redTeamController;
            enemyTeamController = blueTeamController;
        }

        zoneStrengthBar.SetBarColor(TeamSettings.teamColors[myTeam]);

        Transform mySpawn = spawnPositions[TeamSettings.MyIndex].transform;
        player1 = PhotonNetwork.Instantiate(TeamSettings.MyPlayer, mySpawn.position, mySpawn.rotation, 0);
        playerControls = player1.GetComponent<MultiplayerControls>();
        playerControls.DisableControls();

        zoneStrengthBar.gameObject.SetActive(GameSettings.UseVariableZoneStrength);
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (!gameStarted)
        {
            Countdown();
        }

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

        if (GameSettings.GameActive && gameStarted)
        {
            // Check player status
            if (playerControls.movementEnabled)
            {
                DamageCheck();
                hpText.text = "HP: " + playerControls.hitPoints.ToString();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                AIDamageCheck();
            }

            CooldownCheck();

            // Update UI
            UpdateScores();
            UpdateTimer();
            UpdateZoneBar();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (!propertiesThatChanged.ContainsKey((System.Byte)253))
        {
            int test = 0;
        }
    }

    private void Countdown()
    {
        Animation animation = centreScreenText.gameObject.GetComponent<Animation>();

        if (!animation.isPlaying && countdown > 0)
        {
            countdown--;
            centreScreenText.text = countdown == 0 ? "GO!" : countdown.ToString();
            animation.Play("TextZoom");
        }
        else if (!animation.isPlaying && countdown == 0)
        {
            centreScreenText.text = "";
            centreScreenText.gameObject.transform.localScale = new Vector3(1, 1, 1);
            if (GameSettings.GameActive)
            {
                gameStarted = true;
                startTime = GameSettings.StartTime;
                playerControls.EnableControls();
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                GameSettings.StartTime = PhotonNetwork.Time;
                GameSettings.GameActive = true;
            }
        }
    }

    /// <summary>
    /// Check if the local player has recieved damage and update the UI to reflect this.
    /// </summary>
    private void DamageCheck()
    {
        if (enemyTeamController.IsAround(player1.transform.position))
        {
            playerControls.TakeDamage();
            damageTakenPanel.SetActive(true);
            damageTakenPanel.GetComponent<Image>().color = new Color(1, 0, 0, 0.2f * (Mathf.Sin(playerControls.hitPoints * MultiplayerControls.damageSpeed * 5) + 1));
            if (playerControls.hitPoints <= 0)
            {
                playerControls.KillPlayer(spawnPositions);
            }
        }
        else
        {
            damageTakenPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Perform damage checks on the AI players on both teams.
    /// Only to be used by the Master Client.
    /// </summary>
    private void AIDamageCheck()
    {
        foreach (GameObject player in redTeamController.players)
        {
            if (player != null)
            {
                MultiplayerControls controls = player.GetComponent<MultiplayerControls>();
                if (controls.isAIPlayer && controls.movementEnabled)
                {
                    if (blueTeamController.IsAround(player.transform.position))
                    {
                        controls.TakeDamage();

                        if (controls.hitPoints <= 0)
                        {
                            controls.KillPlayer(redSpawnPositions);
                        }
                    }
                }
            }
        }

        foreach (GameObject player in blueTeamController.players)
        {
            if (player != null)
            {
                MultiplayerControls controls = player.GetComponent<MultiplayerControls>();
                if (controls.isAIPlayer && controls.movementEnabled)
                {
                    if (redTeamController.IsAround(player.transform.position))
                    {
                        controls.TakeDamage();

                        if (controls.hitPoints <= 0)
                        {
                            controls.KillPlayer(blueSpawnPositions);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if the local player has died and reflect this in the UI.
    /// </summary>
    private void CooldownCheck()
    {
        if (playerControls.coolDowmTime > 0.0f)
        {
            int coolDownTimeInt = (int)playerControls.coolDowmTime;
            centreScreenText.text = "Respawn in: " + coolDownTimeInt.ToString();
        }
        else if (centreScreenText.text != "")
        {
            centreScreenText.text = "";
        }
    }

    /// <summary>
    /// Update the game timer and check if the game has ended.
    /// </summary>
    private void UpdateTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining = (float)(startTime - PhotonNetwork.Time) + GameSettings.GameTime;
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

    /// <summary>
    /// Update the UI to reflect any changes in the score.
    /// </summary>
    private void UpdateScores()
    {
        redScoreText.text = redTeamController.score.ToString();
        blueScoreText.text = blueTeamController.score.ToString();
    }

    public void UpdateZoneBar()
    {
        if (zoneStrengthBar.gameObject.activeSelf)
        {
            zoneStrengthBar.SetBarFill(teamController.damageMultiplier);
        }
    }

    /// <summary>
    /// To be called when the game is over.
    /// Update the UI to show the winner.
    /// </summary>
    private void EndGame()
    {
        playerControls.DisableControls();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameEndPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            RestartButton.SetActive(true);
            GameSettings.GameActive = false;
        }
        else
        {
            RestartButton.SetActive(false);
        }

        if (teamController.score > enemyTeamController.score)
        {
            gameEndText.text = "You Win!";
            PlayerSettings.AddWin();
        }
        else if (teamController.score == enemyTeamController.score)
        {
            gameEndText.text = "Draw";
        }
        else
        {
            gameEndText.text = "You Lose";
            PlayerSettings.AddLoss();
        }
    }

    /// <summary>
    /// Give the Master client the ability to restart the gane.
    /// </summary>
    public void RestartGame()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    /// <summary>
    /// Return to the main menu at the end of the game.
    /// </summary>
    public void ReturnToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Destroy(GameObject.Find("Settings"));
        base.OnLeftRoom();
        // TODO: Find a way to stop this from happening when closing the game
        //PhotonNetwork.LoadLevel("LoadScreen");
    }

    /// <summary>
    /// Exit the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}