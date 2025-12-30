using System.Collections;
using UnityEngine;
/// <summary>
/// Plays a sound clip when the object is clicked, starting from a specified time and for a set duration.
/// </summary>
public class ClickSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;// The AudioSource to play the sound
    [SerializeField] private AudioClip clip;// The audio clip to play
    [SerializeField] private float startTime = 0f;// Time in seconds to start playback from

    [Header("Timing")]
    [SerializeField] private float playDuration = 5f;// Duration to play the sound

    private Coroutine routine;// Reference to the running coroutine

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    /// <summary>
    /// Handles mouse click to play the sound.
    /// </summary>   
    private void OnMouseDown()
    {
        if (audioSource == null || clip == null) return;

        if (routine != null)
            StopCoroutine(routine);// Stop any existing sound playback

        routine = StartCoroutine(PlaySound());// Start playing the sound
    }

    /// <summary>
    /// Coroutine to play the sound from a specific time for a set duration.
    /// </summary>
    /// <returns></returns>
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
