using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutSourcer : MonoBehaviour
{
    private LayoutSourcer _instance;
    public LayoutSourcer Instance => _instance;

    [SerializeField] List<GameObject> ButtonGroups = new List<GameObject>();

    public RectTransform ReadLayout(int groupSize, int buttonIndex) 
    {
        int groupIndex = groupSize - 2;

            if (groupIndex < 0) 
            {
                Debug.Log(groupSize + " is fewer options than expected");
                groupIndex = 0;
            }
            else if(groupIndex > ButtonGroups.Count) 
            {
                Debug.Log(groupSize + " is more options than expected");
                groupIndex = ButtonGroups.Count;
            }

        return ButtonGroups[groupIndex].transform.GetChild(buttonIndex).GetComponent<RectTransform>();
    }



    private void Awake()
    {
        _instance = this;
    }
}
