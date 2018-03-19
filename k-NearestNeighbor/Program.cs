using System;
using System.IO;
using System.Linq;

namespace k_NearestNeighbor
{
    class Program
    {
        static void Main(string[] args)
        {
            //string filename = @"Data\winequality-white.csv";
            //string filename = @"Data\WhiteWine10k.csv";
            string filename = @"Data\iris.csv";

            kNN kNN = new kNN();
            if (kNN.ParseData(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, filename)))  //Data\WhiteWine10k.csv
            {
                if (UserWantsLogging())
                {
                    kNN.EnableLogging();
                }

                kNN.CalculationMethod method = SelectDistanceMethod();
                int k = SelectK();

                kNN.Start_kNN(k: k, kfold: 10, method: method);
            }
            else
            {
                Console.WriteLine("Error parsing the file.");
            }
            Console.ReadKey();
        }

        static kNN.CalculationMethod SelectDistanceMethod()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose a distance calculation method:\n");

                int i = 1;
                foreach (var method in typeof(kNN.CalculationMethod).GetEnumNames())
                {
                    Console.WriteLine("{0}: {1}", i, method);
                    i++;
                }

                string sinput = Console.ReadLine();
                int input = 0;
                if (int.TryParse(sinput, out input))
                {
                    try
                    {
                        Console.Clear();
                        return (kNN.CalculationMethod)Enum.GetValues(typeof(kNN.CalculationMethod)).GetValue(input - 1);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }
                }
            }
        }

        static bool UserWantsLogging()
        {
            bool logging = false;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Do you want to Enable logging? ");

                if (logging == false)
                {
                    Console.WriteLine(" Yes  <No>");
                }
                else
                {
                    Console.WriteLine("<Yes>  No ");
                }

                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.LeftArrow)
                {
                    logging = true;
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    logging = false;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }

            Console.Clear();
            return logging;
        }

        static int SelectK()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose k:\n");

                string sinput = Console.ReadLine();
                int input = 0;
                if (int.TryParse(sinput, out input))
                {
                    try
                    {
                        Console.Clear();
                        return input;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }
                }
            }
        }
    }
}
