using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextAutoTranslate : MonoBehaviour {
    public bool translateOnStart = true;
    public bool translateOnTextChanged = true;
    public bool keepOriginalInTooltip = false;

    private TMP_Text tmp;
    private string originalText;
    private bool hasTranslated = false;

    void Awake() {
        tmp = GetComponent<TMP_Text>();
    }

    void OnEnable() {
        originalText = tmp.text;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChangedEvent);
        if (translateOnStart) {
            TryTranslateNow();
        }
    }

    void OnDisable() {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChangedEvent);
    }

    void OnTextChangedEvent(Object obj) {
        if (obj == tmp && translateOnTextChanged) {
            if (tmp.text != originalText && !string.IsNullOrEmpty(tmp.text)) {
                originalText = tmp.text;
                TryTranslateNow();
            }
        }
    }

    public void TryTranslateNow() {
        var textToTranslate = tmp.text;
        if (string.IsNullOrEmpty(textToTranslate)) {
            return;
        }

        if (hasTranslated && LooksLikeChinese(textToTranslate)) {
            return;
        }

        if (TranslatorClient.Instance == null) {
            Debug.LogWarning("[TextAutoTranslate] TranslatorClient.Instance is null. Add TranslatorClient to the scene.");
            return;
        }

        TranslatorClient.Instance.Translate(textToTranslate, (translated) => {
            if (string.IsNullOrEmpty(translated) || translated == textToTranslate) {
                return;
            }

            tmp.text = translated;
            hasTranslated = true;

            if (keepOriginalInTooltip) {
                gameObject.name = $"{gameObject.name} (orig:{Truncate(originalText, 30)})";
            }
        });
    }

    bool LooksLikeChinese(string s) {
        foreach (char c in s) {
            if (c >= 0x4E00 && c <= 0x9FFF) {
                return true;
            }
        }
        return false;
    }

    string Truncate(string s, int len) {
        if (s == null) return "";
        return s.Length <= len ? s : s.Substring(0, len) + "...";
    }
}
