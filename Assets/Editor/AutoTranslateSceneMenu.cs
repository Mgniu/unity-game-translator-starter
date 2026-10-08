using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AutoTranslateSceneMenu {
    [MenuItem("Tools/Translator/Create Manager")]
    public static void CreateManager() {
        if (Object.FindObjectOfType<TranslatorClient>() != null) {
            Debug.Log("[Translator] TranslatorManager already exists in the scene.");
            return;
        }

        var go = new GameObject("TranslatorManager");
        Undo.RegisterCreatedObjectUndo(go, "Create TranslatorManager");
        var client = go.AddComponent<TranslatorClient>();
        client.serverUrl = "http://localhost:5000/translate";
        client.sourceLang = "en";
        client.targetLang = "zh";
        client.maxRetries = 2;
        client.maxConcurrentRequests = 2;

        SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        Selection.activeObject = go;
        Debug.Log("[Translator] TranslatorManager created with default en->zh settings.");
    }

    [MenuItem("Tools/Translator/Auto Translate Scene")]
    public static void AutoTranslateCurrentScene() {
        var activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();
        int count = 0;

        foreach (var root in roots) {
            var texts = root.GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var text in texts) {
                if (text.GetComponent<TextAutoTranslate>() == null) {
                    text.gameObject.AddComponent<TextAutoTranslate>();
                    count++;
                }
            }
        }

        Debug.Log($"[Translator] Attached TextAutoTranslate to {count} TMP text objects in scene '{activeScene.name}'.");
    }
}
