using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIWindowFam : MonoBehaviour
{

    protected void SwitchPhase(GameObject nextPhase)
    {
        nextPhase.SetActive(true);
        gameObject.SetActive(false);
    }

   
}
