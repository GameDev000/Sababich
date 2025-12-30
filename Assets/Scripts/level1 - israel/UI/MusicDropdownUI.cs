using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// manages the music selection dropdown UI, allowing players to choose background music tracks.
/// </summary>
public class MusicDropdownUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button musicButton;// Button to toggle the dropdown
    [SerializeField] private GameObject dropdownPanel;// Panel containing the dropdown list
    [SerializeField] private Transform contentParent;// Parent transform for the track buttons
    [SerializeField] private MusicTrackButton buttonPrefab;// Prefab for individual track buttons

    private readonly List<MusicTrackButton> spawned = new();

    private void Awake()
    {
        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleDropdown);// Setup button click listener
    }
    /// <summary>
    /// Initializes the dropdown UI on start.
    /// </summary>
    private void Start()
    {
        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);

        BuildList();// Populate the dropdown list with tracks
    }
    /// <summary>
    /// Toggles the visibility of the dropdown panel.
    /// </summary>
    private void ToggleDropdown()
    {
        if (dropdownPanel == null) return;
        dropdownPanel.SetActive(!dropdownPanel.activeSelf);
    }

    /// <summary>
    /// Builds the list of music tracks in the dropdown panel.
    /// </summary>
    private void BuildList()
    {

        Debug.Log("BuildList called. MusicPlayer=" + (MusicPlayer.Instance != null));

        spawned.Clear();// Clear existing buttons

        if (contentParent == null || buttonPrefab == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        var mp = MusicPlayer.Instance;// Get the MusicPlayer instance
        if (mp == null) return;

        var tracks = mp.GetTracks();// Get available music tracks
        Debug.Log("Tracks count: " + (tracks == null ? -1 : tracks.Length));

        if (tracks == null) return;
        // Create a button for each track
        foreach (var t in tracks)
        {
            if (t == null) continue;

            var b = Instantiate(buttonPrefab, contentParent);
            b.Init(t.id, t.displayName, OnTrackChosen);// Initialize button with track info
            spawned.Add(b);// Keep track of spawned buttons
        }

    }

    private void OnTrackChosen(string id)
    {
        MusicPlayer.Instance?.SetTrack(id, playImmediately: true);

        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }


}
