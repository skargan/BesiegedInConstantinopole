using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeScript : MonoBehaviour
{
    [SerializeField] GameObject manager;
    public void Onwards()
    {
        manager.SetActive(true);
        Destroy(this.gameObject);
    }

    
}
