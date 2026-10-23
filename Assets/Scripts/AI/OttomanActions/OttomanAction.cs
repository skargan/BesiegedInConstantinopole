using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OttomanAction : MonoBehaviour
{
    abstract public bool IsRestricted();

    abstract public void Execute();

}
