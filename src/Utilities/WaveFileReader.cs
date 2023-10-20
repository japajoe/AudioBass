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
using System.IO;

namespace AudioBass.Utilities
{
    public sealed class WaveFileReader : WaveStream
    {
        private byte[] buffer = new byte[128];
        private bool loop = false;
        private bool process = false;

        public WaveFileReader() : base()
        {
        }

        public override bool OpenFile(string filepath, bool loop, byte[] data = null)
        {
            this.process = false;
            this.loop = loop;

            if (!File.Exists(filepath))
            {
                Console.WriteLine("File does not exist: " + filepath);
                return false;
            }

            if (Open(filepath, data))
            {
                Array.Fill<byte>(buffer, 0);

                stream.Read(buffer, 0, 44);

                var chunkId = BitConverter.ToInt32(buffer, 0);
                var chunkSize = BitConverter.ToInt32(buffer, 4);
                var format = BitConverter.ToInt32(buffer, 8);
                var subChunk1Id = BitConverter.ToInt32(buffer, 12);
                var subChunk1Size = BitConverter.ToInt32(buffer, 16);
                var audioFormat = BitConverter.ToInt16(buffer, 20);
                var numChannels = BitConverter.ToInt16(buffer, 22);
                var sampleRate = BitConverter.ToInt32(buffer, 24);
                var byteRate = BitConverter.ToInt32(buffer, 28);
                var blockAlign = BitConverter.ToInt16(buffer, 32);
                var bitsPerSample = BitConverter.ToInt16(buffer, 34);
                var subChunk2Id = BitConverter.ToInt32(buffer, 36);
                var subChunk2Size = BitConverter.ToInt32(buffer, 40);

                int bufferSize = AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Short);

                switch (bitsPerSample)
                {
                    case 8:
                        bufferSize = AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Byte);
                        break;
                    case 16:
                        bufferSize = AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Short);
                        break;
                    case 32:
                        bufferSize = AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Float);
                        break;
                    default:
                        bufferSize = AudioSettings.GetBufferSize(AudioSettings.AudioFormat.Short);
                        break;
                }

                //If data has 1 channel, half the buffer size so that we end up with just as many samples after each read as with 2 channels
                //Reasoning: 
                //Considering a buffer of size 1024 that contains data of 2 channels, it effectively has only 512 samples
                //We don't want to end up reading 1024 samples in case of 1 channel, when an outputstream only expects 512
                streamInfo = new FileStreamInfo(filepath, subChunk2Size, numChannels == 2 ? bufferSize : bufferSize / 2);

                waveFormat = new WaveFormat(audioFormat, sampleRate, subChunk2Size, numChannels, bitsPerSample, bitsPerSample / 8);

                this.process = true;

                //To do: depending on format the data may not always begin at offset 44, so this has to be taken care of at some point
                stream.Seek(44, SeekOrigin.Begin);

                return true;
            }

            this.process = false;

            return false;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void Reset(long offset)
        {
            if(stream?.CanSeek == true)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }
        }        

        public override void Dispose()
        {
            stream?.Close();
            stream?.Dispose();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;

            if(stream?.CanRead == true)
            {
                if(process)
                {
                    bytesRead = stream.Read(buffer, 0, streamInfo.chunkSize);

                    if(bytesRead == 0 || bytesRead < streamInfo.chunkSize)
                    {
                        if(loop)
                        {
                            int numBytesLeft = buffer.Length - bytesRead;
                            stream.Seek(44, SeekOrigin.Begin);
                            stream.Read(buffer, bytesRead, numBytesLeft);
                        }
                        else
                        {
                            //Nothing to write anymore so clear buffer to avoid clicks
                            Array.Fill<byte>(buffer, 0);
                            process = false;
                            readFinished?.Invoke();
                            Reset(44);
                        }
                    }
                }
                else
                {
                    readFinished?.Invoke();
                    Reset(44);
                }
            }

            return bytesRead;
        }

        private bool Open(string filepath, byte[] data)
        {
            if(stream == null)
            {
                this.filepath = filepath;
                stream = CreateStream(filepath, data);
                return true;
            }
            else
            {
                if(this.filepath == filepath)
                {
                    if(stream.CanRead && stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        Dispose();
                        stream = CreateStream(filepath, data);
                    }
                }
                else
                {
                    this.filepath = filepath;
                    Dispose();
                    stream = CreateStream(filepath, data);
                }
                return true;
            }            
        }

        private Stream CreateStream(string filepath, byte[] data)
        {
            if(data == null)
                return new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            else
                return new MemoryStream(data);
        }
    }
}