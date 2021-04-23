using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Core;

[SetupTab("References", texture = "icon_settings")]
[CreateAssetMenu(fileName = "Project Database", menuName = "Content/Project Database")]
public class ProjectDatabase : ScriptableObject, IInitialized
{
    public GameObject gem;
    public GameObject pillar;
    public GameObject start;
    public GameObject finish;
    public GameObject straitLine;
    public GameObject turnLeft;
    public GameObject turnRight;
    public GameObject rails;
    public GameObject ascendingRails;
    public GameObject descendingRails;
    public GameObject tramplin;

    public GameObject obstacleBlock;

    public Material obstacleBodyMaterial;
    public Material obstacleSurfaceMaterial;

    public void Init()
    {

    }
}