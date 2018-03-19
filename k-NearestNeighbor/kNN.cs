using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace k_NearestNeighbor
{
    class kNN
    {
        #region Parameters
        private bool _LoggingEnabled;

        // Key: Each Wine Attributes, Value: Wine Quality
        private List<DataStructure> _AllWines = new List<DataStructure>();
        private Dictionary<string, List<DataStructure>> _TotalWineData = new Dictionary<string, List<DataStructure>>();
        private Dictionary<string, List<DataStructure>> _DoubleWineData = new Dictionary<string, List<DataStructure>>();

        // Key: Chunk-Id, Value: DataStructure
        private Dictionary<int, List<DataStructure>> _Chunks = new Dictionary<int, List<DataStructure>>();

        private List<string> _TotalResults = new List<string>();
        private SortedDictionary<string, SortedDictionary<string, int>> _ConfusionMatrix = new SortedDictionary<string, SortedDictionary<string, int>>();

        //Key of Dict: current Wine; Sorted List Key: Distance, Value: Wine to that distance
        private SortedList<float, string> _NearestNeighbor = new SortedList<float, string>();

        public enum CalculationMethod
        {
            EuclideanDistance = 1,
            ManhattanDistance,
            CorrectedEuclideanDistance
        }
        #endregion

        #region Construcor
        /// <summary>
        /// Creates a new Instance for a kNN-Algorithm for WhiteWine.
        /// </summary>
        public kNN()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Enables the Logging Feature of the Algorithm.
        /// </summary>
        public void EnableLogging()
        {
            _LoggingEnabled = true;
        }

        /// <summary>
        /// Starts the k-NearestNeighbor Algorithm using the EuclideanDistance Method.
        /// </summary>
        /// <param name="k">The number of Chunks to use.</param>
        public void Start_kNN(int k, int kfold, CalculationMethod method = CalculationMethod.EuclideanDistance)
        {
            DistanceCalculator calculationMethod = new EuclideanDistanceStrategy();
            if (method == CalculationMethod.ManhattanDistance)
            {
                calculationMethod = new ManhattanDistanceStrategy();
            }
            else if (method == CalculationMethod.CorrectedEuclideanDistance)
            {
                calculationMethod = new CorrectedEuclideanStrategy();
            }

            Console.WriteLine("Calculating with method: {0} ...", method.ToString());

            #region Calculate Chunk Size
            SplitDataIntoChunks(kfold);
            #endregion

            #region Heuristic

            // The Result
            List<List<string>> confusionMatrix = new List<List<string>>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var testingChunk in _Chunks)
            {
                foreach (var unknownWine in testingChunk.Value) // For each wine, which is not part of the current Chunk
                {
                    _NearestNeighbor = new SortedList<float, string>(new DuplicateKeyComparer<float>());

                    // Get NearestNeighbor
                    foreach (var nnChunk in _Chunks.Where(x => testingChunk.Key != x.Key))
                    {
                        foreach (var knownWine in nnChunk.Value)
                        {
                            float currDistance = calculationMethod.GetDistance(knownWine, unknownWine);

                            _NearestNeighbor.Add(currDistance, knownWine.Qualifier);
                        }
                    }

                    // Get most occuring quality 
                    var w = _NearestNeighbor.Values.Take(k);
                    var dict = w.ToLookup(x => x);
                    var maxCount = dict.Max(x => x.Count());
                    string mostQualifier = dict.Where(x => x.Count() == maxCount).Select(x => x.Key).FirstOrDefault();

                    // Add the current guess to the result
                    _ConfusionMatrix[unknownWine.Qualifier][mostQualifier]++;
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0} Minutes, {1} Seconds, {2} ms", ts.Minutes, ts.Seconds, ts.Milliseconds);

            PrintResult(elapsedTime);

            #endregion
        }

        /// <summary>
        /// Prints all the relevant data into a beautiful matrix.
        /// </summary>
        /// <param name="confusionMatrix">The confusion matrix as result.</param>
        /// <param name="rightWines">The amount of right wines</param>
        /// <param name="wrongWines">The amount of wrong wines</param>
        /// <param name="elapsedTime">The elapsed Time as string.</param>
        private void PrintResult(string elapsedTime)
        {
            int rightWines = 0, wrongWines = 0;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nElapsed Time: " + elapsedTime + " sec\n\n");

            //for (int j = 0, i = 0; i < 10; ++j, i = j%10 == 0 ? i+1 : i, j = j % 10 )
            //{}

            Console.WriteLine("Confusion Matrix:");

            foreach(string i in _ConfusionMatrix.Keys)
            {
                // Print Axis Label
                if (i == _ConfusionMatrix.First().Key)
                {
                    Console.Write("\n\n");
                    Console.Write("           ");
                    foreach(string j in _ConfusionMatrix[i].Keys)
                    {
                        Console.Write(" " + j + "  ");
                    }
                    Console.Write("\n");
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Quality " + i + ": ");
                foreach (string j in _ConfusionMatrix[i].Keys)
                {
                    if (i == j)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        rightWines += _ConfusionMatrix[i][j];
                    }
                    else if (_ConfusionMatrix[i][j] != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        wrongWines += _ConfusionMatrix[i][j];
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(_ConfusionMatrix[i][j].ToString("D3") + " ");
                }

                Console.Write("\n");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n Wrong Wines: " + wrongWines);
            Console.WriteLine(" Right Wines: " + rightWines);
            Console.WriteLine(" Accuracy: " + ((rightWines / (float)(wrongWines + rightWines)) * 100.0).ToString("0.00") + "%");
        }

        /// <summary>
        /// Splits the <see cref="_TotalWineData"/> into k Chunks for calculation.
        /// </summary>
        /// <param name="k">The amount of Chunks to split into</param>
        /// <returns>True if all Data was split, False otherwise.</returns>
        private bool SplitDataIntoChunks(int k)
        {
            for (int chunk = 0; chunk < k; chunk++)
            {
                _Chunks.Add(chunk, new List<DataStructure>());

                foreach (var indList in _TotalWineData.Where(x => x.Value.Any()))
                {
                    var lst = indList.Value;
                    string qualifier = indList.Key;

                    int offset = (lst.Count * ((chunk + 1) / (float)k)) % 1 == 0 ? 0 : 1;

                    if (_LoggingEnabled) Console.WriteLine("i = " + (int)(lst.Count * (chunk / (float)k)) + "; i < " + ((lst.Count * ((chunk + 1) / (float)k)) - offset));

                    for (int i = (int)(lst.Count * (chunk / (float)k)); i < (lst.Count * ((chunk + 1) / (float)k)) - offset; i++)
                    {
                        _Chunks[chunk].Add(_TotalWineData[qualifier][i]);
                    }
                }
            }

            if (_LoggingEnabled)
            {
                Console.WriteLine("\nCount After splitting data: " + _Chunks.Values.Sum(list => list.Count));

                foreach (var dataSet in _Chunks)
                {
                    Console.WriteLine("ChunkNum: " + dataSet.Key + " - ChunkSize: " + dataSet.Value.Count());
                }
            }

            int numOfTotalWines = _TotalWineData.Values.Sum(list => list.Count);
            int numOfChunkWines = _Chunks.Values.Sum(list => list.Count);
            return numOfTotalWines == numOfChunkWines;
        }
        #endregion

        /// <summary>
        /// Parses all the Attributes out of one Line.
        /// </summary>
        /// <param name="line">The string representing the current line.</param>
        /// <returns>A List, which holds all the Data.</returns>
        public DataStructure ParseAttributes(string line)
        {
            DataStructure ds = new DataStructure();
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            string[] atts = line.Split(';');
            if (atts.Count() <= 1)
            {
                atts = line.Split(',');
            }

            List<float> lst = new List<float>();
            for(int i = 0; i < atts.Count() - 1; i++)
            {
                lst.Add(float.Parse(atts[i]));
            }

            ds.data = lst;
            ds.Qualifier = atts.Last();

            if(!_TotalResults.Contains(ds.Qualifier))
            {
                _TotalResults.Add(ds.Qualifier);
            }

            return ds;
        }

        /// <summary>
        /// Reads all the Data from the CSV file and parses it into the Dictionaries.
        /// </summary>
        /// <param name="filename">The path to the File including the File Extension.</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public bool ParseData(string filename)
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
                        DataStructure ds = (ParseAttributes(line));
                        if (!_TotalWineData.Keys.Contains(ds.Qualifier))
                        {
                            _TotalWineData.Add(ds.Qualifier, new List<DataStructure>());
                            _DoubleWineData.Add(ds.Qualifier, new List<DataStructure>());
                        }

                        if (_TotalWineData[ds.Qualifier].Where(x => x.Equals(ds)).Count() == 0)
                        {
                            _TotalWineData[ds.Qualifier].Add(ds);
                        }
                        else
                        {
                            _DoubleWineData[ds.Qualifier].Add(ds);
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

            // Set up confusion matrix
            foreach(string outer in _TotalResults)
            {
                _ConfusionMatrix.Add(outer, new SortedDictionary<string, int>());
                foreach(string inner in _TotalResults)
                {
                    _ConfusionMatrix[outer].Add(inner, 0);
                }
            }

            return true;

        }
    }
}
