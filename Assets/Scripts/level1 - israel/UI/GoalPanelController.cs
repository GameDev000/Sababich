using UnityEngine;
using System.Collections;

public class GoalPanelController : MonoBehaviour
{
    [SerializeField] private GameObject goalPanel;
    [SerializeField] private float showDuration = 5f;

    void Start()
    {
        goalPanel.SetActive(true);
        StartCoroutine(HidePanelAfterTime());
    }

    private IEnumerator HidePanelAfterTime()
    {
        yield return new WaitForSeconds(showDuration);
        goalPanel.SetActive(false);
    }
}
