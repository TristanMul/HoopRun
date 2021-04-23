using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;
using System;

public class LevelVisualizer
{
    private List<GameObject> visualizeLevel;
    private Dictionary<Level.BlockType, GameObject> blockPrefabs;
    private ProjectDatabase database;
    private LevelDatabase levelDatabase;
    private UnityEngine.Object target;

    public Transform container;
    public Transform Container
    {
        get
        {
            if (container == null)
            {
                GameObject tempContainer = GameObject.Find("[EDITOR]");
                if (tempContainer == null)
                {
                    tempContainer = new GameObject("[EDITOR]");
                }
                if (tempContainer != null)
                {
                    container = tempContainer.transform;
                }
            }
            return container;
        }
    }

    public void InitDatabase(ProjectDatabase database, LevelDatabase levelDatabase)
    {
        this.levelDatabase = levelDatabase;
        this.database = database;
        blockPrefabs = new Dictionary<Level.BlockType, GameObject>();
        blockPrefabs.Add(Level.BlockType.START, database.start);
        blockPrefabs.Add(Level.BlockType.FINISH, database.finish);
        blockPrefabs.Add(Level.BlockType.STRAIGHT_LINE, database.straitLine);
        blockPrefabs.Add(Level.BlockType.TURN_LEFT, database.turnLeft);
        blockPrefabs.Add(Level.BlockType.TURN_RIGHT, database.turnRight);
        blockPrefabs.Add(Level.BlockType.RAILS, database.rails);
        blockPrefabs.Add(Level.BlockType.TRAMPLIN, database.tramplin);
        blockPrefabs.Add(Level.BlockType.ASCENDING_RAILS, database.ascendingRails);
        blockPrefabs.Add(Level.BlockType.DESCENDING_RAILS, database.descendingRails);
    }

    public void SpawnGemsEditor(Vector3Int[] gems, Transform parent = null)
    {
        visualizeLevel = new List<GameObject>();
        if (parent == null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(database.straitLine);
            gameObject.transform.SetParent(Container);
            gameObject.transform.position = new Vector3(0, 0, 0);
            visualizeLevel.Add(gameObject);
            parent = gameObject.transform;
        }

        for (int i = 0; i < gems.Length; i++)
        {
            visualizeLevel.Add(SpawnGem(gems[i], parent, true));
        }
    }

    private void SpawnGems(Vector3Int[] gems, Transform parent)
    {
        for (int i = 0; i < gems.Length; i++)
        {
            SpawnGem(gems[i], parent, false);
        }
    }

    private GameObject SpawnGem(Vector3Int gem, Transform parent, bool editor)
    {
        GameObject gameObject;
        if (editor)
        {
            gameObject = UnityEngine.Object.Instantiate(database.gem);
        }
        else
        {
            gameObject = PoolManager.GetPoolByName("GEM").GetPooledObject();
        }

        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = new Vector3((-0.75f + gem.x * 0.375f) + gem.y * 0.375f, 0.25f + gem.y / 2f, 4f - gem.z);

        return gameObject;
    }

    public void DestroyGems()
    {
        if (visualizeLevel != null)
        {
            int count = visualizeLevel.Count;
            for (int i = 0; i < count; i++)
            {
                if (visualizeLevel[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualizeLevel[i]);
                }
            }
            visualizeLevel = null;
        }
    }

