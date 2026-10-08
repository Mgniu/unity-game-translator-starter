using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LibreResponse {
    public string translatedText;
}

public class TranslatorClient : MonoBehaviour {
    public static TranslatorClient Instance { get; private set; }

    [Header("Translation Server (LibreTranslate)")]
    public string serverUrl = "http://localhost:5000/translate";
    public string sourceLang = "en";
    public string targetLang = "zh";
    public int maxRetries = 2;

    private Dictionary<string, string> cache = new Dictionary<string, string>();
    private string cachePath => System.IO.Path.Combine(Application.persistentDataPath, "translation_cache.json");

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadDiskCache();
    }

    void OnApplicationQuit() {
        SaveDiskCache();
    }

    public void Translate(string text, Action<string> onResult) {
        if (string.IsNullOrEmpty(text)) {
            onResult?.Invoke(text);
            return;
        }

        if (cache.TryGetValue(text, out var cached)) {
            onResult?.Invoke(cached);
            return;
        }

        StartCoroutine(TranslateCoroutine(text, onResult));
    }

    IEnumerator TranslateCoroutine(string text, Action<string> onResult) {
        int attempt = 0;
        string payload = JsonUtility.ToJson(new { q = text, source = sourceLang, target = targetLang, format = "text" });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

        while (attempt <= maxRetries) {
            using (var uwr = new UnityWebRequest(serverUrl, "POST")) {
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");
                yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isError = uwr.result != UnityWebRequest.Result.Success;
#else
                bool isError = uwr.isNetworkError || uwr.isHttpError;
#endif
                if (!isError) {
                    try {
                        var respText = uwr.downloadHandler.text;
                        var resp = JsonUtility.FromJson<LibreResponse>(respText);
                        var translated = resp != null && !string.IsNullOrEmpty(resp.translatedText) ? resp.translatedText : respText;
                        cache[text] = translated;
                        onResult?.Invoke(translated);
                        yield break;
                    }
                    catch (Exception e) {
                        Debug.LogWarning("[TranslatorClient] parse failed: " + e + " raw: " + uwr.downloadHandler.text);
                        onResult?.Invoke(text);
                        yield break;
                    }
                }
                else {
                    Debug.LogWarning($"[TranslatorClient] request failed (attempt {attempt}): {uwr.error}");
                    attempt++;
                    yield return new WaitForSeconds(0.2f * attempt);
                }
            }
        }

        onResult?.Invoke(text);
    }

    void LoadDiskCache() {
        try {
            if (System.IO.File.Exists(cachePath)) {
                var json = System.IO.File.ReadAllText(cachePath);
                var kv = JsonUtility.FromJson<SerializableDict>(json);
                if (kv != null && kv.keys != null && kv.values != null) {
                    for (int i = 0; i < Math.Min(kv.keys.Length, kv.values.Length); i++) {
                        cache[kv.keys[i]] = kv.values[i];
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogWarning("[TranslatorClient] LoadDiskCache failed: " + e);
        }
    }

    void SaveDiskCache() {
        try {
            var kv = new SerializableDict();
            var keys = new List<string>();
            var values = new List<string>();
            foreach (var kvp in cache) {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
            kv.keys = keys.ToArray();
            kv.values = values.ToArray();
            var json = JsonUtility.ToJson(kv);
            System.IO.File.WriteAllText(cachePath, json);
        }
        catch (Exception e) {
            Debug.LogWarning("[TranslatorClient] SaveDiskCache failed: " + e);
        }
    }

    [Serializable]
    private class SerializableDict {
        public string[] keys;
        public string[] values;
    }
}
