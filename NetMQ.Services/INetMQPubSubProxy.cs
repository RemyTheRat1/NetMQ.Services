namespace NetMQ.Services
{
    public interface INetMQPubSubProxy
    {
        bool IsRunning { get; }

        void Start(int subscribePort, int publishPort);
        void Stop();
    }
}