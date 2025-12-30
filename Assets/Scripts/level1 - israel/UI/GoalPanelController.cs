using UnityEngine;
using System.Collections;
/// <summary>
/// Controls the display of the goal panel UI element, showing it for a set duration at the start.
/// </summary>
public class GoalPanelController : MonoBehaviour
{
    [SerializeField] private GameObject goalPanel;
    [SerializeField] private float showDuration = 5f;

    /// <summary>
    /// Initializes the goal panel display on start.
    /// </summary>
    void Start()
    {
        goalPanel.SetActive(true);
        StartCoroutine(HidePanelAfterTime());
    }
    /// <summary>
    /// Coroutine to hide the goal panel after a specified duration.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HidePanelAfterTime()
    {
        yield return new WaitForSeconds(showDuration);
        goalPanel.SetActive(false);
    }
}
