using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class Controller : MonoBehaviour
{
    public bool remoteControl = true;
    private ControllerMapping controllerMapping;
    private float X, Y, Z, Roll, Pitch, Yaw;
    private bool Torp1, Torp2, Claw, Reset;
    public Rigidbody rb;
    public Vector3 Com; // Center of Mass object
    public Camera frontRightCamera;
    public Camera frontLeftCamera;
    public Camera bottomCamera;
    public Camera sceneCamera;
    private int cameraIndex = 0;
    private float debounceTime = 0.2f; // 200 milliseconds
    private float lastSwitchTime = 0;
    public GameObject gate;
    public GameObject Qualification_Pole;
    Thread thread;
    public int connectionPort = 50001;
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
        // Debug.Log($"X: {X} Y: {Y} Z: {Z} Roll: {Roll} Pitch: {Pitch} Yaw: {Yaw} Torp1: {Torp1} Torp2: {Torp2} A: {A} B: {B} X: {Xb} Y: {Yb} DPadUp: {DPadUp} DPadDown: {DPadDown} localForce: {localForce} localTorque: {localTorque} cameraIndex: {cameraIndex}");

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
    public int Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        // Perform the mapping from one range to another as a float.
        float mappedValue = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

        // Round the result to the nearest whole number and then cast to int.
        return (int)Math.Round(mappedValue);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == gate || collision.gameObject == Qualification_Pole)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
    void UpdateData(string data)
    {
        string[] values = data.Split(',');
        if (values.Length == 10)
        {
            X = float.Parse(values[0]);
            Y = float.Parse(values[1]);
            Z = float.Parse(values[2]);
            Roll = float.Parse(values[3]);
            Pitch = float.Parse(values[4]);
            Yaw = float.Parse(values[5]);
            Torp1 = bool.Parse(values[6]);
            Torp2 = bool.Parse(values[7]);
            Claw = bool.Parse(values[8]);
            Reset = bool.Parse(values[8]);
            
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

            if (Time.time - lastSwitchTime > debounceTime)
            {
                if (Reset)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
    void FixedUpdate()
    {
        // if (Starter.manualControl)
        // {
        //     ControllerControl();
        // }
        // else {
        //     // Do Nothing
        // }
    }
}
