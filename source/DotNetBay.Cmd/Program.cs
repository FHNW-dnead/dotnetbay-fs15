using System;
using DotNetBay.Core.Service;
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

            var auctionService = new AuctionService(new FileStorage());

            var allAuctions = auctionService.GetAllAuctions();

            Console.WriteLine("Found {0} auctions in store.", allAuctions.Count);

            Console.Write("Press enter to quit");
            Console.ReadLine();

            Environment.Exit(0);
        }
    }
}
