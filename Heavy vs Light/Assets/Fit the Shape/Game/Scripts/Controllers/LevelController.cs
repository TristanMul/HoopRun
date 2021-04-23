#pragma warning disable 649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

public class LevelController : MonoBehaviour
{
    private Level level;
    private LevelVisualizer visualizer;

    [SerializeField]
    private ProjectDatabase projectDatabase;

    [SerializeField]
    private LevelDatabase levelDatabase;

    [SerializeField]
    private Transform levelContainer;

    [System.NonSerialized]
    public List<Vector3> graph;

    public Vector3 nextObstaclePosition;
    public int nextObstacleBlockIndex;

    public static LevelController instance;

    public Material growingObstacleMaterial;

    private int growObstacleCount = 0;

    public void Init(Level level)
    {
        instance = this;

        if (visualizer != null)
        {

            visualizer.DestroyLevel(this.level);
        }
        else
        {
            visualizer = new LevelVisualizer();
            visualizer.InitDatabase(projectDatabase, levelDatabase);
        }
        this.level = level;

        graph = LevelToGraph(level);
        nextObstacleBlockIndex = -1;
    }

    public int GemsInLevel()
    {
        int result = 0;
        foreach (Level.Block block in level.blocks)
        {
            if (block.type == Level.BlockType.STRAIGHT_LINE)
            {
                if (block.gemPattern != null)
                {
                    result += block.gemPattern.gems.Length;
                }
            }
        }
        return result;
    }

    public bool IsOnlyStraitBetween(int blockIndex)
    {
        //Debug.Log(nextObstacleBlockIndex);
        if (blockIndex > nextObstacleBlockIndex)
            return false;
        for (int i = blockIndex; i < nextObstacleBlockIndex; i++)
        {
            if (level.blocks[i].type != Level.BlockType.STRAIGHT_LINE)
                return false;
        }
        return true;
    }

    public Level.BlockType GetBlockType(int blockIndex)
    {
        return level.blocks[blockIndex].type;
    }

    public int GetObstaclesCount(int blockIndex)
    {
        var obstacle = level.blocks[blockIndex];
        if (obstacle.type != Level.BlockType.STRAIGHT_LINE)
            return 0;
        if (obstacle.obstacleID == -1)
            return 0;
        return obstacle.ammountOfObstacles;

    }

    public Obstacle GetObstacle(int blockIndex)
    {
        var block = level.blocks[blockIndex];

        if (block.type != Level.BlockType.STRAIGHT_LINE)
            return null;
        if (block.obstacleID == -1)
            return null;
        return block.obstacle;
    }

    public Transform GetObstacleContainer(int blockIndex)
    {
        var obstacle = level.blocks[blockIndex];
        if (obstacle.type != Level.BlockType.STRAIGHT_LINE)
            return null;
        if (obstacle.obstacleID == -1)
            return null;
        return levelContainer.GetChild(blockIndex).Find("obstacles");
    }

    public GameObject GetObstacleObject(int blockIndex, int index)
    {
        var obstacle = level.blocks[blockIndex];
        if (obstacle.type != Level.BlockType.STRAIGHT_LINE)
            return null;
        if (obstacle.obstacleID == -1)
            return null;
        var obstacleContainer = levelContainer.GetChild(blockIndex).Find("obstacles");
        if (obstacleContainer.childCount == 0)
            return null;

        return obstacleContainer.GetChild(index).gameObject;
    }

    public Transform GetBlockTransform(int blockIndex)
    {
        return levelContainer.GetChild(blockIndex).transform;
    }

    public bool HasBlock(int index)
    {
        return index < level.blocks.Length;
    }