    public void DestroyEditorLevel()
    {
        UnityEngine.Object.DestroyImmediate(GameObject.Find("[EDITOR]"));
        var baitContainer = GameObject.Find("BaitAnim");
        foreach (Transform child in baitContainer.transform)
        {
            UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    public void DestroyLevel(Level level)
    {
        foreach (var obstacle in levelDatabase.obstacles)
        {
            PoolManager.GetPoolByName(obstacle.name + "_OBSTACLE").ReturnToPoolEverything(true);
            PoolManager.GetPoolByName(obstacle.name + "_OBSTACLE_DETAILED").ReturnToPoolEverything(true);
        }
        PoolManager.GetPoolByName("GEM").ReturnToPoolEverything(true);

        foreach (var blockType in Enum.GetValues(typeof(Level.BlockType)))
        {
            PoolManager.GetPoolByName(blockType.ToString() + "_BLOCK").ReturnToPoolEverything(true);
        }

        var baitContainer = GameObject.Find("BaitAnim");
        foreach (Transform child in baitContainer.transform)
        {
            UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    public void VisualizeLevel(Level level, Transform parent, bool editor)
    {

        var baitContainer = GameObject.Find("BaitAnim");
        if (baitContainer.transform.childCount != 0)
        {
            UnityEngine.Object.Destroy(baitContainer.transform.GetChild(0).gameObject);
        }
        if (!editor)
        {
            DestroyEditorLevel();
        }

        visualizeLevel = new List<GameObject>();
        var position = new Vector3();
        var step = new Vector3();
        var rotationY = 0;

        baitContainer.transform.parent.position = Vector3.zero;
        baitContainer.transform.parent.rotation = new Quaternion(0, 0, 0, 1);
        baitContainer.transform.localPosition = Vector3.zero;
        baitContainer.transform.localRotation = new Quaternion(0, 0, 0, 1);
        if (level.type == Level.LevelType.JELLY_RUSH)
        {
            GameObject bait = UnityEngine.Object.Instantiate(levelDatabase.baits[level.baitID].prefab);
            bait.transform.SetParent(baitContainer.transform);
            bait.transform.localPosition = Vector3.zero;
            bait.transform.localRotation = new Quaternion(0, 0, 0, 1);
        }

        Level.BlockType prevBlock = Level.BlockType.START;
        foreach (var block in level.blocks)
        {
            GameObject levelBlock;
            if (editor)
            {
                levelBlock = UnityEngine.Object.Instantiate(blockPrefabs[block.type]);
            }
            else
            {
                levelBlock = PoolManager.GetPoolByName(block.type.ToString() + "_BLOCK").GetPooledObject();
            }
            if (block.type == Level.BlockType.RAILS || block.type == Level.BlockType.TRAMPLIN || block.type == Level.BlockType.ASCENDING_RAILS || block.type == Level.BlockType.DESCENDING_RAILS)
            {
                RestoreRails(levelBlock.transform);
            }


            levelBlock.transform.SetParent(parent);

            visualizeLevel.Add(levelBlock);
            if (block.type == Level.BlockType.STRAIGHT_LINE)
            {
                switch (prevBlock)
                {
                    case Level.BlockType.TRAMPLIN:
                    case Level.BlockType.RAILS:
                        step *= 1.5f;
                        break;
                    case Level.BlockType.TURN_LEFT:
                    case Level.BlockType.TURN_RIGHT:
                        step = Quaternion.AngleAxis(-rotationY, Vector3.up) * step;
                        step.z *= 1.5f;
                        step = Quaternion.AngleAxis(rotationY, Vector3.up) * step;
                        break;
                    case Level.BlockType.STRAIGHT_LINE:
                        step *= 2;
                        break;
                    default:
                        step *= 2;
                        break;
                    case Level.BlockType.ASCENDING_RAILS:
                    case Level.BlockType.DESCENDING_RAILS:
                        step = Quaternion.AngleAxis(-rotationY, Vector3.up) * step;
                        step.z *= 1.5f;
                        step = Quaternion.AngleAxis(rotationY, Vector3.up) * step;
                        break;

                }
            }
            else if (block.type == Level.BlockType.ASCENDING_RAILS || block.type == Level.BlockType.DESCENDING_RAILS)
            {
                step = Quaternion.AngleAxis(-rotationY, Vector3.up) * step;
                step.z *= 1.5f;
                step = Quaternion.AngleAxis(rotationY, Vector3.up) * step;
            }
            else if (block.type == Level.BlockType.FINISH)
            {
                step = Quaternion.AngleAxis(-rotationY, Vector3.up) * step;
                step.z *= 2.5f;
                step = Quaternion.AngleAxis(rotationY, Vector3.up) * step;
            }


            position += step;

            levelBlock.transform.position = new Vector3(position.x, position.y, position.z);
            levelBlock.transform.localEulerAngles = new Vector3(0, rotationY, 0);

            if (block.gemPatternID >= 0)
            {
                if (editor)
                {
                    SpawnGemsEditor(levelDatabase.gemPatterns[block.gemPatternID].gems, levelBlock.transform);
                }
                else
                {
                    SpawnGems(levelDatabase.gemPatterns[block.gemPatternID].gems, levelBlock.transform);
                }

            }

            var obstacleStep = 12f / (block.ammountOfObstacles + 1);
            var obstaclePosZ = -6f + obstacleStep;
            if (block.obstacleID >= 0 && block.type == Level.BlockType.STRAIGHT_LINE)
            {
                var obstacleContainer = levelBlock.transform.Find("obstacles");
                if (obstacleContainer == null)
                {
                    obstacleContainer = new GameObject("obstacles").transform;
                    obstacleContainer.transform.SetParent(levelBlock.transform, false);
                }

                for (int i = 0; i < block.ammountOfObstacles; i++)
                {
                    if (levelDatabase.obstacles[block.obstacleID].prefab == null)
                    {
                        Debug.LogError("NULL");
                    }
                    GameObject obstacle;
                    if (editor)
                    {
                        obstacle = UnityEngine.Object.Instantiate(levelDatabase.obstacles[block.obstacleID].prefab);
                    }
                    else
                    {
                        obstacle = PoolManager.GetPoolByName(levelDatabase.obstacles[block.obstacleID].name + "_OBSTACLE").GetPooledObject();
                    }
                    obstacle.SetActive(true);
                    obstacle.transform.SetParent(obstacleContainer, false);
                    obstacle.transform.localPosition = new Vector3(0, 0, obstaclePosZ);
                    obstacle.transform.localRotation = new Quaternion(0, 0, 0, 1);
                    obstaclePosZ += obstacleStep;


                    visualizeLevel.Add(obstacle);
                }
            }


            switch (block.type)
            {
                case Level.BlockType.TURN_LEFT:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(-10, 0, 5);
                    rotationY -= 90;
                    break;
                case Level.BlockType.TURN_RIGHT:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(10, 0, 5);
                    rotationY += 90;
                    break;

                case Level.BlockType.ASCENDING_RAILS:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(0, 1, 10);
                    break;
                case Level.BlockType.DESCENDING_RAILS:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(0, -1, 10);
                    break;
                case Level.BlockType.STRAIGHT_LINE:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(0, 0, 10);
                    break;
                case Level.BlockType.START:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(0, 0, 15);
                    break;
                default:
                    step = Quaternion.AngleAxis(rotationY, Vector3.up) * new Vector3(0, 0, 10);
                    break;
            }
            prevBlock = block.type;
        }
    }

    private void RestoreRails(Transform rail)
    {
        for (int i = 0; i < rail.childCount; i++)
        {
            var child = rail.GetChild(i);
            if (child.name.EndsWith("_ALT"))
            {
                for (int j = 0; j < child.childCount; j++)
                {
                    var grandchild = child.GetChild(j);
                    grandchild.gameObject.SetActive(true);
                    grandchild.localScale = new Vector3(1, 1, 1);
                }
                break;
            }
        }
    }

    public void VisualizeLevel(Level level)
    {
        VisualizeLevel(level, Container, true);
    }
}