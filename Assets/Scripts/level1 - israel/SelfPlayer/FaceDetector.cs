// using UnityEngine;
// using Unity.InferenceEngine;
// using System.Collections.Generic;

// public class FaceDetector : MonoBehaviour
// {
//     [SerializeField] private ModelAsset modelAsset;
//     [SerializeField] [Range(0.3f, 0.95f)] private float confidenceThreshold = 0.75f;
//     [SerializeField] [Range(0f, 1f)] private float headPadding = 0.35f;

//     private Model runtimeModel;
//     private Worker worker;
//     private float[] anchors; // flat array: [cy0, cx0, cy1, cx1, ...] — one (cy,cx) pair per anchor

//     private const int InputSize   = 128;
//     private const int NumAnchors  = 896;

//     void Awake()
//     {
//         if (modelAsset == null)
//         {
//             Debug.LogError("[FaceDetector] modelAsset is not assigned. Assign blazeface.onnx in the Inspector.");
//             enabled = false;
//             return;
//         }

//         runtimeModel = ModelLoader.Load(modelAsset);
//         worker = new Worker(runtimeModel, BackendType.CPU);
//         GenerateAnchors();

//         // Log output names so you can verify them in Console on first play
//         foreach (var output in runtimeModel.outputs)
//             Debug.Log($"[FaceDetector] output layer: {output.name}");
//     }

//     // BlazeFace front-camera anchor grid for 128x128 input
//     // Stride 8  → 16x16 grid × 2 anchors = 512
//     // Stride 16 → 8x8  grid × 6 anchors = 384
//     // Total = 896
//     private void GenerateAnchors()
//     {
//         var list = new List<float>(NumAnchors * 2);
//         int[] strides          = { 8, 16 };
//         int[] anchorsPerCell   = { 2, 6 };

//         for (int s = 0; s < strides.Length; s++)
//         {
//             int stride      = strides[s];
//             int gridSize    = InputSize / stride;
//             int cellAnchors = anchorsPerCell[s];

//             for (int row = 0; row < gridSize; row++)
//             {
//                 for (int col = 0; col < gridSize; col++)
//                 {
//                     float cy = (row + 0.5f) / gridSize;
//                     float cx = (col + 0.5f) / gridSize;
//                     for (int a = 0; a < cellAnchors; a++)
//                     {
//                         list.Add(cy);
//                         list.Add(cx);
//                     }
//                 }
//             }
//         }
//         anchors = list.ToArray();
//     }

//     // Resizes any Texture2D to targetW × targetH using RenderTexture blit
//     private Texture2D ResizeTexture(Texture2D src, int targetW, int targetH)
//     {
//         var rt   = RenderTexture.GetTemporary(targetW, targetH, 0);
//         Graphics.Blit(src, rt);
//         var prev = RenderTexture.active;
//         RenderTexture.active = rt;
//         var result = new Texture2D(targetW, targetH, TextureFormat.RGB24, false);
//         result.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
//         result.Apply();
//         RenderTexture.active = prev;
//         RenderTexture.ReleaseTemporary(rt);
//         return result;
//     }

//     // Converts a 128x128 Texture2D to a [1,3,128,128] tensor normalized to [-1,1]
//     private Tensor<float> TextureToTensor(Texture2D tex)
//     {
//         Color[] pixels = tex.GetPixels(); // bottom-left origin
//         float[] data   = new float[1 * 3 * InputSize * InputSize];

//         for (int y = 0; y < InputSize; y++)
//         {
//             for (int x = 0; x < InputSize; x++)
//             {
//                 // Flip Y: texture bottom-left → model top-left
//                 Color c      = pixels[(InputSize - 1 - y) * InputSize + x];
//                 int baseIdx  = y * InputSize + x;
//                 data[0 * InputSize * InputSize + baseIdx] = c.r * 2f - 1f;
//                 data[1 * InputSize * InputSize + baseIdx] = c.g * 2f - 1f;
//                 data[2 * InputSize * InputSize + baseIdx] = c.b * 2f - 1f;
//             }
//         }

//         return new Tensor<float>(new TensorShape(1, 3, InputSize, InputSize), data);
//     }

