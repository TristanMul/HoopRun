using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelGeneratorPreset
{
    public Level.LevelType levelType;
    public int amountOfBlocks;
    public float turnChance;
    public float obstacleChance;
    public float railsChance;
    public float ascendingRailsChance;
    public float descendingRailsChance;
    public float tramplinChance;
    public float multipleObstacleChance;

    public float gemPatternChance;

    public string name;
}