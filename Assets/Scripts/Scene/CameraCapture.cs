using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CameraCapture : MonoBehaviour
{
    public Camera[] cameras; // Assign your cameras in the inspector

    IEnumerator Start()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            yield return StartCoroutine(CaptureAndUploadImage(cameras[i], i));
        }
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
        form.AddBinaryData("file", bytes, "screenshot1" + index + ".png", "image/png");

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
}