//     private float Sigmoid(float v) => 1f / (1f + Mathf.Exp(-v));

//     // Fallback: center square crop (normalized, top-left origin)
//     private Rect CenterCropNormalized() => new Rect(0.2f, 0.05f, 0.6f, 0.75f);

//     /// <summary>
//     /// Runs BlazeFace on <paramref name="texture"/> and returns a normalized Rect
//     /// (top-left origin, values in [0,1]) of the detected face.
//     /// Falls back to a center crop if no face is found above the threshold.
//     /// </summary>
//    /// <summary>
// /// Runs BlazeFace on <paramref name="texture"/> and returns a normalized Rect
// /// (top-left origin, values in [0,1]) of the detected face.
// /// Falls back to a center crop if no face is found above the threshold.
// /// </summary>
// public Rect DetectFace(Texture2D texture)
// {
//     if (texture == null)
//     {
//         Debug.LogWarning("[FaceDetector] DetectFace called with null texture — using fallback.");
//         return CenterCropNormalized();
//     }

//     Texture2D resized = ResizeTexture(texture, InputSize, InputSize);

//     using Tensor<float> input = TextureToTensor(resized);
//     Destroy(resized);

//     worker.Schedule(input);

//     // ---- IMPORTANT -------------------------------------------------------
//     // If Unity Console shows different output names on first play,
//     // replace "classificators" and "regressors" below with the correct names.
//     // ----------------------------------------------------------------------
//     Tensor<float> scoresTensor = worker.PeekOutput("classificators") as Tensor<float>;
//     Tensor<float> boxesTensor = worker.PeekOutput("regressors") as Tensor<float>;

//     if (scoresTensor == null || boxesTensor == null)
//     {
//         Debug.LogError("[FaceDetector] Could not find model outputs 'classificators'/'regressors'. " +
//                        "Check the output layer names logged in Awake and update the strings.");
//         return CenterCropNormalized();
//     }

//     Tensor<float> scoresCpu = scoresTensor.ReadbackAndClone() as Tensor<float>;
//     Tensor<float> boxesCpu = boxesTensor.ReadbackAndClone() as Tensor<float>;

//     if (scoresCpu == null || boxesCpu == null)
//     {
//         Debug.LogError("[FaceDetector] Failed to read model outputs back to CPU.");
//         scoresCpu?.Dispose();
//         boxesCpu?.Dispose();
//         return CenterCropNormalized();
//     }

//     try
//     {
//         // Find highest-confidence anchor
//         int bestIdx = -1;
//         float bestScore = confidenceThreshold;

//         for (int i = 0; i < NumAnchors; i++)
//         {
//             float score = Sigmoid(scoresCpu[0, i, 0]);

//             if (score > bestScore)
//             {
//                 bestScore = score;
//                 bestIdx = i;
//             }
//         }

//         if (bestIdx < 0)
//         {
//             Debug.Log("[FaceDetector] No face detected — using center crop fallback.");
//             return CenterCropNormalized();
//         }

//         // Decode bounding box from anchor + regressor delta
//         float anchorY = anchors[bestIdx * 2];
//         float anchorX = anchors[bestIdx * 2 + 1];

//         // Stride determines the anchor cell size
//         float anchorSize = (bestIdx < 512 ? 8f : 16f) / InputSize;

//         float cy = anchorY + boxesCpu[0, bestIdx, 0] * anchorSize;
//         float cx = anchorX + boxesCpu[0, bestIdx, 1] * anchorSize;
//         float h = boxesCpu[0, bestIdx, 2] * anchorSize;
//         float w = boxesCpu[0, bestIdx, 3] * anchorSize;

//         // Add vertical padding to include the top of the head/hair
//         cy -= h * headPadding * 0.5f;
//         h *= 1f + headPadding;

//         float xMin = Mathf.Clamp01(cx - w * 0.5f);
//         float yMin = Mathf.Clamp01(cy - h * 0.5f);
//         float xMax = Mathf.Clamp01(cx + w * 0.5f);
//         float yMax = Mathf.Clamp01(cy + h * 0.5f);

//         Debug.Log($"[FaceDetector] Face detected — score={bestScore:F2} rect=({xMin:F2},{yMin:F2},{xMax - xMin:F2},{yMax - yMin:F2})");

