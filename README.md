# AudioBass
Cross platform library for audio playback and processing.

# Why this library?
Other libraries require too much fiddling to get things working and/or their license isn't very appealing. 

# Features
- Play multiple audio sources simultaneously.
- Stream audio from disk or from memory.
- Callback system to fill an audio buffer with data which is played instantly.
- Base class for implementing custom audio effects. See examples for a demo.

# Limitations
- Currently only supports 16 bit wave files.

# Requirements
- SDL2

# Examples
```csharp
using System;
using System.Collections.Generic;
using AudioBass;

namespace AudioBassTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AudioMixer.Initialize(44100);
            
            AudioSource audioSource = new AudioSource();
            
            audioSource.Loop = true;

            AudioClip clip = new AudioClip("some_file.wav");
            audioSource.Play(clip);

            Console.ReadLine();

            AudioMixer.Dispose();
        }
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using AudioBass;

namespace AudioBassTest
{
    class Program
    {
        private static AudioSource audioSource;
        private static List<AudioClip> clips;
        private static int clipIndex = 0;

        static void Main(string[] args)
        {
            AudioMixer.Initialize(44100);
            
            audioSource = new AudioSource();
            audioSource.playbackEnded += OnPlaybackEnded;

            clips = new List<AudioClip>();
            clips.Add(new AudioClip("some_file.wav"));
            clips.Add(new AudioClip("some_other_file.wav"));
            clips.Add(new AudioClip("another_file.wav"));

            PlayClip();

            Console.ReadLine();

            AudioMixer.Dispose();
        }

        private static void OnPlaybackEnded()
        {
            PlayClip();
        }

        private static void PlayClip()
        {
            audioSource.Play(clips[clipIndex]);
            Console.WriteLine("Now playing: " + clips[clipIndex].FilePath);

            clipIndex++;
            if(clipIndex >= clips.Count)
                clipIndex = 0;            
        }
    }
}
```

```csharp
using System;
using AudioBass;

namespace AudioBassTest
{
    class Program
    {
        private static AudioSource audioSource;
        private static float sampleRate = 44100;
        private static float frequency = 440;
        private static float tremoloFrequency = 3.3f;
        private static float amp = 0.5f;
        private static ulong time = 0;

        static void Main(string[] args)
        {
            AudioMixer.Initialize(44100);
            
            audioSource = new AudioSource();
            audioSource.read += OnAudioRead;

            audioSource.Play();

            Console.ReadLine();

            AudioMixer.Dispose();
        }

        private static void OnAudioRead(float[] samples, int channels)
        {
            float sample = 0;
            float tremolo = 0;

            for (int i = 0; i < samples.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * time / sampleRate) * amp;
                tremolo = (float)Math.Sin(2 * Math.PI * tremoloFrequency * time / sampleRate);

                sample *= tremolo;

                samples[i] = sample;
                if(channels == 2)
                    samples[i + 1] = sample;

                time++;
            }
        }
    }
}
```

```csharp
using System;
using AudioBass;
using AudioBass.Effects;

namespace AudioBassTest
{
    class Program
    {
        private static AudioSource audioSource;
        private static float sampleRate = 44100;
        private static float frequency = 440;
        private static float tremoloFrequency = 3.3f;
        private static float amp = 0.5f;
        private static ulong time = 0;

        static void Main(string[] args)
        {
            AudioMixer.Initialize(44100);
            
            audioSource = new AudioSource();
            audioSource.read += OnAudioRead;
            
            //You might want to turn your volume down a bit :)
            var distortion = audioSource.AddEffect<DistortionEffect>();
            distortion.drive = 10.0f;
            distortion.range = 10;
            distortion.blend = 0.5f;
            distortion.volume = 2.0f;

            audioSource.Play();

            Console.ReadLine();

            AudioMixer.Dispose();
        }

        private static void OnAudioRead(float[] samples, int channels)
        {
            float sample = 0;
            float tremolo = 0;

            for (int i = 0; i < samples.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * time / sampleRate) * amp;
                tremolo = (float)Math.Sin(2 * Math.PI * tremoloFrequency * time / sampleRate);

                sample *= tremolo;

                samples[i] = sample;
                if(channels == 2)
                    samples[i + 1] = sample;

                time++;
            }
        }
    }
}
```