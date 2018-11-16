using NetMQ.Sockets;
using System;

namespace NetMQ.Services
{
    public class NetMQSubscriberSocket : INetMQSubscriberSocket
    {
        private int _batchLimit;
        private SubscriberSocket _subscribeSocket;
        private NetMQPoller _poller;

        public event EventHandler<NetMQMessage> MessageReceived;

        public bool IsConnected { get; private set; }

        public void Connect(string ipAddress, int port, int batchLimit = 500)
        {
            if (IsConnected)
            {
                return;
            }

            _batchLimit = batchLimit;
            _subscribeSocket = new SubscriberSocket();
            _subscribeSocket.Connect($"tcp://{ipAddress}:{port}");

            _poller = new NetMQPoller { _subscribeSocket };
            _subscribeSocket.ReceiveReady += SubscribeSocket_ReceiveReady;
            _poller.RunAsync();

            IsConnected = true;
        }

        public void Close()
        {
            if (!IsConnected)
            {
                return;
            }

            _poller.Stop();
            _subscribeSocket.Close();
            IsConnected = false;
        }

        public void SubscribeToTopic(string topic)
        {
            if (IsConnected)
            {
                _subscribeSocket.Subscribe(topic);
            }
        }

        public void UnsubscribeFromTopic(string topic)
        {
            if (IsConnected)
            {
                _subscribeSocket.Unsubscribe(topic);
            }
        }

        private void SubscribeSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            if (e.Socket == null)
            {
                return;
            }

            for (int i = 0; i < _batchLimit; i++)
            {
                var msg = new NetMQMessage();
                if (e.Socket.TryReceiveMultipartMessage(ref msg))
                {
                    OnMessageReceived(msg);
                }
                else
                {
                    break;
                }
            }
        }

        protected virtual void OnMessageReceived(NetMQMessage message)
        {
            MessageReceived?.Invoke(this, message);
        }
    }
}
