using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Vector3 cameraPositionOffsetStart = new Vector3(7f, 2f, -7f);
    public Vector3 cameraPositionOffsetGame = new Vector3(2f, 2.5f, -9f);

    public Vector3 cameraRotationOffsetStart = new Vector3(15f, -50f, 0f);
    public Vector3 cameraRotationOffsetGame = new Vector3(3, -10, 0);

    private Vector3 cameraPositionOffsetStartTransition;
    private Vector3 cameraRotationOffsetStartTransition;

    private Camera mainCamera;

    private float cameraStartTransition = 0f;
    private float cameraOffset = 0;
    private float finishRotation = 0f;

    private bool changing = false;

    public IEnumerator FovJump(float offset1, float duration1, float offset2, float duration2)
    {
        while (changing)
            yield return new WaitForFixedUpdate();

        changing = true;
        var isFov = true;
        var time = 0f;
        var oldFov = mainCamera.fieldOfView;

        while (isFov)
        {
            time += Time.deltaTime;
            if (time < duration1)
            {
                mainCamera.fieldOfView += offset1 * Time.deltaTime / duration1;
            }
            else if (time < duration1 + duration2)
            {
                mainCamera.fieldOfView += offset2 * Time.deltaTime / duration2;
            }
            else
            {
                mainCamera.fieldOfView = oldFov + offset1 + offset2;
                isFov = false;
            }
            yield return new WaitForFixedUpdate();
        }

        changing = false;
    }

    void Start()
    {
        cameraPositionOffsetStartTransition = cameraPositionOffsetGame - cameraPositionOffsetStart;
        cameraRotationOffsetStartTransition = cameraRotationOffsetGame - cameraRotationOffsetStart;
        mainCamera = GetComponent<Camera>();
    }

    public void Init()
    {
        cameraStartTransition = 0f;
        cameraOffset = 0;
        finishRotation = 0f;
        mainCamera.fieldOfView = 60;
    }

    void Update()
    {
        switch (GameController.instance.gameStage)
        {
            case GameStage.START:
                transform.position = player.position + cameraPositionOffsetStart;
                transform.rotation = player.rotation * Quaternion.Euler(cameraRotationOffsetStart);
                break;
            case GameStage.GAME:
                if (cameraStartTransition < 1f)
                {
                    cameraStartTransition += Time.deltaTime;
                    transform.position = player.position + cameraPositionOffsetStart + cameraStartTransition * cameraPositionOffsetStartTransition;
                    transform.rotation = player.rotation * Quaternion.Euler(cameraRotationOffsetStart + cameraStartTransition * cameraRotationOffsetStartTransition);
                }
                else
                {
                    var diff = cameraOffset - GameController.instance.turboLevel / 2f;
                    if (diff > 0)
                    {
                        cameraOffset -= Time.deltaTime;
                        diff -= Time.deltaTime;
                        if (diff < 0)
                        {
                            cameraOffset = GameController.instance.turboLevel / 2f;
                        }
                    }
                    else if (diff < 0)
                    {
                        cameraOffset += Time.deltaTime * 5;
                        diff += Time.deltaTime * 5;
                        if (diff > 0)
                        {
                            cameraOffset = GameController.instance.turboLevel / 2f;
                        }
                    }
                    var currentCameraPositionOffsetGame = cameraPositionOffsetGame + new Vector3(0, 0, -cameraOffset);
                    transform.position = player.position + player.rotation * currentCameraPositionOffsetGame;
                    transform.rotation = player.rotation * Quaternion.Euler(cameraRotationOffsetGame);
                }
                break;

            case GameStage.FINISH:

                if (cameraOffset > 0.1f)
                    cameraOffset -= Time.deltaTime;
                if (cameraOffset < 0)
                    cameraOffset = 0;
                transform.position = player.position + Quaternion.Euler(0, -finishRotation, 0) * player.rotation * (cameraPositionOffsetGame + new Vector3(0, 0, -cameraOffset));
                transform.rotation = player.rotation * Quaternion.Euler(cameraRotationOffsetGame + new Vector3(0, -finishRotation, 0));
                finishRotation += Time.deltaTime * 10;
                break;
        }
    }
}