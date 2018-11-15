using NetMQ.Sockets;
using System.Threading.Tasks;

namespace NetMQ.Services
{
    public class NetMQPubSubProxy
    {
        private int _subscribePort;
        private int _publishPort;
        private Proxy _proxy;

        public bool IsRunning { get; private set; }

        /// <summary>
        /// Start the XPubSub proxy
        /// </summary>
        public void Start(int subscribePort, int publishPort)
        {
            _subscribePort = subscribePort;
            _publishPort = publishPort;

            Task.Run(() =>
            {

                //note the @ symbol here is shorthand for a bind address
                using (var xpubSocket = new XPublisherSocket($"@tcp://*:{_publishPort}"))
                using (var xsubSocket = new XSubscriberSocket($"@tcp://*:{_subscribePort}"))
                {
                    IsRunning = true;

                    _proxy = new Proxy(xsubSocket, xpubSocket);
                    _proxy.Start();
                }
            });
        }

        /// <summary>
        /// Stop the XPubSub Proxy
        /// </summary>
        public void Stop()
        {
            _proxy?.Stop();
            IsRunning = false;
        }
    }
}
