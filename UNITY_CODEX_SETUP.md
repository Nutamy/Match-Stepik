# Codex и Unity

Настройка от 2026-11-25.

- Версия проекта: Unity **6000.3.8f1**. Не обновлять редактор автоматически.
- В `Packages/manifest.json` добавлен MCP for Unity **10.0.0**, закреплённый тегом.
- В пользовательской конфигурации Codex зарегистрирован `unityMCP`: `http://localhost:8080/mcp`.
- Установлен uv 0.13.10; сервер `mcpforunityserver==10.0.0` загружен, команда `--help` выполнена успешно.

## Завершение подключения

1. Найти или восстановить Unity 6000.3.8f1. Запись установщика и C#-проект указывают на `C:\Unity\6000.3.8f1\Editor\Unity.exe`, но 2026-11-25 этот файл отсутствовал.
2. Открыть этот проект в Unity и дождаться импорта пакетов. `packages-lock.json` должен обновить сам Unity Package Manager.
3. Открыть **Window → MCP for Unity**. Использовать HTTP и локальный адрес `http://localhost:8080`, запустить сервер и соединение из окна пакета.
4. Если uv не найден, перезапустить Unity Hub после изменения PATH либо указать `C:\Users\Nutamy\AppData\Roaming\Python\Python314\Scripts\uv.exe` в настройках пакета.
5. Обновить подключение MCP в Codex; если инструменты не появились, перезапустить приложение. Проверить получение списка сцен и чтение консоли без изменения игры.

При необходимости сервер можно запустить в PowerShell:

```powershell
python -m uv tool run --from mcpforunityserver==10.0.0 mcp-for-unity --transport http --http-host 127.0.0.1 --http-port 8080
```

Не запускать второй сервер на том же порту. Для работы с редактором требуется открытый Unity с подключённым пакетом.

## Что ещё не проверено

Импорт пакета в Unity, компиляция C#, чтение сцен через MCP и Play Mode. Настройка адреса в Codex сама по себе не означает соединение с редактором.

Документация: https://coplaydev.github.io/unity-mcp/getting-started/install

До начала настройки уже был изменён `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`; это изменение не относится к подключению и не включается в коммит настройки.
