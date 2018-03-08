using System;

namespace k_NearestNeighbor
{
    class Program
    {
        static void Main(string[] args)
        {
            WhiteWine_kNN kNN = new WhiteWine_kNN();
            if (kNN.ReadWineData(@"C:\Users\marku\Dropbox\FH\MLE\white_wine\winequality-white.csv"))
            {
                //kNN.EnableLogging();
                kNN.Start_kNN(13);
            }
            else
            {
                Console.WriteLine("Error parsing the file.");
            }
            Console.ReadKey();
        }
    }
}
