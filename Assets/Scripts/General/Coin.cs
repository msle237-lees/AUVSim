using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    // Start is called before the first frame update
    public PointCounter pointCounter;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Destroy the coin object
            Destroy(gameObject);

            // Increment the point counter
            pointCounter.AddPoint();
        }
    }
}
