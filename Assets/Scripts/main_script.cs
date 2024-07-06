using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.Pool;

public class main_script : MonoBehaviour
{
    // Setting up the references to the UI components
    public Button startButton;
    public Toggle prequalsCheckbox;
    public Toggle manualCheckbox;
    private bool startButtonPressed = false;

    // Setup the rigid body of the sub
    public Rigidbody rb;

    // Setup the manual control bool
    public bool manualControl = true;

    // Setup the server control variables
    private float X, Y, Z, Rx, Ry, Rz, claw;
    private bool torp1, torp2;
    private float[] distanceMeasurementData;
    private float[] temperatureMeasurement;
    private float[] depthMeasurement;
    private float XAccelLast = 0.0f;
    private float YAccelLast = 0.0f;
    private float ZAccelLast = 0.0f;
    private float XGyroLast = 0.0f;
    private float YGyroLast = 0.0f;
    private float ZGyroLast = 0.0f;

    // Setup the distance measurement variables
    private float distance = 0.0f;

    // Setup the temperature variables
    private float temperature = 0.0f;

    // Setup the depth variables
    private float depth = 0.0f;

    // Setup the camera variables
    public Camera frontRightCamera;
    public Camera frontLeftCamera;
    public Camera bottomCamera;
    public Camera sceneCamera;
    private int cameraIndex = 0;

    // Setup the debounce time
    private float debounceTime = 0.2f; // 200 milliseconds

    // Setup the last switch time
    private float lastSwitchTime = 0;

    // Setup the controller mapping
    private ControllerMapping controllerMapping;

    // Setup the game objects that need to be distanced
    public GameObject pool;
    public GameObject gate1;
    public GameObject gate2;
    public GameObject pole;
    public GameObject bouy;

    // Prequals position
    private Vector3 prequalsPosition = new Vector3(215.6f, 25.0f, 0.0f);
    // Qualifications position
    private Vector3 qualificationsPosition = new Vector3(353.6f, 25.0f, 545.2f);

