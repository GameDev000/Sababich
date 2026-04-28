# BlazeFace Head Crop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace full-frame face capture in FaceCaptureController with an AI-detected bounding box crop using BlazeFace via Unity AI Inference, so the stored sprite contains only the player's face/head.

**Architecture:** A new MonoBehaviour `FaceDetector` loads `blazeface.onnx`, runs inference on each captured Texture2D, and returns a normalized `Rect` of the detected face. `FaceCaptureController` passes that Rect to `ToSprite()`, which crops to it instead of using the full frame. If no face is detected above the confidence threshold, a center-crop fallback is used.

**Tech Stack:** Unity AI Inference (`com.unity.ai.inference` 2.6.1, namespace `Unity.InferenceEngine`), BlazeFace ONNX (`Assets/Models/blazeface.onnx`), WebCamTexture, Unity Sprite

---

## File Map

| File | Action | Responsibility |
|------|--------|----------------|
| `Assets/Scripts/level1 - israel/SelfPlayer/FaceDetector.cs` | **CREATE** | Load model, generate anchors, run inference, decode bbox, return Rect |
| `Assets/Scripts/level1 - israel/SelfPlayer/FaceCaptureController.cs` | **MODIFY** | Add `[SerializeField] FaceDetector` field, pass detected Rect to `ToSprite()` |

---

## Important Notes Before Starting

- **Namespace:** `Unity.InferenceEngine` (NOT `Unity.Sentis` — that was the old name)
- **Worker API:** `new Worker(model, BackendType.CPU)` → `worker.Schedule(tensor)` → `worker.PeekOutput("name") as Tensor<float>` → `.CompleteOperationsAndDownload()`
- **Model output names:** BlazeFace ONNX models vary by source. The plan uses `"classificators"` and `"regressors"`. Step 1 includes a log to verify names at runtime — check the Unity Console on first play and adjust the string names if different.
- **Coordinate origin:** BlazeFace outputs use top-left origin. Unity `Sprite.Create` rect uses bottom-left origin. The conversion is handled explicitly in `ToSprite()`.

---

## Task 1 — Create FaceDetector.cs (Model Load + Anchors)

**Files:**
- Create: `Assets/Scripts/level1 - israel/SelfPlayer/FaceDetector.cs`

- [ ] **Step 1: Create the file with model loading and anchor generation**

```csharp
using UnityEngine;
using Unity.InferenceEngine;
using System.Collections.Generic;

public class FaceDetector : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] [Range(0.3f, 0.95f)] private float confidenceThreshold = 0.75f;

    private Model runtimeModel;
    private Worker worker;
    private float[] anchors; // flat array: [y0, x0, y1, x1, ...]

    private const int InputSize   = 128;
    private const int NumAnchors  = 896;

    void Awake()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.CPU);
        GenerateAnchors();

        // Log output names so you can verify them in Console on first play
        foreach (var output in runtimeModel.outputs)
            Debug.Log($"[FaceDetector] output layer: {output.name}");
    }

    // BlazeFace front-camera anchor grid for 128x128 input
    // Stride 8  → 16x16 grid × 2 anchors = 512
    // Stride 16 → 8x8  grid × 6 anchors = 384
    // Total = 896
    private void GenerateAnchors()
    {
        var list = new List<float>(NumAnchors * 2);
        int[] strides          = { 8, 16 };
        int[] anchorsPerCell   = { 2, 6 };

        for (int s = 0; s < strides.Length; s++)
        {
            int stride      = strides[s];
            int gridSize    = InputSize / stride;
            int cellAnchors = anchorsPerCell[s];

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    float cy = (row + 0.5f) / gridSize;
                    float cx = (col + 0.5f) / gridSize;
                    for (int a = 0; a < cellAnchors; a++)
                    {
                        list.Add(cy);
                        list.Add(cx);
                    }
                }
            }
        }
        anchors = list.ToArray();
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}
```

- [ ] **Step 2: Verify the file compiles in Unity**

Open Unity. Wait for it to compile. Check the Console panel — there should be **no errors** in `FaceDetector.cs`. Warnings about unused variables are fine at this stage.

---

## Task 2 — Implement Inference + Bounding Box Decoding

**Files:**
- Modify: `Assets/Scripts/level1 - israel/SelfPlayer/FaceDetector.cs`

Add the following methods inside the `FaceDetector` class (after `GenerateAnchors`):

- [ ] **Step 3: Add ResizeTexture helper**

```csharp
// Resizes any Texture2D to targetW × targetH using RenderTexture blit
private Texture2D ResizeTexture(Texture2D src, int targetW, int targetH)
{
    var rt   = RenderTexture.GetTemporary(targetW, targetH, 0);
    Graphics.Blit(src, rt);
    var prev = RenderTexture.active;
    RenderTexture.active = rt;
    var result = new Texture2D(targetW, targetH, TextureFormat.RGB24, false);
    result.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
    result.Apply();
    RenderTexture.active = prev;
    RenderTexture.ReleaseTemporary(rt);
    return result;
}
```

