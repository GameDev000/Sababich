using UnityEngine;
using UnityEngine.UI;

public class FaceCaptureController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot; // FaceCapturePanel root
    [SerializeField] private RawImage preview;      // shows WebCamTexture
    [SerializeField] private TMPro.TMP_Text instructionText; // "Smile" etc.

    [Header("Capture")]
    [SerializeField] private int requestedWidth = 512;
    [SerializeField] private int requestedHeight = 512;

    private WebCamTexture cam;
    private int stage = 0; // 0=happy,1=angry,2=furious

    private Texture2D capturedHappy;
    private Texture2D capturedAngry;
    private Texture2D capturedFurious;

    [SerializeField] private string introText = "צלם 3 תמונות: חייך, כועס, כועס מאוד";


    [SerializeField] private float firstStageDelay = 3f;
    [Header("Face Detection")]
    [SerializeField] private FaceDetector faceDetector;

    // Opens the face capture UI and starts the process
    public void Open()
    {
        panelRoot.SetActive(true); // show panel
        capturedHappy = capturedAngry = capturedFurious = null; // reset

        StartCoroutine(OpenSequence()); // start capture sequence
    }


    // Sequence coroutine for opening and initial delay before first capture
    private System.Collections.IEnumerator OpenSequence()
    {
        stage = -1; // waiting for intro
        if (instructionText != null) // show intro text
            instructionText.text = introText;

        StartCamera(); // start camera

        yield return new WaitForSeconds(firstStageDelay); // wait before first capture

        stage = 0; // first capture stage
        UpdateInstruction(); // update instruction text
    }

    // Closes the face capture UI and stops the camera
    public void Close()
    {
        StopCamera();
        panelRoot.SetActive(false);
    }

    // Starts the device camera and shows preview
    private void StartCamera()
    {
        if (cam != null) return;

        if (WebCamTexture.devices == null || WebCamTexture.devices.Length == 0) // no camera
        {
            Debug.LogWarning("No camera devices found.");
            return;
        }

        cam = new WebCamTexture(requestedWidth, requestedHeight); // create camera texture
        cam.Play(); // start camera

        if (preview != null) // show preview link to UI
            preview.texture = cam;
    }

    // Stops the device camera
    private void StopCamera()
    {
        if (cam == null) return;
        cam.Stop();
        Destroy(cam);
        cam = null;

        if (preview != null)
            preview.texture = null;
    }

    // Captures the current frame for the current stage
    public void CaptureCurrentStage()
    {
        if (stage < 0)
        {
            Debug.Log("Waiting for intro delay...");
            return;
        }
        // Ensure camera is running
        if (cam == null || !cam.isPlaying)
        {
            Debug.LogWarning("Camera not running.");
            return;
        }

        // Grab current frame
        Texture2D snap = new Texture2D(cam.width, cam.height, TextureFormat.RGBA32, false);
        snap.SetPixels(cam.GetPixels()); // copy pixels
        snap.Apply(); // apply changes

        // Store based on stage
        if (stage == 0) capturedHappy = snap;
        else if (stage == 1) capturedAngry = snap;
        else if (stage == 2) capturedFurious = snap;

        stage++; // advance stage

        if (stage >= 3) // all done
        {
            Rect? happyRect = faceDetector != null ? faceDetector.DetectFace(capturedHappy) : (Rect?)null;
            Rect? angryRect = faceDetector != null ? faceDetector.DetectFace(capturedAngry) : (Rect?)null;
            Rect? furiousRect = faceDetector != null ? faceDetector.DetectFace(capturedFurious) : (Rect?)null;

            Sprite happy = ToSprite(capturedHappy, happyRect);
            Sprite angry = ToSprite(capturedAngry, angryRect);
            Sprite furious = ToSprite(capturedFurious, furiousRect);

            PlayerFaceStore.Set(happy, angry, furious);

            Close();
            return;
        }

        UpdateInstruction(); // update for next stage
    }

    // Updates the instruction text based on the current stage
    private void UpdateInstruction()
    {
        if (instructionText == null) return;

        if (stage == 0) instructionText.text = "חייך 🙂";
        else if (stage == 1) instructionText.text = "כועס 😠";
        else instructionText.text = "כועס מאוד 🤬";
    }

    // faceRectNormalized: top-left origin, values in [0,1]. Null = full frame.
    private Sprite ToSprite(Texture2D tex, Rect? faceRectNormalized = null)
    {
        if (tex == null) return null;

        Rect pixelRect;
        if (faceRectNormalized.HasValue)
        {
            Rect fr = faceRectNormalized.Value;
            // Convert top-left normalized → bottom-left pixel (Unity Sprite.Create convention)
            float pixX = fr.x * tex.width;
            float pixY = (1f - fr.y - fr.height) * tex.height;
            float pixW = fr.width * tex.width;
            float pixH = fr.height * tex.height;
            if (pixW <= 0f || pixH <= 0f)
            {
                Debug.LogWarning("[FaceCaptureController] Face rect has zero size — falling back to full frame.");
                pixelRect = new Rect(0, 0, tex.width, tex.height);
            }
            else
            {
                pixelRect = new Rect(pixX, pixY, pixW, pixH);
            }
        }
        else
        {
            pixelRect = new Rect(0, 0, tex.width, tex.height);
        }

        return Sprite.Create(tex, pixelRect, new Vector2(0.5f, 0.5f), 100f);
    }
}
