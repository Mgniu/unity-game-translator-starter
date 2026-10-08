using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextAutoTranslate : MonoBehaviour {
    public bool translateOnStart = true;
    public bool translateOnTextChanged = true;
    public bool keepOriginalInTooltip = false;
    public bool skipIfAlreadyChinese = true;
    public bool autoTranslateRuntimeOnly = true;

    private TMP_Text tmp;
    private string originalText;
    private bool hasTranslated = false;
    private bool isTranslating = false;

    void Awake() {
        tmp = GetComponent<TMP_Text>();
        originalText = tmp.text;
    }

    void OnEnable() {
        if (tmp == null) {
            tmp = GetComponent<TMP_Text>();
        }

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
        if (obj == tmp && translateOnTextChanged && !isTranslating) {
            if (!string.IsNullOrEmpty(tmp.text) && tmp.text != originalText) {
                originalText = tmp.text;
                TryTranslateNow();
            }
        }
    }

    public void TryTranslateNow() {
        if (tmp == null) {
            tmp = GetComponent<TMP_Text>();
        }

        if (tmp == null) {
            return;
        }

        var textToTranslate = tmp.text;
        if (string.IsNullOrEmpty(textToTranslate)) {
            return;
        }

        if (skipIfAlreadyChinese && LooksLikeChinese(textToTranslate)) {
            return;
        }

        if (TranslatorClient.Instance == null) {
            Debug.LogWarning("[TextAutoTranslate] TranslatorClient is missing. Create a TranslatorManager object.");
            return;
        }

        if (isTranslating) {
            return;
        }

        isTranslating = true;
        originalText = textToTranslate;

        TranslatorClient.Instance.Translate(textToTranslate, (translated) => {
            isTranslating = false;

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

    public void ResetTextToOriginal() {
        if (tmp == null) {
            tmp = GetComponent<TMP_Text>();
        }
        if (tmp != null) {
            tmp.text = originalText;
        }
    }

    private bool LooksLikeChinese(string s) {
        foreach (char c in s) {
            if (c >= 0x4E00 && c <= 0x9FFF) {
                return true;
            }
        }
        return false;
    }

    private string Truncate(string s, int len) {
        if (s == null) return "";
        return s.Length <= len ? s : s.Substring(0, len) + "...";
    }
}
