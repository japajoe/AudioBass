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

namespace AudioBass.Utilities
{
    public enum WaveFormatType
    {
        PCM8,
        PCM16,
        PCM32,
        IEEE,
        ALaw,
        MuLaw,
        Unknown
    }

    public struct WaveFormat
    {
        public WaveFormatType type;
        public int sampleRate;
        public long dataLength;
        public int channels;
        public int bitsPerSample;
        public int bytesPerSample;

        public WaveFormat(short audioFormat, int sampleRate, long dataLength, int channels, int bitsPerSample, int bytesPerSample)
        {
            this.type = GetAudioFormatType(audioFormat, bitsPerSample);
            this.sampleRate = sampleRate;
            this.dataLength = dataLength;
            this.channels = channels;
            this.bitsPerSample = bitsPerSample;
            this.bytesPerSample = bytesPerSample;
        }

        private WaveFormatType GetAudioFormatType(short audioFormat, int bitsPerSample)
        {
            WaveFormatType type = WaveFormatType.Unknown;

            if (audioFormat == 1) // PCM
            {
                if (bitsPerSample == 8)
                {
                    type = WaveFormatType.PCM8;
                }
                else if (bitsPerSample == 16)
                {
                    type = WaveFormatType.PCM16;
                }
                else if (bitsPerSample == 32)
                {
                    type = WaveFormatType.PCM32;
                }
                else
                {
                    type = WaveFormatType.Unknown;
                }
            }
            else if (audioFormat == 3) // IEEE float
            {
                if (bitsPerSample == 32)
                {
                    type = WaveFormatType.IEEE;
                }
                else
                {
                    type = WaveFormatType.Unknown;
                }
            }
            else if (audioFormat == 6) // A-law
            {
                if (bitsPerSample == 8)
                {
                    type = WaveFormatType.ALaw;
                }
                else
                {
                    type = WaveFormatType.Unknown;
                }
            }
            else if (audioFormat == 7) // Mu-law
            {
                if (bitsPerSample == 8)
                {
                    type = WaveFormatType.MuLaw;
                }
                else
                {
                    type = WaveFormatType.Unknown;
                }
            }
            else
            {
                type = WaveFormatType.Unknown;
            }

            return type;
        }
    }
}