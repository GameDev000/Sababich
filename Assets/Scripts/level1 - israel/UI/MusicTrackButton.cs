using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
/// <summary>
/// Represents a button for selecting a music track in the dropdown UI.
/// </summary>
public class MusicTrackButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;// Text label to display the track name
    private string trackId;
    private Action<string> onChosen;// Callback for when the track is chosen

    public string TrackId => trackId;// Public getter for the track ID

    /// <summary>
    /// Initializes the music track button with the given parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="displayName"></param>
    /// <param name="onChosen"></param>
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
