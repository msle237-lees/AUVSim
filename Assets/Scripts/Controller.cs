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
    public TMPro.TextMeshProUGUI scoreText;
    private int score = 0;
    private float distanceToGate;
    private float distanceToPole;
    Thread thread;
    public int connectionPort = 50001;
    TcpListener listener;
    TcpClient client;
    bool running;
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
            StartServer();
        }
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Com;
        frontRightCamera.enabled = false;
        frontLeftCamera.enabled = false;
        bottomCamera.enabled = false;
        sceneCamera.enabled = true;
        scoreText.text = score.ToString();
    }
    String distanceMeasurement()
    {
        GameObject obj = gate;
        GameObject obj2 = Qualification_Pole;
        distanceToGate = Vector3.Distance(transform.position, obj.transform.position) / 10.0f;
        distanceToPole = Vector3.Distance(transform.position, obj2.transform.position) / 10.0f;
        Debug.Log($"Distance to Gate: {distanceToGate} Distance to Pole: {distanceToPole}");
        return $"{distanceToGate},{distanceToPole}";
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
        if (Time.time - lastSwitchTime > debounceTime)
        {
            DataGenerator();
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
    void StartServer()
    {
        ThreadStart ts = new ThreadStart(GetData);
        thread = new Thread(ts);
        thread.Start();
        Debug.Log("Server started");
    }
    void GetData()
    {
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            Connection();
        }
        client.Close();
        listener.Stop();
    }
    void Connection()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        byte[] dataBytes = Encoding.UTF8.GetBytes(dataReceived);
        string data;
        if (dataReceived != null && dataReceived != "")
        {
            // Remove any trailing newline characters and characters that are not part of the data
            dataReceived = dataReceived.Replace("\n", "").Replace("\r", "").Replace("\0", "");
            Debug.Log(dataReceived);
            UpdateData(dataReceived);
            if (Time.time - lastSwitchTime > debounceTime)
            {
                data = DataGenerator();
                dataBytes = Encoding.UTF8.GetBytes(data);
            }
            nwStream.Write(dataBytes, 0, bytesRead);
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
    // IMU / Accel / Temp / Image data generator
    String DataGenerator()
    {
        float XAccelLast = 0.0f;
        float YAccelLast = 0.0f;
        float ZAccelLast = 0.0f;
        float XGyroLast = 0.0f;
        float YGyroLast = 0.0f;
        float ZGyroLast = 0.0f;

        float XAccel = (rb.transform.position.x - XAccelLast) / Time.deltaTime;
        float YAccel = (rb.transform.position.y - YAccelLast) / Time.deltaTime;
        float ZAccel = (rb.transform.position.z - ZAccelLast) / Time.deltaTime;
        float XGyro = (rb.transform.rotation.x - XGyroLast) / Time.deltaTime;
        float YGyro = (rb.transform.rotation.y - YGyroLast) / Time.deltaTime;
        float ZGyro = (rb.transform.rotation.z - ZGyroLast) / Time.deltaTime;

        // Get image data for front right camera
        RenderTexture frontCameraTexture = frontRightCamera.targetTexture;
        RenderTexture.active = frontCameraTexture;
        Texture2D frontCameraTexture2D = new Texture2D(frontCameraTexture.width, frontCameraTexture.height);
        frontCameraTexture2D.ReadPixels(new Rect(0, 0, frontCameraTexture.width, frontCameraTexture.height), 0, 0);
        frontCameraTexture2D.Apply();
        byte[] frontCameraBytes = frontCameraTexture2D.EncodeToPNG();
        string frontCameraBase64 = Convert.ToBase64String(frontCameraBytes);

        // Get image data for front left camera
        RenderTexture frontLeftCameraTexture = frontLeftCamera.targetTexture;
        RenderTexture.active = frontLeftCameraTexture;
        Texture2D frontLeftCameraTexture2D = new Texture2D(frontLeftCameraTexture.width, frontLeftCameraTexture.height);
        frontLeftCameraTexture2D.ReadPixels(new Rect(0, 0, frontLeftCameraTexture.width, frontLeftCameraTexture.height), 0, 0);
        frontLeftCameraTexture2D.Apply();
        byte[] frontLeftCameraBytes = frontLeftCameraTexture2D.EncodeToPNG();
        string frontLeftCameraBase64 = Convert.ToBase64String(frontLeftCameraBytes);

        // Get image data for bottom camera
        RenderTexture bottomCameraTexture = bottomCamera.targetTexture;
        RenderTexture.active = bottomCameraTexture;
        Texture2D bottomCameraTexture2D = new Texture2D(bottomCameraTexture.width, bottomCameraTexture.height);
        bottomCameraTexture2D.ReadPixels(new Rect(0, 0, bottomCameraTexture.width, bottomCameraTexture.height), 0, 0);
        bottomCameraTexture2D.Apply();
        byte[] bottomCameraBytes = bottomCameraTexture2D.EncodeToPNG();
        string bottomCameraBase64 = Convert.ToBase64String(bottomCameraBytes);

        // Get Distance to Gate and Pole
        string distance = distanceMeasurement();
        // Join all data into a single string
        string data = $"{XAccel},{YAccel},{ZAccel},{XGyro},{YGyro},{ZGyro},{frontCameraBase64},{frontLeftCameraBase64},{bottomCameraBase64},{distance}";
        return data;
    }
    void FixedUpdate()
    {
        if (!remoteControl)
        {
            ControllerControl();
        }
    }
}



