using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MusicTrackButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    private string trackId;
    private Action<string> onChosen;

    public string TrackId => trackId;

    public void Init(string id, string displayName, Action<string> onChosen)
    {
        trackId = id;
        this.onChosen = onChosen;

        if (label != null) label.text = displayName;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => this.onChosen?.Invoke(trackId));
    }
}
