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

    [Header("Translation Server")]
    public string serverUrl = "http://localhost:5000/translate";
    public string sourceLang = "en";
    public string targetLang = "zh";
    public int maxRetries = 2;
    public int maxConcurrentRequests = 2;

    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();
    private readonly Queue<TranslateRequest> queue = new Queue<TranslateRequest>();
    private int activeRequests = 0;
    private bool isProcessingQueue = false;
    private string cachePath => System.IO.Path.Combine(Application.persistentDataPath, "translation_cache.json");

    private class TranslateRequest {
        public string Text;
        public Action<string> Callback;
        public bool ForceRefresh;
    }

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

    public void Translate(string text, Action<string> onResult, bool forceRefresh = false) {
        if (string.IsNullOrEmpty(text)) {
            onResult?.Invoke(text);
            return;
        }

        if (!forceRefresh && cache.TryGetValue(text, out var cached)) {
            onResult?.Invoke(cached);
            return;
        }

        var req = new TranslateRequest { Text = text, Callback = onResult, ForceRefresh = forceRefresh };
        queue.Enqueue(req);
        if (!isProcessingQueue) {
            StartCoroutine(ProcessQueue());
        }
    }

    public void TranslateBatch(List<string> texts, Action<Dictionary<string, string>> onResult, bool forceRefresh = false) {
        if (texts == null || texts.Count == 0) {
            onResult?.Invoke(new Dictionary<string, string>());
            return;
        }

        var resultMap = new Dictionary<string, string>();
        var pending = new List<string>();

        foreach (var text in texts) {
            if (string.IsNullOrEmpty(text)) {
                continue;
            }

            if (!forceRefresh && cache.TryGetValue(text, out var cached)) {
                resultMap[text] = cached;
            }
            else {
                pending.Add(text);
            }
        }

        if (pending.Count == 0) {
            onResult?.Invoke(resultMap);
            return;
        }

        int total = pending.Count;
        var completed = 0;

        foreach (var text in pending) {
            Translate(text, (translated) => {
                if (!string.IsNullOrEmpty(translated)) {
                    resultMap[text] = translated;
                }
                completed++;
                if (completed >= total) {
                    onResult?.Invoke(resultMap);
                }
            }, forceRefresh);
        }
    }

    private IEnumerator ProcessQueue() {
        isProcessingQueue = true;

        while (queue.Count > 0) {
            while (activeRequests >= maxConcurrentRequests) {
                yield return null;
            }

            var req = queue.Dequeue();
            if (req == null) {
                continue;
            }

            activeRequests++;
            StartCoroutine(TranslateCoroutine(req.Text, req.Callback, req.ForceRefresh, () => activeRequests--));
        }

        isProcessingQueue = false;
    }

    private IEnumerator TranslateCoroutine(string text, Action<string> onResult, bool forceRefresh, Action onComplete) {
        int attempt = 0;
        while (attempt <= maxRetries) {
            using (var uwr = new UnityWebRequest(serverUrl, "POST")) {
                string payload = JsonUtility.ToJson(new { q = text, source = sourceLang, target = targetLang, format = "text" });
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

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
                        if (string.IsNullOrEmpty(translated)) {
                            translated = text;
                        }

                        cache[text] = translated;
                        onResult?.Invoke(translated);
                        onComplete?.Invoke();
                        yield break;
                    }
                    catch (Exception e) {
                        Debug.LogWarning("[TranslatorClient] parse failed: " + e + " raw: " + uwr.downloadHandler.text);
                        onResult?.Invoke(text);
                        onComplete?.Invoke();
                        yield break;
                    }
                }
                else {
                    attempt++;
                    if (attempt > maxRetries) {
                        Debug.LogWarning($"[TranslatorClient] Translation failed after {maxRetries} retries. Text: {text}; Error: {uwr.error}");
                        onResult?.Invoke(text);
                        onComplete?.Invoke();
                        yield break;
                    }

                    yield return new WaitForSeconds(0.2f * attempt);
                }
            }
        }

        onComplete?.Invoke();
    }

    private void LoadDiskCache() {
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

    private void SaveDiskCache() {
        try {
            var kv = new SerializableDict();
            var keys = new List<string>();
            var values = new List<string>();
            foreach (var item in cache) {
                keys.Add(item.Key);
                values.Add(item.Value);
            }
            kv.keys = keys.ToArray();
            kv.values = values.ToArray();
            System.IO.File.WriteAllText(cachePath, JsonUtility.ToJson(kv));
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
