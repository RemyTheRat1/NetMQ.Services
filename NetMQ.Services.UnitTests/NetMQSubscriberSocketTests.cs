using NetMQ.Sockets;
using System;
using System.Threading;
using Xunit;

namespace NetMQ.Services.UnitTests
{
    public class NetMQSubscriberSocketTests : IClassFixture<TestFixture>
    {
        [Fact]
        public void IsConnectedTest()
        {
            var sut = new NetMQSubscriberSocket();
            Assert.False(sut.IsConnected);

            sut.Connect("127.0.0.1", 12345);
            Assert.True(sut.IsConnected);

            sut.Close();
            Assert.False(sut.IsConnected);
        }

        [Fact]
        public void SubscribeAndReceiveMessageTest()
        {
            int port = 14500;
            string ipAddress = "127.0.0.1";
            string topic = "topic";
            string message = "published message";

            var publisherSocket = new PublisherSocket();
            publisherSocket.Bind($"tcp://*:{port}");

            var sut = new NetMQSubscriberSocket();
            sut.Connect(ipAddress, port);
            sut.SubscribeToTopic(topic);
            NetMQMessage receivedMessage = null;
            sut.MessageReceived += (sender, msg) => { receivedMessage = msg; };
            //because we are sending and receiving immediately here, we need to wait
            //a bit to give the socket a chance for the subscription to be registered
            Thread.Sleep(500);

            var publishMessage = new NetMQMessage();
            publishMessage.Append(topic);
            publishMessage.Append(message);
            publisherSocket.SendMultipartMessage(publishMessage);

            Assert.True(WaitForCondition(() => receivedMessage != null, 250));
            Assert.Equal(receivedMessage[1].ConvertToString(), message);

            sut.Close();
            publisherSocket.Close();
        }

        [Fact]
        public void UnsubscribeTest()
        {
            //first subscribe, and then publish a message and check that a message is received
            int port = 14500;
            string ipAddress = "127.0.0.1";
            string topic = "topic";
            string message = "published message";

            var publisherSocket = new PublisherSocket();
            publisherSocket.Bind($"tcp://*:{port}");

            var sut = new NetMQSubscriberSocket();
            sut.Connect(ipAddress, port);
            sut.SubscribeToTopic(topic);
            NetMQMessage receivedMessage = null;
            sut.MessageReceived += (sender, msg) => { receivedMessage = msg; };
            //because we are sending and receiving immediately here, we need to wait
            //a bit to give the socket a chance for the subscription to be registered
            Thread.Sleep(500);

            var publishMessage = new NetMQMessage();
            publishMessage.Append(topic);
            publishMessage.Append(message);
            publisherSocket.SendMultipartMessage(publishMessage);

            Assert.True(WaitForCondition(() => receivedMessage != null, 250));

            //unsubscribe, resend the message, and then no message should be received
            sut.UnsubscribeFromTopic(topic);
            receivedMessage = null;
            publisherSocket.SendMultipartMessage(publishMessage);
            Assert.False(WaitForCondition(() => receivedMessage != null, 250));

            sut.Close();
            publisherSocket.Close();
        }

        private bool WaitForCondition(Func<bool> condition, int timeout)
        {
            int count = 0;
            while (!condition() && count < timeout)
            {
                Thread.Sleep(100);
                count += 100;
            }


            return count < timeout;
        }
    }
}
