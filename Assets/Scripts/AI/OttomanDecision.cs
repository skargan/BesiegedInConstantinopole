using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class OttomanDecision : MonoBehaviour
{
    [Serializable]
    class possibility
    {
        [SerializeField] float _chance;
        public float Chance => _chance;

        [SerializeField]
        OttomanAction ottomanAction;

        public bool IsRestricted() 
        {
            return ottomanAction.IsRestricted();
        }

        public void ExecuteAction() 
        {
            ottomanAction.Execute();
        }
    }

    [SerializeField]
    List<possibility> tacticalOptions;

    
    public void EliminateRedundant() 
    { 
        // Iterate backwards so removing an option does not invalidate iteration.
        for (int i = tacticalOptions.Count - 1; i >= 0; i--)
        {
            if (tacticalOptions[i].IsRestricted()) tacticalOptions.RemoveAt(i);
        }
    }

    public int SelectRandomAction() 
    {
        if (tacticalOptions.Count == 0) return -1;
        float totalChance = 0;
        foreach (possibility option in tacticalOptions) { totalChance += option.Chance; }

        float[] cumulativeChance = new float[tacticalOptions.Count];
        cumulativeChance[0] = tacticalOptions[0].Chance;
        for(int i = 1; i < tacticalOptions.Count; i++)
        {
            cumulativeChance[i] = tacticalOptions[i].Chance + cumulativeChance[i - 1];
        }

        float randomFloatWithinChance = UnityEngine.Random.value * totalChance;
        
        int selection = 0;
        for(int i = 0; i < cumulativeChance.Length; i++) 
        { 
            if(randomFloatWithinChance < cumulativeChance[i])
            {
                selection = i;
                break;
            }
        }

        return selection;
    }

    public void ExecuteRandomAction() 
    {
        int selection = SelectRandomAction();
        if (selection >= 0) tacticalOptions[selection].ExecuteAction();
    }

    //DEBUG
    public int DebugNumberOfRolls;

    [ContextMenu("FeignExecuteAction")]

    public void FeignExecuteAction() 
    {

        Debug.Log(SelectRandomAction());
    }


    [ContextMenu("FeignMulipleActions")]
    public void FeignMulipleActions()
    {
        int[] results = new int[tacticalOptions.Count];
        for (int i = DebugNumberOfRolls; i>0; i--) 
        {
            int selection = SelectRandomAction();
            if (selection >= 0) results[selection]++;
        }

        for(int i = 0; i < results.Length; i++) 
        {
            Debug.Log("Action " + i + " feigned: " + results[i] + " times");
        } 
    }

}