    // Start is called before the first frame update
    void Start()
    {
        // Add listener to the start button
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Make sure the rb cant move
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// Called when the start button is clicked.
    /// </summary>
    void OnStartButtonClicked()
    {
        // Get the state of the checkboxes and the value of the input field
        bool isPrequalsChecked = prequalsCheckbox.isOn;
        bool isManualChecked = manualCheckbox.isOn;
        startButtonPressed = true;

        // Output the values to the console for debugging
        Debug.Log("Prequals Checkbox: " + isPrequalsChecked);
        Debug.Log("Manual Checkbox: " + isManualChecked);

        // You can now use these values for further processing
        rb.constraints = RigidbodyConstraints.None;
        if (isManualChecked)
        {
            manualControl = true;
            controllerMapping = new ControllerMapping();
        }
        else
        {
            manualControl = false;
        }
        if (isPrequalsChecked)
        {
            rb.position = prequalsPosition;
        }
        else
        {
            rb.position = qualificationsPosition;
        }
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Hide the UI elements
        startButton.gameObject.SetActive(false);
        prequalsCheckbox.gameObject.SetActive(false);
        manualCheckbox.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (manualControl && startButtonPressed)
        {
            ControllerControl();
        }
        else
        {
            // StartCoroutine(SendDataToServer());
            // StartCoroutine(GetDataFromServer());
            // rb.AddForce(new Vector3(X, Z, Y), ForceMode.Force);
            // rb.AddTorque(new Vector3(0, Rz, 0), ForceMode.Force);
            // if (claw == 1.0f)
            // {
            //     // Fire claw
            // } 
            // else if (claw == -1.0f)
            // {
            //     // Retract claw
            // }
            // if (torp1)
            // {
            //     // Fire torpedo 1
            // }
            // if (torp2)
            // {
            //     // Fire torpedo 2
            // }
        }
        if (Input.GetKey(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (rb.transform.position.y < 0.25f)
        {
            rb.position = new Vector3(rb.transform.position.x, 0.25f, rb.transform.position.z);
        }
    }

    // Called when the manual control is disabled
    float[] distanceMeasurement()
    {
        float[] distance = new float[2];
        distance[0] = (float)Vector3.Distance(rb.transform.position, gate1.transform.position); // Distance between the Sub and the Gate
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

    // Called when manual control is enabled
    void ControllerControl()
    {
        // Controller inputs
        // float LY = controllerMapping.LY.Newaction.ReadValue<float>();
        // float LX = controllerMapping.LX.Newaction.ReadValue<float>();
        // float RX = controllerMapping.RX.Newaction1.ReadValue<float>();
        // float RY = controllerMapping.RY.Newaction.ReadValue<float>();
        // float RT = controllerMapping.TriggerR.Newaction.ReadValue<float>();
        // float LT = controllerMapping.TriggerL.Newaction.ReadValue<float>();
        // bool A = controllerMapping.A.Newaction.ReadValue<float>() > 0;
        // bool B = controllerMapping.B.Newaction.ReadValue<float>() > 0;
        // bool Xb = controllerMapping.X.Newaction.ReadValue<float>() > 0;
        // bool Yb = controllerMapping.Y.Newaction.ReadValue<float>() > 0;
        // bool DPadUp = controllerMapping.DPadU.Newaction.ReadValue<float>() > 0;
        // bool DPadDown = controllerMapping.DPadD.Newaction.ReadValue<float>() > 0;
        // bool DPadLeft = controllerMapping.DPadL.Newaction.ReadValue<float>() > 0;
        // bool DPadRight = controllerMapping.DPadR.Newaction.ReadValue<float>() > 0;

        if (Input.GetKey(KeyCode.W))
        {
            X = -1.0f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            X = 1.0f;
        }
        else
        {
            X = 0.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Z = 1.0f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Z = -1.0f;
        }
        else
        {
            Z = 0.0f;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            Y = -1.0f;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            Y = 1.0f;
        }
        else
        {
            Y = 0.0f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Rz = 1.0f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            Rz = -1.0f;
        }
        else
        {
           Rz = 0.0f;
        }
        if (Input.GetKey(KeyCode.R))
        {
            torp1 = true;
        }
        else
        {
            torp1 = false;
        }
        if (Input.GetKey(KeyCode.F))
        {
            torp2 = true;
        }
        else
        {
            torp2 = false;
        }
        if (Input.GetKey(KeyCode.V))
        {
            claw = 1.0f;
        }
        else if (Input.GetKey(KeyCode.X))
        {
            claw = -1.0f;
        }
        else
        {
            claw = 0.0f;
        }
        if (Time.time - lastSwitchTime > debounceTime)
        {
            if (Input.GetKey(KeyCode.T))
            {
                cameraIndex += 1;
                lastSwitchTime = Time.time;
            }
            else if (Input.GetKey(KeyCode.G))
            {
                cameraIndex -= 1;
                lastSwitchTime = Time.time;
            }
        }

        // Debug
        // Debug.Log($"X: {X} Y: {Y} Z: {Z} Rz: {Rz} Torp1: {torp1} Torp2: {torp2} Claw: {claw} CameraIndex: {cameraIndex}");

        // // Adjusted movement inputs
        // X = -LY;
        // Y = -LX;
        // Z = RY;
        // Rz = -RX;
        // torp1 = RT > 0.0f;
        // torp2 = LT > 0.0f;

        // Mapping input values to a specific range for movement
        float min_out = -10.0f;
        float max_out = 10.0f;
        float min_Z_in = -25.0f;
        float max_Z_in = 25.0f;
        float min_1_out = -0.75f;
        float max_1_out = 0.75f;
        float min_in = -1.0f;
        float max_in = 1.0f;

        // Apply input mapping
        X = Map(X, min_in, max_in, min_out, max_out);
        Y = Map(Y, min_in, max_in, min_out, max_out);
        Z = Map(Z, min_in, max_in, min_Z_in, max_Z_in);
        Rz = Map(Rz, min_in, max_in, min_1_out, max_1_out);

        Rz = -Rz; // Invert Rz

        // Add a ceiling to the Y axis based on player location
        if (rb.transform.position.y > 30.0f)
        {
            Y = Mathf.Min(Y, 0.0f);
        }

        // Direction and magnitude of force to apply, in local space
        Vector3 forceDirection = new Vector3(X, Y, Z);
        Vector3 torqueDirection = new Vector3(0, Rz, 0); // Adjust for local space orientation

        // Convert forceDirection and torqueDirection to be relative to the rigidbody's orientation
        Vector3 localForce = rb.transform.TransformDirection(forceDirection);
        Vector3 localTorque = rb.transform.TransformDirection(torqueDirection);

        // Apply forces and torques to the rigidbody in local space
        rb.AddForce(localForce, ForceMode.Force);
        rb.AddTorque(localTorque, ForceMode.Force);

        // Debug output
        // Debug.Log($"X: {X} Y: {Y} Z: {Z} Roll: {Roll} Pitch: {Pitch} Rz: {Rz} Torp1: {Torp1} Torp2: {Torp2} A: {A} B: {B} X: {Xb} Y: {Yb} DPadUp: {DPadUp} DPadDown: {DPadDown} localForce: {localForce} localTorque: {localTorque} cameraIndex: {cameraIndex}");

        
        if (cameraIndex < 0)
        {
            cameraIndex = 0;
        }
        else if (cameraIndex > 3)
        {
            cameraIndex = 3;
        }
        if (cameraIndex == 0)
        {
            frontRightCamera.enabled = false;
            frontLeftCamera.enabled = false;
            bottomCamera.enabled = false;
            sceneCamera.enabled = true;
        }
        else if (cameraIndex == 1)
        {
            frontRightCamera.enabled = true;
            frontLeftCamera.enabled = false;
            bottomCamera.enabled = false;
            sceneCamera.enabled = false;
        }
        else if (cameraIndex == 2)
        {
            frontRightCamera.enabled = false;
            frontLeftCamera.enabled = true;
            bottomCamera.enabled = false;
            sceneCamera.enabled = false;
        }
        else if (cameraIndex == 3)
        {
            frontRightCamera.enabled = false;
            frontLeftCamera.enabled = false;
            bottomCamera.enabled = true;
            sceneCamera.enabled = false;
        }
    }
    public int Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        // Perform the mapping from one range to another as a float.
        float mappedValue = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

        // Round the result to the nearest whole number and then cast to int.
        return (int)Math.Round(mappedValue);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == gate1 || collision.gameObject == pole)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}