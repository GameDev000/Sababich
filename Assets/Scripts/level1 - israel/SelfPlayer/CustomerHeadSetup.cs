using UnityEngine;

public class CustomerHeadSetup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private CustomerMoodTimer_levels moodTimer;

    // Call this from Customer.Init after assigning Data
    public void Apply(CustomerType data)
    {
        if (data == null) return;

        if (bodyRenderer != null && data.sprite != null)
            bodyRenderer.sprite = data.sprite;

        if (moodTimer != null && headRenderer != null)
        {
            moodTimer.SetRenderer(headRenderer);
            moodTimer.Configure(data.happyFace, data.angryFaces);
        }
    }
}
