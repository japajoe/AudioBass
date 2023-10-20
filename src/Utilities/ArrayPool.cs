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
    public sealed class ArrayPool<T>
    {
        private sealed class PoolItem
        {
            public T[] array;
            public bool isInUse;
            public int id;

            public PoolItem(int arraySize, int id)
            {
                array = new T[arraySize];
                isInUse = false;
                this.id = id;
            }
        }

        private PoolItem[] items;

        public ArrayPool(int poolSize, int arraySize)
        {
            items = new PoolItem[poolSize];

            for(int i = 0; i < items.Length; i++)
            {
                items[i] = new PoolItem(arraySize, i);
            }
        }

        public T[] GetArray(out int id)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if(!items[i].isInUse)
                {
                    items[i].isInUse = true;
                    id = items[i].id;
                    return items[i].array;
                }
            }

            id = -1;
            return null;
        }

        public void ReturnArray(int id)
        {
            if(id < 0)
                return;

            for(int i = 0; i < items.Length; i++)
            {
                if(items[i].id == id)
                {
                    if (items[i].isInUse)
                    {
                        items[i].isInUse = false;
                    }
                }
            }
        }
    }
}