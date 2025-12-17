using System.Collections;
using UnityEngine;

public class ClickSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float startTime = 0f;

    [Header("Timing")]
    [SerializeField] private float playDuration = 5f;

    private Coroutine routine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        if (audioSource == null || clip == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PlaySound());
    }

    private IEnumerator PlaySound()
    {
        audioSource.clip = clip;
        audioSource.time = startTime;
        audioSource.Play();

        if (playDuration > 0f)
        {
            yield return new WaitForSeconds(playDuration);
            audioSource.Stop();
        }

        routine = null;
    }
}
