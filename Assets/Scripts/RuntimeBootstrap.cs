using UnityEngine;

// RuntimeBootstrap: ensures a TranslatorManager is available at runtime by instantiating
// a TranslatorManager prefab located in Resources/TranslatorManager (if present).
// Place this script on any GameObject in your startup scene.
public class RuntimeBootstrap : MonoBehaviour {
    void Awake() {
        if (FindObjectOfType<TranslatorClient>() == null) {
            var prefab = Resources.Load<GameObject>("TranslatorManager");
            if (prefab != null) {
                Instantiate(prefab);
                Debug.Log("[RuntimeBootstrap] Instantiated TranslatorManager from Resources/TranslatorManager.prefab");
            } else {
                Debug.LogWarning("[RuntimeBootstrap] TranslatorManager prefab not found in Resources/TranslatorManager. Please create it via Tools > Translator > Create Prefab Manager or add a TranslatorManager to your startup scene.");
            }
        }
    }
}
