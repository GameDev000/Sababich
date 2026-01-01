using UnityEngine;

public class SoundCoins : MonoBehaviour
{
    [SerializeField] private AudioClip coinClip;
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
}
