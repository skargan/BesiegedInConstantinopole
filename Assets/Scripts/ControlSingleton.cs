using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlSingleton : MonoBehaviour
{

    private static ControlSingleton _instance;
    public static ControlSingleton Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    // GO BACK functionality

    bool escActive;
    public bool CanGoBack => escActive;
    public System.Action ReturnAction;

    public GameObject returnFrom;
    public GameObject returnTo;

    List<(GameObject from, GameObject to)> backHistory;


    public void LogToEsc(GameObject goingFrom, GameObject goingTo)
    {
        if(returnFrom != null && returnTo != null)
        {
            (GameObject, GameObject) historyLog = (returnFrom, returnTo);
            backHistory.Add(historyLog);
        }

        returnFrom = goingTo;
        returnTo = goingFrom;
        escActive = true;
    }




    public void ClearHistory()
    {
        ReturnAction = null;
        backHistory.Clear();
        returnFrom = null;
        returnTo = null;
        escActive = false;
    }

    public void GoBack()
    {
        if(WorldMapUI.Close()||WarTheatreUI.Close()||TechnologyArtwork.ClosePreview())return;
        if (ReturnAction != null) { var action=ReturnAction; ClearHistory(); action(); return; }
        if (returnTo == null || returnFrom == null) { ClearHistory(); return; }
        returnTo.SetActive(true);
        returnFrom.SetActive(false);

        if(backHistory.Count == 0)
        {
            returnFrom = null;
            returnTo = null;
            escActive = false;
        }
        else
        {
            returnFrom = backHistory[backHistory.Count - 1].from;
            returnTo = backHistory[backHistory.Count - 1].to;
            backHistory.RemoveAt(backHistory.Count - 1);
        }
    }


    //Unity

    private void Awake()
    {
        _instance = this;
        backHistory = new List<(GameObject, GameObject)>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(AudioSettingsUI.Close()||WorldMapUI.Close()||WarTheatreUI.Close()||SaveMenu.Close()||TechnologyArtwork.ClosePreview())return;
            if (CampaignHUD.Instance != null && CampaignHUD.Instance.HasModal) { CampaignHUD.Instance.TogglePause(); return; }
            if (escActive)
            {
                GoBack();
            }

            else
            {
                CampaignHUD.Instance.TogglePause();
            }
        }
    }

}
