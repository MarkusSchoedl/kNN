using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace k_NearestNeighbor
{
    class WhiteWine_kNN
    {
        #region Parameters
        private bool _LoggingEnabled;

        // Key: Each Wine Attributes, Value: Wine Quality
        private List<WineAttributes> _AllWines = new List<WineAttributes>();
        private Dictionary<int, List<WineAttributes>> _TotalWineData = new Dictionary<int, List<WineAttributes>>();
        private Dictionary<int, List<WineAttributes>> _DoubleWineData = new Dictionary<int, List<WineAttributes>>();

        // Key: Chunk-Id, Value: WineAttributes
        private Dictionary<int, List<WineAttributes>> _Chunks = new Dictionary<int, List<WineAttributes>>();

        // Our Result
        private int[,] _ConfusionMatrix;
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

            _ConfusionMatrix = new int[10, 10];
        }
        #endregion

        #region Methods
        public void EnableLogging()
        {
            _LoggingEnabled = true;
        }

        public void Start_kNN(int k)
        {
            Console.WriteLine("Calculating...");

            #region Calculate Chunk Size
            SplitDataIntoChunks(k);
            #endregion

            #region Heuristic
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int wrongWines = 0;
            int rightWines = 0;
            foreach (var testingChunk in _Chunks)
            {
                foreach (var unknownWine in testingChunk.Value)
                {
                    WineAttributes nearestNeighbor = null;
                    double lowestDistance = double.MaxValue;

                    foreach (var nnChunk in _Chunks.Where(x => testingChunk.Key != x.Key))
                    {
                        foreach (var knownWine in nnChunk.Value)
                        {
                            double currDistance = unknownWine.GetEuclideanDistance(knownWine);
                            if (currDistance < lowestDistance)
                            {
                                lowestDistance = currDistance;
                                nearestNeighbor = knownWine;
                            }
                        }
                    }

                    _ConfusionMatrix[unknownWine.Quality, nearestNeighbor.Quality]++;

                    if (_LoggingEnabled)
                    {
                        Console.WriteLine("Nearest Neighbor Distance: " + lowestDistance + "\nUnknown: " + unknownWine.ToString() + "\nKnown  : " + nearestNeighbor.ToString());
                    }

                    if (nearestNeighbor.Quality != unknownWine.Quality)
                    {
                        wrongWines++;
                    }
                    else
                    {
                        rightWines++;
                    }
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nElapsed Time: " + elapsedTime + " sec\n\n");

            //for (int j = 0, i = 0; i < 10; ++j, i = j%10 == 0 ? i+1 : i, j = j % 10 )
            //{
            //    Console.Write(_ConfusionMatrix[i,j].ToString("D3") + " ");
            //}

            Console.WriteLine("Confusion Matrix:");

            for (int i = 0; i < 10; i++)
            {
                // Print Axis Label
                if (i == 0)
                {
                    Console.Write("\n\n");
                    Console.Write("           ");
                    for (int j = 0; j < 10; j++)
                    {
                        Console.Write(" " + j + "  ");
                    }
                    Console.Write("\n");
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Quality " + i + ": ");
                for (int j = 0; j < 10; j++)
                {
                    if (i == j)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    else if (_ConfusionMatrix[i, j] != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(_ConfusionMatrix[i, j].ToString("D3") + " ");
                }

                Console.Write("\n");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n Wrong Wines: " + wrongWines);
            Console.WriteLine(" Right Wines: " + rightWines);
            Console.WriteLine(" Accuracy: " + ((rightWines / (float)(wrongWines + rightWines)) * 100.0).ToString("0.00") + "%");
            #endregion
        }
        #endregion

        #region File Parsing 
        public bool ReadWineData(string filename)
        {
            try
            {
                string line;

                // Read the file and display it line by line.  
                using (StreamReader sr = new StreamReader(filename))
                {
                    sr.ReadLine(); //Throw away header
                    while ((line = sr.ReadLine()) != null)
                    {
                        WineAttributes attr = ParseAttributes(line);

                        if (_TotalWineData[attr.Quality].Where(x => x.Equals(attr)).Count() == 0)
                        {
                            _TotalWineData[attr.Quality].Add(attr);
                        }
                        else
                        {
                            _DoubleWineData[attr.Quality].Add(attr);
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

        private WineAttributes ParseAttributes(string line)
        {
            int i = 0;
            WineAttributes attribute = new WineAttributes();

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            var fields = typeof(WineAttributes).GetFields();

            var lst = line.Split(';');
            foreach (var item in fields)
            {
                if (item.FieldType == typeof(float))
                    item.SetValue(attribute, float.Parse(lst[i], NumberStyles.Any, ci));
                i++;
            }

            attribute.Quality = int.Parse(lst.Last());

            return attribute;
        }
        #endregion

        //private double CalculateDistance(WineAttributes guessWine, WineAttributes learnWine)
        //{
        //    foreach (var field in typeof(WineAttributes).GetFields())
        //    {
        //        result += Math.Pow((float)field.GetValue(guessWine) - (float)field.GetValue(learnWine), 2);
        //    }
        //}

        private bool SplitDataIntoChunks(int k)
        {

            for (int chunk = 0; chunk < k; chunk++)
            {
                _Chunks.Add(chunk, new List<WineAttributes>());

                foreach (var indList in _TotalWineData.Where(x => x.Value.Any()))
                {
                    var lst = indList.Value;
                    int quality = indList.Key;

                    int offset = (lst.Count * ((chunk + 1) / (float)k)) % 1 == 0 ? 0 : 1;

                    if (_LoggingEnabled) Console.WriteLine("i = " + (int)(lst.Count * (chunk / (float)k)) + "; i < " + ((lst.Count * ((chunk + 1) / (float)k)) - offset));

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

            int numOfTotalWines = _TotalWineData.Values.Sum(list => list.Count);
            int numOfChunkWines = _Chunks.Values.Sum(list => list.Count);
            return numOfTotalWines == numOfChunkWines;
        }
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

        public int Quality = 0;


        public double GetEuclideanDistance(WineAttributes other)
        {
            double result = 0;

            result = Math.Pow(other.Alcohol - Alcohol, 2);
            result += Math.Pow(other.Chlorides - Chlorides, 2);
            result += Math.Pow(other.CitricAcid - CitricAcid, 2);
            result += Math.Pow(other.Density - Density, 2);
            result += Math.Pow(other.FixedAcidity - FixedAcidity, 2);
            result += Math.Pow(other.FreeSulfurDioxide - FreeSulfurDioxide, 2);
            result += Math.Pow(other.PH - PH, 2);
            result += Math.Pow(other.ResidualSugar - ResidualSugar, 2);
            result += Math.Pow(other.Sulphates - Sulphates, 2);
            result += Math.Pow(other.TotalSulfurDioxide - TotalSulfurDioxide, 2);
            result += Math.Pow(other.VolatileAcidity - VolatileAcidity, 2);

            return Math.Sqrt(result);
        }

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

        public override string ToString()
        {
            return FixedAcidity + " - " + VolatileAcidity + " - " + CitricAcid + " - " + ResidualSugar + " - " + Chlorides + " - " + FreeSulfurDioxide
                + " - " + TotalSulfurDioxide + " - " + Density + " - " + PH + " - " + Sulphates + " - " + Alcohol;
        }
    }
}
