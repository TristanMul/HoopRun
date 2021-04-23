using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushTrigger : MonoBehaviour
{

    bool isPlayer;
    Player p;
    Enemy e;
    List<GameObject> gameObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;

        if (transform.parent.CompareTag("Player"))
        {
            isPlayer = true;
            p = GetComponentInParent<Player>();
        }
        else if (transform.parent.CompareTag("Enemy"))
        {
            isPlayer = false;
            e = GetComponentInParent<Enemy>();
        }
        else
        {
            Debug.Log("Parent not player or enemy");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Push Obs"))
        {
            gameObjects.Add(other.gameObject);
            CheckList();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Push Obs"))
        {
            gameObjects.Remove(other.gameObject);
            CheckList();
        }
    }

    private void CheckList()
    {
        if (isPlayer)
        {
            p.SetPushing(gameObjects.Count > 0);
        }
        else
        {
            e.SetPushing(gameObjects.Count > 0);
        }
    }
}
