using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSharpeningS01E02_E03
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var filePath = string.Empty;
            var topN = 1;
            var help = false;

            var optionSet = new OptionSet {
                { "f|filePath=",    "Path to winning lottery numbers file.",
                    s => filePath = s },
                { "t|top=",  "Number of most common winning numbers to return.",
                    (int n) => topN = n },
                { "h|help",     "Display help.",
                    h => help = h != null },
            };

            // Handle some of the bad input options.
            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("CSharpeningS01E02_E3.exe: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'CSharpeningS01E02_E3.exe --help' for more information.");
                return;
            }

            // Print help information if requested or no parameters given. 
            if (help || args.Length == 0)
            {
                optionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            //--------------------------------------------------------

            long dataSetSize = 0;
            long processedDataSize = 0;
            var progress = 0.0;
            var progressLimiter = 0;

            // Creating a frequency counter for each possible lottery number (0-49)
            var counters = new int[50];

            // Setting the start time for processing esimation. 
            // Setting it already here to get a bit more stable estimation (applies for smaller datasets).
            var startTime = DateTime.Now;

            // Getting baseline for progress estimation.
            try
            {
                dataSetSize = new FileInfo(filePath).Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get file info: {ex.Message}");
                Environment.Exit(-1);
            }

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    // The first line is a comment we don't need but we still consider the data size.
                    processedDataSize += sr.ReadLine().Length;

                    // Initial calculations for estimations based on data amount and displaying them.
                    progress = 100.0 * processedDataSize / dataSetSize;
                    Console.WriteLine($"Progress: {(int)progress}% ");
                    progressLimiter = (int)Math.Ceiling(progress);
                    var spentTime = (DateTime.Now - startTime).Seconds;
                    Console.Write("Estimated time left: {0:hh\\:mm\\:ss}", TimeSpan.FromSeconds(spentTime / progress * (100 - progress)));

                    while (sr.EndOfStream == false)
                    {
                        var line = sr.ReadLine();
                        processedDataSize += line.Length;
                        var splitted = line.Split('\t');

                        // Disregarding irrelevant data and iterating the counters of winning numbers of this row
                        for (int i = 0; i < 10; i++)
                        {
                            counters[int.Parse(splitted[i + 2])]++;
                        }

                        progress = 100.0 * processedDataSize / dataSetSize;

                        // Each time progress increments another full precentage worth update the user. 
                        if (progress > progressLimiter)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"Progress: {(int)progress}% ");
                            progressLimiter = (int)Math.Ceiling(progress);
                            spentTime = (DateTime.Now - startTime).Seconds;
                            Console.Write("Estimated time left: {0:hh\\:mm\\:ss}", TimeSpan.FromSeconds(spentTime / progress * (100 - progress)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read or process data: {ex.Message}");
                Environment.Exit(-1);
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine($"Progress: 100% ");
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\rThe top {topN} most common winning numbers:");

            // Find the top N most common numbers and return them to the user.
            for (int i = 0; i < topN; i++)
            {
                var index = Array.IndexOf(counters, counters.Max());
                Console.WriteLine($"{index} occurs {counters[index]} times");
                counters[index] = 0;
            }
        }
    }
}
