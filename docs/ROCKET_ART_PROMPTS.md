# Графика для ракеты: запросы для GPT

Обновлено 2026-11-28: на поле вместо фишки стоит короткая «пухлая» ракета `Rocket.png` (от Наташи, GPT) носом в сторону полёта, вертикальная — носом вверх. Летит тонкая `RocketFlight.png`. Если `Rocket.png` нет, на поле рисуется `RocketFlight.png`; если нет обеих, код рисует заглушку-стрелку (`Assets/Scripts/Game/Tiles/SpecialSprites.cs`).

Ниже исходные запросы; раздел 1 был для первого варианта значка (две ракеты), его заменила ракета от Наташи.

## 1. Значок ракеты на фишке — первый вариант (заменён)

Рисуется поверх обычной фишки и показывает, что она стала ракетой. Рисуется горизонтальным; для вертикальной ракеты игра поворачивает его на 90°.

Запрос:

```
A single game icon for a match-3 mobile game: a symmetrical double rocket pointing left and right,
two small golden rockets joined back to back in the middle, like a "◄ ►" double arrow.
Style: glossy embossed relief, soft 3D, like carved and polished enamel, rounded shapes,
thin Kazakh ornament (koshkar-muiz curls) engraved on the rocket bodies.
Colors: warm gold and cream white body, dark brown outline 6–8 px thick so it reads on red, green,
yellow, purple, brown and orange backgrounds; small orange-white flames at both nozzles in the middle.
Composition: centered, horizontal, occupies about 85% of the width and 45% of the height,
nothing else in the frame.
Transparent background (PNG with alpha), no shadow on the background, no text, no frame, no tile underneath.
Square canvas 1024×1024.
```

## 2. Летящая ракета — `RocketFlight.png`

Две такие ракеты вылетают из клетки в обе стороны и летят до края поля. Шлейф за ней рисует игра (в цвет фишки), на картинке нужна только сама ракета, носом вправо.

Запрос:

```
A single small rocket for a match-3 mobile game, flying to the right, side view.
Style matches a glossy embossed enamel icon set: soft 3D, rounded, polished highlights,
a thin Kazakh ornament band (koshkar-muiz curls) around the body.
Colors: gold and cream body, red-orange nose cone, dark brown outline 6–8 px thick,
a short bright orange-yellow flame at the tail on the left.
Composition: centered, horizontal, nose touches the right side at about 90% of the width,
height about 35% of the canvas, nothing else in the frame.
Transparent background (PNG with alpha), no motion blur, no smoke trail, no text.
Square canvas 1024×1024.
```

## Если хочется отдельные фишки-ракеты для каждого цвета (по желанию)

Вместо одного значка поверх фишки можно нарисовать ракету прямо на каждой фишке набора. Это красивее, но нужно 7 картинок на набор, и код придётся доработать (сейчас он рисует один значок поверх любой фишки). Запрос для одной фишки, цвет менять:

```
Match-3 game tile, same style as the attached tile: rounded square, glossy embossed enamel relief,
Kazakh ornament border. In the center instead of the usual symbol: a double rocket pointing left
and right, embossed in the same material and color as the tile, with a bright golden glow.
Tile color: red. Square 1024×1024, transparent corners.
```

К этому запросу приложите в GPT одну из текущих фишек, например `Assets/Sprites/SetKZ01/ChatGPT Image Apr 9, 2025, 10_07_20 AM.png` (красный тюльпан), чтобы стиль совпал.

## Как положить в игру

1. Сохранить PNG с прозрачным фоном. Если GPT отдал картинку на белом или клетчатом фоне, фон нужно убрать (например, в Photoshop или remove.bg), иначе вокруг значка будет квадрат.
2. Положить `Rocket.png` (ракета на поле) и `RocketFlight.png` (летящая) в `Assets/Resources/Specials/` (имена важны).
3. В Unity у каждой картинки в Inspector: Texture Type = Sprite (2D and UI), Sprite Mode = Single, Apply.
4. Размер подгоняется сам: ракета на поле занимает 95% клетки, летящая — 0,7 клетки.
