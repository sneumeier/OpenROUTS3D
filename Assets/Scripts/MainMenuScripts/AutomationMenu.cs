using Assets.Scripts.CarScripts;
using Assets.Scripts.MultiplayerMessages;
using MainMenuScripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AutomationMenu : MonoBehaviour {

    public Button automationButton;
    public Button manualButton;
    public Button multiplayerButton;

    public GameObject StartButton;
    public GameObject JoinButton;

    public GameObject automationContainer;
    public GameObject manualContainer;
    public GameObject multiplayerContainer;

    public ClientMenuConnector ConnectionScreen;

    public FileBrowser automationBrowser;
    public InputField nameField;

    public ColorCar car;

    public void Start()
    {
        Settings.automatedPlay = false;
        Settings.isMPServer = false;
        Settings.isHost = false;
        nameField.text = Settings.userName;
    }

    public void StartBrowser()
    {
        automationBrowser.FileExtension = ".xml";
        if (!string.IsNullOrEmpty(Settings.autorunXmlPath) && Directory.Exists(Path.GetDirectoryName(Settings.autorunXmlPath)))
        {
            automationBrowser.Browse(Path.GetDirectoryName(Settings.autorunXmlPath));
            Debug.Log("Preparing browser with last location where a file was loaded");

        }
        else
        {
            Debug.Log("No last map path available, starting at documents");

        }
        automationBrowser.gameObject.SetActive(true);
        gameObject.SetActive(false);
        automationBrowser.FileSelected += AutomationBrowser_FileSelected;
    }

    private void AutomationBrowser_FileSelected(object sender, string path)
    {
        automationBrowser.gameObject.SetActive(false);
        gameObject.SetActive(true);
        automationBrowser.FileSelected -= AutomationBrowser_FileSelected;
        Settings.autorunXmlPath = path;
        Debug.Log("Path = "+path);
    }

    public void NameChange()
    {
        Settings.userName = nameField.text;
    }

    public void SwitchToAutomation() 
    {
        multiplayerButton.interactable = true;
        manualButton.interactable = true;
        automationButton.interactable = false;

        automationContainer.SetActive(true);
        manualContainer.SetActive(false);
        multiplayerContainer.SetActive(false);

        JoinButton.SetActive(false);
        StartButton.SetActive(true); 

        Settings.automatedPlay = true;
        Settings.isMPServer = false;
        car.gameObject.SetActive(false);
    }

    public void SwitchToManual()
    {
        multiplayerButton.interactable = true;
        manualButton.interactable = false;
        automationButton.interactable = true;

        automationContainer.SetActive(false);
        manualContainer.SetActive(true);
        multiplayerContainer.SetActive(false);

        JoinButton.SetActive(false);
        StartButton.SetActive(true);

        Settings.automatedPlay = false;
        Settings.isMPServer = false;
        car.gameObject.SetActive(false);
    }

    public void SwitchToMultiplayer()
    {
        multiplayerButton.interactable = false;
        manualButton.interactable = true;
        automationButton.interactable = true;

        automationContainer.SetActive(false);
        manualContainer.SetActive(false);
        multiplayerContainer.SetActive(true);

        JoinButton.SetActive(true);
        StartButton.SetActive(false);

        Settings.automatedPlay = false;
        Settings.isMPServer = true;
        car.gameObject.SetActive(true);
    }

    public void ApplyHost()
    {
        Settings.isHost = true;
    }

    public void StartConnection()
    {
        if(Settings.isMPServer && !Settings.isHost)
        {
            Settings.isSumoServer = false;
            ConnectionScreen.gameObject.SetActive(true);
            ConnectionScreen.EstablishConnection();
        }
    }
}
