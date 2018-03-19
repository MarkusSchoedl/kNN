using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace k_NearestNeighbor
{
    #region IComparator
    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;   // Handle equality as beeing greater
            else
                return result;
        }
    }
    #endregion

    class DataStructure : IDataStructure
    {
        public List<float> data = new List<float>();

        public string Qualifier;

        public bool Equals(IDataStructure ds)
        {
            var y = (DataStructure)ds;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] != y.data[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
