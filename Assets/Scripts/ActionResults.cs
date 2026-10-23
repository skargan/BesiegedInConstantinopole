using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionResults : MonoBehaviour
{
    private static ActionResults _instance;
    public static ActionResults Instance => _instance;

    public enum ActionType 
    { 
        RepairWall,
        RecruitTroops,
        UpgradeTroops
    }

    [Serializable]
    class ActionResultText 
    {
        [SerializeField] ActionType actionType;
        [SerializeField] [TextArea] string actionText;

        public void saveEntry(string[] stringArray)     //this may not work when array is passed as argument
        {
            stringArray[(int)actionType] = actionText;
        }
    }

    [SerializeField] List<ActionResultText> actionResultTexts;


    string[] actionTextStorage;
    /**Action indices:
     * 0 - repair wall
     * 1 - recruit troops
     * 2 - upgrade troops
     */
    private int _lastAction;
    public string Message = "You allow the city a moment of rest. The council will hear the next petition.";
    public int LastAction { set => _lastAction = value; }

    public string GetActionText()
    {
        return Message;
    }
    

    private void Awake()
    {
        _instance = this;
        actionTextStorage = new string[actionResultTexts.Count];
        foreach(ActionResultText entry in actionResultTexts) 
        {
            entry.saveEntry(actionTextStorage);
        }
    }

}
