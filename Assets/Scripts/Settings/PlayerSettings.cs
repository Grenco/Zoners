using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    private static string mouseSensitivityKey = "MouseSensitivity";
    public static float minMouseSensitivity = 5f;
    public static float maxMouseSensitivity = 15f;
    public static float MouseSensitivity
    {
        get { return PlayerPrefs.GetFloat(mouseSensitivityKey, 10f); }
        set
        {
            PlayerPrefs.SetFloat(mouseSensitivityKey, value);
            PlayerPrefs.Save();
        }
    }

    private static string nameKey = "Username";
    public static string Username
    {
        get { return PlayerPrefs.GetString(nameKey, ""); }
        set
        {
            PlayerPrefs.SetString(nameKey, value);
            PlayerPrefs.Save();
        }
    }
}
