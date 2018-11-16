using System;

namespace NetMQ.Services
{
    public interface INetMQDealerSocket
    {
        bool IsConnected { get; }

        event EventHandler<NetMQMessage> MessageReceived;

        void Close();
        void Connect(string ipAddress, int port, int queueSize = 75, string identity = "", int batchLimit = 500);
        void SendMessage(byte[] destinationIdentity, byte[] data);
        void SendMessage(byte[] destinationIdentity, int data);
        void SendMessage(byte[] destinationIdentity, long data);
        void SendMessage(byte[] destinationIdentity, string data);
        void SendMessage(NetMQMessage message);
    }
}