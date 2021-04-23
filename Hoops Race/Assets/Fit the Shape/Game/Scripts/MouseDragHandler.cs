using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseDragHandler : MonoBehaviour, IDragHandler
{
    public PlayerController playerController;

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnDrag(PointerEventData data)
    {
        if (GameController.instance.gameStage == GameStage.START && !GameController.instance.settingsOpen && !GameController.instance.shopOpen)
        {
            GameController.instance.StartGame();
        }

        playerController.Rescale(data.delta.y / 2f);
    }
}