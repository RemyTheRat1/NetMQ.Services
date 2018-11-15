using NetMQ.Sockets;
using System;
using System.Text;
using System.Threading;
using Xunit;

namespace NetMQ.Services.UnitTests
{
    public class NetMQRouterTests
    {
        [Fact]
        public void IsRunningTest()
        {
            var sut = new NetMQRouter();
            Assert.False(sut.IsRunning);

            sut.Start(14500);
            Assert.True(sut.IsRunning);

            sut.Stop();
            Assert.False(sut.IsRunning);
        }

        [Fact]
        public void RouteMessageTest()
        {
            string client1Identity = "sender";
            string client2Identity = "receiver";
            int port = 14501;
            string msgPayload = "message sent by client";

            var sut = new NetMQRouter();
            sut.Start(port);

            //setup server socket
            var server = new DealerSocket();
            server.Options.Identity = Encoding.Unicode.GetBytes(client2Identity);
            server.Connect($"tcp://127.0.0.1:{port}");

            //setup client socket
            var client = new DealerSocket();
            client.Options.Identity = Encoding.Unicode.GetBytes(client1Identity);
            client.Connect($"tcp://127.0.0.1:{port}");

            //the sockets need a small wait to connect and set their buffers up
            Thread.Sleep(500);

            //send a msg from the client to the server
            var msg = new NetMQMessage();
            msg.AppendEmptyFrame();
            msg.Append(Encoding.Unicode.GetBytes(client2Identity));
            msg.Append(msgPayload);
            client.SendMultipartMessage(msg);

            NetMQMessage serverMsgReceived = null;
            server.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(250), ref serverMsgReceived);
            Assert.NotNull(serverMsgReceived);
            //the message should be the 2nd frame
            Assert.Equal(msgPayload, serverMsgReceived[1].ConvertToString());

            sut.Stop();
            server.Close();
            client.Close();
        }
    }
}
