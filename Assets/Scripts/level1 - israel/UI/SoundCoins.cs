using UnityEngine;

public class SoundCoins : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip negativeClip; // negative indication

    [Header("Source")]
    [SerializeField] private AudioSource audioSource;

    public void PlayFromSecond(float startTime)
    {
        audioSource.clip = coinClip;
        if (startTime < 0f)
            startTime = 0f;

        if (startTime > coinClip.length)
            startTime = coinClip.length;

        audioSource.time = startTime;
        audioSource.Play();
    }

    // For duble-click
    public void PlayNegative()
    {
        if (negativeClip == null || audioSource == null)
            return;

        audioSource.pitch = Random.Range(0.95f, 1.05f); // The pitch of the sound
        audioSource.PlayOneShot(negativeClip);
        audioSource.pitch = 1f;
    }
}
