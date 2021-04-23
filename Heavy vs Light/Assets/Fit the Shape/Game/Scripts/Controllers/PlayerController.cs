#pragma warning disable 414

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Watermelon;

public class PlayerController : MonoBehaviour
{
    //public CharacterCustomization characterCustomization;
    //public float size;


    public Watermelon.AudioSettings audioSettings;
    public LevelController levelController;

    private Vector3 currentBlockEndPoint;

    private Rigidbody rb;

    public ParticleSystem turboTrailParticleSystem;
    public ParticleSystem rombParticleSystem;
    public ParticleSystem finishParticleSystem;
    public ParticleSystem turboBurstParticleSystem;
    public ParticleSystem turboBurstParticleSystem1;
    public ParticleSystem turboBurstParticleSystem2;
    public ParticleSystem turboBurstParticleSystem3;

    public Animator playerAnimator;
    public Transform projection;
    public CameraController cameraController;

    [Range(10, 100), Tooltip("Speed of player rescaling")]
    public float scalingSpeed = 100;
    public int catchedGems = 0;

    private bool start = false;
    private bool tramplin = false;
    private bool vibrating = false;
    private bool hittingObstacle;
    private bool catchedBait = false;

    private float cameraStartTransition = 0f;
    private float standartSpeed = 10;
    private float currentSpeed = 0;

    private float minScaleX = 0.25f;
    private float maxScaleX = 1.75f;
    private float minScaleY = 0.25f;
    private float maxScaleY = 1.75f;

    private float finishRotation = 0f;
    private float distanceBetweenBlocks;
    private float TURBO_DURATION = 10;
    private float turboTime = 0;

    private int hittingObstacleAnimation;
    private int needToRotateY = 0;
    private int currentBlock = -1;


    public void Init()
    {
        hittingObstacleAnimation = Animator.StringToHash("Hitting Obstacle Animation");
        rb = GetComponent<Rigidbody>();
        rb.transform.position = new Vector3(0, 0, 2);
        rb.transform.localScale = new Vector3(1.15f, 0.85f, 1f);
        rb.transform.rotation = new Quaternion(0, 0, 0, 1);
        currentBlock = -1;
        NextBlock(rb.transform.rotation);

        cameraStartTransition = 0f;

        turboTime = 0;

        currentSpeed = 0;
        hittingObstacle = false;
        catchedBait = false;
        finishRotation = 0f;
        catchedGems = 0;

        InitProjection();

        var emission = turboTrailParticleSystem.emission;
        emission.rateOverTime = 0;
        start = true;
    }

    public void Revive()
    {
        var last = currentBlockEndPoint;

        NextBlock(rb.rotation);
        rb.position = last + rb.rotation * Vector3.forward;
        currentSpeed = 0;
        hittingObstacle = false;
        turboTime = 0;
        UpdateParitcle();
    }

    public void InitProjection()
    {
        var projectionRenderer = projection.Find("projection_back").gameObject.GetComponent<Renderer>();
        var projectionBody = projection.Find("projection_body").gameObject.GetComponent<Renderer>();

        //var projectionColor = projectionRenderer.material.GetColor(Shader.PropertyToID("_main_color"));
        //Debug.LogError(projectionColor);
        Color projectionColor = GameController.instance.skin.color;
        projectionColor.a = 0.7f;

        Color projectionBackColor = GameController.instance.skin.color;
        projectionBackColor.a = 0.3f;

        projectionRenderer.material.SetColor(Shader.PropertyToID("_main_color"), projectionColor);
        projectionBody.material.SetColor(Shader.PropertyToID("_main_color"), projectionBackColor);
    }

    public void Rescale(float delta)
    {
        if (GameController.instance.gameStage == GameStage.GAME)
        {
            var amount = delta / scalingSpeed;
            var newScaleY = transform.localScale.y + amount;

            newScaleY = newScaleY < minScaleY ? minScaleY : newScaleY;
            newScaleY = newScaleY > maxScaleY ? maxScaleY : newScaleY;

            var newScaleX = transform.localScale.x - amount;

            newScaleX = newScaleX < minScaleX ? minScaleX : newScaleX;
            newScaleX = newScaleX > maxScaleX ? maxScaleX : newScaleX;

            transform.localScale = new Vector3(newScaleX, newScaleY, 1f);
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            var shape = turboTrailParticleSystem.shape;
            //shape.position = new Vector3(0, newScaleY / 2, 0);
            shape.scale = new Vector3(newScaleX, newScaleY, 0.4f);
        }
    }

