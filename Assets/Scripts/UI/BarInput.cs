using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BarInput : MonoBehaviour
{
    [SerializeField] RectTransform myRect;

    [SerializeField] int division;
    
    int currentState = 0;

    public int Value
    {
        get => currentState;
        set
        {
            if (value >= 0)
            {
                if (value < division) currentState = value;
                else currentState = division;
                myRect.anchorMax = new Vector3((float) currentState / division, myRect.anchorMax.y, 0);
            }
        }
    }


    //DEBUG

    [SerializeField]
    int debugValue;

    [ContextMenu("SetDebugValue")]
    void SetDebugValue()
    {
        Value = debugValue;
    }
    
}
