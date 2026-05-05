using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class SessionExporter
{
    private const string DashboardFolder = "Dashboard";
    private const string HtmlFileName = "sababich_dashboard.html";
    private const string DataMarker = "const SABABICH_DATA =";
    private const string DataEndMarker = "};";

    private static string OutputPath =>
        Path.Combine(Application.persistentDataPath, DashboardFolder, HtmlFileName);

    private static string TemplatePath =>
        Path.Combine(Application.streamingAssetsPath, DashboardFolder, HtmlFileName);

    public static async Task ExportSession(SessionRecord record)
    {
        Debug.Log($"[SE] ExportSession: player={record.displayName} lastLevelReached={record.lastLevelReached} levels.Count={record.levels?.Count ?? -1}");

        // 1. Always load HTML structure from StreamingAssets template (picks up any HTML edits).
        bool templateExists = File.Exists(TemplatePath);
        Debug.Log($"[SE] TemplatePath={TemplatePath} exists={templateExists}");
        string html = templateExists ? File.ReadAllText(TemplatePath) : BuildMinimalShell();

        // 2. Load previous sessions from output file only; discard any with empty levels (broken old data).
        var sessions = new List<SessionRecord>();
        bool outputExists = File.Exists(OutputPath);
        Debug.Log($"[SE] OutputPath={OutputPath} exists={outputExists}");
        if (outputExists)
        {
            var all = ExtractSessions(File.ReadAllText(OutputPath));
            int beforeFilter = all.Count;

            foreach (var s in all)
            {
                if (s.levels != null && s.levels.Count > 0)
                {
                    sessions.Add(s);
                }
            }

            Debug.Log($"[SE] Previous sessions: before filter={beforeFilter} after filter={sessions.Count}");
        }

        // 3. Append new session.
        sessions.Add(record);
        Debug.Log($"[SE] Total sessions to write={sessions.Count}");

        // 4. Serialize the full list and embed it back into the HTML.
        var file = new SessionDataFile { sessions = sessions };
        string json = JsonUtility.ToJson(file, prettyPrint: false);
        html = EmbedData(html, json);

        // 5. Write updated HTML to persistentDataPath/Dashboard/.
        SaveHtml(html);

        bool written = File.Exists(OutputPath);
        Debug.Log($"[SE] File written successfully={written} path={OutputPath}");

        // 6. Push to cloud (skipped for guests).
        await CloudSessionHistory.AppendSession(record);

        Debug.Log("[SE] ExportSession complete");
    }

    private static List<SessionRecord> ExtractSessions(string html)
    {
        try
        {
            int start = html.IndexOf(DataMarker);
            if (start < 0)
            {
                return new List<SessionRecord>();
            }

            int jsonStart = html.IndexOf('{', start);
            int jsonEnd = html.IndexOf(DataEndMarker, jsonStart);
            if (jsonStart < 0 || jsonEnd < 0)
            {
                return new List<SessionRecord>();
            }

            string json = html.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var existing = JsonUtility.FromJson<SessionDataFile>(json);
            return existing?.sessions ?? new List<SessionRecord>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SessionExporter] Could not parse existing data — starting fresh. {e.Message}");
            return new List<SessionRecord>();
        }
    }

    private static string EmbedData(string html, string json)
    {
        int start = html.IndexOf(DataMarker);
        if (start < 0)
        {
            // Marker missing — inject before closing </script> tag.
            return html.Replace("</script>", $"{DataMarker} {json};\n</script>");
        }

        // Replace from the marker through the closing </script> tag — preserves it.
        int blockEnd = html.IndexOf("</script>", start);
        if (blockEnd >= 0)
        {
            blockEnd += "</script>".Length;
            return html.Substring(0, start) + $"{DataMarker} {json};</script>" + html.Substring(blockEnd);
        }

        // Fallback: no </script> found (minimal shell) — replace just the line.
        int lineEnd = html.IndexOf('\n', start);
        if (lineEnd < 0)
        {
            lineEnd = html.Length;
        }

        return html.Substring(0, start) + $"{DataMarker} {json};" + html.Substring(lineEnd);
    }

    private static void SaveHtml(string html)
    {
        string dir = Path.Combine(Application.persistentDataPath, DashboardFolder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(OutputPath, html, System.Text.Encoding.UTF8);

        // Copy supporting files so the browser can find them next to the HTML.
        CopyAsset("dashboard.css", dir);
        CopyAsset("dashboard.js", dir);
    }

    private static void CopyAsset(string fileName, string destDir)
    {
        string src = Path.Combine(Application.streamingAssetsPath, DashboardFolder, fileName);
        string dest = Path.Combine(destDir, fileName);

        if (File.Exists(src))
        {
            File.Copy(src, dest, overwrite: true);
        }
        else
        {
            Debug.LogWarning($"[SessionExporter] Asset not found in StreamingAssets: {fileName}");
        }
    }

    private static string BuildMinimalShell()
    {
        return "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
               "<title>Sababich Dashboard</title>" +
               "<script>const SABABICH_DATA = {\"sessions\":[]};</script>" +
               "</head><body><p>Dashboard loading...</p></body></html>";
    }
}