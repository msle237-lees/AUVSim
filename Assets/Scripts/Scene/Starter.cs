using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Starter : MonoBehaviour
{
    // References to UI components
    public Button startButton;
    public Toggle prequalsCheckbox;
    public Toggle manualCheckbox;
    public InputField ipInputField;

    // Rigid body of the sub
    public Rigidbody rb;

    // Manual control bool
    public bool manualControl = false;
    void Start()
    {
        // Add listener to the start button
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    /// <summary>
    /// Called when the start button is clicked.
    /// </summary>
    void OnStartButtonClicked()
    {
        // Get the state of the checkboxes and the value of the input field
        bool isPrequalsChecked = prequalsCheckbox.isOn;
        bool isManualChecked = manualCheckbox.isOn;
        string ipAddress = ipInputField.text;

        // Output the values to the console for debugging
        Debug.Log("Prequals Checkbox: " + isPrequalsChecked);
        Debug.Log("Manual Checkbox: " + isManualChecked);
        Debug.Log("IP Address: " + ipAddress);

        // You can now use these values for further processing
    }

    // Make sure that the manual control bool is accessible from other scripts
    [System.Serializable]
    public class ManualControl
    {
        public bool manualControl;
    }
}
