using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// BuildTranslationExporter: Editor utility to collect all TextMeshPro strings from scenes
// in Build Settings and export a translations JSON by calling the configured LibreTranslate server.
// The output is written to Assets/Resources/translations.json so it can be loaded at runtime.
public static class BuildTranslationExporter {
    [Serializable]
    private class LibreResponse {
        public string translatedText;
    }

    [Serializable]
    private class SerializableDict {
        public string[] keys;
        public string[] values;
    }

    [MenuItem("Tools/Translator/Export Translations For Build")]
    public static void ExportTranslationsForBuild() {
        // Ask for server URL
        var serverUrl = EditorUtility.DisplayDialogComplex("Translation Server",
            "Use the default local LibreTranslate server at http://localhost:5000/translate?",
            "Use Default", "Cancel", "Custom URL");

        string url = "http://localhost:5000/translate";
        if (serverUrl == 1) { // Cancel
            return;
        } else if (serverUrl == 2) { // Custom
            url = EditorUtility.DisplayDialogComplex("Custom URL",
                "Please set custom URL in the next step. This dialog is a placeholder.",
                "OK", "Cancel", "").ToString();
            // Instead of complex input, show a prompt in console and let user modify TranslatorClient later.
            Debug.LogWarning("[BuildTranslationExporter] Custom URL flow not implemented; using default http://localhost:5000/translate");
        }

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
        if (scenes.Count == 0) {
            EditorUtility.DisplayDialog("Export Translations", "No enabled scenes found in Build Settings.", "OK");
            return;
        }

        var allTexts = new HashSet<string>();
        int sceneIndex = 0;

        foreach (var scenePath in scenes) {
            sceneIndex++;
            EditorUtility.DisplayProgressBar("Collecting texts", $"Processing {scenePath}", (float)sceneIndex / scenes.Count);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots) {
                var tmps = root.GetComponentsInChildren<TMPro.TMP_Text>(true);
                foreach (var t in tmps) {
                    var s = t.text;
                    if (!string.IsNullOrWhiteSpace(s)) {
                        allTexts.Add(s);
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();

        if (allTexts.Count == 0) {
            EditorUtility.DisplayDialog("Export Translations", "No TextMeshPro texts found in enabled scenes.", "OK");
            return;
        }

        // Confirm
        if (!EditorUtility.DisplayDialog("Export Translations",
            $"Found {allTexts.Count} unique text entries. Translate them all using {url}?",
            "Start", "Cancel")) {
            return;
        }

        // Translate each text synchronously. This is simple but may be slow for many strings.
        var client = new HttpClient();
        var resultMap = new Dictionary<string, string>();
        int i = 0;
        int total = allTexts.Count;

        try {
            foreach (var text in allTexts) {
                i++;
                EditorUtility.DisplayProgressBar("Translating", Truncate(text, 80), (float)i / total);

                var payload = new { q = text, source = "en", target = "zh", format = "text" };
                var payloadJson = JsonUtility.ToJson(payload);
                var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

                HttpResponseMessage resp = client.PostAsync(url, content).Result;
                if (resp.IsSuccessStatusCode) {
                    var respText = resp.Content.ReadAsStringAsync().Result;
                    try {
                        var lr = JsonUtility.FromJson<LibreResponse>(respText);
                        var translated = lr != null && !string.IsNullOrEmpty(lr.translatedText) ? lr.translatedText : respText;
                        resultMap[text] = translated;
                    }
                    catch (Exception e) {
                        Debug.LogWarning($"[BuildTranslationExporter] parse failed for '{text}': {e}. Raw: {respText}");
                        resultMap[text] = text; // fallback
                    }
                } else {
                    Debug.LogWarning($"[BuildTranslationExporter] HTTP {resp.StatusCode} for text: {text}");
                    resultMap[text] = text; // fallback
                }
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        // Write to Assets/Resources/translations.json
        var dir = "Assets/Resources";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var kv = new SerializableDict();
        kv.keys = resultMap.Keys.ToArray();
        kv.values = resultMap.Values.ToArray();
        var outJson = JsonUtility.ToJson(kv, true);
        var outPath = Path.Combine(dir, "translations.json");
        File.WriteAllText(outPath, outJson, Encoding.UTF8);

        AssetDatabase.ImportAsset(outPath);
        EditorUtility.DisplayDialog("Export Translations", $"Exported {resultMap.Count} translations to {outPath}", "OK");
    }

    private static string Truncate(string s, int len) {
        if (s == null) return "";
        return s.Length <= len ? s : s.Substring(0, len) + "...";
    }
}
