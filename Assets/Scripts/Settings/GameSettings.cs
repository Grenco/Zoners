using Photon.Pun;

public class GameSettings : MonoBehaviourPunCallbacks
{
    private static T GetProperty<T>(string key, T deafult)
    {
        ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
        if (table.ContainsKey(key)) return (T)table[key];
        else return deafult;

    }

    private static void SetProperty(string key, object value)
    {
        ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
        if (table.ContainsKey(key))
        {
            table[key] = value;
        }
        else
        {
            table.Add(key, value);
        }
        PhotonNetwork.SetPlayerCustomProperties(table);
    }

    //public override void OnJoinedRoom()
    //{
    //    base.OnJoinedRoom();
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        SetDefaults();
    //    }
    //}

    //private void SetDefaults()
    //{
    //    IncludeAIPlayers = true;
    //    UseVariableZoneStrength = true;
    //    UsePowerUps = true;
    //    GameTime = 120;
    //    MapRows = 15;
    //    MapCols = 15;
    //    PlayersPerTeam = 4;
    //}

    public static bool IncludeAIPlayers
    {
        get { return GetProperty("IncludePlayers", true); }
        set { SetProperty("IncludePlayers", value); }
    }

    public static bool UseVariableZoneStrength
    {
        get { return GetProperty("UseVariableZoneStrength", true); }
        set { SetProperty("UseVariableZoneStrength", value); }
    }

    public static bool UsePowerUps
    {
        get { return GetProperty("UsePowerUps", true); }
        set { SetProperty("UsePowerUps", value); }
    }

    public static float GameTime
    {
        get { return GetProperty("GameTime", 120f); }
        set { SetProperty("GameTime", value); }
    }

    public static int MapRows
    {
        get { return GetProperty("MapRows", 15); }
        set { SetProperty("MapRows", value); }
    }

    public static int MapCols
    {
        get { return GetProperty("MapCols", 15); }
        set { SetProperty("MapCols", value); }
    }

    public static int PlayersPerTeam
    {
        get { return GetProperty("PlayersPerTeam", 4); }
        set { SetProperty("PlayersPerTeam", value); }
    }
}