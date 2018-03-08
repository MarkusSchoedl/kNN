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
        private bool _LoggingEnabled;

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
        public WhiteWine_kNN()
        {
            for (int i = 0; i <= 10; i++)
            {
                _TotalWineData.Add(i, new List<WineAttributes>());
                _DoubleWineData.Add(i, new List<WineAttributes>());
            }
        }
        #endregion

        #region Methods
        public void EnableLogging()
        {
            _LoggingEnabled = true;
        }

        public bool Start_kNN(int k)
        {
            #region Calculate Chunk Size
            for (int chunk = 0; chunk < k; chunk++)
            {
                _Chunks.Add(chunk, new List<WineAttributes>());

                foreach (var indList in _TotalWineData.Where(x => x.Value.Any()))
                {
                    var lst = indList.Value;
                    int quality = indList.Key;

                    int offset = (lst.Count * ((chunk + 1) / (float)k)) % 1 == 0 ? 0 : 1;

                    if(_LoggingEnabled) Console.WriteLine("i = " + (int)(lst.Count * (chunk / (float)k)) + "; i < " + ((lst.Count * ((chunk + 1) / (float)k)) - offset));

                    for (int i = (int)(lst.Count * (chunk / (float)k)); i < (lst.Count * ((chunk + 1) / (float)k)) - offset; i++)
                    {
                        _Chunks[chunk].Add(_TotalWineData[quality][i]);
                    }
                }

                if (_LoggingEnabled) Console.WriteLine("\n");
            }

            if (_LoggingEnabled)
            {
                Console.WriteLine("Count After splitting data: " + _Chunks.Values.Sum(list => list.Count));

                foreach (var dataSet in _Chunks)
                {
                    Console.WriteLine("ChunkNum: " + dataSet.Key + " - ChunkSize: " + dataSet.Value.Count());
                }

                Dictionary<int, List<WineAttributes>> chunkData = new Dictionary<int, List<WineAttributes>>();
                _TotalWineData.ToList().ForEach(x => chunkData.Add(x.Key, new List<WineAttributes>()));
                foreach (var c in _Chunks)
                {
                    chunkData.ToList().ForEach(x => x.Value.Clear());

                    foreach (var x in c.Value)
                    {
                        int quality = 0;
                        _TotalWineData.ToList().ForEach(z => { if (z.Value.Contains(x)) { quality = z.Key; } });
                        chunkData[quality].Add(x);
                    }

                    foreach (var dataSet in chunkData)
                    {
                        Console.WriteLine("quality: " + dataSet.Key + " - wines: " + dataSet.Value.Count());
                    }
                    Console.WriteLine("\n");
                }
            }
            #endregion

            #region Heuristic
            #endregion
            return false;
        }
        #endregion

        #region File Parsing 
        public bool ReadWineData(string filename)
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

            if (_LoggingEnabled)
            {
                Console.WriteLine("Found " + _TotalWineData.Values.Sum(list => list.Count) + " unique wine datasets.");
                Console.WriteLine("Found " + _DoubleWineData.Values.Sum(list => list.Count) + " redundant wine datasets.\n");

                foreach (var dataSet in _TotalWineData.Where(x => x.Value.Count() > 0))
                {
                    Console.WriteLine("quality: " + dataSet.Key + " - wines: " + dataSet.Value.Count());
                }
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
