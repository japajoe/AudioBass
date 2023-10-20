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
    public struct FileStreamInfo
    {
        public string filename;
        public long fileSize;
        public int chunkSize;
        public long totalChunks;
        public int lastChunkSize;

        public FileStreamInfo(string filename, long fileSize, int chunkSize = 1024)
        {
            this.filename = filename;
            this.fileSize = fileSize;
            this.chunkSize = chunkSize;

            totalChunks = fileSize / chunkSize;
            lastChunkSize = (int)(fileSize % chunkSize);

            if (lastChunkSize != 0)
            {
                ++totalChunks;
            }
            else
            {
                lastChunkSize = chunkSize;
            }
        }

        public int GetChunkSize(long chunkIndex)
        {
            if (chunkIndex >= totalChunks)
                return 0;

            if (chunkIndex == (totalChunks - 1))
                return lastChunkSize;
            else
                return chunkSize;
        }
    }
}