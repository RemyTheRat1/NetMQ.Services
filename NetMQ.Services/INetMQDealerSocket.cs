using System;

namespace NetMQ.Services
{
    public interface INetMQDealerSocket
    {
        bool IsConnected { get; }

        event EventHandler<NetMQMessage> MessageReceived;

        void Close();
        void Connect(string ipAddress, int port, string identity = "", int queueSize = 75, int batchLimit = 500);
        void SendMessage(byte[] destinationIdentity, byte[] data);
        void SendMessage(byte[] destinationIdentity, int data);
        void SendMessage(byte[] destinationIdentity, long data);
        void SendMessage(byte[] destinationIdentity, string data);
        void SendMessage(NetMQMessage message);
        void SendMessage(string destinationIdentity, byte[] data);
        void SendMessage(string destinationIdentity, int data);
        void SendMessage(string destinationIdentity, long data);
        void SendMessage(string destinationIdentity, string data);
    }
}