namespace DotNetBay.Core.Execution
{
    public interface IAuctionRunner
    {
        IAuctioneer Auctioneer { get; }

        void Start();

        void Stop();
    }
}
