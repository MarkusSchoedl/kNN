using System;
using System.Linq;

namespace k_NearestNeighbor
{
    class Program
    {
        static void Main(string[] args)
        {
            WhiteWine_kNN kNN = new WhiteWine_kNN();
            if (kNN.ReadWineData(@"Data\winequality-white.csv"))
            {
                if (UserWantsLogging())
                {
                    kNN.EnableLogging();
                }

                WhiteWine_kNN.CalculationMethod method = SelectDistanceMethod();
                int k = SelectK();

                kNN.Start_kNN(k: 100, kfold: 1000, method: method);
            }
            else
            {
                Console.WriteLine("Error parsing the file.");
            }
            Console.ReadKey();
        }

        static WhiteWine_kNN.CalculationMethod SelectDistanceMethod()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose a distance calculation method:\n");

                int i = 1;
                foreach (var method in typeof(WhiteWine_kNN.CalculationMethod).GetEnumNames())
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
                        return (WhiteWine_kNN.CalculationMethod)Enum.GetValues(typeof(WhiteWine_kNN.CalculationMethod)).GetValue(input - 1);
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
