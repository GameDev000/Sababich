using UnityEngine;

public class ViewDashboardButton : MonoBehaviour
{
    // Attach to a UI Button's OnClick() in the MainMenu scene Inspector
    public void OpenDashboard()
    {
        string path = System.IO.Path.Combine(
            Application.persistentDataPath, "Dashboard", "sababich_dashboard.html");

        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning("[ViewDashboardButton] Dashboard not found yet — play a session first.");
            return;
        }

        // file:/// URL — opens in the system default browser, no server needed
        Application.OpenURL("file:///" + path.Replace("\\", "/"));
    }
}
