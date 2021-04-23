using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Obstacle
{
    public string name;

    public Line [] lines = new Line[15];

    public Vector2Int baitPoint;
    
    public GameObject prefab;

    public GameObject detailedPrefab;

    public float minX, minY, maxX, maxY; 

    [System.Serializable]
    public class Line{
        public bool[] line = new bool[15];
    }
}