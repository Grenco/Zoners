using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider mouseSensitivitySlider;
    public InputField nameInputField;

    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
