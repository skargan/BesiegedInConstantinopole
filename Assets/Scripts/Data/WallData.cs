using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallData
{
    [SerializeField] string enduringWallName;                                //this is component to construct variable key such as "fOuterWallHealth" <= "f" + enduringWall
    public string GetCodeName() => enduringWallName;

    [SerializeField] string outerWallName;
    [SerializeField] string middlewallName;
    [SerializeField] string innerWallName;

    public WallData(string wallName) { enduringWallName = wallName; }

    public float GetHealth() => VariableSingleton.GetFloatVariable("f" + enduringWallName);

    public string GetDisplayName() 
    { 
        switch (enduringWallName)
        {
            case "OuterWall":
                return outerWallName;

            case "MiddleWall":
                return middlewallName;

            case "InnerWall":
                return innerWallName;
        }
        Debug.Log("Exception invalid enduringWall = " + enduringWallName);
        return enduringWallName; 
    }

    public bool CityLost { get; private set; }
    public void Restore(string name, bool lost)
    {
        enduringWallName = name;
        CityLost = lost;
    }

    public void Abandon() 
    {
        if (enduringWallName == "OuterWall") enduringWallName = "MiddleWall";
        else if (enduringWallName == "MiddleWall") enduringWallName = "InnerWall";
        else CityLost = true;
    }
}
