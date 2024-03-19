using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {   
        Debug.Log("Collision with " + collision.gameObject.tag);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Untagged")) {
            Debug.Log("Collision with " + other.gameObject.tag);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