    private void UpdateParitcle()
    {
        var emission = turboTrailParticleSystem.emission;
        emission.rateOverTime = GameController.instance.turboLevel;
    }

    private void PlayParticle()
    {
        rombParticleSystem.Play();
    }

    private void FixedUpdate()
    {
        if (GameController.instance.gameStage == GameStage.GAME)
        {
            if (start)
            {
                playerAnimator.SetTrigger("Start");
                start = false;
            }

            if (GameController.instance.turboLevel == 5)
            {
                turboTime -= Time.deltaTime;
                if (turboTime < 0)
                {
                    turboTime = 0;
                    GameController.instance.turboLevel = 0;
                    GameController.instance.ChangeTurbo();
                    StartCoroutine(cameraController.FovJump(-5f, 1f, 0, 0f));
                    UpdateParitcle();
                }
            }

            Vector3 position;
            Quaternion rotation;
            /* if(hittingObstacle){
               
                if(playerAnimator.GetCurrentAnimatorStateInfo(0).length <
                    playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime){
                        
                        hittingObstacle = false;
                        playerAnimator.SetBool(hittingObstacleAnimation, false);
                        currentSpeed = 5;

                        Move(1f, rb.position, rb.rotation, out position, out rotation);
                        rb.MovePosition(position);
                        rb.rotation = rotation;
                    }
            } else*/
            {
                Move(1f, rb.position, rb.rotation, out position, out rotation);
                rb.MovePosition(position);
                rb.rotation = rotation;
            }
            if (projection.gameObject.activeInHierarchy)
            {
                var zScale = (levelController.nextObstaclePosition - rb.position).magnitude;
                projection.localScale = new Vector3(1, 1, zScale);
            }

            /*
            if (size > 100)
            {
                size = 100;
            }
            if (size < 0)
            {
                size = 0;
            }
            //CalculateCamera();
            characterCustomization.SetBodyShape(CharacterCustomization.BodyShapeType.Fat, size * 2.5f);
            characterCustomization.SetBodyShape(CharacterCustomization.BodyShapeType.Muscles, size);
            characterCustomization.SetBodyShape(CharacterCustomization.BodyShapeType.Thin, 100-size);
            characterCustomization.SetHeight(size/250);
            */


        }
        else if (GameController.instance.gameStage == GameStage.FINISH)
        {
            //CalculateCamera();
        }
        if (vibrating)
        {
            vibrating = false;
        }

    }

    private float lastDistanceToPointSqr = float.PositiveInfinity;

