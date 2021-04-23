using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Core;

[CreateAssetMenu(fileName = "Level Database", menuName = "Content/Level Database")]
public class LevelDatabase : ScriptableObject, IInitialized
{
    public Level[] levels;

    public Obstacle[] obstacles;

    public Bait[] baits;

    public GemPattern[] gemPatterns;

    public LevelGeneratorPreset[] generatorPresets;

    public ColorPreset turboPreset;
    public ColorPreset gemRushPreset;
    public ColorPreset defaultPreset;

    public ColorPreset[] colorPresets;

    public Skin[] skins;

    public string[] skinGroups;
    public int[] skinGroupCosts;

    [System.NonSerialized]
    public List<Level> jellyRushes;

    public void Init()
    {
        jellyRushes = new List<Level>();
        if (levels != null)
        {
            foreach (var level in levels)
            {
                if (baits != null)
                {
                    level.bait = baits[level.baitID];
                    if (level.blocks != null)
                    {
                        foreach (var block in level.blocks)
                        {
                            if (block.type == Level.BlockType.STRAIGHT_LINE && obstacles != null && block.obstacleID >= 0)
                            {
                                block.obstacle = obstacles[block.obstacleID];
                            }
                            if (block.gemPatternID >= 0 && gemPatterns != null)
                            {
                                block.gemPattern = gemPatterns[block.gemPatternID];
                            }
                        }
                    }
                }
                if (level.type == Level.LevelType.JELLY_RUSH)
                {
                    jellyRushes.Add(level);
                }
            }
        }
    }

    public Level GetLevel(int id)
    {
        if (id >= 0 && id < levels.Length)
        {
            return levels[id];
        }
        return null;
    }
}

[System.Serializable]
public class ColorPreset
{
    public string name;

    public Color backgroundStart;
    public Color backgroundFinish;

    public Color platformBody;
    public Color platformSurface;

    public Color obstacleBody;
    public Color obstacleSurface;

    public Color railBody;
    public Color railSurface;

    public Color railPart;

    public ColorPreset Copy()
    {
        ColorPreset other = new ColorPreset();

        other.backgroundStart = new Color(backgroundStart.r, backgroundStart.g, backgroundStart.b);
        other.backgroundFinish = new Color(backgroundFinish.r, backgroundFinish.g, backgroundFinish.b);

        other.platformBody = new Color(platformBody.r, platformBody.g, platformBody.b);
        other.platformSurface = new Color(platformSurface.r, platformSurface.g, platformSurface.b);

        other.obstacleBody = new Color(obstacleBody.r, obstacleBody.g, obstacleBody.b);
        other.obstacleSurface = new Color(obstacleSurface.r, obstacleSurface.g, obstacleSurface.b);

        other.railBody = new Color(railBody.r, railBody.g, railBody.b);
        other.railSurface = new Color(railSurface.r, railSurface.g, railSurface.b);
        other.railPart = new Color(railPart.r, railPart.g, railPart.b);

        return other;
    }
}

[System.Serializable]
public class Skin
{

    public string name;
    public GameObject prefab;
    public Color color;
    public string group;
}