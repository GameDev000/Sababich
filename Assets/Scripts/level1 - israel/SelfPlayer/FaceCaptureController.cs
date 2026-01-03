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

    [SerializeField] private string introText = "צלם 3 תמונות:\n1. חייך\n2. כועס\n3. כועס מאוד";


    [SerializeField] private float firstStageDelay = 3f;

    public void Open()
    {
        panelRoot.SetActive(true);
        capturedHappy = capturedAngry = capturedFurious = null;

        StartCoroutine(OpenSequence());
    }


    private System.Collections.IEnumerator OpenSequence()
    {
        stage = -1;
        if (instructionText != null)
            instructionText.text = introText;

        StartCamera();

        yield return new WaitForSeconds(firstStageDelay);

        stage = 0;
        UpdateInstruction();
    }

    public void Close()
    {
        StopCamera();
        panelRoot.SetActive(false);
    }

    private void StartCamera()
    {
        if (cam != null) return;

        if (WebCamTexture.devices == null || WebCamTexture.devices.Length == 0)
        {
            Debug.LogWarning("No camera devices found.");
            return;
        }

        cam = new WebCamTexture(requestedWidth, requestedHeight);
        cam.Play();

        if (preview != null)
            preview.texture = cam;
    }

    private void StopCamera()
    {
        if (cam == null) return;
        cam.Stop();
        Destroy(cam);
        cam = null;

        if (preview != null)
            preview.texture = null;
    }

    public void CaptureCurrentStage()
    {
        if (stage < 0)
        {
            Debug.Log("Waiting for intro delay...");
            return;
        }
        if (cam == null || !cam.isPlaying)
        {
            Debug.LogWarning("Camera not running.");
            return;
        }

        // Grab current frame
        Texture2D snap = new Texture2D(cam.width, cam.height, TextureFormat.RGBA32, false);
        snap.SetPixels(cam.GetPixels());
        snap.Apply();

        if (stage == 0) capturedHappy = snap;
        else if (stage == 1) capturedAngry = snap;
        else if (stage == 2) capturedFurious = snap;

        stage++;

        if (stage >= 3)
        {
            // Convert to sprites and store globally
            Sprite happy = ToSprite(capturedHappy);
            Sprite angry = ToSprite(capturedAngry);
            Sprite furious = ToSprite(capturedFurious);

            PlayerFaceStore.Set(happy, angry, furious);

            Close(); // done
            return;
        }

        UpdateInstruction();
    }

    private void UpdateInstruction()
    {
        if (instructionText == null) return;

        if (stage == 0) instructionText.text = "חייך 🙂";
        else if (stage == 1) instructionText.text = "כועס 😠";
        else instructionText.text = "כועס מאוד 🤬";
    }

    private Sprite ToSprite(Texture2D tex)
    {
        if (tex == null) return null;
        Rect r = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(tex, r, pivot, 100f);
    }
}
