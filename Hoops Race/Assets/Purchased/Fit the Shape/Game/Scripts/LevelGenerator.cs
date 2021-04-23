using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;

public class LevelGenerator
{
    LevelDatabase levelDatabase;

    public LevelGenerator(LevelDatabase levelDatabase)
    {
        this.levelDatabase = levelDatabase;
    }

#if UNITY_EDITOR
    public Level GenerateLevel(SerializedProperty preset)
    {
        int amountOfBlocks = preset.FindPropertyRelative("amountOfBlocks").intValue;
        float turnChance = preset.FindPropertyRelative("turnChance").floatValue;
        float obstacleChance = preset.FindPropertyRelative("obstacleChance").floatValue;
        float gemPatternChance = preset.FindPropertyRelative("gemPatternChance").floatValue;
        float railsChance = preset.FindPropertyRelative("railsChance").floatValue;
        float tramplinChance = preset.FindPropertyRelative("tramplinChance").floatValue;
        float multipleObstacleChance = preset.FindPropertyRelative("multipleObstacleChance").floatValue;
        float ascendingRailsChance = preset.FindPropertyRelative("ascendingRailsChance").floatValue;
        float descendingRailsChance = preset.FindPropertyRelative("descendingRailsChance").floatValue;
        Level.LevelType type = (Level.LevelType)preset.FindPropertyRelative("levelType").enumValueIndex;

        var tempLevel = new Level();
        tempLevel.type = type;
        if (type == Level.LevelType.JELLY_RUSH)
        {
            tempLevel.baitID = Random.Range(0, levelDatabase.baits.Length - 1);
        }
        tempLevel.blocks = new Level.Block[amountOfBlocks];
        Level.Block previous = null;

        var graph = new List<Vector3>();
        graph.Add(new Vector3(0, 0, 0));

        var rotationY = 0;

        for (int i = 0; i < amountOfBlocks; i++)
        {
            var quaternion = Quaternion.AngleAxis(rotationY, Vector3.up);
            var tempBlock = new Level.Block();
            tempBlock.gemPatternID = -1;
            tempBlock.obstacleID = -1;
            tempBlock.ammountOfPillars = 0;
            if (i == 0)
            {
                tempBlock.type = Level.BlockType.START;
            }
            else if (i == amountOfBlocks - 1)
            {
                tempBlock.type = Level.BlockType.FINISH;
            }
            else
            {
                if (previous.type != Level.BlockType.STRAIGHT_LINE || i == amountOfBlocks - 2 || i < 5)
                {
                    tempBlock.type = Level.BlockType.STRAIGHT_LINE;
                }
                else
                {
                    var random = Random.value;
                    if (random < turnChance / 2)
                    {
                        tempBlock.type = Level.BlockType.TURN_LEFT;
                        rotationY -= 90;
                    }
                    else if (random < turnChance)
                    {
                        tempBlock.type = Level.BlockType.TURN_RIGHT;
                        rotationY += 90;
                    }
                    else if (random < turnChance + tramplinChance)
                    {
                        tempBlock.type = Level.BlockType.TRAMPLIN;
                    }
                    else if (random < turnChance + tramplinChance + railsChance)
                    {
                        tempBlock.type = Level.BlockType.RAILS;
                    }
                    else if (random < turnChance + tramplinChance + railsChance + ascendingRailsChance)
                    {
                        tempBlock.type = Level.BlockType.ASCENDING_RAILS;
                    }
                    else if (random < turnChance + tramplinChance + railsChance + ascendingRailsChance + descendingRailsChance)
                    {
                        tempBlock.type = Level.BlockType.DESCENDING_RAILS;
                    }
                    else
                    {
                        tempBlock.type = Level.BlockType.STRAIGHT_LINE;
                    }

                }
            }

            Vector3[] positionVectors = GetPositionVector(tempBlock.type, quaternion, graph[graph.Count - 1]);

            if (PositionCheck(positionVectors, graph))
            {
                AddVectorsToGraph(positionVectors, graph);
                previous = tempBlock;
                tempLevel.blocks[i] = tempBlock;
            }
            else
            {
                previous = tempLevel.blocks[i - 3];
                graph.RemoveAt(graph.Count - 1);
                graph.RemoveAt(graph.Count - 1);
                if (tempBlock.type == Level.BlockType.TURN_LEFT)
                    rotationY += 90;
                if (tempBlock.type == Level.BlockType.TURN_RIGHT)
                    rotationY -= 90;
                if (tempLevel.blocks[i - 1].type == Level.BlockType.TURN_LEFT)
                    rotationY += 90;
                if (tempLevel.blocks[i - 1].type == Level.BlockType.TURN_RIGHT)
                    rotationY -= 90;
                if (tempLevel.blocks[i - 2].type == Level.BlockType.TURN_LEFT)
                    rotationY += 90;
                if (tempLevel.blocks[i - 2].type == Level.BlockType.TURN_RIGHT)
                    rotationY -= 90;
                i -= 3;
            }
        }

        var amountOfObstacles = levelDatabase.baits.Length;
        var amountOfPatterns = levelDatabase.gemPatterns.Length;
        for (int i = 0; i < amountOfBlocks; i++)
        {
            var tempBlock = tempLevel.blocks[i];
            if (tempBlock.type == Level.BlockType.STRAIGHT_LINE)
            {
                var random = Random.value;
                if (random < obstacleChance)
                {
                    tempBlock.obstacleID = Random.Range(0, amountOfObstacles - 1);

                    random = Random.value;
                    if (random < multipleObstacleChance)
                    {
                        tempBlock.ammountOfObstacles = Random.Range(2, 3);
                    }
                    else
                    {
                        tempBlock.ammountOfObstacles = 1;
                    }
                }
                if (random < gemPatternChance)
                {
                    tempBlock.gemPatternID = Random.Range(0, amountOfPatterns - 1);
                }
            }
        }
        return tempLevel;
    }
#endif


