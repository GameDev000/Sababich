using UnityEngine;

public class FeatureHintsSequence : MonoBehaviour
{
    public enum TriggerType { OnLevelStart, OnCustomerIndex }

    [System.Serializable]
    public class Step
    {
        [TextArea(2, 5)]
        public string message;

        public Transform target;
        public TriggerType trigger;
        public int levelId = 1;
        public int customerIndex = 1;
    }

    [Header("Steps (in order)")]
    [SerializeField] private Step[] steps;

    [Header("UI")]
    [SerializeField] private FeatureHintOverlay overlay;

    [Header("State")]
    [SerializeField] private int currentLevelId = 1;

    private int nextStep = 0;
    private int currentCustomerIndex = 0;
    private bool showing = false;

    public void OnLevelStarted(int levelId)
    {
        currentLevelId = levelId;
        currentCustomerIndex = 0;
        TryRunNext();
    }

    public void OnCustomerSpawned()
    {
        currentCustomerIndex++;
        TryRunNext();
    }

    private void TryRunNext()
    {
        if (showing) return;
        if (steps == null || nextStep >= steps.Length) return;

        Step s = steps[nextStep];

        bool shouldShow =
            (s.trigger == TriggerType.OnLevelStart && s.levelId == currentLevelId) ||
            (s.trigger == TriggerType.OnCustomerIndex && s.customerIndex == currentCustomerIndex);

        if (!shouldShow) return;

        showing = true;
        Time.timeScale = 0f;

        overlay.Show(s.message, s.target, () =>
        {
            overlay.Hide();
            Time.timeScale = 1f;
            showing = false;
            nextStep++;
            TryRunNext();
        });
    }
}
