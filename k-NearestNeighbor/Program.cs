using System;

namespace k_NearestNeighbor
{
    class Program
    {
        static void Main(string[] args)
        {
            WhiteWine_kNN kNN = new WhiteWine_kNN(@"C:\Users\marku\Dropbox\FH\MLE\white_wine\winequality-white.csv");
            kNN.Start_kNN();

            Console.ReadKey();
        }
    }
}