    private bool PositionCheck(Vector3[] positionVectors, List<Vector3> graph)
    {
        foreach (var newPos in positionVectors)
        {
            foreach (var oldPos in graph)
            {
                if (Mathf.Abs(newPos.x - oldPos.x) < 0.5 && Mathf.Abs(newPos.z - oldPos.z) < 0.5 && Mathf.Abs(newPos.y - oldPos.y) < 3)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static void AddVectorsToGraph(Vector3[] positionVectors, List<Vector3> graph)
    {
        for (int i = 0; i < positionVectors.Length; i++)
        {
            graph.Add(positionVectors[i]);
        }
    }

    private static Vector3[] GetPositionVector(Level.BlockType type, Quaternion quaternion, Vector3 previous)
    {
        Vector3[] positionVector;
        Vector3 first = previous + quaternion * new Vector3(0, 0, 1);

        switch (type)
        {
            case Level.BlockType.TURN_LEFT:
                positionVector = new Vector3[] { first, first + quaternion * new Vector3(-1, 0, 1) };
                break;
            case Level.BlockType.TURN_RIGHT:
                positionVector = new Vector3[] { first, first + quaternion * new Vector3(1, 0, 1) };
                break;
            case Level.BlockType.TRAMPLIN:
            case Level.BlockType.RAILS:
            case Level.BlockType.START:
                positionVector = new Vector3[] { first };
                break;
            case Level.BlockType.ASCENDING_RAILS:
                positionVector = new Vector3[] { first, first + quaternion * new Vector3(0, 1, 1) };
                break;
            case Level.BlockType.DESCENDING_RAILS:
                positionVector = new Vector3[] { first, first + quaternion * new Vector3(0, -1, 1) };
                break;
            default:
                positionVector = new Vector3[] { first, first + quaternion * new Vector3(0, 0, 1) };
                break;
        }

        return positionVector;
    }

    public static List<Vector3> LevelToGraph(Level level)
    {
        var graph = new List<Vector3>();
        graph.Add(new Vector3(0, 0, 0));
        var rotationY = 0;

        for (int i = 1; i < level.blocks.Length; i++)
        {
            var quaternion = Quaternion.AngleAxis(rotationY, Vector3.up);
            AddVectorsToGraph(GetPositionVector(level.blocks[i].type, quaternion, graph[i - 1]), graph);
            if (level.blocks[i].type == Level.BlockType.TURN_LEFT)
            {
                rotationY -= 90;
            }
            else if (level.blocks[i].type == Level.BlockType.TURN_RIGHT)
            {
                rotationY += 90;
            }
        }

        return graph;
    }
}