using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hik.Utility
{
    /// <summary>
    /// 工具方法类
    /// </summary>
    public static class CommonMethods
    {
        /// <summary>把多维数组转化为一维数组</summary>
        /// <param name="array">多维数组</param>
        /// <param name="lengths">多维数组的各个维度的长度的列表，在Recover方法还原时使用</param>
        /// <returns>转化得到的一维数组</returns>
        public static Array Flatten(Array array, out int[] lengths)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            lengths = new int[array.Rank];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = array.GetLength(i);
            }
            Array flattenedArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
            long counter = 0;
            int[] indices = Enumerable.Repeat(-1, array.Rank).ToArray();
            int currentLevel = 0;
            while (counter < array.LongLength)
            {
                if (currentLevel == array.Rank - 1)
                {
                    for (indices[currentLevel] = 0; indices[currentLevel] < lengths[lengths.Length - 1]; indices[currentLevel]++, counter++)
                    {
                        flattenedArray.SetValue(array.GetValue(indices), counter);
                    }
                    currentLevel--;
                }
                else
                {
                    indices[currentLevel]++;
                    if (indices[currentLevel] == lengths[currentLevel])
                    {
                        indices[currentLevel] = -1;
                        currentLevel--;
                    }
                    else
                    {
                        currentLevel++;
                    }
                }
            }
            return flattenedArray;
        }

        /// <summary>把由Flatten方法转化的一维数组还原为多维数组</summary>
        /// <param name="flattenedArray">一维数组</param>
        /// <param name="lengths">多维数组的各个维度的长度列表</param>
        /// <returns>多维数组</returns>
        public static Array Recover(Array flattenedArray, int[] lengths)
        {
            if (flattenedArray == null)
            {
                throw new ArgumentNullException("flattenedArray");
            }
            if (lengths == null || lengths.Length == 0)
            {
                throw new ArgumentNullException("lengths");
            }
            long totalLength = 1;
            for (int i = 0; i < lengths.Length; i++)
            {
                totalLength *= lengths[i];
            }
            if (totalLength != flattenedArray.LongLength)
            {
                throw new ArgumentException("多维数组的各个维度的长度列表中所有值的乘积与一维数组的长度不等，无法还原。", "lengths");
            }
            int[] indices = Enumerable.Repeat(-1, lengths.Length).ToArray();
            Array originalArray = Array.CreateInstance(flattenedArray.GetType().GetElementType(), lengths);
            long counter = 0;
            int currentLevel = 0;
            while (counter < originalArray.LongLength)
            {
                if (currentLevel == originalArray.Rank - 1)
                {
                    for (indices[currentLevel] = 0; indices[currentLevel] < lengths[lengths.Length - 1]; indices[currentLevel]++, counter++)
                    {
                        originalArray.SetValue(flattenedArray.GetValue(counter), indices);
                    }
                    currentLevel--;
                }
                else
                {
                    indices[currentLevel]++;
                    if (indices[currentLevel] == lengths[currentLevel])
                    {
                        indices[currentLevel] = -1;
                        currentLevel--;
                    }
                    else
                    {
                        currentLevel++;
                    }
                }
            }
            return originalArray;
        }

    }
}
