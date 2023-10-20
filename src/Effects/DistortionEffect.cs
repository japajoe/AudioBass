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

namespace AudioBass.Effects
{
    public sealed class DistortionEffect : AudioEffect
    {
        public float drive = 1.0f;
        public float range = 1.0f;
        public float blend = 1.0f;
        public float volume = 1.0f;

        public override void Process(float[] input, float[] output, int sampleFrames, int channels)
        {
            for(int i = 0; i < input.Length; i++)
            {
                output[i] = Distort(input[i], drive, range, blend, volume);
            }
        }

        private float Distort(float x, float drive, float range, float blend, float volume)
        {
            float xClean = x;
            x *= drive * range;
            double result = (((((2.0f / Math.PI) * Math.Atan(x)) * blend) + (xClean * (1.0f - blend))) / 2.0f) * volume;
            return (float)result;
        }        
    }
}