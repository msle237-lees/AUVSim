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
        // Debug.Log($"X: {X} Y: {Y} Z: {Z} Roll: {Roll} Pitch: {Pitch} Yaw: {Yaw} Torp1: {Torp1} Torp2: {Torp2}");

        if (Xb) {
            if (cameraIndex == 0) {
                frontRightCamera.enabled = true;
                frontLeftCamera.enabled = false;
                bottomCamera.enabled = false;
                sceneCamera.enabled = false;
                cameraIndex = 1;
            }
            else if (cameraIndex == 1) {
                frontRightCamera.enabled = false;
                frontLeftCamera.enabled = true;
                bottomCamera.enabled = false;
                sceneCamera.enabled = false;
                cameraIndex = 2;
            }
            else if (cameraIndex == 2) {
                frontRightCamera.enabled = false;
                frontLeftCamera.enabled = false;
                bottomCamera.enabled = true;
                sceneCamera.enabled = false;
                cameraIndex = 3;
            }
            else if (cameraIndex == 3) {
                frontRightCamera.enabled = false;
                frontLeftCamera.enabled = false;
                bottomCamera.enabled = false;
                sceneCamera.enabled = true;
                cameraIndex = 0;
            
            }
        }
        if (Yb) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void ProcessTCPData()
    {
        // TCP data processing placeholder
        Debug.Log("Processing TCP Data...");
    }
}



