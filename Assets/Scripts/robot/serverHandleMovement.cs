using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using System.Threading;

public class handleMovement : MonoBehaviour
{
    public Rigidbody rb; // Assign to the Sub with the Rigidbody
    public GameObject gate;
    public GameObject pole;
    private float XAccelLast = 0.0f;
    private float YAccelLast = 0.0f;
    private float ZAccelLast = 0.0f;
    private float XGyroLast = 0.0f;
    private float YGyroLast = 0.0f;
    private float ZGyroLast = 0.0f;
    private float X;
    private float Y;
    private float Z;
    private float Rx;
    private float Ry;
    private float Rz;
    private float claw;
    private bool torp1;
    private bool torp2;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetDataFromServer());
        StartCoroutine(SendDataToServer());
    }

    // Update is called once per frame
    void Update()
    {
        // Move the Sub
        rb.transform.position = new Vector3(X, Y, Z);
        rb.transform.rotation = Quaternion.Euler(Rx, Ry, Rz);
        // Open or close the claw
        // claw is a float value between 0 and 1
        // 0 is fully closed, 1 is fully open
        // You can use this value to animate the claw
        // torp1 and torp2 are boolean values
        // If true, fire the torpedoes
        // If false, do not fire the torpedoes
    }

    float[] distanceMeasurement()
    {
        float[] distance = new float[2];
        distance[0] = (float)Vector3.Distance(rb.transform.position, gate.transform.position); // Distance between the Sub and the Gate
        distance[1] = (float)Vector3.Distance(rb.transform.position, pole.transform.position); // Distance between the Sub and the Pole
        return distance;
    }

    float[] getTemperature()
    {
        float[] temperature = new float[2];
        temperature[0] = 0.0f; // Temperature of the Sub
        temperature[1] = 0.0f; // Temperature of the Water
        return temperature;
    }

    float getDepth()
    {
        return rb.transform.position.y; // Depth of the Sub
    }

    IEnumerator SendDataToServer()
    {
        float XAccel = (rb.transform.position.x - XAccelLast) / Time.deltaTime;
        float YAccel = (rb.transform.position.y - YAccelLast) / Time.deltaTime;
        float ZAccel = (rb.transform.position.z - ZAccelLast) / Time.deltaTime;
        float XGyro = (rb.transform.rotation.x - XGyroLast) / Time.deltaTime;
        float YGyro = (rb.transform.rotation.y - YGyroLast) / Time.deltaTime;
        float ZGyro = (rb.transform.rotation.z - ZGyroLast) / Time.deltaTime;

        XAccelLast = rb.transform.position.x;
        YAccelLast = rb.transform.position.y;
        ZAccelLast = rb.transform.position.z;
        XGyroLast = rb.transform.rotation.x;
        YGyroLast = rb.transform.rotation.y;
        ZGyroLast = rb.transform.rotation.z;

        float[] distance = distanceMeasurement();

        float[] temperature = getTemperature();

        float depth = getDepth();

        // Create a form and add the data to it
        WWWForm form = new WWWForm();
        form.AddField("XAccel", XAccel.ToString());
        form.AddField("YAccel", YAccel.ToString());
        form.AddField("ZAccel", ZAccel.ToString());
        form.AddField("XGyro", XGyro.ToString());
        form.AddField("YGyro", YGyro.ToString());
        form.AddField("ZGyro", ZGyro.ToString());
        form.AddField("Distance0", distance[0].ToString());
        form.AddField("Distance1", distance[1].ToString());
        form.AddField("Temperature0", temperature[0].ToString());
        form.AddField("Temperature1", temperature[1].ToString());
        form.AddField("Depth", depth.ToString());

        // Upload to the server
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:5000/uploadData", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Data uploaded successfully");
            }
        }
    }

    IEnumerator GetDataFromServer()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/getData"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // Parse and use the data
                string json = www.downloadHandler.text;
                DataFromServer data = JsonUtility.FromJson<DataFromServer>(json);
                X = data.X;
                Y = data.Y;
                Z = data.Z;
                Rx = data.Rx;
                Ry = data.Ry;
                Rz = data.Rz;
                claw = data.claw;
                torp1 = data.torp1;
                torp2 = data.torp2;

            }
        }
    }
}

[System.Serializable]
public class DataFromServer
{
    public float X;
    public float Y;
    public float Z;
    public float Rx;
    public float Ry;
    public float Rz;
    public float claw;
    public bool torp1;
    public bool torp2;
}