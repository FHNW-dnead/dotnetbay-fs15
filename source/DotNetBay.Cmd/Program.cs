using System;
using System.Linq;
using DotNetBay.Core.Service;
using DotNetBay.Data.FileStorage;
using DotNetBay.Model;

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

            var auctionService = new AuctionService(new FileStorageProvider("store.json"));

            var allAuctions = auctionService.GetAllAuctions();

            Console.WriteLine("Found {0} auctions returned by the service.", allAuctions.Count());


            Console.Write("Press enter to quit");
            Console.ReadLine();

            Environment.Exit(0);
        }
    }
}
