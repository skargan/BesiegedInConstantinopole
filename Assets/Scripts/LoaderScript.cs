using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoaderScript : MonoBehaviour
{
    public void RunLoader()
    {
        if (File.Exists("Save"))
        {
            VariableSingleton.Instance.Load();
        }
        else 
        {
            Debug.Log("No save file found.");
        }
        Destroy(this.gameObject);
    }
}
