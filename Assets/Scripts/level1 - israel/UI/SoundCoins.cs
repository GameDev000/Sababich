// using UnityEngine;

// public class SoundCoins : MonoBehaviour
// {
//     [Header("Clips")]
//     [SerializeField] private AudioClip coinClip;
//     [SerializeField] private AudioClip negativeClip; // negative indication

//     [Header("Source")]
//     [SerializeField] private AudioSource audioSource;

//     [SerializeField] private AudioClip duplicateClickClip;

//     public void PlayFromSecond(float startTime)
//     {
//         audioSource.clip = coinClip;
//         if (startTime < 0f)
//             startTime = 0f;

//         if (startTime > coinClip.length)
//             startTime = coinClip.length;

//         audioSource.time = startTime;
//         audioSource.Play();
//     }

//     // For duble-click
//     public void PlayNegative()
//     {
//         if (negativeClip == null || audioSource == null)
//             return;

//         audioSource.pitch = Random.Range(0.95f, 1.05f); // The pitch of the sound
//         audioSource.PlayOneShot(negativeClip);
//         audioSource.pitch = 1f;
//     }

//     // For duplicate ingredient click penalty
//     public void PlayDuplicateClick()
//     {
//         if (duplicateClickClip == null || audioSource == null)
//             return;

//         audioSource.pitch = Random.Range(0.95f, 1.05f);
//         audioSource.PlayOneShot(duplicateClickClip);
//         audioSource.pitch = 1f;
//     }
// }


using System.Collections;
using UnityEngine;

public class SoundCoins : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip negativeClip;
    [SerializeField] private AudioClip duplicateClickClip;

    [Header("Source")]
    [SerializeField] private AudioSource audioSource;

    private bool isDuplicateClickClipPlaying = false;

    public void PlayFromSecond(float startTime)
    {
        if (coinClip == null || audioSource == null)
            return;

        audioSource.clip = coinClip;

        if (startTime < 0f)
            startTime = 0f;

        if (startTime > coinClip.length)
            startTime = coinClip.length;

        audioSource.time = startTime;
        audioSource.Play();
    }

    public void PlayNegative()
    {
        if (negativeClip == null || audioSource == null)
            return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(negativeClip);
        audioSource.pitch = 1f;
    }

    public void PlayDuplicateClick()
    {
        if (duplicateClickClip == null || audioSource == null)
            return;

        if (isDuplicateClickClipPlaying)
            return;

        StartCoroutine(PlayDuplicateClickRoutine());
    }

    private IEnumerator PlayDuplicateClickRoutine()
    {
        isDuplicateClickClipPlaying = true;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(duplicateClickClip);
        audioSource.pitch = 1f;

        yield return new WaitForSeconds(duplicateClickClip.length);

        isDuplicateClickClipPlaying = false;
    }
}