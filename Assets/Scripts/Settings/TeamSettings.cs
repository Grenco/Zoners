using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class TeamSettings : MonoBehaviour
{
    public static Team myTeam;
    public static Team enemyTeam;
    public TeamController redTeamController;
    public TeamController blueTeamController;
    public static TeamController myTeamController;
    public static TeamController enemyTeamController;

    public static List<Player> redTeamPlayers;
    public static List<Player> blueTeamPlayers;

    private readonly static string redPlayer = "RedPlayer";
    private readonly static string bluePlayer = "BluePlayer";
    public static string myPlayer;

    public enum Team
    {
        red,
        blue,
        none
    }

    // Start is called before the first frame update
    void Start()
    {
        if (myTeam == Team.red)
        {
            myTeamController = redTeamController;
            enemyTeamController = blueTeamController;
        }
        else if (myTeam == Team.blue)
        {
            myTeamController = blueTeamController;
            enemyTeamController = redTeamController;
        }
    }

    public static void SetTeam(Team team)
    {
        myTeam = team;
        if (team == Team.red)
        {
            enemyTeam = Team.blue;
            myPlayer = redPlayer;
        }
        else if (team == Team.blue)
        {
            enemyTeam = Team.red;
            myPlayer = bluePlayer;
        }
    }
}
