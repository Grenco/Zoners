using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

public class TeamSettings : MonoBehaviourPunCallbacks
{
    public static Team myTeam = Team.none;
    public static Team enemyTeam = Team.none;
    public TeamController redTeamController;
    public TeamController blueTeamController;
    public static TeamController myTeamController;
    public static TeamController enemyTeamController;
    public Dictionary<Team, TeamController> teamControllers = new Dictionary<Team, TeamController>();

    public static List<Player> redTeamPlayers = new List<Player>();
    public static List<Player> blueTeamPlayers = new List<Player>();
    public static Dictionary<Team, List<Player>> teamPlayers = new Dictionary<Team, List<Player>>()
    {
        { Team.red, redTeamPlayers },
        { Team.blue, blueTeamPlayers }
    };

    private readonly static string redPlayer = "RedPlayer";
    private readonly static string bluePlayer = "BluePlayer";
    public static string myPlayer;
    public static int maxPlayers = 4;

    public static Dictionary<Team, Color> teamColors = new Dictionary<Team, Color>()
    {
        { Team.red, Color.red },
        { Team.blue, Color.cyan }
    };

    public int MyIndex
    {
        get
        {
            return PositionInTeam(PhotonNetwork.LocalPlayer, myTeam);
        }
    }

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
        teamControllers.Add(Team.red, redTeamController);
        teamControllers.Add(Team.blue, blueTeamController);
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
        else
        {
            enemyTeam = Team.none;
            myPlayer = "";
        }
    }

    private static Team OtherTeam(Team team)
    {
        if (team == Team.red)
        {
            return Team.blue;
        }
        else if (team == Team.blue)
        {
            return Team.red;
        }
        return Team.none;
    }

    [PunRPC]
    public void TeamJoinRequest(Team team, PhotonMessageInfo info)
    {
        if (!teamPlayers[team].Contains(info.Sender) && teamPlayers[team].Count < maxPlayers)
        {
            photonView.RPC("AddPlayerToTeam", RpcTarget.All, team, info.Sender);
        }
        else
        {
            Launcher launcher = gameObject.GetComponent<Launcher>();
            launcher.photonView.RPC("TeamAccessDenied",info.Sender);
        }
    }

    [PunRPC]
    public void AddPlayerToTeam(Team team, Player player)
    {
        teamPlayers[team].Add(player);
        if (player == PhotonNetwork.LocalPlayer)
        {
            SetTeam(team);
            Debug.Log("Successfully joined " + team.ToString() + " team.");
        }
    }

    [PunRPC]
    public void RemovePlayerFromTeam(Team team, Player player)
    {
        if (teamPlayers[team].Contains(player))
        {
            teamPlayers[team].Remove(player);

            if (player == PhotonNetwork.LocalPlayer)
            {
                if (myTeam == team)
                {
                    SetTeam(Team.none);
                }
                Debug.Log("Successfully left " + team.ToString() + " team.");
            }
        }
    }

    public int PositionInTeam(Player player, Team team)
    {
        return teamPlayers[team].IndexOf(player);
    }
}
