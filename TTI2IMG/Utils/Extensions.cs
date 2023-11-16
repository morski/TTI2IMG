using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTI2IMG.Utils
{
    public static class Extensions
    {
        public static List<List<T>> Split<T>(this List<T> array, T seperator)
        {
            var currentIndex = 0;
            var splitedList = new List<List<T>>();
            while (currentIndex < array.Count)
            {
                var part = array.Skip(currentIndex).TakeWhile(item => !item.Equals(seperator)).ToList();
                splitedList.Add(part);
                currentIndex += part.Count + 1;
            }
            return splitedList;
        }
    }
}