- [ ] **Step 4: Add TextureToTensor helper**

BlazeFace expects NCHW float32 in range [-1, 1].

```csharp
// Converts a 128x128 Texture2D to a [1,3,128,128] tensor normalized to [-1,1]
private Tensor<float> TextureToTensor(Texture2D tex)
{
    Color[] pixels = tex.GetPixels(); // bottom-left origin
    float[] data   = new float[1 * 3 * InputSize * InputSize];

    for (int y = 0; y < InputSize; y++)
    {
        for (int x = 0; x < InputSize; x++)
        {
            // Flip Y: texture bottom-left → model top-left
            Color c      = pixels[(InputSize - 1 - y) * InputSize + x];
            int baseIdx  = y * InputSize + x;
            data[0 * InputSize * InputSize + baseIdx] = c.r * 2f - 1f;
            data[1 * InputSize * InputSize + baseIdx] = c.g * 2f - 1f;
            data[2 * InputSize * InputSize + baseIdx] = c.b * 2f - 1f;
        }
    }

    return new Tensor<float>(new TensorShape(1, 3, InputSize, InputSize), data);
}
```

- [ ] **Step 5: Add Sigmoid helper and center-crop fallback**

```csharp
private float Sigmoid(float v) => 1f / (1f + Mathf.Exp(-v));

// Fallback: center square crop (normalized, top-left origin)
private Rect CenterCropNormalized() => new Rect(0.2f, 0.05f, 0.6f, 0.75f);
```

- [ ] **Step 6: Add the main DetectFace method**

```csharp
/// <summary>
/// Runs BlazeFace on <paramref name="texture"/> and returns a normalized Rect
/// (top-left origin, values in [0,1]) of the detected face.
/// Falls back to a center crop if no face is found above the threshold.
/// </summary>
public Rect DetectFace(Texture2D texture)
{
    Texture2D resized = ResizeTexture(texture, InputSize, InputSize);

    using Tensor<float> input = TextureToTensor(resized);
    Destroy(resized);

    worker.Schedule(input);

    // ---- IMPORTANT -------------------------------------------------------
    // If Unity Console shows different output names on first play,
    // replace "classificators" and "regressors" below with the correct names.
    // ----------------------------------------------------------------------
    var scoresTensor = worker.PeekOutput("classificators") as Tensor<float>;
    var boxesTensor  = worker.PeekOutput("regressors")     as Tensor<float>;

    scoresTensor.CompleteOperationsAndDownload();
    boxesTensor.CompleteOperationsAndDownload();

    // Find highest-confidence anchor
    int   bestIdx   = -1;
    float bestScore = confidenceThreshold;

    for (int i = 0; i < NumAnchors; i++)
    {
        float score = Sigmoid(scoresTensor[0, i, 0]);
        if (score > bestScore)
        {
            bestScore = score;
            bestIdx   = i;
        }
    }

    if (bestIdx < 0)
    {
        Debug.Log("[FaceDetector] No face detected — using center crop fallback.");
        return CenterCropNormalized();
    }

    // Decode bounding box from anchor + regressor delta
    float anchorY = anchors[bestIdx * 2];
    float anchorX = anchors[bestIdx * 2 + 1];

    // Stride determines the anchor cell size
    float anchorSize = (bestIdx < 512 ? 8f : 16f) / InputSize;

    float cy = anchorY    + boxesTensor[0, bestIdx, 0] * anchorSize;
    float cx = anchorX    + boxesTensor[0, bestIdx, 1] * anchorSize;
    float h  =              boxesTensor[0, bestIdx, 2] * anchorSize;
    float w  =              boxesTensor[0, bestIdx, 3] * anchorSize;

    // Add vertical padding to include the top of the head (hair)
    float padTop = 0.35f;
    cy -= h * padTop * 0.5f;
    h  *= (1f + padTop);

    float xMin = Mathf.Clamp01(cx - w * 0.5f);
    float yMin = Mathf.Clamp01(cy - h * 0.5f);
    float xMax = Mathf.Clamp01(cx + w * 0.5f);
    float yMax = Mathf.Clamp01(cy + h * 0.5f);

    Debug.Log($"[FaceDetector] Face detected — score={bestScore:F2} rect=({xMin:F2},{yMin:F2},{xMax-xMin:F2},{yMax-yMin:F2})");

    // Return top-left origin rect (matches model coordinate space)
    return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
}
```

- [ ] **Step 7: Verify compilation in Unity — no errors**

---

## Task 3 — Modify FaceCaptureController to Use FaceDetector

**Files:**
- Modify: `Assets/Scripts/level1 - israel/SelfPlayer/FaceCaptureController.cs` (lines 1–150)

