using UnityEngine;
using Unity.InferenceEngine;
using System.Collections.Generic;

public class FaceDetector : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] [Range(0.3f, 0.95f)] private float confidenceThreshold = 0.75f;

    private Model runtimeModel;
    private Worker worker;
    private float[] anchors; // flat array: [cy0, cx0, cy1, cx1, ...] — one (cy,cx) pair per anchor

    private const int InputSize   = 128;
    private const int NumAnchors  = 896;

    void Awake()
    {
        if (modelAsset == null)
        {
            Debug.LogError("[FaceDetector] modelAsset is not assigned. Assign blazeface.onnx in the Inspector.");
            enabled = false;
            return;
        }

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

    private float Sigmoid(float v) => 1f / (1f + Mathf.Exp(-v));

    // Fallback: center square crop (normalized, top-left origin)
    private Rect CenterCropNormalized() => new Rect(0.2f, 0.05f, 0.6f, 0.75f);

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

    void OnDestroy()
    {
        worker?.Dispose();
    }
}
