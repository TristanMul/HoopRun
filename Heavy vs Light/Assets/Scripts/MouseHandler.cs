using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHandler : MonoBehaviour, IDragHandler
{
    public Player player;

    public void OnDrag(PointerEventData data)
    {
        Debug.Log("test");
        player.size += data.delta.y / 2f;
    }
}
