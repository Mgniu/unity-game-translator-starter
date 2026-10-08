# Unity Game Translator Starter

A lightweight real-time English-to-Chinese translation starter for Unity, powered by a free local LibreTranslate service.

## Features

- Real-time translation for TextMeshPro text
- Local LibreTranslate integration (free and self-hosted)
- Simple in-memory + disk caching
- Scene scanning to auto-register text objects
- Editor menu for one-click scene setup
- Queue-based request handling to reduce overload

## Setup

1. Start a local translation server:

```bash
docker pull libretranslate/libretranslate
docker run -d -p 5000:5000 libretranslate/libretranslate
```

2. Copy the script files into your Unity project:
   - `Assets/Plugins/Translator/TranslatorClient.cs`
   - `Assets/Plugins/Translator/TextAutoTranslate.cs`
   - `Assets/Plugins/Translator/SceneAutoTranslator.cs`
   - `Assets/Editor/AutoTranslateSceneMenu.cs`

3. In Unity, open the menu:
   - `Tools > Translator > Create Manager`

4. Then run:
   - `Tools > Translator > Auto Translate Scene`

5. Play the scene and verify that TextMeshPro objects are translated from English to Chinese.

## Default configuration

- `serverUrl`: `http://localhost:5000/translate`
- `sourceLang`: `en`
- `targetLang`: `zh`

## Notes

- This is a starter framework for rapid prototyping and gameplay localization.
- For production, add pre-generated localization tables or a dedicated server deployment.

## License

MIT
