using System;

namespace NetMQ.Services
{
    public interface INetMQSubscriberSocket
    {
        bool IsConnected { get; }

        event EventHandler<NetMQMessage> MessageReceived;

        void Close();
        void Connect(string ipAddress, int port, int batchLimit = 500);
        void SubscribeToTopic(string topic);
        void UnsubscribeFromTopic(string topic);
    }
}