    private void Move(float distancePersentage, Vector3 currentPos, Quaternion currentRot, out Vector3 position, out Quaternion rotation)
    {
        position = currentPos;
        rotation = currentRot;

        if (tramplin)
        {
            tramplin = false;
            NextBlock(rotation);
            Move(1f, position, rotation, out position, out rotation);
            return;
        }

        float distanceToPointSqr = (currentBlockEndPoint - position).sqrMagnitude;

        float oneBlock = 1f / levelController.graph.Count;
        GameController.instance.UpdatePlayerPosition(currentBlock * oneBlock + oneBlock - Mathf.Sqrt(distanceToPointSqr) / distanceBetweenBlocks * oneBlock);

        if (lastDistanceToPointSqr <= distanceToPointSqr && !hittingObstacle)
        {
            Debug.Log("NextBlock " + hittingObstacle.ToString());
            NextBlock(rotation);
            Move(1f, position, rotation, out position, out rotation);
            return;
        }
        float targetSpeed = standartSpeed + (GameController.instance.turboLevel == 5 ? GameController.instance.turboLevel * 1.5f + 1 : GameController.instance.turboLevel * 1.5f);
        if (currentSpeed != targetSpeed)
        {
            var persentage = currentSpeed / targetSpeed + 0.1f;
            currentSpeed += persentage * 50 * Time.deltaTime;
            if (currentSpeed >= targetSpeed)
                currentSpeed = targetSpeed;
        }
        Vector3 pathToMove = Time.deltaTime * currentSpeed * distancePersentage * Vector3.forward;
        float distanceToMoveSqr = pathToMove.sqrMagnitude;

        if (distanceToMoveSqr < distanceToPointSqr)
        {
            if (needToRotateY != 0)
            {
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y + (needToRotateY / 7.85398163397f * Mathf.Sqrt(distanceToMoveSqr)) * distancePersentage, 0);
            }
            pathToMove = rotation * pathToMove;

            position = position + pathToMove;
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
            position = new Vector3(currentBlockEndPoint.x, position.y, currentBlockEndPoint.z);

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

            var distanceToMove = Mathf.Sqrt(distanceToMoveSqr); // var distanceToMove = (currentBlockEndPoint - currentPosition).magnitude;
            var distanceToPoint = Mathf.Sqrt(distanceToPointSqr);

            Move(1 - distanceToPoint / distanceToMove, position, rotation, out position, out rotation);
        }
    }

    private void NextBlock(Quaternion rotation)
    {
        currentBlock++;
        if (currentBlock == levelController.graph.Count)
        {
            GameController.instance.Finish();
            UpdateParitcle();
            finishRotation = 0f;
            finishParticleSystem.Play();
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

        projection.gameObject.SetActive(levelController.IsOnlyStraitBetween(currentBlock));
        lastDistanceToPointSqr = float.PositiveInfinity;

        currentBlockEndPoint = levelController.graph[currentBlock];
        switch (levelController.GetBlockType(currentBlock))
        {
            case Level.BlockType.TURN_LEFT:
                if (GameController.instance.isSound)
                {
                    AudioController.PlaySound(audioSettings.sounds.turn1, AudioController.AudioType.Sound, 1);
                    AudioController.PlaySound(audioSettings.sounds.turn2, AudioController.AudioType.Sound, 1);
                }
                needToRotateY = -90;
                break;
            case Level.BlockType.TURN_RIGHT:
                if (GameController.instance.isSound)
                {
                    AudioController.PlaySound(audioSettings.sounds.turn1, AudioController.AudioType.Sound, 1);
                    AudioController.PlaySound(audioSettings.sounds.turn2, AudioController.AudioType.Sound, 1);
                }
                needToRotateY = 90;
                break;
            default:
                needToRotateY = 0;
                break;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (hittingObstacle)
        {
            StartCoroutine(WaitForNull());
        }
    }

    IEnumerator WaitForNull()
    {
        yield return new WaitForSeconds(0.2f);
        hittingObstacle = false;
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        yield return new WaitForFixedUpdate();
        switch (other.gameObject.tag)
        {
            case "Obstacle Part":
                {
                    //hittingObstacle = false;
                    if (GameController.instance.turboLevel != 5)
                    {
                        if (other.attachedRigidbody.constraints != RigidbodyConstraints.None)
                        {
                            other.isTrigger = false;
                            other.attachedRigidbody.constraints = RigidbodyConstraints.None;
                            other.attachedRigidbody.useGravity = true;
                            other.attachedRigidbody.AddForce(rb.rotation * new Vector3(10 * UnityEngine.Random.value, 50 * (UnityEngine.Random.value + 1), 50 * (UnityEngine.Random.value + 1)));
                            other.attachedRigidbody.AddTorque(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                            other.gameObject.layer = 9;

                            if (!hittingObstacle)
                            {
                                currentSpeed = 0;
                                hittingObstacle = true;
                                StartCoroutine(cameraController.FovJump(-GameController.instance.turboLevel, 0.5f, 0, 0f));
                                GameController.instance.turboLevel = 0;
                                UpdateParitcle();
                                GameController.instance.ChangeTurbo();
                                //playerAnimator.SetBool(hittingObstacleAnimation, hittingObstacle);
                                rb.AddForce(rb.rotation * new Vector3(0, 50, -300));

                            }
                        }
                    }
                    else
                    {
                    }
                    break;
                }
            case "Obstacle":
                {
                    if (hittingObstacle)
                        break;

                    PlayParticle();
                    if (!vibrating && GameController.instance.isHaptic)
                    {
                        Watermelon.Vibration.Vibrate(audioSettings.vibrations.longVibration);
                        vibrating = true;
                    }

                    if (GameController.instance.turboLevel != 5 || GameController.instance.GetCurrentLevelType() == Level.LevelType.GEM_RUSH)
                    {
                        if (GameController.instance.isSound)
                        {
                            AudioController.PlaySound(audioSettings.sounds.obstacle, AudioController.AudioType.Sound, 0.8f, 0.7f + 0.05f * GameController.instance.turboLevel);
                        }
                        if (levelController.IsObstacleLastInBlock(currentBlock))
                        {
                            //Debug.Log("BEFORE FOV");
                            StartCoroutine(cameraController.FovJump(2f, 0.5f, -1, 0.25f));
                            if (GameController.instance.GetCurrentLevelType() == Level.LevelType.JELLY_RUSH)
                            {
                                GameController.instance.turboLevel++;
                                if (GameController.instance.turboLevel == 5)
                                {
                                    turboTime = TURBO_DURATION;
                                }
                                UpdateParitcle();
                                GameController.instance.ChangeTurbo();
                            }
                            else
                            {
                                if (GameController.instance.turboLevel != 4)
                                {
                                    GameController.instance.turboLevel++;
                                }
                            }
                        }
                        levelController.GrowObstacle(currentBlock);
                    }
                    else
                    {
                        //Debug.Log("Explode");
                        if (GameController.instance.isSound)
                        {
                            AudioController.PlaySound(audioSettings.sounds.explosion, AudioController.AudioType.Sound, 1f, 0.8f);
                        }
                        turboBurstParticleSystem.Play();
                        turboBurstParticleSystem1.Play();
                        turboBurstParticleSystem2.Play();
                        turboBurstParticleSystem3.Play();
                        ExplodeObstacle(other.transform);
                        levelController.SwapObstacle(currentBlock);
                    }
                    projection.gameObject.SetActive(levelController.IsOnlyStraitBetween(currentBlock));
                    break;
                }
            case "Bait":
                {
                    if (currentBlock != 0)
                    {
                        if (!GameController.instance.baitFinished && !GameController.instance.catchedBait)
                        {
                            var baitAnim = other.transform.GetChild(0).GetChild(0);
                            GameController.instance.catchedBait = true;
                            GameController.instance.PlayBaitParticles();
                            if (GameController.instance.isSound)
                            {
                                AudioController.PlaySound(audioSettings.sounds.bait, AudioController.AudioType.Sound, 1f, 1f);
                            }

                            if (baitAnim.childCount != 0)
                            {
                                baitAnim.GetChild(0).gameObject.SetActive(false);
                                Destroy(baitAnim.GetChild(0).gameObject);
                                catchedBait = true;
                            }
                        }
                    }
                    break;
                }
            case "Gem":
                {
                    if (GameController.instance.isSound)
                    {
                        AudioController.PlaySound(audioSettings.sounds.gem, AudioController.AudioType.Sound, 0.8f, 1.2f);
                    }
                    if (!vibrating && GameController.instance.isHaptic)
                    {
                        Watermelon.Vibration.Vibrate(audioSettings.vibrations.shortVibration);
                        vibrating = true;
                    }
                    other.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    other.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    catchedGems++;
                    GameController.instance.AddGems(1);
                    yield return new WaitForSeconds(2);
                    other.transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case "Tramplin":
                {
                    tramplin = true;
                    rb.AddForce(0, 125 - GameController.instance.turboLevel * 10, 0);
                    break;
                }
            case "Game Over":
                {
                    GameController.instance.GameOver();
                    break;
                }
            case "Rail Part":
                {
                    if (!vibrating && GameController.instance.isHaptic)
                    {
                        Watermelon.Vibration.Vibrate(audioSettings.vibrations.shortVibration);
                        vibrating = true;
                    }
                    levelController.GrowRailPart(other.gameObject);
                    break;
                }
        }
    }

    private void ExplodeObstacle(Transform parent)
    {
        foreach (Transform part in parent)
        {
            var partCollider = part.gameObject.GetComponent<BoxCollider>();
            partCollider.isTrigger = false;
            partCollider.attachedRigidbody.constraints = RigidbodyConstraints.None;
            partCollider.attachedRigidbody.useGravity = true;
            //partCollider.attachedRigidbody.AddForce(rb.rotation * new Vector3(10 * UnityEngine.Random.value, 50 * (UnityEngine.Random.value + 1), 100 * (UnityEngine.Random.value + 1)));
            partCollider.attachedRigidbody.AddForce(new Vector3(
                50 * (1 + UnityEngine.Random.value) * part.localPosition.x,

                Mathf.Clamp(400 * (1 + UnityEngine.Random.value) * part.localPosition.y, 200, 400),
                0
            ));

            partCollider.attachedRigidbody.AddTorque((-0.5f + UnityEngine.Random.value) * 5, (-0.5f + UnityEngine.Random.value) * 5, (-0.5f + UnityEngine.Random.value) * 5);
            partCollider.gameObject.layer = 9;
        }
    }
}