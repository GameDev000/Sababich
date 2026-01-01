// using UnityEngine;

// /// <summary>
// /// Per-scene binder that assigns the coin sound source to the persistent ScoreManager.
// /// Put this object in EACH scene (MainMenu / Tutorial / Level1 / Level2 / ...).
// /// </summary>
// public class SceneSoundBinder : MonoBehaviour
// {
//     // [Header("Scene Audio References")]
//     // [SerializeField] private SoundExample coinSoundSource; // Drag the scene's SoundExample here

//     // private void Start()
//     // {
//     //     // If ScoreManager is persistent (DontDestroyOnLoad), it may keep old/empty references.
//     //     // This binder refreshes the reference each time the scene loads.
//     //     if (ScoreManager.Instance != null && coinSoundSource != null)
//     //     {
//     //         ScoreManager.Instance.SetSoundCoins(coinSoundSource);
//     //     }
//     //     else
//     //     {
//     //         if (ScoreManager.Instance == null)
//     //             Debug.LogWarning("SceneSoundBinder: ScoreManager.Instance is null.");
//     //         if (coinSoundSource == null)
//     //             Debug.LogWarning("SceneSoundBinder: coinSoundSource is not assigned in Inspector.");
//     //     }
//     // }
// }
