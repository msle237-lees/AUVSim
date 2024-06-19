using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CameraCapture : MonoBehaviour
{
    public Camera[] cameras; // Assign your cameras in the inspector
    public Rigidbody rb; // Assign to the Sub with the Rigidbody
    public GameObject gate;
    public GameObject pole;

    private float XAccelLast = 0.0f;
    private float YAccelLast = 0.0f;
    private float ZAccelLast = 0.0f;
    private float XGyroLast = 0.0f;
    private float YGyroLast = 0.0f;
    private float ZGyroLast = 0.0f;


    IEnumerator Start()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            yield return StartCoroutine(CaptureAndUploadImage(cameras[i], i));
        }

        StartCoroutine(GetDataFromServer());
        StartCoroutine(SendDataToServer());
    }

    IEnumerator CaptureAndUploadImage(Camera cam, int index)
    {
        // Set up RenderTexture
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        cam.Render();

        // Create a new Texture2D and read the RenderTexture data into it
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Reset the camera's target texture
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Encode texture into PNG
        byte[] bytes = screenshot.EncodeToPNG();

        // Create a form and add the image to it
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", bytes, "screenshot" + index + ".png", "image/png");

        // Upload to the server
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:5000/upload", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Image uploaded successfully");
            }
        }
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
        using (UnityWebRequest www = UnityWebRequest.Post("http://yourserver.com/uploadData", form))
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
                Debug.Log("Acceleration X: " + data.accelerationX);
                Debug.Log("Acceleration Y: " + data.accelerationY);
                Debug.Log("Acceleration Z: " + data.accelerationZ);
                Debug.Log("Rotation X: " + data.rotationX);
                Debug.Log("Rotation Y: " + data.rotationY);
                Debug.Log("Rotation Z: " + data.rotationZ);
            }
        }
    }
}

[System.Serializable]
public class DataFromServer
{
    public float accelerationX;
    public float accelerationY;
    public float accelerationZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
}