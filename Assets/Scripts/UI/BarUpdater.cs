using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarUpdater : MonoBehaviour
{
    [SerializeField] RectTransform barRect;
    [SerializeField] string variableName;

    private void OnEnable()
    {
        barRect.anchorMax = new Vector2(1, VariableSingleton.GetFloatVariable(variableName));
    }
}
