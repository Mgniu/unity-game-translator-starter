# Unity Game Translator — Finalized Plugin Version

This repository now contains a near-drop-in Unity plugin that you can import into your projects. New additions:

- Assets/Prefabs are created via an Editor menu (Create Prefab Manager)
- RuntimeBootstrap to auto-instantiate the TranslatorManager prefab from Resources at runtime
- BuildTranslationExporter that scans enabled scenes and exports translations.json to Assets/Resources for offline builds

How to use (quick):

1. In Unity Editor, open the project containing this plugin (or copy the Assets folder into your project).
2. Tools -> Translator -> Create Prefab Manager. This creates TranslatorManager.prefab in Assets/Prefabs and Assets/Resources.
3. In your startup scene, add RuntimeBootstrap (to any GameObject) or rely on the Resources prefab to be instantiated at runtime.
4. Tools -> Translator -> Auto Translate Scene to attach TextAutoTranslate to all TextMeshPro texts in the current scene.
5. To pre-generate offline translations before a build, use Tools -> Translator -> Export Translations For Build. The exporter calls the configured LibreTranslate server (default http://localhost:5000/translate) and writes Assets/Resources/translations.json.

At runtime you can load and use the translations.json from Resources to avoid network calls.

Notes & Caveats:
- The build-time exporter sends individual HTTP requests for each unique string. Large projects may take a long time; consider batching or running on a machine with reliable network.
- The exporter uses synchronous HTTP calls in the Editor for simplicity. If you have special network requirements, adapt the code.
- For production, consider deploying a dedicated translate server or pre-translating content during localization pipelines.

