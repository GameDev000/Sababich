// using UnityEngine;

// public class FeatureHintsSequence : MonoBehaviour
// {
//     public enum TriggerType { OnLevelStart, OnCustomerIndex }

//     [System.Serializable]
//     public class Step
//     {
//         [TextArea(2, 5)]
//         public string message;

//         public Transform target;
//         public TriggerType trigger;
//         public int levelId = 1;
//         public int customerIndex = 1;
//     }

//     [Header("Steps (in order)")]
//     [SerializeField] private Step[] steps;

//     [Header("UI")]
//     [SerializeField] private FeatureHintOverlay overlay;

//     [Header("State")]
//     [SerializeField] private int currentLevelId = 1;

//     private int nextStep = 0;
//     private int currentCustomerIndex = 0;
//     private bool showing = false;

//     public void OnLevelStarted(int levelId)
//     {
//         currentLevelId = levelId;
//         currentCustomerIndex = 0;
//         TryRunNext();
//     }

//     public void OnCustomerSpawned()
//     {
//         currentCustomerIndex++;
//         TryRunNext();
//     }

//     private void TryRunNext()
//     {
//         if (showing) return;
//         if (steps == null || nextStep >= steps.Length) return;

//         Step s = steps[nextStep];

//         bool shouldShow =
//             (s.trigger == TriggerType.OnLevelStart && s.levelId == currentLevelId) ||
//             (s.trigger == TriggerType.OnCustomerIndex && s.customerIndex == currentCustomerIndex);

//         if (!shouldShow) return;

//         showing = true;
//         Time.timeScale = 0f;

//         overlay.Show(s.message, s.target, () =>
//         {
//             overlay.Hide();
//             Time.timeScale = 1f;
//             showing = false;
//             nextStep++;
//             TryRunNext();
//         });
//     }
// }



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

        [Header("Voice")]
        public AudioClip instructionVoiceClip;
    }

    [Header("Steps (in order)")]
    [SerializeField] private Step[] steps;

    [Header("UI")]
    [SerializeField] private FeatureHintOverlay overlay;

    [Header("Instruction Voice")]
    [SerializeField] private AudioSource instructionVoiceAudioSource;
    [SerializeField, Range(0f, 1f)] private float instructionVoiceVolume = 1f;

    [Tooltip("If true, the instruction voice can keep playing even while the game is paused.")]
    [SerializeField] private bool ignoreListenerPause = true;

    [Header("State")]
    [SerializeField] private int currentLevelId = 1;

    private int nextStep = 0;
    private int currentCustomerIndex = 0;
    private bool showing = false;

    private void Awake()
    {
        if (instructionVoiceAudioSource != null)
        {
            instructionVoiceAudioSource.playOnAwake = false;
            instructionVoiceAudioSource.loop = false;
            instructionVoiceAudioSource.ignoreListenerPause = ignoreListenerPause;
        }
    }

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
        if (showing)
        {
            return;
        }

        if (steps == null || nextStep >= steps.Length)
        {
            return;
        }

        Step s = steps[nextStep];

        bool shouldShow =
            (s.trigger == TriggerType.OnLevelStart && s.levelId == currentLevelId) ||
            (s.trigger == TriggerType.OnCustomerIndex && s.customerIndex == currentCustomerIndex);

        if (!shouldShow)
        {
            return;
        }

        if (overlay == null)
        {
            Debug.LogWarning("FeatureHintsSequence: Overlay is not assigned.");
            return;
        }

        showing = true;
        Time.timeScale = 0f;

        PlayInstructionVoice(s);

        overlay.Show(s.message, s.target, () =>
        {
            StopInstructionVoice();

            overlay.Hide();
            Time.timeScale = 1f;

            showing = false;
            nextStep++;

            TryRunNext();
        });
    }

    private void PlayInstructionVoice(Step step)
    {
        if (step == null)
        {
            return;
        }

        if (instructionVoiceAudioSource == null)
        {
            return;
        }

        if (step.instructionVoiceClip == null)
        {
            return;
        }

        instructionVoiceAudioSource.Stop();
        instructionVoiceAudioSource.clip = step.instructionVoiceClip;
        instructionVoiceAudioSource.volume = instructionVoiceVolume;
        instructionVoiceAudioSource.loop = false;
        instructionVoiceAudioSource.ignoreListenerPause = ignoreListenerPause;
        instructionVoiceAudioSource.Play();
    }

    private void StopInstructionVoice()
    {
        if (instructionVoiceAudioSource == null)
        {
            return;
        }

        instructionVoiceAudioSource.Stop();
        instructionVoiceAudioSource.clip = null;
    }

    private void OnDisable()
    {
        StopInstructionVoice();

        if (showing)
        {
            Time.timeScale = 1f;
            showing = false;
        }
    }
}