- [ ] **Step 8: Add the FaceDetector field at the top of the class (after existing SerializeFields)**

Add this line after the existing `[SerializeField]` fields (around line 10):

```csharp
[SerializeField] private FaceDetector faceDetector;
```

- [ ] **Step 9: Replace the ToSprite method to accept an optional face Rect**

Replace the existing `ToSprite` method (lines 143–148):

**Old:**
```csharp
private Sprite ToSprite(Texture2D tex)
{
    if (tex == null) return null;
    Rect r = new Rect(0, 0, tex.width, tex.height);
    Vector2 pivot = new Vector2(0.5f, 0.5f);
    return Sprite.Create(tex, r, pivot, 100f);
}
```

**New:**
```csharp
// faceRectNormalized: top-left origin, values in [0,1]. Null = full frame.
private Sprite ToSprite(Texture2D tex, Rect? faceRectNormalized = null)
{
    if (tex == null) return null;

    Rect pixelRect;
    if (faceRectNormalized.HasValue)
    {
        Rect fr = faceRectNormalized.Value;
        // Convert top-left normalized → bottom-left pixel (Unity Sprite.Create convention)
        float pixX = fr.x       * tex.width;
        float pixY = (1f - fr.y - fr.height) * tex.height;
        float pixW = fr.width   * tex.width;
        float pixH = fr.height  * tex.height;
        pixelRect = new Rect(pixX, pixY, pixW, pixH);
    }
    else
    {
        pixelRect = new Rect(0, 0, tex.width, tex.height);
    }

    return Sprite.Create(tex, pixelRect, new Vector2(0.5f, 0.5f), 100f);
}
```

- [ ] **Step 10: Update CaptureCurrentStage to call FaceDetector before storing sprites**

Replace the `if (stage >= 3)` block inside `CaptureCurrentStage` (lines 116–127):

**Old:**
```csharp
if (stage >= 3)
{
    Sprite happy = ToSprite(capturedHappy);
    Sprite angry = ToSprite(capturedAngry);
    Sprite furious = ToSprite(capturedFurious);

    PlayerFaceStore.Set(happy, angry, furious);

    Close();
    return;
}
```

**New:**
```csharp
if (stage >= 3)
{
    Rect? happyRect   = faceDetector != null ? faceDetector.DetectFace(capturedHappy)   : (Rect?)null;
    Rect? angryRect   = faceDetector != null ? faceDetector.DetectFace(capturedAngry)   : (Rect?)null;
    Rect? furiousRect = faceDetector != null ? faceDetector.DetectFace(capturedFurious) : (Rect?)null;

    Sprite happy   = ToSprite(capturedHappy,   happyRect);
    Sprite angry   = ToSprite(capturedAngry,   angryRect);
    Sprite furious = ToSprite(capturedFurious, furiousRect);

    PlayerFaceStore.Set(happy, angry, furious);

    Close();
    return;
}
```

- [ ] **Step 11: Verify compilation — no errors in Unity Console**

---

## Task 4 — Wire Up in Unity Editor

- [ ] **Step 12: Add FaceDetector component to the FaceCapturePanel GameObject**

In the Unity Hierarchy, find the GameObject that has `FaceCaptureController`. Select it and:
1. Click **Add Component** → search for **FaceDetector**
2. Drag `Assets/Models/blazeface.onnx` into the **Model Asset** field on FaceDetector
3. Set **Confidence Threshold** to `0.75` (default)

- [ ] **Step 13: Assign FaceDetector reference in FaceCaptureController**

With the same GameObject selected, find the **FaceCaptureController** component:
- Drag the **FaceDetector** component (from the same GameObject) into the **Face Detector** field

- [ ] **Step 14: Test — Play the scene, capture 3 photos, check Console logs**

Expected Console output after capturing:
```
[FaceDetector] output layer: classificators
[FaceDetector] output layer: regressors
[FaceDetector] Face detected — score=0.91 rect=(0.28,0.12,0.44,0.58)
[FaceDetector] Face detected — score=0.89 rect=(0.27,0.11,0.45,0.60)
[FaceDetector] Face detected — score=0.88 rect=(0.29,0.13,0.43,0.57)
```

**If output layer names are different** (e.g. `output_0`, `scores`): open `FaceDetector.cs` and replace `"classificators"` and `"regressors"` with the names shown in the Console.

**If "No face detected" appears:** lower `confidenceThreshold` to `0.5` on the Inspector and try again, or move closer to the camera.

---

## Possible Follow-Up Adjustments

| Symptom | Fix |
|---------|-----|
| Crop cuts off the top of the head | Increase `padTop` constant in `DetectFace` (default 0.35) |
| Crop includes too much background | Decrease `padTop` |
| Inference is slow on device | Switch `BackendType.CPU` to `BackendType.GPUCompute` in `FaceDetector.Awake` |
| Output names wrong | Read Console log from Step 14, update strings in `DetectFace` |
