#pragma warning disable 414

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaitController : MonoBehaviour
{
    public LevelDatabase levelDatabase;
    public Animator amimator;

    public LevelController levelController;
    public ParticleSystem confetti;
    public ParticleSystem trails;

    public bool isMoving = false;

    private Vector3 currentBlockEndPoint = Vector3.zero;
    private Vector3 lastBlockEndPoint = Vector3.zero;

    private bool gameStartAnimationValue = true;
    private bool evading = false;

    private float lastDistanceToPointSqr = float.PositiveInfinity;
    private float distanceBetweenBlocks;

    private int gameStartAnimation;
    private int needToRotateY = 0;
    private int currentBlock = -1;
    private int speed = 12;
    private int currentObstacle = 0;


    public void PlayParticles()
    {
        speed = 10;
        confetti.Play();
        trails.Play();
    }

    void Start()
    {
        gameStartAnimation = Animator.StringToHash("Game Start");

    }

    public void Init()
    {
        transform.position = Vector3.zero;
        transform.rotation = new Quaternion();
        transform.GetChild(0).position = Vector3.zero;
        transform.GetChild(0).GetChild(0).transform.position = Vector3.zero;
        transform.GetChild(0).GetChild(0).transform.localScale = Vector3.one;
        currentBlock = -1;
        needToRotateY = 0;
        speed = 12;
        lastBlockEndPoint = Vector3.zero;
        currentBlockEndPoint = Vector3.zero;
        isMoving = false;
        lastDistanceToPointSqr = float.PositiveInfinity;
        GameController.instance.catchedBait = false;
    }

    public void StartGame()
    {
        lastDistanceToPointSqr = float.PositiveInfinity;
        gameStartAnimationValue = true;
        if (levelDatabase.levels[GameController.instance.currentLevel].type == Level.LevelType.JELLY_RUSH)
        {
            amimator.SetBool(gameStartAnimation, gameStartAnimationValue);
            currentBlock = -1;
            NextBlock(transform.rotation);
        }

    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            if (gameStartAnimationValue == true)
            {
                gameStartAnimationValue = false;
                amimator.SetBool(gameStartAnimation, gameStartAnimationValue);
            }
            Vector3 position;
            Quaternion rotation;

            Move(1f, transform.position, transform.rotation, out position, out rotation);

            transform.position = position;
            transform.rotation = rotation;
        }
    }

    IEnumerator EvadeObstacle(Vector3 point, float obstaclesCount)
    {
        Transform baitEvasion = transform.GetChild(0);
        var easing = Watermelon.Ease.GetFunction(Watermelon.Ease.Type.SineInOut);
        var overAllTime = (3f + 3f * obstaclesCount) / 12f;
        var time = 0f;
        while (time < overAllTime)
        {
            time += Time.deltaTime;
            if (time < 0.25f)
            {
                var p = time * 4;
                baitEvasion.localPosition = new Vector3(easing(p) * point.x, easing(p) * point.y, 0);
            }
            else if (time >= overAllTime - 0.25f)
            {
                var p = (overAllTime - time) * 4f;
                baitEvasion.localPosition = new Vector3(easing(p) * point.x, easing(p) * point.y, 0);
            }
            else
            {
                baitEvasion.localPosition = point;
            }
            yield return new WaitForFixedUpdate();
        }
        baitEvasion.localPosition = Vector3.zero;
        evading = false;
    }

    private void Move(float distancePersentage, Vector3 currentPos, Quaternion currentRot, out Vector3 position, out Quaternion rotation)
    {
        position = currentPos;
        rotation = currentRot;
        float distanceToPointSqr = (currentBlockEndPoint - position).sqrMagnitude;

        float oneBlock = 1f / levelController.graph.Count;

        GameController.instance.UpdateBaitPosition(currentBlock * oneBlock + oneBlock - Mathf.Sqrt(distanceToPointSqr) / distanceBetweenBlocks * oneBlock);

        if (!evading)
        {
            var obstacleObject = levelController.GetObstacleObject(currentBlock, 0);
            if (obstacleObject != null)
            {
                var distanceToObstacle = (position - obstacleObject.transform.position).magnitude;
                if (distanceToObstacle < 3f)
                {
                    evading = true;
                    var obstacle = levelController.GetObstacle(currentBlock);
                    var evasionPoint = new Vector3(-1.5f + obstacle.baitPoint.x * 0.2f + 0.1f, 3f - obstacle.baitPoint.y * 0.2f - 0.2f);
                    StartCoroutine(EvadeObstacle(evasionPoint, levelController.GetObstaclesCount(currentBlock)));
                }
            }
        }

        if (lastDistanceToPointSqr < distanceToPointSqr)
        {
            Debug.Log("position = " + position);
            Debug.Log("currentBlockEndPoint = " + currentBlockEndPoint);
            Debug.Log("currentBlock = " + currentBlock);
            Debug.Log("lastDistanceToPointSqr = " + lastDistanceToPointSqr);
            Debug.Log("distanceToPointSqr = " + distanceToPointSqr);

            NextBlock(rotation);
            if (!isMoving)
                return;
            position = lastBlockEndPoint;
            Move(1f, position, rotation, out position, out rotation);
            //Debug.Log("Statter");
            return;
        }

        Vector3 pathToMove = Time.deltaTime * speed * distancePersentage * Vector3.forward;
        float distanceToMoveSqr = pathToMove.sqrMagnitude;

        if (distanceToMoveSqr < distanceToPointSqr)
        {
            if (needToRotateY != 0)
            {
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y + (needToRotateY / 7.85398163397f * Mathf.Sqrt(distanceToMoveSqr)) * distancePersentage, 0);
            }
            pathToMove = rotation * pathToMove;
            position = position + pathToMove;

            if (levelController.GetBlockType(currentBlock) == Level.BlockType.ASCENDING_RAILS || levelController.GetBlockType(currentBlock) == Level.BlockType.DESCENDING_RAILS)
            {

                if (currentPos.y != currentBlockEndPoint.y)
                {
                    var k = (new Vector2(position.x, position.z) - new Vector2(lastBlockEndPoint.x, lastBlockEndPoint.z)).magnitude /
                            (new Vector2(lastBlockEndPoint.x, lastBlockEndPoint.z) - new Vector2(currentBlockEndPoint.x, currentBlockEndPoint.z)).magnitude;
                    if (currentBlockEndPoint.y > lastBlockEndPoint.y)
                    {
                        position.y = lastBlockEndPoint.y + k;
                    }
                    else
                    {
                        position.y = lastBlockEndPoint.y - k;
                    }

                    //Debug.Log(position.y + " " + currentBlockEndPoint.y);
                }
            }

            lastDistanceToPointSqr = distanceToPointSqr;
        }
        else
        {
            if (GameController.instance.gameStage == GameStage.FINISH)
            {
                return;
            }
            if (needToRotateY != 0)
            {
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y + (needToRotateY / 7.85398163397f * Mathf.Sqrt(distanceToPointSqr)) * distancePersentage, 0);
            }
            pathToMove = rotation * pathToMove;

            position = currentBlockEndPoint;
            //Debug.Log(position.y + " !!!!!!! " + currentBlockEndPoint.y);

            float roundedY = rotation.eulerAngles.y;
            var mod = roundedY % 90f;
            if (mod > 10)
            {
                roundedY += 90f - roundedY % 90f;
            }
            else
            {
                roundedY -= roundedY % 90f;
            }

            rotation = Quaternion.Euler(0, roundedY, 0);


            NextBlock(rotation);
            if (!isMoving)
                return;

            var distanceToMove = Mathf.Sqrt(distanceToMoveSqr);
            var distanceToPoint = Mathf.Sqrt(distanceToPointSqr);

            Move(1 - distanceToPoint / distanceToMove, position, rotation, out position, out rotation);
        }
    }

    private void NextBlock(Quaternion rotation)
    {
        currentBlock++;
        currentObstacle = 0;
        if (!levelController.HasBlock(currentBlock))
        {
            isMoving = false;
            amimator.SetTrigger("Finish");
            GameController.instance.baitFinished = true;
            return;
        }

        if (currentBlock != 0)
        {
            distanceBetweenBlocks = (levelController.graph[currentBlock] - levelController.graph[currentBlock - 1]).magnitude;
        }
        else
        {
            distanceBetweenBlocks = (levelController.graph[0] - levelController.graph[1]).magnitude;
        }

        lastDistanceToPointSqr = float.PositiveInfinity;

        lastBlockEndPoint = currentBlockEndPoint;

        currentBlockEndPoint = levelController.graph[currentBlock];

        switch (levelController.GetBlockType(currentBlock))
        {
            case Level.BlockType.TURN_LEFT:
                needToRotateY = -90;
                break;
            case Level.BlockType.TURN_RIGHT:
                needToRotateY = 90;
                break;
            default:
                needToRotateY = 0;
                break;
        }
    }
}