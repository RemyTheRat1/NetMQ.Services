namespace NetMQ.Services
{
    public interface INetMQPublisherSocket
    {
        bool IsRunning { get; }

        void PublishMessage(string topic, byte[] payload);
        void PublishMessage(string topic, int payload);
        void PublishMessage(string topic, long payload);
        void PublishMessage(string topic, string payload);
        void Start(int port, int queueSize = 75);
        void Stop();
    }
}