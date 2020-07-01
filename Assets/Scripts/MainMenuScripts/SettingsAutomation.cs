using MainMenuScripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAutomation : MonoBehaviour
{
    public Button generalButton;
    public Button controlsButton;
    public GameObject closeButton;
    public GameObject saveButton;
    public GameObject generalContainer;
    public GameObject controlsContainer;


    public void Start()
    {
        SwitchToGeneral();
    }

    public void OnEnable()
    {
        SwitchToGeneral();
    }

    public void SwitchToGeneral()
    {
        generalButton.interactable = false;
        controlsButton.interactable = true;
        generalContainer.SetActive(true);
        controlsContainer.SetActive(false);
        closeButton.SetActive(true);
        saveButton.SetActive(true);
    }

    public void SwitchToControls()
    {
        generalButton.interactable = true;
        controlsButton.interactable = false;
        generalContainer.SetActive(false);
        controlsContainer.SetActive(true);
        closeButton.SetActive(true);
        saveButton.SetActive(true);
    }
}

