

// using UnityEngine;
// using UnityEngine.SceneManagement;

// /// <summary>
// /// Manages the main menu interactions and the navigation between gameplay scenes,
// /// end-level scenes, tutorial, and main menu.
// /// </summary>
// public class MainMenu : MonoBehaviour
// {
//     public void OnTutorialButtonClicked()
//     {
//         SceneManager.LoadScene("TutorialScene");
//     }

//     public void OnPlayButtonClicked()
//     {
//         SceneManager.LoadScene("level1 - israel");
//     }

//     public void OnBackToMainButtonClicked()
//     {
//         SceneManager.LoadScene("MainMenu");
//     }

//     public void PhaseOneButtonClicked()
//     {
//         SceneManager.LoadScene("level1 - israel");
//     }

//     /// <summary>
//     /// Handles the continue button from Level 1 end scene.
//     /// If Level 1 was completed successfully, load Level 1.1.
//     /// Otherwise, replay Level 1.
//     /// </summary>
//     public void OnEndLevelOne()
//     {
//         ShowAdThen(() =>
//         {
//             if (LevelOneState.IsSuccess)
//                 SceneManager.LoadScene("level1.1 - israel");
//             else
//                 SceneManager.LoadScene("level1 - israel");
//         });
//     }

//     /// <summary>
//     /// Handles the continue button from Level 1.1 end scene.
//     /// If Level 1.1 was completed successfully, load Level 1.2.
//     /// Otherwise, replay Level 1.1.
//     /// </summary>
//     public void OnEndLevelOneOne()
//     {
//         ShowAdThen(() =>
//         {
//             if (LevelOneOneState.IsSuccess)
//                 SceneManager.LoadScene("ChinaTransitionVideoScene");
//             else
//                 SceneManager.LoadScene("level1.1 - israel");
//         });
//     }

//     /// <summary>
//     /// Handles the continue button from Level 1.2 end scene.
//     /// If Level 1.2 was completed successfully, load Level 2.
//     /// Otherwise, replay Level 1.2.
//     /// </summary>
//     public void OnEndLevelOneTwo()
//     {
//         ShowAdThen(() =>
//         {
//             if (LevelOneTwoState.IsSuccess)
//                 SceneManager.LoadScene("level2 - china");
//             else
//                 SceneManager.LoadScene("level1.2 - china");
//         });
//     }

//     /// <summary>
//     /// Handles the continue button from Level 2 end scene.
//     /// If Level 2 was completed successfully, load Level 3.1 USA.
//     /// Otherwise, replay Level 2.
//     /// </summary>
//     public void OnEndLevelTwo()
//     {
//         ShowAdThen(() =>
//         {
//             if (LevelTwoState.IsSuccess)
//                 SceneManager.LoadScene("USATransitionVideoScene");
//             else
//                 SceneManager.LoadScene("level2 - china");
//         });
//     }

//     /// <summary>
//     /// Handles the continue button from Level 3.1 USA end scene.
//     /// If Level 3.1 was completed successfully, load Level 3 USA.
//     /// Otherwise, replay Level 3.1 USA.
//     /// </summary>
//     public void OnEndLevelThreeOne()
//     {
//         ShowAdThen(() =>
//         {
//             if (LevelThreeOneState.IsSuccess)
//                 SceneManager.LoadScene("MainMenu");
//             else
//                 SceneManager.LoadScene("level3.1 - USA");
//         });
//     }

//     /// <summary>
//     /// Handles the continue button from Level 3 end scene.
//     /// If Level 3 was completed successfully, return to the main menu.
//     /// Otherwise, replay Level 3.
//     /// </summary>
//     public void OnEndLevelThree()
//     {
//         ShowAdThen(() =>
//         {
//             if (LevelThreeState.IsSuccess)
//                 SceneManager.LoadScene("level3.1 - USA");
//             else
//                 SceneManager.LoadScene("level3 - USA");
//         });
//     }

//     /// <summary>
//     /// Shows an interstitial ad before continuing, if AdsManager exists.
//     /// If AdsManager is missing, continues immediately.
//     /// </summary>
//     private void ShowAdThen(System.Action afterAd)
//     {
//         if (global::AdsManager.Instance == null)
//         {
//             afterAd?.Invoke();
//             return;
//         }

