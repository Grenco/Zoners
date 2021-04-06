using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class TeamSettings : MonoBehaviourPunCallbacks
{
    public readonly static string redTeam = "Red", blueTeam = "Blue";
    private readonly static string redPlayer = "RedPlayer", bluePlayer = "BluePlayer";
    public static int maxPlayers = 4;
    public static PhotonTeamsManager teamManager;

    public static string MyTeam
    {
        get
        {
            return PhotonNetwork.LocalPlayer.GetPhotonTeam().Name;
        }
    }
    public static string MyPlayer
    {
        get
        {
            if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == redTeam)
            {
                return redPlayer;
            }
            else
            {
                return bluePlayer;
            }
        }
    }

    public static Dictionary<string, Color> teamColors = new Dictionary<string, Color>
    {
        { redTeam, Color.red },
        { blueTeam, Color.cyan }
    };

    public static int MyIndex
    {
        get
        {
            return PositionInTeam(PhotonNetwork.LocalPlayer);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        teamManager = gameObject.GetComponent<PhotonTeamsManager>();
    }

    public static string OtherTeam(string team)
    {
        if (team == redTeam)
        {
            return blueTeam;
        }
        else if (team == blueTeam)
        {
            return redTeam;
        }
        return "";
    }


    public static int PositionInTeam(Player player)
    {
        if (teamManager.TryGetTeamMembers(player.GetPhotonTeam(), out Player[] members))
        {
            return Array.IndexOf(members, player);
        }
        return 0;
    }

    public static Player[] GetPlayers(string team)
    {
        teamManager.TryGetTeamMembers(team, out Player[] teamMembers);
        return teamMembers;
    }
}
