using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

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
        public void Start_kNN(int k, CalculationMethod method = CalculationMethod.EuclideanDistance)
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
            SplitDataIntoChunks(k);
            #endregion

            #region Heuristic
            int wrongWines = 0;
            int rightWines = 0;

            // The Result
            int[,] confusionMatrix = new int[10, 10];
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var testingChunk in _Chunks)
            {
                foreach (var unknownWine in testingChunk.Value) // For each wine, which is not part of the current Chunk
                {
                    WineAttributes nearestNeighbor = null;
                    double lowestDistance = double.MaxValue;

                    // Get NearestNeighbor
                    foreach (var nnChunk in _Chunks.Where(x => testingChunk.Key != x.Key))
                    {
                        foreach (var knownWine in nnChunk.Value)
                        {
                            double currDistance = calculationMethod.GetDistance(knownWine, unknownWine);

                            if (currDistance < lowestDistance)
                            {
                                lowestDistance = currDistance;
                                nearestNeighbor = knownWine;
                            }
                        }
                    }

                    // Add the current guess to the result
                    confusionMatrix[unknownWine.Quality, nearestNeighbor.Quality]++;

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

            PrintResult(confusionMatrix, rightWines, wrongWines, elapsedTime);

            #endregion
        }

        /// <summary>
        /// Prints all the relevant data into a beautiful matrix.
        /// </summary>
        /// <param name="confusionMatrix">The confusion matrix as result.</param>
        /// <param name="rightWines">The amount of right wines</param>
        /// <param name="wrongWines">The amount of wrong wines</param>
        /// <param name="elapsedTime">The elapsed Time as string.</param>
        private void PrintResult(int[,] confusionMatrix, int rightWines, int wrongWines, string elapsedTime)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nElapsed Time: " + elapsedTime + " sec\n\n");

            //for (int j = 0, i = 0; i < 10; ++j, i = j%10 == 0 ? i+1 : i, j = j % 10 )
            //{}

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
                    else if (confusionMatrix[i, j] != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(confusionMatrix[i, j].ToString("D3") + " ");
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
        #endregion

        #region File Parsing 
        /// <summary>
        /// Reads all the Data from the CSV file and parses it into the Dictionaries.
        /// </summary>
        /// <param name="filename">The path to the File including the File Extension.</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
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

        /// <summary>
        /// Parses all the Attributes out of one Line.
        /// </summary>
        /// <param name="line">The string representing the current line.</param>
        /// <returns>An object, which holds all the Wine's Data.</returns>
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

        // Erased because TOO SLOW
        //private double CalculateDistance(WineAttributes guessWine, WineAttributes learnWine)
        //{
        //    foreach (var field in typeof(WineAttributes).GetFields())
        //    {
        //        result += Math.Pow((float)field.GetValue(guessWine) - (float)field.GetValue(learnWine), 2);
        //    }
        //}
    }

    internal class WineAttributes
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

    #region StrategyDesignPattern
    abstract class DistanceCalculator
    {
        public abstract double GetDistance(WineAttributes wine1, WineAttributes wine2);
    }
    
    class EuclideanDistanceStrategy : DistanceCalculator
    {
        public override double GetDistance(WineAttributes wine1, WineAttributes wine2)
        {
            double result = 0;
            double temp = 0;

            temp = wine1.Alcohol - wine2.Alcohol;
            result = temp * temp;
            temp = wine1.Chlorides - wine2.Chlorides;
            result += temp * temp;
            temp = wine1.CitricAcid - wine2.CitricAcid;
            result += temp * temp;
            temp = wine1.Density - wine2.Density;
            result += temp * temp;
            temp = wine1.FixedAcidity - wine2.FixedAcidity;
            result += temp * temp;
            temp = wine1.FreeSulfurDioxide - wine2.FreeSulfurDioxide;
            result += temp * temp;
            temp = wine1.PH - wine2.PH;
            result += temp * temp;
            temp = wine1.ResidualSugar - wine2.ResidualSugar;
            result += temp * temp;
            temp = wine1.Sulphates - wine2.Sulphates;
            result += temp * temp;
            temp = wine1.TotalSulfurDioxide - wine2.TotalSulfurDioxide;
            result += temp * temp;
            temp = wine1.VolatileAcidity - wine2.VolatileAcidity;
            result += temp * temp;

            return Math.Sqrt(result);
        }
    }

    class ManhattanDistanceStrategy : DistanceCalculator
    {
        public override double GetDistance(WineAttributes wine1, WineAttributes wine2)
        {
            double result = 0;

            result += Math.Abs(wine1.Alcohol - wine2.Alcohol);
            result += Math.Abs(wine1.Chlorides - wine2.Chlorides);
            result += Math.Abs(wine1.CitricAcid - wine2.CitricAcid);
            result += Math.Abs(wine1.Density - wine2.Density);
            result += Math.Abs(wine1.FixedAcidity - wine2.FixedAcidity);
            result += Math.Abs(wine1.FreeSulfurDioxide - wine2.FreeSulfurDioxide);
            result += Math.Abs(wine1.PH - wine2.PH);
            result += Math.Abs(wine1.ResidualSugar - wine2.ResidualSugar);
            result += Math.Abs(wine1.Sulphates - wine2.Sulphates);
            result += Math.Abs(wine1.TotalSulfurDioxide - wine2.TotalSulfurDioxide);
            result += Math.Abs(wine1.VolatileAcidity - wine2.VolatileAcidity);

            return result;
        }
    }

    class CorrectedEuclideanStrategy : DistanceCalculator
    {
        public override double GetDistance(WineAttributes wine1, WineAttributes wine2)
        {
            // Removed following parameters:
            // Residual Sugar
            // Density
            // Chlorides
            // Sulphates
            // Citric Acid

            double result = 0;
            double temp = 0;

            temp = wine1.Alcohol - wine2.Alcohol;
            result += temp * temp;
            temp = wine1.FixedAcidity - wine2.FixedAcidity;
            result += temp * temp;
            temp = wine1.FreeSulfurDioxide - wine2.FreeSulfurDioxide;
            result += temp * temp;
            temp = wine1.PH - wine2.PH;
            result += temp * temp;
            temp = wine1.TotalSulfurDioxide - wine2.TotalSulfurDioxide;
            result += temp * temp;
            temp = wine1.VolatileAcidity - wine2.VolatileAcidity;
            result += temp * temp;

            return Math.Sqrt(result);
        }
    }

    #endregion
}
