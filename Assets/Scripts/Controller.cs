using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Controller : MonoBehaviour
{
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
    void Start()
    {
        // Initialize ControllerMapping
        controllerMapping = new ControllerMapping();
        controllerMapping.Enable(); // Make sure to enable the actions

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
    void OnCollisionEnter(Collision collision)
    {   
        if (collision.gameObject == gate || collision.gameObject == Qualification_Pole) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (collision.gameObject == coin_0) {
            coin_0.SetActive(false);
            coin_1.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_1) {
            coin_1.SetActive(false);
            coin_2.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_2) {
            coin_2.SetActive(false);
            coin_3.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_3) {
            coin_3.SetActive(false);
            coin_4.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_4) {
            coin_4.SetActive(false);
            coin_5.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_5) {
            coin_5.SetActive(false);
            coin_6.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_6) {
            coin_6.SetActive(false);
            coin_7.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_7) {
            coin_7.SetActive(false);
            coin_8.SetActive(true);
            score += 1;
            scoreText.text = score.ToString();
        }
        else if (collision.gameObject == coin_8) {
            coin_8.SetActive(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
    void FixedUpdate()
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
        X = -RY;
        Y = -LX;
        Z = LY;
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

        if (Time.time - lastSwitchTime > debounceTime) {
            if (DPadUp) {
                cameraIndex += 1;
                lastSwitchTime = Time.time;
            }
            else if (DPadDown) {
                cameraIndex -= 1;
                lastSwitchTime = Time.time;
            }
        }
        if (cameraIndex < 0) {
            cameraIndex = 0;
        }
        else if (cameraIndex > 3) {
            cameraIndex = 3;
        }
        if (cameraIndex == 0) {
            frontRightCamera.enabled = false;
            frontLeftCamera.enabled = false;
            bottomCamera.enabled = false;
            sceneCamera.enabled = true;
        }
        else if (cameraIndex == 1) {
            frontRightCamera.enabled = true;
            frontLeftCamera.enabled = false;
            bottomCamera.enabled = false;
            sceneCamera.enabled = false;
        }
        else if (cameraIndex == 2) {
            frontRightCamera.enabled = false;
            frontLeftCamera.enabled = true;
            bottomCamera.enabled = false;
            sceneCamera.enabled = false;
        }
        else if (cameraIndex == 3) {
            frontRightCamera.enabled = false;
            frontLeftCamera.enabled = false;
            bottomCamera.enabled = true;
            sceneCamera.enabled = false;        
        }
        if (Time.time - lastSwitchTime > debounceTime) {
            if (Yb) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    void ProcessTCPData()
    {
        // TCP data processing placeholder
        Debug.Log("Processing TCP Data...");
    }
}



