using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _pitchSoundSource;
        [SerializeField] private AudioSource _normalSoundSource;
        [SerializeField] private AudioMixer _audioMixer;
        
        [Header("Clips")]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _gameMusic;
        [SerializeField] private AudioClip _click;
        [SerializeField] private AudioClip _deselect;
        [SerializeField] private AudioClip _match;
        [SerializeField] private AudioClip _noMatch;
        [SerializeField] private AudioClip _stopMusic;
        [SerializeField] private AudioClip _pop;
        [SerializeField] private AudioClip _remove;
        [SerializeField] private AudioClip _win;
        [SerializeField] private AudioClip _lose;
        [SerializeField] private AudioClip _whoosh;

        [Header("Cascade group sounds")]
        [SerializeField] private AudioClip _matchFour;
        [SerializeField] private AudioClip _matchFive;
        [SerializeField] private AudioClip _matchCross;

        // Свой источник на каждую группу шага: у PlayOneShot один pitch на источник
        private readonly List<AudioSource> _groupSources = new List<AudioSource>();
        private int _nextGroupSource;
        
        // game Data

        private bool _isEnabledSound = true;

        public void PlayClick() => PlayNormalPitch(_click);
        public void PlayDeselect() => PlayNormalPitch(_deselect);
        public void PlayMatch() => PlayNormalPitch(_match);
        public void PlayNoMatch() => PlayNormalPitch(_noMatch);
        public void PlayStopMusic() => PlayNormalPitch(_stopMusic);
        public void PlayPop() => PlayRandomPitch(_pop);
        public void PlayRemove() => PlayRandomPitch(_remove);
        public void PlayWin() => PlayNormalPitch(_win);
        public void PlayLose() => PlayNormalPitch(_lose);
        public void PlayWhoosh() => PlayRandomPitch(_whoosh);

        /// <summary>
        /// Один звук на группу совпадения. Тон зависит от шага каскада, delay — сдвиг внутри шага.
        /// </summary>
        public void PlayGroup(GroupSound sound, float pitch, float delay)
        {
            var clip = ClipFor(sound);
            if (clip == null) return;

            var source = NextGroupSource();
            source.Stop();
            source.clip = clip;
            source.pitch = pitch;
            source.PlayDelayed(delay);
        }

        private AudioClip ClipFor(GroupSound sound)
        {
            switch (sound)
            {
                case GroupSound.Four: return _matchFour != null ? _matchFour : _remove;
                case GroupSound.Five: return _matchFive != null ? _matchFive : _remove;
                case GroupSound.Cross: return _matchCross != null ? _matchCross : _remove;
                default: return _remove;
            }
        }

        private AudioSource NextGroupSource()
        {
            if (_groupSources.Count < CascadeSound.MaxSoundsPerStep)
            {
                var go = new GameObject($"GroupSound{_groupSources.Count}");
                go.transform.SetParent(transform, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = _pitchSoundSource.outputAudioMixerGroup;
                source.volume = _pitchSoundSource.volume;
                source.spatialBlend = 0f;
                _groupSources.Add(source);
                return source;
            }

            var next = _groupSources[_nextGroupSource];
            _nextGroupSource = (_nextGroupSource + 1) % _groupSources.Count;
            return next;
        }
        

        public void SetSoundVolume()
        {
            if (_isEnabledSound)
            {
                _audioMixer.SetFloat("Volume", -6f);
                _musicSource.Play();
            }
            else
            {
                _audioMixer.SetFloat("Volume", -80f);
                _musicSource.Stop();
            }
        }

        public void PlayGameMusic()
        {
            _musicSource.Stop();
            _musicSource.clip = _gameMusic;
            SetSoundVolume();
        }

        public void PlayMenuMusic()
        {
            _musicSource.Stop();
            _musicSource.clip = _menuMusic;
            SetSoundVolume();
        }

        public void StopMusic() => _musicSource.Stop();

        private void PlayRandomPitch(AudioClip clip)
        {
            _pitchSoundSource.pitch = Random.Range(0.8f, 1.2f);
            _pitchSoundSource.PlayOneShot(clip);
        }

        private void PlayNormalPitch(AudioClip clip)
        {
            _normalSoundSource.PlayOneShot(clip);
        }
        
    }
}