using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// CreateTranslatorPrefab: editor script that creates a TranslatorManager prefab saved to
// Assets/Prefabs/TranslatorManager.prefab and also copies it to Assets/Resources/TranslatorManager.prefab
// so that RuntimeBootstrap can load it at runtime using Resources.Load.
public static class CreateTranslatorPrefab {
    [MenuItem("Tools/Translator/Create Prefab Manager")]
    public static void CreatePrefabManager() {
        // create GameObject
        if (Object.FindObjectOfType<TranslatorClient>() != null) {
            Debug.Log("[Translator] TranslatorManager already exists in scene. Will create prefab from existing instance.");
        }

        GameObject go = new GameObject("TranslatorManager");
        var client = go.AddComponent<TranslatorClient>();
        client.serverUrl = "http://localhost:5000/translate";
        client.sourceLang = "en";
        client.targetLang = "zh";
        client.maxRetries = 2;
        client.maxConcurrentRequests = 2;

        // Ensure directories
        var prefabsDir = "Assets/Prefabs";
        if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);
        var resourcesDir = "Assets/Resources";
        if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);

        // Save as prefab in Prefabs and Resources
        var prefabPath = Path.Combine(prefabsDir, "TranslatorManager.prefab");
        var resourcesPath = Path.Combine(resourcesDir, "TranslatorManager.prefab");

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.SaveAsPrefabAsset(go, resourcesPath);

        // cleanup created GameObject in scene
        Object.DestroyImmediate(go);

        AssetDatabase.Refresh();
        Debug.Log($"[Translator] TranslatorManager prefab created at {prefabPath} and {resourcesPath}");
    }
}
