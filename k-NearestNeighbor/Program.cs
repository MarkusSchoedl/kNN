using System;

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

                kNN.Start_kNN(13);
            }
            else
            {
                Console.WriteLine("Error parsing the file.");
            }
            Console.ReadKey();
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
    }
}
