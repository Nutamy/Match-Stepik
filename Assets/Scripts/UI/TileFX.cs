using UnityEngine;

namespace UI
{
    /// <summary>
    /// Перекрашивает и масштабирует вспышку из пула. Исходные значения префаба запоминаются один раз,
    /// поэтому повторное использование из пула не накапливает цвет и размер.
    /// </summary>
    public class TileFX : MonoBehaviour
    {
        private ParticleSystem[] _systems;
        private Color[] _baseColors;
        private float[] _baseSizes;
        private Light[] _lights;
        private Color[] _baseLightColors;
        private float[] _baseLightRanges;

        private void Cache()
        {
            if (_systems != null) return;

            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _baseColors = new Color[_systems.Length];
            _baseSizes = new float[_systems.Length];
            for (int i = 0; i < _systems.Length; i++)
            {
                var main = _systems[i].main;
                _baseColors[i] = main.startColor.color;
                _baseSizes[i] = main.startSizeMultiplier;
                NeutralizeColorOverLifetime(_systems[i]);
            }

            _lights = GetComponentsInChildren<Light>(true);
            _baseLightColors = new Color[_lights.Length];
            _baseLightRanges = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _baseLightColors[i] = _lights[i].color;
                _baseLightRanges[i] = _lights[i].range;
            }
        }

        // Свой градиент цвета у частиц префаба (белый → синий → красный) смешивался бы с цветом фишки в грязь.
        // Оставляем от него только прозрачность, цвет задаёт фишка.
        private static void NeutralizeColorOverLifetime(ParticleSystem system)
        {
            var colorOverLifetime = system.colorOverLifetime;
            if (!colorOverLifetime.enabled) return;

            var source = colorOverLifetime.color.gradient;
            if (source == null) return;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                source.alphaKeys);
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        public void Play(Color tileColor, float scale)
        {
            Cache();

            for (int i = 0; i < _systems.Length; i++)
            {
                var main = _systems[i].main;
                main.startColor = TileFXStyle.Tint(_baseColors[i], tileColor);
                main.startSizeMultiplier = _baseSizes[i] * scale;
                // Частицы, выпущенные при включении объекта, ещё старого цвета — перезапускаем
                _systems[i].Clear(false);
                _systems[i].Play(false);
            }

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].color = TileFXStyle.Tint(_baseLightColors[i], tileColor);
                _lights[i].range = _baseLightRanges[i] * scale;
            }
        }
    }
}
