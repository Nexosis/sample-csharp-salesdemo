using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Nexosis.Api.Client;
using Nexosis.Api.Client.Model;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var targetSetName = "testForecast";
            var targetData = "/Users/steensn/Desktop/Developer/Demo/testdata.csv";
            var targetColumn = "Target";
            var targetStartDate = "2017-01-17T00:00:00.0000000Z";
            var targetEndDate = "2017-01-21T00:00:00.0000000Z";
            ResultInterval targetInterval = ResultInterval.Day;
            var apiKey = "";
            
            // Ask for default or user input
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Do you want to use default values or user input?");
            Console.WriteLine("  1 for default");
            Console.WriteLine("  2 for user input");
            Console.WriteLine("");
            var sessionType = Console.ReadLine();

            // If 2, get user define input
            if (sessionType == "2")
            {
                Console.WriteLine("//You will need the following before you start:");
                Console.WriteLine(" 1 dataset path");
                Console.WriteLine(" 2 unique dataset name");
                Console.WriteLine(" 3 forecast column name");
                Console.WriteLine(" 4 forecast start date");
                Console.WriteLine(" 5 forecast end date");
                Console.WriteLine(" 6 results interval");
                Console.WriteLine(" 7 API key");
                Console.WriteLine("");
                Console.WriteLine("(press enter to continue)");
                var nada = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Enter unique dataset name:");
                targetSetName = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Please paste target forecast file path:");
                targetData = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Please enter target forecast column name from csv:");
                targetColumn = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Please enter forecast start date:");
                targetStartDate = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Please enter forecast end date:");
                targetEndDate = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Please enter forecast interval (default day):");
                Console.WriteLine("  1 for hour");
                Console.WriteLine("  2 for day");
                Console.WriteLine("  3 for week");
                Console.WriteLine("  4 for month");
                Console.WriteLine("  5 for year");
                Console.WriteLine("");
                var targetIntervalNum = Console.ReadLine();

                switch (targetIntervalNum)
                {
                    case "1":
                        targetInterval = ResultInterval.Hour;
                        break;
                    case "2":
                        targetInterval = ResultInterval.Day;
                        break;
                    case "3":
                        targetInterval = ResultInterval.Week;
                        break;
                    case "4":
                        targetInterval = ResultInterval.Month;
                        break;
                    case "5":
                        targetInterval = ResultInterval.Year;
                        break;
                    default:
                        break;
                }

                Console.WriteLine("");
                Console.WriteLine("Please enter API Key:");
                apiKey = Console.ReadLine();
            }
            
            Console.WriteLine("");

            // If user input API provided, start client w/that key, if not use system key
            var client = (sessionType == "2") ? new NexosisClient(apiKey) : new NexosisClient(); 

            using (var file = File.OpenText(targetData))
            {
                var dataSetName = targetSetName;
                try
                {
                    // Delete old datset & associated sessions
                    client.DataSets.Remove(dataSetName, DataSetDeleteOptions.CascadeSessions).GetAwaiter().GetResult();

                    // Uploading new dataset & write data upload cost
                    var data = client.DataSets.Create(dataSetName, file).GetAwaiter().GetResult();
                    Console.WriteLine("");
                    Console.WriteLine("Data Upload Cost ${0}:", data.Cost);

                    // Start forecasting session & write session status + ID
                    var session = client.Sessions.CreateForecast(dataSetName, targetColumn, DateTimeOffset.Parse(targetStartDate), DateTimeOffset.Parse(targetEndDate), targetInterval).GetAwaiter().GetResult();

                    Console.WriteLine("");
                    Console.WriteLine("Session Status: {0}:", session.Status);
                    Console.WriteLine("Session Id:     {0}:", session.SessionId);

                    // Wait for session to be complete before proceeding
                    Console.WriteLine("");
                    Console.WriteLine("When you recieve email telling you forecast is complete, press enter");
                    Console.ReadLine();

                    // Write results into results-file.csv
                    using (var output = new StreamWriter(File.OpenWrite("results-file.csv")))
                    {
                        client.Sessions.GetResults(session.SessionId, output).GetAwaiter().GetResult();
                    }

                    Console.WriteLine("");
                    Console.WriteLine("Data saved to results-file.csv | Success!!!");      
                }
                catch (NexosisClientException nce) 
                {
                    // If API error, print error details
                    Console.WriteLine(nce.ErrorResponse.StatusCode);
                    Console.WriteLine(nce.ErrorResponse.Message);
                    Console.WriteLine(nce.ErrorResponse.ErrorType);
                    foreach (var detail in nce.ErrorResponse.ErrorDetails)
                    {
                        Console.WriteLine(detail.Key + " " + detail.Value);
                    }
                }                         
            }
        }
    }
}
