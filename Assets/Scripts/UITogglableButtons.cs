using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITogglableButtons : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;

    bool[] orderArray = new bool[6];    //will probablly need ot be moved out
    //0 train;
    //1 controlCrowds;
    //2 guardWalls;
    //3 harrasEnemy;
    //4 rest;
    //5 helpConstruction;

    public void ToggleOrder(byte i)
    {
        orderArray[i] = !orderArray[i];
        eventSystem.currentSelectedGameObject.GetComponent<Image>().color = orderArray[i] ? Color.yellow : Color.white;
    }

    public void ToggleTrain()
    {
        ToggleOrder(0);
    }

    public void ToggleCC()
    {
        ToggleOrder(1);
    }

    public void ToggleWalls()
    {
        ToggleOrder(2);
    }

    public void ToggleHarras()
    {
        ToggleOrder(3);
    }

    public void ToggleRest()
    {
        ToggleOrder(4);
    }

    public void ToggleConstruction()
    {
        ToggleOrder(5);
    }
}
