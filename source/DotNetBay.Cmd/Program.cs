using System;
using System.Linq;

using DotNetBay.Core;
using DotNetBay.Data.FileStorage;

namespace DotNetBay.Cmd
{
    /// <summary>
    /// Main Entry for program
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DotNetBay Commandline");

            var store = new FileSystemMainRepository("store.json");
            
            var auctionService = new AuctionService(store);
            var auctionRunner = new AuctionRunner(store);
            
            Console.WriteLine("Started AuctionRunner");
            auctionRunner.Start();

            var allAuctions = auctionService.GetAuctions();

            Console.WriteLine("Found {0} auctions returned by the service.", allAuctions.Count());

            Console.Write("Press enter to quit");
            Console.ReadLine();

            Environment.Exit(0);
        }
    }
}
