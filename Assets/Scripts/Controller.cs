using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public bool remoteControl = false;
    private ControllerMapping controllerMapping;
    private float X, Y, Z, Roll, Pitch, Yaw;
    private bool Torp1, Torp2;
    public Rigidbody rb;
    public Vector3 Com; // Center of Mass object
    public Camera frontRightCamera;
    public Camera frontLeftCamera;
    public Camera bottomCamera;
    public Camera sceneCamera;
    private int cameraIndex = 0;
    private float debounceTime = 0.2f; // 200 milliseconds
    private float lastSwitchTime = 0;
    public GameObject coin_0, coin_1, coin_2, coin_3, coin_4, coin_5, coin_6, coin_7, coin_8;
    public GameObject gate;
    public GameObject Qualification_Pole;
    public TMPro.TextMeshProUGUI scoreText;
    private int score = 0;
    private string url = "http://localhost:5000";
    public GameObject motor_1;
    public GameObject motor_2;
    public GameObject motor_3;
    public GameObject motor_4;
    public GameObject motor_5;
    public GameObject motor_6;
    public GameObject motor_7;
    public GameObject motor_8;
    private GameObject[] horizontalMotors;
    private GameObject[] verticalMotors;
    void Start()
    {
        if (!remoteControl)
        {
            // Initialize ControllerMapping
            controllerMapping = new ControllerMapping();
            controllerMapping.Enable(); // Make sure to enable the actions
        }
        else
        {
            // Placeholder for remote control initialization
        }

        horizontalMotors = new GameObject[] { motor_1, motor_2, motor_3, motor_4 };
        verticalMotors = new GameObject[] { motor_5, motor_6, motor_7, motor_8 };

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Com;
        frontRightCamera.enabled = false;
        frontLeftCamera.enabled = false;
        bottomCamera.enabled = false;
        sceneCamera.enabled = true;
        scoreText.text = score.ToString();

        coin_0.SetActive(true);
        coin_1.SetActive(false);
        coin_2.SetActive(false);
        coin_3.SetActive(false);
        coin_4.SetActive(false);
        coin_5.SetActive(false);
        coin_6.SetActive(false);
        coin_7.SetActive(false);
        coin_8.SetActive(false);
    }
    void ControllerControl()
    {
        // Controller inputs
        float LY = controllerMapping.LY.Newaction.ReadValue<float>();
        float LX = controllerMapping.LX.Newaction.ReadValue<float>();
        float RX = controllerMapping.RX.Newaction1.ReadValue<float>();
        float RY = controllerMapping.RY.Newaction.ReadValue<float>();
        float RT = controllerMapping.TriggerR.Newaction.ReadValue<float>();
        float LT = controllerMapping.TriggerL.Newaction.ReadValue<float>();
        bool A = controllerMapping.A.Newaction.ReadValue<float>() > 0;
        bool B = controllerMapping.B.Newaction.ReadValue<float>() > 0;
        bool Xb = controllerMapping.X.Newaction.ReadValue<float>() > 0;
        bool Yb = controllerMapping.Y.Newaction.ReadValue<float>() > 0;
        bool DPadUp = controllerMapping.DPadU.Newaction.ReadValue<float>() > 0;
        bool DPadDown = controllerMapping.DPadD.Newaction.ReadValue<float>() > 0;
        bool DPadLeft = controllerMapping.DPadL.Newaction.ReadValue<float>() > 0;
        bool DPadRight = controllerMapping.DPadR.Newaction.ReadValue<float>() > 0;

        // Adjusted movement inputs
        X = -LY;
        Y = -LX;
        Z = RY;
        Yaw = -RX;
        Torp1 = RT > 0.0f;
        Torp2 = LT > 0.0f;

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
        Yaw = Map(Yaw, min_in, max_in, min_1_out, max_1_out);

        Yaw = -Yaw; // Invert Yaw

        // Add a ceiling to the Y axis based on player location
        if (rb.transform.position.y > 10.0f)
        {
            Z = Mathf.Min(Z, 0.0f);
        }

        // Direction and magnitude of force to apply, in local space
        Vector3 forceDirection = new Vector3(X, Z, Y);
        Vector3 torqueDirection = new Vector3(0, Yaw, 0); // Adjust for local space orientation

        // Convert forceDirection and torqueDirection to be relative to the rigidbody's orientation
        Vector3 localForce = rb.transform.TransformDirection(forceDirection);
        Vector3 localTorque = rb.transform.TransformDirection(torqueDirection);

        // Apply forces and torques to the rigidbody in local space
        rb.AddForce(localForce, ForceMode.Force);
        rb.AddTorque(localTorque, ForceMode.Force);

        // Debug output
        Debug.Log($"X: {X} Y: {Y} Z: {Z} Roll: {Roll} Pitch: {Pitch} Yaw: {Yaw} Torp1: {Torp1} Torp2: {Torp2} A: {A} B: {B} X: {Xb} Y: {Yb} DPadUp: {DPadUp} DPadDown: {DPadDown} localForce: {localForce} localTorque: {localTorque} cameraIndex: {cameraIndex}");

        if (Time.time - lastSwitchTime > debounceTime)
        {
            if (DPadUp)
            {
                cameraIndex += 1;
                lastSwitchTime = Time.time;
            }
            else if (DPadDown)
            {
                cameraIndex -= 1;
                lastSwitchTime = Time.time;
            }
        }
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
        if (Time.time - lastSwitchTime > debounceTime)
        {
            if (Yb)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
    void NetworkControl()
    {
        // Placeholder for remote control
        StartCoroutine(getData(url));
    }
    /**
     * @brief Map a value from one range to another.
     */
    public int Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        // Perform the mapping from one range to another as a float.
        float mappedValue = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

        // Round the result to the nearest whole number and then cast to int.
        return (int)Math.Round(mappedValue);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastSwitchTime > debounceTime)
        {
            switch (other.gameObject)
            {
                case var _ when other.gameObject == coin_0:
                    coin_0.SetActive(false);
                    coin_1.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_1:
                    coin_1.SetActive(false);
                    coin_2.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_2:
                    coin_2.SetActive(false);
                    coin_3.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_3:
                    coin_3.SetActive(false);
                    coin_4.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_4:
                    coin_4.SetActive(false);
                    coin_5.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_5:
                    coin_5.SetActive(false);
                    coin_6.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_6:
                    coin_6.SetActive(false);
                    coin_7.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_7:
                    coin_7.SetActive(false);
                    coin_8.SetActive(true);
                    break;
                case var _ when other.gameObject == coin_8:
                    coin_8.SetActive(false);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;
            }
            score += 1;
            scoreText.text = score.ToString();
        }
    }
    // HTTP functions
    IEnumerator getData(string url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }
    }
    private void postData(string url, string sensor_data, int[][] img1, int[][] img2, int[][] img3)
    {
        // Create a posting method to send three images to the server
        // UnityWebRequest uwr = new UnityWebRequest(url, "POST");
        // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(sensor_data);
    }
    void ProcessTCPData()
    {
        // TCP data processing placeholder
        Debug.Log("Processing TCP Data...");
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == gate || collision.gameObject == Qualification_Pole)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
    void FixedUpdate()
    {
        if (!remoteControl)
        {
            ControllerControl();
        }
        else
        {
            // Placeholder for remote control
            NetworkControl();
        }
    }
}



