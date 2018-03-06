using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

namespace k_NearestNeighbor
{
    class WhiteWine_kNN
    {
        #region Parameters
        // Key: Each Wine Attributes, Value: Wine Quality
        private Dictionary<int, List<WineAttributes>> _TotalWineData = new Dictionary<int, List<WineAttributes>>();
        private Dictionary<int, List<WineAttributes>> _DoubleWineData = new Dictionary<int, List<WineAttributes>>();

        // Key: Chunk-Id, Value: WineAttributes
        private Dictionary<int, List<WineAttributes>> _Chunks = new Dictionary<int, List<WineAttributes>>();
        #endregion

        #region Construcor
        /// <summary>
        /// Creates a new Instance for a kNN-Algorithm for WhiteWine.
        /// </summary>
        public WhiteWine_kNN(string filename)
        {
            for (int i = 0; i <= 10; i++)
            {
                _TotalWineData.Add(i, new List<WineAttributes>());
                _DoubleWineData.Add(i, new List<WineAttributes>());
            }

            if (ReadFile(filename))
            {
                Console.WriteLine("Found " + _TotalWineData.Values.Sum(list => list.Count) + " unique wine datasets.");
                Console.WriteLine("Found " + _DoubleWineData.Values.Sum(list => list.Count) + " redundant wine datasets.\n");

                foreach (var dataSet in _TotalWineData.Where(x => x.Value.Count() > 0))
                {
                    Console.WriteLine("quality: " + dataSet.Key + " - wines: " + dataSet.Value.Count());
                }
            }
            else
            {
                Console.WriteLine("Error while reading Data.");
                throw new InvalidDataException();
            }
        }
        #endregion

        #region Methods
        public bool Start_kNN(int k)
        {
            #region Calculate Chunk Size
            for(int i = 0; i < k; i++)
            {
                _Chunks.Add(i, new )

                foreach (var lst in _TotalWineData.Where(x => x.Value.Count() > 0))
                {
                    if (lst.Value.Count() % k == 0)
                    {

                    }
                    else
                    {

                    }
                }
            }
            #endregion

            #region Heuristic
            #endregion
            return false;
        }
        #endregion

        #region File Parsing 
        private bool ReadFile(string filename)
        {
            try
            {
                int quality = -1;
                string line;

                // Read the file and display it line by line.  
                using (StreamReader sr = new StreamReader(filename))
                {
                    sr.ReadLine(); //Throw away header
                    while ((line = sr.ReadLine()) != null)
                    {
                        WineAttributes attr = ParseAttributes(line, out quality);

                        if (_TotalWineData[quality].Where(x => x.Equals(attr)).Count() == 0)
                        {
                            _TotalWineData[quality].Add(attr);
                        }
                        else
                        {
                            _DoubleWineData[quality].Add(attr);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        private WineAttributes ParseAttributes(string line, out int quality)
        {
            int i = 0;
            WineAttributes attribute = new WineAttributes();

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            var fields = typeof(WineAttributes).GetFields();

            var lst = line.Split(';');
            foreach (var item in fields)
            {
                item.SetValue(attribute, float.Parse(lst[i], NumberStyles.Any, ci));
                i++;
            }

            quality = int.Parse(lst.Last());

            return attribute;
        }
        #endregion

        #region Maths
        private static List<int> FindFactors(int num)
        {
            List<int> result = new List<int>();

            // Take out the 2s.
            while (num % 2 == 0)
            {
                result.Add(2);
                num /= 2;
            }

            // Take out other primes.
            int factor = 3;
            while (factor * factor <= num)
            {
                if (num % factor == 0)
                {
                    // This is a factor.
                    result.Add(factor);
                    num /= factor;
                }
                else
                {
                    // Go to the next odd number.
                    factor += 2;
                }
            }

            // If num is not 1, then whatever is left is prime.
            if (num > 1) result.Add(num);

            return result;
        }
        #endregion 
    }

    class WineAttributes
    {
        public float FixedAcidity = 1.0f;
        public float VolatileAcidity = 1.0f;
        public float CitricAcid = 1.0f;
        public float ResidualSugar = 1.0f;
        public float Chlorides = 1.0f;
        public float FreeSulfurDioxide = 1.0f;
        public float TotalSulfurDioxide = 1.0f;
        public float Density = 1.0f;
        public float PH = 1.0f;
        public float Sulphates = 1.0f;
        public float Alcohol = 1.0f;

        public bool Equals(WineAttributes y)
        {
            return Alcohol == y.Alcohol &&
                    Chlorides == y.Chlorides &&
                    CitricAcid == y.CitricAcid &&
                    Density == y.Density &&
                    FixedAcidity == y.FixedAcidity &&
                    FreeSulfurDioxide == y.FreeSulfurDioxide &&
                    PH == y.PH &&
                    ResidualSugar == y.ResidualSugar &&
                    Sulphates == y.Sulphates &&
                    TotalSulfurDioxide == y.TotalSulfurDioxide &&
                    VolatileAcidity == y.VolatileAcidity;
        }
    }
}
