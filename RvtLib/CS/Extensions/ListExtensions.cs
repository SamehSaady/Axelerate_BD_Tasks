using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.CS.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns a copy of the passed <paramref name="list"/> and excludes the item of the passed <paramref name="index"/> from it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static List<T> RemoveItemAtIndex<T>(this List<T> list, int index)
        {
            // Check if the item is valid
            if (index < 0 || index >= list.Count)
            {
                throw new IndexOutOfRangeException("Index is out of range");
            }

            // Create a new list with the item removed
            List<T> newList = new List<T>(list);
            newList.RemoveAt(index);

            return newList;
        }

        /// <summary>
        /// Returns a copy of the passed <paramref name="list"/> and excludes the passed <paramref name="item"/> from it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> RemoveItem<T>(this List<T> list, T item)
        {
            List<T> newList = new List<T>(list);
            newList.Remove(item);

            return newList;
        }
    }
}
