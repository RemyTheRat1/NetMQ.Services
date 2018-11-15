using NetMQ.Sockets;
using System;
using Xunit;

namespace NetMQ.Services.UnitTests
{
    public class NetMQPublisherSocketTests
    {
        [Fact]
        public void IsConnectedTest()
        {
            var sut = new NetMQPublisherSocket();
            Assert.False(sut.IsRunning);

            sut.Start(14700);
            Assert.True(sut.IsRunning);

            sut.Stop();
            Assert.False(sut.IsRunning);
        }

        [Fact]
        public void PublishMessageTest()
        {
            int port = 14701;
            string topic = "topic";

            var sut = new NetMQPublisherSocket();
            sut.Start(port);

            var subscriber = new SubscriberSocket($"tcp://127.0.0.1:{port}");
            subscriber.Subscribe(topic);
            //the subscriber socket needs a delay so it can set its socket connection up before subscriptions can be made
            System.Threading.Thread.Sleep(500);

            string message = "message test";
            sut.PublishMessage(topic, message);

            NetMQMessage receivedMessage = null;
            subscriber.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(1000), ref receivedMessage);
            Assert.NotNull(receivedMessage);
            Assert.Equal(topic, receivedMessage[0].ConvertToString());
            Assert.Equal(message, receivedMessage[1].ConvertToString());

            sut.Stop();
            subscriber.Close();
        }

    }
}
