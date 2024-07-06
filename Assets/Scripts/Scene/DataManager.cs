using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Starter manualControl;
    public DataFromServer dataFromServer;

    void Awake()
    {
        // Initialize the instances if necessary
        manualControl = new Starter();
        dataFromServer = new DataFromServer();
    }
}
