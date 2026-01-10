using UnityEngine;

public class InfoPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPopup;

    public void Open()
    {
        if (infoPopup != null)
            infoPopup.SetActive(true);
    }

    public void Close()
    {
        if (infoPopup != null)
            infoPopup.SetActive(false);
    }
}
