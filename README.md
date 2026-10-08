# Unity Game Translator Starter

A lightweight real-time English-to-Chinese translation starter for Unity, powered by a free local LibreTranslate service.

## Features

- Real-time translation for TextMeshPro text
- Local LibreTranslate integration (free and self-hosted)
- Simple in-memory + disk caching
- Scene scanning to auto-register text objects
- Editor menu for one-click scanning
- Queue-based request handling to reduce overload

## Requirements

- Unity 2020.3+ (works with newer versions as well)
- TextMeshPro installed in the project
- Docker for local translation server

## Start the translation server

```bash
docker pull libretranslate/libretranslate
docker run -d -p 5000:5000 libretranslate/libretranslate
```

## Quick setup

1. Import TextMeshPro if not already present.
2. Copy the scripts into your Unity project:
   - `Assets/Plugins/Translator/TranslatorClient.cs`
   - `Assets/Plugins/Translator/TextAutoTranslate.cs`
   - `Assets/Plugins/Translator/SceneAutoTranslator.cs`
   - `Assets/Editor/AutoTranslateSceneMenu.cs`
3. Create an empty GameObject named `TranslatorManager`.
4. Add `TranslatorClient` to it.
5. In the Unity Editor, open the menu:
   - `Tools > Auto Translate Scene`
6. Or attach `SceneAutoTranslator` to a root object and run the scene.

## Usage

The `TextAutoTranslate` component automatically translates any attached TextMeshPro text when it becomes active. It respects caches and avoids repeatedly translating already-translated Chinese strings.

## Notes

- Default endpoint: `http://localhost:5000/translate`
- Default language pair: `en -> zh`
- You can change `serverUrl`, `sourceLang`, and `targetLang` in the inspector.
- This is a starter framework to support rapid prototyping and gameplay text localization.

## License

MIT