//         global::AdsManager.Instance.ShowInterstitialThen(() =>
//         {
//             afterAd?.Invoke();
//         });
//     }
// }

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the main menu interactions and the navigation between gameplay scenes,
/// end-level scenes, tutorial, transition videos, and main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public void OnTutorialButtonClicked()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("level1 - israel");
    }

    public void OnBackToMainButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PhaseOneButtonClicked()
    {
        SceneManager.LoadScene("level1 - israel");
    }

    /// <summary>
    /// Loads Level 2.1 after the USA transition video.
    /// Connect this method to the continue/finish button of USATransitionVideoScene if needed.
    /// </summary>
    public void OnUSATransitionFinished()
    {
        SceneManager.LoadScene("level2.1 - USA");
    }

    /// <summary>
    /// Handles the continue button from Level 1 end scene.
    /// If Level 1 was completed successfully, load Level 1.1.
    /// Otherwise, replay Level 1.
    /// </summary>
    public void OnEndLevelOne()
    {
        ShowAdThen(() =>
        {
            if (LevelOneState.IsSuccess)
            {
                SceneManager.LoadScene("level1.1 - israel");
            }
            else
            {
                SceneManager.LoadScene("level1 - israel");
            }
        });
    }

    /// <summary>
    /// Handles the continue button from Level 1.1 end scene.
    /// If Level 1.1 was completed successfully, load the transition to China.
    /// Otherwise, replay Level 1.1.
    /// </summary>
    public void OnEndLevelOneOne()
    {
        ShowAdThen(() =>
        {
            if (LevelOneOneState.IsSuccess)
            {
                SceneManager.LoadScene("ChinaTransitionVideoScene");
            }
            else
            {
                SceneManager.LoadScene("level1.1 - israel");
            }
        });
    }

    /// <summary>
    /// Handles the continue button from Level 1.2 end scene.
    /// If Level 1.2 was completed successfully, load Level 2.
    /// Otherwise, replay Level 1.2.
    /// </summary>
    public void OnEndLevelOneTwo()
    {
        ShowAdThen(() =>
        {
            if (LevelOneTwoState.IsSuccess)
            {
                SceneManager.LoadScene("level2 - china");
            }
            else
            {
                SceneManager.LoadScene("level1.2 - china");
            }
        });
    }

    /// <summary>
    /// Handles the continue button from Level 2 end scene.
    /// If Level 2 was completed successfully, load the transition to USA.
    /// Otherwise, replay Level 2.
    /// </summary>
    public void OnEndLevelTwo()
    {
        ShowAdThen(() =>
        {
            if (LevelTwoState.IsSuccess)
            {
                SceneManager.LoadScene("USATransitionVideoScene");
            }
            else
            {
                SceneManager.LoadScene("level2 - china");
            }
        });
    }

    /// <summary>
    /// Handles the continue button from Level 2.1 USA end scene.
    /// If Level 2.1 was completed successfully, load Level 2.2 USA.
    /// Otherwise, replay Level 2.1 USA.
    /// </summary>
    public void OnEndLevelTwoOne()
    {
        ShowAdThen(() =>
        {
            if (LevelTwoOneState.IsSuccess)
            {
                SceneManager.LoadScene("level2.2 - USA");
            }
            else
            {
                SceneManager.LoadScene("level2.1 - USA");
            }
        });
    }

    /// <summary>
    /// Handles the continue button from Level 2.2 USA end scene.
    /// If Level 2.2 was completed successfully, load Level 3 USA.
    /// Otherwise, replay Level 2.2 USA.
    /// </summary>
    public void OnEndLevelTwoTwo()
    {
        ShowAdThen(() =>
        {
            if (LevelTwoTwoState.IsSuccess)
            {
                SceneManager.LoadScene("level3 - USA");
            }
            else
            {
                SceneManager.LoadScene("level2.2 - USA");
            }
        });
    }

    /// <summary>
    /// Handles the continue button from Level 3 end scene.
    /// If Level 3 was completed successfully, return to the main menu.
    /// Otherwise, replay Level 3.
    /// </summary>
    public void OnEndLevelThree()
    {
        ShowAdThen(() =>
        {
            if (LevelThreeState.IsSuccess)
            {
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                SceneManager.LoadScene("level3 - USA");
            }
        });
    }

    /// <summary>
    /// Shows an interstitial ad before continuing, if AdsManager exists.
    /// If AdsManager is missing, continues immediately.
    /// </summary>
    private void ShowAdThen(System.Action afterAd)
    {
        if (global::AdsManager.Instance == null)
        {
            afterAd?.Invoke();
            return;
        }

        global::AdsManager.Instance.ShowInterstitialThen(() =>
        {
            afterAd?.Invoke();
        });
    }
}