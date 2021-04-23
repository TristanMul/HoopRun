using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    //not finished, shut up
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().force *= 2;
        }
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().force *= 2;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().force /= 2;
        }
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().force /= 2;
        }
    }
}
