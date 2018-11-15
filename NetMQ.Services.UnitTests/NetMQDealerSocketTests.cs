using NetMQ.Sockets;
using System;
using System.Text;
using Xunit;

namespace NetMQ.Services.UnitTests
{
    public class NetMQDealerSocketTests : IClassFixture<TestFixture>
    {
        [Fact]
        public void ConnectAndCloseTest()
        {
            string ipAddress = "127.0.0.1";
            int port = 14600;            

            var sut = new NetMQDealerSocket();
            Assert.False(sut.IsConnected);

            var router = new NetMQRouter();
            router.Start(port);

            sut.Connect(ipAddress, port);
            Assert.True(sut.IsConnected);

            sut.Close();
            Assert.False(sut.IsConnected);

            router.Stop();
        }


        [Fact]
        public void SendMessageTest()
        {
            int port = 14601;
            string message = "test message";

            var router = new NetMQRouter();
            router.Start(port);

            byte[] dealerIdentity = Encoding.Unicode.GetBytes("dealer");
            var dealerSocket = new DealerSocket();
            dealerSocket.Options.Identity = dealerIdentity;
            dealerSocket.Connect($"tcp://127.0.0.1:{port}");

            var sut = new NetMQDealerSocket();
            sut.Connect("127.0.0.1", port);
            sut.SendMessage(dealerIdentity, message);

            NetMQMessage receivedMessage = null;
            dealerSocket.TryReceiveMultipartMessage(TimeSpan.FromSeconds(1), ref receivedMessage);
            Assert.NotNull(receivedMessage);
            Assert.Equal(message, receivedMessage[1].ConvertToString());

            dealerSocket.Close();
            sut.Close();
            router.Stop();
        }


    }
}
