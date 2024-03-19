using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script attached to a UI Text element to handle point counting.
/// </summary>
public class PointCounter : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI Text component displaying the points.
    /// </summary>
    public Text pointText;

    private int points = 0;

    private void Start()
    {
        // Update the UI with the initial points
        UpdatePointText();
    }

    /// <summary>
    /// Increments the points by 1 and updates the UI.
    /// </summary>
    public void AddPoint()
    {
        points++;
        UpdatePointText();
    }

    /// <summary>
    /// Updates the UI Text component with the current points.
    /// </summary>
    private void UpdatePointText()
    {
        pointText.text = "Points: " + points;
    }
}