//         // Return top-left origin rect
//         return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
//     }
//     finally
//     {
//         scoresCpu.Dispose();
//         boxesCpu.Dispose();
//     }
// }

//     void OnDestroy()
//     {
//         worker?.Dispose();
//     }
// }


using UnityEngine;
using Unity.InferenceEngine;
using System.Collections.Generic;

/// <summary>
/// Detects a face inside a Texture2D using BlazeFace ONNX model through Unity Inference Engine.
/// Returns a normalized Rect that can be used later to crop the detected face.
/// </summary>
public class FaceDetector : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private ModelAsset modelAsset;

    [Header("Detection Settings")]
    [SerializeField] [Range(0.3f, 0.95f)] private float confidenceThreshold = 0.75f;
    [SerializeField] [Range(0f, 1f)] private float headPadding = 0.35f;

    private Model runtimeModel;
    private Worker worker;

    // Flat array: [cy0, cx0, cy1, cx1, ...]
    // One (cy, cx) pair per anchor.
    private float[] anchors;

    private const int InputSize = 128;
    private const int NumAnchors = 896;

    private void Awake()
    {
        if (modelAsset == null)
        {
            Debug.LogError("[FaceDetector] Model Asset is not assigned. Assign Assets/Models/blazeface.onnx in the Inspector.");
            enabled = false;
            return;
        }

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.CPU);

        GenerateAnchors();

        // Log model output names so we can verify them in the Console.
        foreach (var output in runtimeModel.outputs)
        {
            Debug.Log($"[FaceDetector] output layer: {output.name}");
        }
    }

    /// <summary>
    /// Generates BlazeFace anchors for 128x128 input.
    ///
    /// Stride 8:
    /// 16x16 grid * 2 anchors = 512 anchors
    ///
    /// Stride 16:
    /// 8x8 grid * 6 anchors = 384 anchors
    ///
    /// Total:
    /// 896 anchors
    /// </summary>
    private void GenerateAnchors()
    {
        List<float> list = new List<float>(NumAnchors * 2);

        int[] strides = { 8, 16 };
        int[] anchorsPerCell = { 2, 6 };

        for (int s = 0; s < strides.Length; s++)
        {
            int stride = strides[s];
            int gridSize = InputSize / stride;
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

        if (anchors.Length != NumAnchors * 2)
        {
            Debug.LogWarning($"[FaceDetector] Anchor count mismatch. Expected {NumAnchors * 2}, got {anchors.Length}.");
        }
    }

    /// <summary>
    /// Resizes any Texture2D to the requested size using RenderTexture.
    /// </summary>
    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(targetWidth, targetHeight, 0);

        RenderTexture previous = RenderTexture.active;

        Graphics.Blit(source, renderTexture);

        RenderTexture.active = renderTexture;

        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return result;
    }

    /// <summary>
    /// Converts a 128x128 Texture2D into a Tensor with the shape:
    /// [1, 128, 128, 3]
    ///
    /// This is NHWC format:
    /// Batch, Height, Width, Channels.
    ///
    /// The model expects this shape:
    /// (1, 128, 128, 3)
    /// </summary>
    private Tensor<float> TextureToTensor(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();

        // BlazeFace model expects NHWC:
        // batch = 1, height = 128, width = 128, channels = 3.
        TensorShape shape = new TensorShape(1, InputSize, InputSize, 3);

        float[] data = new float[InputSize * InputSize * 3];

        int dataIndex = 0;

        for (int y = 0; y < InputSize; y++)
        {
            for (int x = 0; x < InputSize; x++)
            {
                // Unity Texture2D pixels start from bottom-left.
                // The model expects top-left image order, so we flip Y.
                Color color = pixels[(InputSize - 1 - y) * InputSize + x];

                // Keep the same normalization style as before: [-1, 1].
                data[dataIndex++] = color.r * 2f - 1f;
                data[dataIndex++] = color.g * 2f - 1f;
                data[dataIndex++] = color.b * 2f - 1f;
            }
        }

        return new Tensor<float>(shape, data);
    }

    private float Sigmoid(float value)
    {
        return 1f / (1f + Mathf.Exp(-value));
    }

    /// <summary>
    /// Fallback crop when no face is detected.
    /// Values are normalized and use top-left origin logic.
    /// </summary>
    private Rect CenterCropNormalized()
    {
        return new Rect(0.2f, 0.05f, 0.6f, 0.75f);
    }

    /// <summary>
    /// Runs BlazeFace on the given texture and returns a normalized Rect of the detected face.
    /// The returned Rect uses normalized values between 0 and 1.
    /// </summary>
    public Rect DetectFace(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogWarning("[FaceDetector] DetectFace was called with a null texture. Using fallback crop.");
            return CenterCropNormalized();
        }

        if (worker == null)
        {
            Debug.LogWarning("[FaceDetector] Worker is not ready. Using fallback crop.");
            return CenterCropNormalized();
        }

        if (anchors == null || anchors.Length == 0)
        {
            Debug.LogWarning("[FaceDetector] Anchors are not ready. Using fallback crop.");
            return CenterCropNormalized();
        }

        Texture2D resized = ResizeTexture(texture, InputSize, InputSize);

        using Tensor<float> input = TextureToTensor(resized);

        Destroy(resized);

        worker.Schedule(input);

        Tensor<float> scoresTensor = worker.PeekOutput("classificators") as Tensor<float>;
        Tensor<float> boxesTensor = worker.PeekOutput("regressors") as Tensor<float>;

        if (scoresTensor == null || boxesTensor == null)
        {
            Debug.LogError("[FaceDetector] Could not find model outputs 'classificators' and/or 'regressors'. Check the output names in the Console.");
            return CenterCropNormalized();
        }

        Tensor<float> scoresCpu = scoresTensor.ReadbackAndClone() as Tensor<float>;
        Tensor<float> boxesCpu = boxesTensor.ReadbackAndClone() as Tensor<float>;

        if (scoresCpu == null || boxesCpu == null)
        {
            Debug.LogError("[FaceDetector] Failed to read model outputs back to CPU.");

            scoresCpu?.Dispose();
            boxesCpu?.Dispose();

            return CenterCropNormalized();
        }

        try
        {
            int bestIndex = -1;
            float bestScore = confidenceThreshold;

            for (int i = 0; i < NumAnchors; i++)
            {
                float score = Sigmoid(scoresCpu[0, i, 0]);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                Debug.Log("[FaceDetector] No face detected above threshold. Using fallback crop.");
                return CenterCropNormalized();
            }

            float anchorY = anchors[bestIndex * 2];
            float anchorX = anchors[bestIndex * 2 + 1];

            // BlazeFace regressors are in pixel space (0–128 scale) — divide by InputSize to normalize
            float cy = anchorY + boxesCpu[0, bestIndex, 0] / InputSize;
            float cx = anchorX + boxesCpu[0, bestIndex, 1] / InputSize;
            float h  = boxesCpu[0, bestIndex, 2] / InputSize;
            float w  = boxesCpu[0, bestIndex, 3] / InputSize;

            // Add padding upward so the crop includes more head/hair area.
            cy -= h * headPadding * 0.5f;
            h *= 1f + headPadding;

            float xMin = Mathf.Clamp01(cx - w * 0.5f);
            float yMin = Mathf.Clamp01(cy - h * 0.5f);
            float xMax = Mathf.Clamp01(cx + w * 0.5f);
            float yMax = Mathf.Clamp01(cy + h * 0.5f);

            float rectWidth = xMax - xMin;
            float rectHeight = yMax - yMin;

            if (rectWidth <= 0.01f || rectHeight <= 0.01f)
            {
                Debug.LogWarning("[FaceDetector] Detected face rect is too small. Using fallback crop.");
                return CenterCropNormalized();
            }

            Rect faceRect = new Rect(xMin, yMin, rectWidth, rectHeight);

            Debug.Log($"[FaceDetector] Face detected. Score={bestScore:F2}, Rect={faceRect}");

            return faceRect;
        }
        finally
        {
            scoresCpu.Dispose();
            boxesCpu.Dispose();
        }
    }

    private void OnDestroy()
    {
        worker?.Dispose();
        worker = null;
    }
}