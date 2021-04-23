using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistakeTrigger : MonoBehaviour
{
    public ObstacleTrigger nextObstacle;
    public int chance;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var randomchance = Random.Range(1, chance);
            if (randomchance == 1)
            {
                Debug.Log("oops I set myself to " + nextObstacle.targetSize);
                var opposite = 100f - nextObstacle.targetSize;
                other.GetComponentInParent<Enemy>().SetTargetSize(opposite);
            }
        }
    }
}
