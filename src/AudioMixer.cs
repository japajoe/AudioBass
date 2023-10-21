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
using System.Runtime.InteropServices;
using AudioBass.Utilities;
using AudioBass.SDL2;

namespace AudioBass
{
    public static class AudioMixer
    {
        public sealed class AudioSourceInfo
        {
            public AudioSource audioSource;

            public AudioSourceInfo(AudioSource audioSource)
            {
                this.audioSource = audioSource;
            }
        }

        private static SDL.SDL_AudioSpec specDesired;
        private static SDL.SDL_AudioSpec specObtained;
        private static SDL.SDL_AudioCallback audioCallback;
        private static List<AudioSourceInfo> audioSources = new List<AudioSourceInfo>();
        private static float[] mixBuffer;
        private static ArrayPool<float> bufferPool;
        private static bool initialized;

        public static void Initialize(int sampleRate)
        {
            if(initialized)
                return;

            ushort bufferSize = (ushort)AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Float);            

            mixBuffer = new float[bufferSize];
            bufferPool = new ArrayPool<float>(10, bufferSize);

            audioCallback = new SDL.SDL_AudioCallback(OnAudioRead);
            specDesired.freq = AudioSettings.outputSampleRate;
            specDesired.channels = 2;
            specDesired.samples = bufferSize; //Buffer size is in number of samples, not number of bytes!
            specDesired.userdata = IntPtr.Zero;
            specDesired.format = SDL.AUDIO_F32;
            specDesired.callback = audioCallback;

            SDL.SDL_InitSubSystem(SDL.SDL_INIT_AUDIO);
            int status = SDL.SDL_OpenAudio(ref specDesired, out specObtained);

            AudioSettings.outputSampleRate = specObtained.freq;

            initialized = status == 0;
        }

        public static void Dispose()
        {
            if(!initialized)
                return;
            
            SDL.SDL_CloseAudio();

            for(int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].audioSource.Dispose();
            }

            SDL.SDL_Quit();
        }

        internal static void PushAudioSource(AudioSource source)
        {
            if(!initialized)
                return;
            
            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].audioSource.GetHashCode() == source.GetHashCode())
                {
                    return;
                }
            }          

            audioSources.Add(new AudioSourceInfo(source));
        }

        internal static void PopAudioSource(AudioSource source)
        {
            if(!initialized)
                return;

            for(int i = audioSources.Count - 1; i >= 0; i--)
            {
                if(audioSources[i].audioSource.GetHashCode() == source.GetHashCode())
                {
                    audioSources.RemoveAt(i);
                    return;
                }
            }
        }

        internal static void Play()
        {   
            //Start playing if at least 1 source is active         
            if(GetNumberOfSourcesPlaying() > 0)
            {
                SDL.SDL_PauseAudio(0);
            }
        }

        internal static void Stop()
        {            
            //Stop playing if all sources are inactive
            if(GetNumberOfSourcesPlaying() == audioSources.Count)
            {
                SDL.SDL_PauseAudio(1);
            }
        }

        private static int GetNumberOfSourcesPlaying()
        {
            int numSourcesPlaying = 0;

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].audioSource.IsPlaying)
                {
                    numSourcesPlaying++;
                }
            }

            return numSourcesPlaying;            
        }

        private static void OnAudioRead(IntPtr userdata, IntPtr stream, int length)
        {
            //Fill the mix buffer with silence
            Array.Fill<float>(mixBuffer, 0);

            for(int i = 0 ; i < audioSources.Count; i++)
            {
                //Skip sources that aren't playing
                if(!audioSources[i].audioSource.IsPlaying)
                    continue;

                float[] dataOut = bufferPool.GetArray(out int bufferIdOut);

                if(dataOut != null)
                {
                    //Fill the buffer with silence
                    Array.Fill<float>(dataOut, 0);

                    //Retrieve data from the playing audio source
                    audioSources[i].audioSource.OnRead(dataOut);

                    //Apply any effects that are active on the audio source
                    if(audioSources[i].audioSource.Effects.Count > 0)
                    {
                        for(int j = 0; j < audioSources[i].audioSource.Effects.Count; j++)
                        {
                            if(audioSources[i].audioSource.Effects[j].bypass)
                                continue;
                            
                            //All audio sources have stereo data
                            audioSources[i].audioSource.Effects[j].Process(dataOut, dataOut, dataOut.Length / 2, 2);
                        }
                    }                    

                    //Add processed audio to the output mix
                    for(int j = 0; j < mixBuffer.Length; j++)
                    {
                        mixBuffer[j] += dataOut[j];
                    }
                }
                
                //Return buffers to the pool
                bufferPool.ReturnArray(bufferIdOut);
            }

            //Clamp output to avoid clipping
            for(int i = 0; i < mixBuffer.Length; i++)
            {
                mixBuffer[i] = Math.Clamp(mixBuffer[i], -1.0f, 1.0f);
            }

            //Copy the buffer to the native stream
            Marshal.Copy(mixBuffer, 0, stream, mixBuffer.Length);
        }
    }
}