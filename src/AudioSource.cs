/* AudioBass - Cross platform .NET library for audio playback and processing
 *
 * Copyright (c) 2023-2024 W.M.R Jap-A-Joe.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * W.M.R Jap-A-Joe https://github.com/japajoe
 *
 */

using System;
using System.Collections.Generic;
using AudioBass.Effects;
using AudioBass.Utilities;

namespace AudioBass
{    
    public delegate void AudioReadEvent(float[] data, int channels);

    public sealed class AudioSource : IDisposable
    {
        public event AudioReadEvent read;
        public event Action playbackEnded;

        private bool isPlaying = false;
        private byte[] buffer;
        private float volume = 1.0f;
        private bool loop;
        private long playbackTime = 0;
        private WaveStream waveStream;
        private WaveFileDecodeFunction decodeFunction;
        private List<AudioEffect> effects;
        private static float[] dataBuffer = null;

        public int Channels
        {
            get
            {
                if(waveStream == null)
                    return 2;
                return waveStream.Format.channels;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
        }

        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if(value < 0)
                    volume = 0;
                else
                    volume = value;
            }
        }

        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
            }
        }

        public long PlaybackTime
        {
            get
            {
                return playbackTime;
            }
        }

        public int SampleRate
        {
            get
            {
                if(waveStream == null)
                    return 44100;
                return waveStream.Format.sampleRate;
            }
        }

        public List<AudioEffect> Effects
        {
            get
            {
                return effects;
            }
        }

        public AudioSource()
        {
            int bufferSize = AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Byte);
            buffer = new byte[bufferSize];

            waveStream = new WaveFileReader();
            waveStream.readFinished += OnReadFinished;

            effects = new List<AudioEffect>();

            AudioMixer.PushAudioSource(this);
        }

        public void Play()
        {
            Stop();
            playbackTime = 0;
            SetIsPlaying(true);
            AudioMixer.Play();
        }

        public void Play(AudioClip clip)
        {
            Stop();

            if(!waveStream.OpenFile(clip.FilePath, loop, clip.StreamFromDisk ? null : clip.Data))
            {
                return;
            }

            switch(waveStream.Format.type)
            {
                case WaveFormatType.PCM8:
                    decodeFunction = WaveFileDecoder.PCM8ToFloat;
                    break;
                case WaveFormatType.PCM16:
                    decodeFunction = WaveFileDecoder.PCM16ToFloat;
                    break;
                case WaveFormatType.PCM32:
                    decodeFunction = WaveFileDecoder.PCM32ToFloat;
                    break;
                case WaveFormatType.IEEE:
                    decodeFunction = WaveFileDecoder.IEEEToFloat;
                    break;
                case WaveFormatType.ALaw:
                    decodeFunction = WaveFileDecoder.ALawToFloat;
                    break;
                case WaveFormatType.MuLaw:
                    decodeFunction = WaveFileDecoder.MuLawToFloat;
                    break;                           
                default:
                    decodeFunction = WaveFileDecoder.PCM16ToFloat;
                    break;
            }

            playbackTime = 0;            
            SetIsPlaying(true);
            AudioMixer.Play();
        }

        public void Stop()
        {
            if(!isPlaying)
                return;

            SetIsPlaying(false);
            AudioMixer.Stop();
            waveStream?.Reset(44);            
            playbackTime = 0;
        }

        internal void OnRead(float[] data)
        {
            OnRead(data, 2);
        }
        
        private void OnRead(float[] data, int channels)
        {
            read?.Invoke(data, channels);

            if(dataBuffer == null)
                dataBuffer = new float[data.Length];

            int bytesRead = waveStream.Read(buffer, 0, 0);

            //Nothing more to do so bail out
            if(bytesRead == 0)
            {
                return;
            }

            int index = 0;
            int bytesPerSample = waveStream.Format.bytesPerSample;
            int numChannels = waveStream.Format.channels;

            playbackTime += (bytesRead / bytesPerSample / waveStream.Format.channels);

            for(int i = 0; i < bytesRead; i += bytesPerSample)
            {
                //Add data rather than simply overwriting any possible existing data written by the read callback
                data[index] += decodeFunction(buffer, i, waveStream.Format.bitsPerSample) * volume;
                index++;
            }

            if(Channels == 1)
            {
                index = 0;
                for(int j = 0; j < data.Length; j+=2)
                {
                    dataBuffer[j] = data[index];
                    dataBuffer[j+1] = data[index];
                    index++;
                }
            }
            else
            {
                index = 0;
                for(int j = 0; j < data.Length; j+=2)
                {
                    dataBuffer[j] = data[j];
                    dataBuffer[j+1] = data[j+1];
                }
            }

            Buffer.BlockCopy(dataBuffer, 0, data, 0, data.Length * sizeof(float));
        }

        private void OnReadFinished()
        {
            if(!loop)
            {
                SetIsPlaying(false);
            }

            playbackTime = 0;
            playbackEnded?.Invoke();
        }

        private void SetIsPlaying(bool playing)
        {
            this.isPlaying = playing;
        }

        public T AddEffect<T>() where T : AudioEffect, new()
        {
            int count = effects.Count;
            T instance = new T();
            effects.Add(instance);
            return instance;
        }

        public T GetEffect<T>() where T : AudioEffect
        {
            for(int i = 0; i < effects.Count; i++)
            {
                if(effects[i].GetType() == typeof(T))
                {
                    return effects[i] as T;
                }
            }
            return null;
        }

        public bool RemoveEffect(AudioEffect effect)
        {
            for(int i = effects.Count - 1; i >= 0; i--)
            {
                if(effects[i].GetHashCode() == effect.GetHashCode())
                {
                    effects.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Stop();
            waveStream?.Dispose();
        }
    }
}
