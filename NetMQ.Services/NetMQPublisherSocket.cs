using NetMQ.Sockets;
using System.Threading.Tasks;

namespace NetMQ.Services
{
    public class NetMQPublisherSocket : INetMQPublisherSocket
    {
        private PublisherSocket _publisherSocket;
        private NetMQPoller _poller;

        public bool IsRunning { get; private set; }

        public void Start(int port, int queueSize = 75)
        {
            if (IsRunning)
            {
                return;
            }

            _publisherSocket = new PublisherSocket();
            _publisherSocket.Options.SendHighWatermark = queueSize;
            _publisherSocket.Bind($"tcp://*:{port}");

            _poller = new NetMQPoller { _publisherSocket };
            _poller.RunAsync();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            _poller.Stop();
            _publisherSocket.Close();
            IsRunning = false;
        }

        public void PublishMessage(string topic, byte[] payload)
        {
            var msg = new NetMQMessage();
            msg.Append(topic);
            msg.Append(payload);

            Publish(msg);
        }

        public void PublishMessage(string topic, int payload)
        {
            var msg = new NetMQMessage();
            msg.Append(topic);
            msg.Append(payload);

            Publish(msg);
        }

        public void PublishMessage(string topic, long payload)
        {
            var msg = new NetMQMessage();
            msg.Append(topic);
            msg.Append(payload);

            Publish(msg);
        }

        public void PublishMessage(string topic, string payload)
        {
            var msg = new NetMQMessage();
            msg.Append(topic);
            msg.Append(payload);

            Publish(msg);
        }

        private void Publish(NetMQMessage msg)
        {
            if (!IsRunning)
            {
                return;
            }

            var task = new Task(() => _publisherSocket.SendMultipartMessage(msg));
            task.Start(_poller);
            task.Wait();
        }
    }
}
