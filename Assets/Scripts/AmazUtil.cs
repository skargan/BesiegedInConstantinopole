using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AmazUtil
{
    public static int NumFromChar(char letter) 
    {
        if (letter > 47 && letter < 58) return letter - 48; 

        else Debug.LogError("Char out of range equivalent to numerical"); return 0;
    }
}