    public void SwapObstacle(int blockIndex)
    {

        var currentBlock = level.blocks[blockIndex];
        if (currentBlock.ammountOfObstacles != currentObstacleCount && currentBlock.type == Level.BlockType.STRAIGHT_LINE)
        {
            GameObject oldObstacle = GetObstacleObject(blockIndex, currentObstacleCount);
            currentObstacleCount++;
            Vector3 localPosition = oldObstacle.transform.localPosition;
            Transform obstacleContainer = oldObstacle.transform.parent;
            oldObstacle.SetActive(false);

            //GameObject swapedObstacle = Object.Instantiate(currentBlock.obstacle.detailedPrefab);
            GameObject swapedObstacle = PoolManager.GetPoolByName(currentBlock.obstacle.name + "_OBSTACLE_DETAILED").GetPooledObject();
            if (swapedObstacle == null)
            {
                PoolManager.GetPoolByName(currentBlock.obstacle.name + "_OBSTACLE_DETAILED").ReturnToPoolEverything(true);
                swapedObstacle = PoolManager.GetPoolByName(currentBlock.obstacle.name + "_OBSTACLE_DETAILED").GetPooledObject();
            }
            RestoreObstacle(swapedObstacle, currentBlock.obstacle);
            swapedObstacle.transform.SetParent(obstacleContainer, false);
            swapedObstacle.transform.localPosition = localPosition;
            swapedObstacle.transform.localRotation = new Quaternion(0, 0, 0, 1);

            nextObstaclePosition = swapedObstacle.transform.position;
            nextObstacleBlockIndex = blockIndex;
        }
        else
        {
            //if(currentBlockIndex == level.blocks.Length - 1) return;
            for (int i = blockIndex + 1; i < level.blocks.Length; i++)
            {
                currentBlock = level.blocks[i];
                GameObject oldObstacle = GetObstacleObject(i, 0);
                if (oldObstacle != null)
                {
                    Vector3 localPosition = oldObstacle.transform.localPosition;
                    Transform obstacleContainer = oldObstacle.transform.parent;
                    oldObstacle.SetActive(false);

                    //GameObject swapedObstacle = Object.Instantiate(currentBlock.obstacle.detailedPrefab);
                    GameObject swapedObstacle = PoolManager.GetPoolByName(currentBlock.obstacle.name + "_OBSTACLE_DETAILED").GetPooledObject();
                    if (swapedObstacle == null)
                    {
                        PoolManager.GetPoolByName(currentBlock.obstacle.name + "_OBSTACLE_DETAILED").ReturnToPoolEverything(true);
                        swapedObstacle = PoolManager.GetPoolByName(currentBlock.obstacle.name + "_OBSTACLE_DETAILED").GetPooledObject();
                    }
                    RestoreObstacle(swapedObstacle, currentBlock.obstacle);
                    swapedObstacle.transform.SetParent(obstacleContainer, false);
                    swapedObstacle.transform.localPosition = localPosition;
                    swapedObstacle.transform.localRotation = new Quaternion(0, 0, 0, 1);

                    //swapedObstacle.GetComponent<ObstacleController>().InitPreset(currentPreset);

                    nextObstaclePosition = swapedObstacle.transform.position;
                    nextObstacleBlockIndex = i;

                    currentObstacleCount = 1;
                    return;
                }
            }
            nextObstacleBlockIndex = -1;
        }
    }

    private void RestoreObstacle(GameObject obstacleObject, Obstacle obstacle)
    {
        var index = 0;
        for (int i = 0; i < 15; i++)
        {
            var line = obstacle.lines[i];
            for (int j = 0; j < 15; j++)
            {
                if (line.line[j])
                {
                    var cubeBit = obstacleObject.transform.GetChild(index);
                    cubeBit.localRotation = new Quaternion(0, 0, 0, 1);
                    cubeBit.localPosition = new Vector3(
                        -1.5f + 0.2f * i + 0.1f,
                        3f - 0.2f * j - 0.1f,
                        0f
                    );

                    var collider = cubeBit.gameObject.GetComponent<BoxCollider>();
                    var rigidbody = cubeBit.gameObject.GetComponent<Rigidbody>();
                    collider.isTrigger = true;
                    rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    rigidbody.useGravity = false;
                    cubeBit.gameObject.layer = 0;
                    index++;
                }
            }
        }

    }

    private int currentObstacleCount = 0;

    public bool IsObstacleLastInBlock(int blockIndex)
    {
        return level.blocks[blockIndex].ammountOfObstacles == currentObstacleCount;
    }

    public void VisualizeLevel(ColorPreset preset)
    {
        visualizer.VisualizeLevel(level, levelContainer, false);
    }

