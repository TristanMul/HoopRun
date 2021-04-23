using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GemPattern
{
    public string name;

    /*
        0)  000 (x, y, z)
        1)  100 (x, y, z)
        ...
    */
    public Vector3Int[] gems;
}