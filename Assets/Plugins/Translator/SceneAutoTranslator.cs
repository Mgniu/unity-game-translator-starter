using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneAutoTranslator : MonoBehaviour {
    public bool autoAttachOnStart = true;
    public bool includeChildren = true;
    public bool skipAlreadyChinese = true;

    void Start() {
        if (autoAttachOnStart) {
            AutoAttach();
        }
    }

    [ContextMenu("Attach AutoTranslate to TextMeshPro objects")]
    public void AutoAttach() {
        List<TMP_Text> texts = new List<TMP_Text>();
        GetTexts(transform, texts);

        foreach (var tmp in texts) {
            if (tmp.GetComponent<TextAutoTranslate>() == null) {
                tmp.gameObject.AddComponent<TextAutoTranslate>();
            }
        }
    }

    private void GetTexts(Transform root, List<TMP_Text> result) {
        foreach (Transform child in root) {
            var tmp = child.GetComponent<TMP_Text>();
            if (tmp != null) {
                result.Add(tmp);
            }

            if (includeChildren) {
                GetTexts(child, result);
            }
        }
    }
}
