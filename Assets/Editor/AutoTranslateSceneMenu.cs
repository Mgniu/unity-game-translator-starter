using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AutoTranslateSceneMenu {
    [MenuItem("Tools/Auto Translate Scene")]
    public static void AutoTranslateCurrentScene() {
        var activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();

        foreach (var root in roots) {
            var texts = root.GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var text in texts) {
                if (text.GetComponent<TextAutoTranslate>() == null) {
                    text.gameObject.AddComponent<TextAutoTranslate>();
                }
            }
        }

        Debug.Log($"[AutoTranslateSceneMenu] Attached TextAutoTranslate to {CountTextsInScene()} TMP objects in scene '{activeScene.name}'.");
    }

    [MenuItem("Tools/Auto Translate Scene/Attach Manager")]
    public static void CreateManager() {
        if (Object.FindObjectOfType<TranslatorClient>() == null) {
            var go = new GameObject("TranslatorManager");
            go.AddComponent<TranslatorClient>();
            Undo.RegisterCreatedObjectUndo(go, "Create TranslatorManager");
        }
        else {
            Debug.Log("[AutoTranslateSceneMenu] TranslatorManager already exists.");
        }
    }

    private static int CountTextsInScene() {
        int count = 0;
        var activeScene = SceneManager.GetActiveScene();
        foreach (var root in activeScene.GetRootGameObjects()) {
            count += root.GetComponentsInChildren<TMPro.TMP_Text>(true).Length;
        }
        return count;
    }
}
