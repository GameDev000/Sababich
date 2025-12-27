using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MusicDropdownUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button musicButton;
    [SerializeField] private GameObject dropdownPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private MusicTrackButton buttonPrefab;

    private readonly List<MusicTrackButton> spawned = new();

    private void Awake()
    {
        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleDropdown);
    }

    private void Start()
    {
        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);

        BuildList();
    }

    private void ToggleDropdown()
    {
        if (dropdownPanel == null) return;
        dropdownPanel.SetActive(!dropdownPanel.activeSelf);
    }

    private void BuildList()
    {

        Debug.Log("BuildList called. MusicPlayer=" + (MusicPlayer.Instance != null));

        spawned.Clear();

        if (contentParent == null || buttonPrefab == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        var mp = MusicPlayer.Instance;
        if (mp == null) return;

        var tracks = mp.GetTracks();
        Debug.Log("Tracks count: " + (tracks == null ? -1 : tracks.Length));

        if (tracks == null) return;

        foreach (var t in tracks)
        {
            if (t == null) continue;

            var b = Instantiate(buttonPrefab, contentParent);
            b.Init(t.id, t.displayName, OnTrackChosen);
            spawned.Add(b);
        }

    }

    private void OnTrackChosen(string id)
    {
        MusicPlayer.Instance?.SetTrack(id, playImmediately: true);

        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }


}
