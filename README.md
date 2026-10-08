# Unity Game Translator Starter

A simple Unity starter project for real-time English-to-Chinese game text translation using a free local translation server.

## Overview

This project provides:
- a Unity C# client that speaks to a local LibreTranslate server
- automatic translation for TextMeshPro UI text
- in-memory + disk cache for repeated text
- a simple setup path for game localization in real time

## Quick start

1. Start the translation server locally with Docker:

```bash
docker pull libretranslate/libretranslate
docker run -d -p 5000:5000 libretranslate/libretranslate
```

2. Open the Unity project or import these scripts into a Unity project.

3. Create an empty GameObject named `TranslatorManager` and attach `TranslatorClient`.

4. Attach `TextAutoTranslate` to each TextMeshPro object you want to translate.

5. Run the scene.

## Included files

- `Assets/Plugins/Translator/TranslatorClient.cs`
- `Assets/Plugins/Translator/TextAutoTranslate.cs`
- `README.md`

## Example translation request

```bash
curl -X POST "http://localhost:5000/translate" \
  -H "Content-Type: application/json" \
  -d '{"q":"Hello world","source":"en","target":"zh","format":"text"}'
```

Expected output:

```json
{"translatedText":"你好，世界"}
```

## Notes

- `TranslatorClient` defaults to `http://localhost:5000/translate`.
- If you want to point to another server, change `serverUrl` in the inspector.
- This is a starter framework for experimentation and rapid development.
- For production, add batching, queueing, and scene-level registration logic.

## License

MIT