    public List<Vector3> LevelToGraph(Level level)
    {
        var graph = new List<Vector3>();
        var rotationY = new Quaternion();
        for (int i = 0; i < level.blocks.Length; i++)
        {
            switch (GetBlockType(i))
            {
                case Level.BlockType.START:
                    graph.Add(new Vector4(0, 0, 20));
                    break;
                case Level.BlockType.STRAIGHT_LINE:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(0, 0, 20));
                    break;
                case Level.BlockType.TURN_LEFT:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(-5, 0, 5));
                    rotationY = Quaternion.Euler(0, rotationY.eulerAngles.y - 90, 0);
                    break;
                case Level.BlockType.TURN_RIGHT:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(5, 0, 5));
                    rotationY = Quaternion.Euler(0, rotationY.eulerAngles.y + 90, 0);
                    break;
                case Level.BlockType.DESCENDING_RAILS:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(0, -1, 10));
                    break;
                case Level.BlockType.ASCENDING_RAILS:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(0, 1, 10));
                    break;
                case Level.BlockType.TRAMPLIN:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(0, 0, 5));
                    break;
                case Level.BlockType.RAILS:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(0, 0, 5));
                    break;
                case Level.BlockType.FINISH:
                    graph.Add(graph[i - 1] + rotationY * new Vector3(0, 0, 15));
                    break;
            }
            //Debug.Log("<color=red>" + graph[i] + "</color>");
        }

        return graph;
    }

    public void GrowObstacle(int blockIndex)
    {

        var copy = PoolManager.GetPoolByName(GetObstacle(blockIndex).name + "_OBSTACLE").GetPooledObject();//GameObject.Instantiate(GetObstacle(blockIndex).prefab);
        copy.transform.SetParent(GetObstacleContainer(blockIndex));

        if (level.blocks[blockIndex].ammountOfObstacles != 1)
        {

            copy.transform.localPosition = GetObstacleObject(blockIndex, growObstacleCount).transform.localPosition;
            growObstacleCount++;
            if (growObstacleCount == level.blocks[blockIndex].ammountOfObstacles)
            {
                growObstacleCount = 0;
            }
        }
        else
        {
            copy.transform.localPosition = GetObstacleObject(blockIndex, 0).transform.localPosition;
        }

        copy.transform.localRotation = Quaternion.Euler(0, 0, 0);
        copy.transform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        StartCoroutine(GrowObstacle(copy));

        SwapObstacle(blockIndex);
    }

    public void GrowRailPart(GameObject part)
    {
        StartCoroutine(GrowRailPartCoroutine(part));
    }

    IEnumerator GrowObstacle(GameObject obstacle)
    {
        var time = 0f;
        Material surfaceMaterial = obstacle.transform.GetChild(0).gameObject.GetComponent<Renderer>().material;
        Material bodyMaterial = obstacle.transform.GetChild(1).gameObject.GetComponent<Renderer>().material;
        // TODO skin projection color
        Color color = GameController.instance.skin.color;
        Color differenceColor = Color.white - color;
        var easing = Ease.GetFunction(Ease.Type.QuadOut);
        Color tempColor;
        surfaceMaterial.renderQueue = 3001;
        bodyMaterial.renderQueue = 3001;
        Vector3 one = new Vector3(1, 1, 1.1f);
        while (time < 1f)
        {
            time += Time.deltaTime;
            tempColor = color + differenceColor * easing(time);
            tempColor.a = 1 - easing(time);
            surfaceMaterial.SetColor(Shader.PropertyToID("_main_color"), tempColor);
            bodyMaterial.SetColor(Shader.PropertyToID("_main_color"), tempColor);
            obstacle.transform.localScale = one + one * easing(time) / 2;
            yield return new WaitForFixedUpdate();
        }
        obstacle.transform.localScale = Vector3.one;
        obstacle.SetActive(false);
        surfaceMaterial.renderQueue = 3000;
        bodyMaterial.renderQueue = 3000;
        surfaceMaterial.SetColor(Shader.PropertyToID("_main_color"), GameController.instance.currentPreset.obstacleSurface);
        bodyMaterial.SetColor(Shader.PropertyToID("_main_color"), GameController.instance.currentPreset.obstacleBody);

    }

    IEnumerator GrowRailPartCoroutine(GameObject railPart)
    {
        var time = 0f;
        var easing = Ease.GetFunction(Ease.Type.QuadOut);
        Material material = railPart.GetComponent<Renderer>().material;
        Color color = GameController.instance.skin.color;
        Color differenceColor = Color.white - color;
        Color tempColor;
        while (time < 2f)
        {
            time += Time.deltaTime;
            if (time < 0.25f)
            {
                railPart.transform.localScale = Vector3.one + Vector3.one * easing(time * 4) / 2;
            }
            tempColor = color + differenceColor * easing(time / 2);
            tempColor.a = 1 - easing(time / 2);
            material.SetColor(Shader.PropertyToID("_main_color"), tempColor);
            yield return new WaitForFixedUpdate();
        }
        railPart.transform.localScale = Vector3.one;
        railPart.SetActive(false);
    }

    public Bait GetBait()
    {
        return level.bait;
    }
}