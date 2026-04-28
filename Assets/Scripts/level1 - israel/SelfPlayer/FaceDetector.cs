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
