namespace NetMQ.Services
{
    public interface INetMQRouter
    {
        bool IsRunning { get; }

        void Start(int routerPort, int batchLimit = 500);
        void Stop();
    }
}