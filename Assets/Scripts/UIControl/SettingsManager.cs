using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;

    [Header("Player Settings")]
    public Slider mouseSensitivitySlider;
    public InputField nameInputField;

    [Header("Game Settings")]
    public InputField gameTimeInputField;
    public InputField mapRowsInputField;
    public InputField mapColsInputField;
    public Toggle includeAiPlayers;
    public Toggle variableZoneStength;

    // Start is called before the first frame update
    private void Start()
    {
        mouseSensitivitySlider.maxValue = PlayerSettings.maxMouseSensitivity;
        mouseSensitivitySlider.minValue = PlayerSettings.minMouseSensitivity;
        mouseSensitivitySlider.value = PlayerSettings.MouseSensitivity;
        nameInputField.text = PlayerSettings.Username;
        settingsPanel.SetActive(false);
    }

    public void ToggleVisiblity()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ChangeName()
    {
        PlayerSettings.Username = nameInputField.text;
    }

    public void ChangeMouseSensitivity()
    {
        PlayerSettings.MouseSensitivity = mouseSensitivitySlider.value;
    }

    public void ToggleAiPlayers()
    {
        GameSettings.IncludeAIPlayers = includeAiPlayers.isOn;
    }

    public void ToggleVariableZoneStrength()
    {
        GameSettings.UseVariableZoneStrength = variableZoneStength.isOn;
    }

    public void SetGameTime()
    {
        GameSettings.GameTime = int.Parse(gameTimeInputField.text);
    }

    public void SetMapSize()
    {
        GameSettings.MapRows = int.Parse(mapRowsInputField.text);
        GameSettings.MapCols = int.Parse(mapColsInputField.text);
    }
}