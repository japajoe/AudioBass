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

namespace AudioBass.Utilities
{
    public delegate float WaveFileDecodeFunction(byte[] data, int index, int bitsPerSample);
    
    public static class WaveFileDecoder
    {
        private const float SHORT_DIVISOR = 1.0f / short.MaxValue;
        private const float INT_DIVISOR = 1.0f / int.MaxValue;

        public static float PCM8ToFloat(byte[] data, int index, int bitsPerSample)
        {
            float value = (float)((float)data[index] / byte.MaxValue);
            return (value * 2.0f) - 1.0f;
        }           

        public static float PCM16ToFloat(byte[] data, int index, int bitsPerSample)
        {
            return (float)BitConverter.ToInt16(data, index) * SHORT_DIVISOR;
        }

        public static float PCM32ToFloat(byte[] data, int index, int bitsPerSample)
        {

            return (float)BitConverter.ToInt32(data, index) * INT_DIVISOR;
        }

        public static float IEEEToFloat(byte[] data, int index, int bitsPerSample)
        {
            return BitConverter.ToSingle(data, index);
        }

        public static float ALawToFloat(byte[] aLawData, int index, int bitsPerSample)
        {
            // Extract the A-law sample
            int samplesPerByte = 8 / bitsPerSample;
            int byteIndex = index / samplesPerByte;
            int sampleIndex = index % samplesPerByte;
            byte aLawSample = (byte)(aLawData[byteIndex] >> (bitsPerSample * sampleIndex));

            // Decode the A-law sample
            short pcmSample = ALawDecode(aLawSample);

            // Convert the PCM sample to a floating-point value
            float floatSample = (float)pcmSample / 32768f;
            return floatSample;
        }

        public static float MuLawToFloat(byte[] muLawData, int index, int bitsPerSample)
        {
            // Extract the mu-law sample
            int samplesPerByte = 8 / bitsPerSample;
            int byteIndex = index / samplesPerByte;
            int sampleIndex = index % samplesPerByte;
            byte muLawSample = (byte)(muLawData[byteIndex] >> (bitsPerSample * sampleIndex));

            // Decode the mu-law sample
            short pcmSample = MuLawDecode(muLawSample);

            // Convert the PCM sample to a floating-point value
            float floatSample = (float)pcmSample / 32768f;
            return floatSample;
        }        

        private static short ALawDecode(byte aLaw)
        {
            // Mask off the sign bit
            int sign = aLaw & 0x80;

            // Extend the sign bit
            int exponent = (aLaw & 0x70) >> 4;
            int data = aLaw & 0x0f;
            data |= (exponent == 0) ? (exponent << 4) : ((exponent ^ 0x07) + 0x08);

            // Return the decoded value
            return (short)(sign == 0 ? data : -data);
        }

        private static short MuLawDecode(byte muLaw)
        {
            // Mask off the sign bit
            int sign = muLaw & 0x80;

            // Extend the sign bit
            int exponent = (muLaw & 0x70) >> 4;
            int data = muLaw & 0x0f;
            data |= (exponent == 0) ? (exponent << 4) : ((exponent ^ 0x07) + 0x08);
            data = (short)(sign == 0 ? data : -data);

            // Compute the decoded value
            int decoded = (int)(data << (exponent + 3)) >> 7;
            return (short)decoded;
        }

        private static short[] MuLawDecode(byte[] muLawData, int bitsPerSample)
        {
            int samplesPerByte = 8 / bitsPerSample;
            short[] decodedData = new short[muLawData.Length * samplesPerByte];
            for (int i = 0; i < muLawData.Length; i++)
            {
                for (int j = 0; j < samplesPerByte; j++)
                {
                    // Extract the sample from the byte
                    int sample = (muLawData[i] >> (bitsPerSample * j)) & ((1 << bitsPerSample) - 1);

                    // Decode the sample
                    decodedData[i * samplesPerByte + j] = MuLawDecode((byte)sample);
                }
            }
            return decodedData;
        }
    }
}