using NetMQ.Sockets;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NetMQ.Services
{
    public class NetMQDealerSocket : INetMQDealerSocket
    {
        private int _batchLimit;
        private DealerSocket _socket;
        private NetMQPoller _poller;

        public event EventHandler<NetMQMessage> MessageReceived;

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Connect to am endpoint for direct one-to-one communication
        /// </summary>
        /// <param name="ipAddress">IP Address to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="queueSize">Max msg queue size (high watermark). Defaults to 75</param>
        /// <param name="identity">Identity for this socket. If left empty an identity will be auto generated</param>
        /// <param name="batchLimit">How many messages to grab in a batch to process at once for incoming messages. Defaults to 500</param>
        public void Connect(string ipAddress, int port, int queueSize = 75, string identity = "", int batchLimit = 500)
        {
            if (IsConnected)
            {
                return;
            }

            _batchLimit = batchLimit;
            _socket = new DealerSocket();
            if (!string.IsNullOrEmpty(identity))
            {
                _socket.Options.Identity = Encoding.Unicode.GetBytes(identity);
            }

            _socket.Options.ReceiveHighWatermark = queueSize;
            _socket.Options.SendHighWatermark = queueSize;
            _socket.ReceiveReady += Socket_ReceiveReady;
            _socket.Connect($"tcp://{ipAddress}:{port}");

            _poller = new NetMQPoller { _socket };
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
            _socket.Close();
            IsConnected = false;
        }

        public void SendMessage(byte[] destinationIdentity, byte[] data)
        {
            var message = BuildMessage(destinationIdentity);
            message.Append(data);

            SendMessage(message);
        }        

        public void SendMessage(byte[] destinationIdentity, int data)
        {
            var message = BuildMessage(destinationIdentity);
            message.Append(data);

            SendMessage(message);
        }

        public void SendMessage(byte[] destinationIdentity, long data)
        {
            var message = BuildMessage(destinationIdentity);
            message.Append(data);

            SendMessage(message);
        }

        public void SendMessage(byte[] destinationIdentity, string data)
        {
            var message = BuildMessage(destinationIdentity);
            message.Append(data);

            SendMessage(message);
        }

        public void SendMessage(NetMQMessage message)
        {
            if (!IsConnected)
            {
                return;
            }

            //start a new task to send the msg through the dealer socket
            var task = new Task(() => _socket.SendMultipartMessage(message));
            //set the task to use the _poller's task scheduler so that the msg is sent on the socket's thread
            task.Start(_poller);
            //wait for the msg to be sent before moving on
            task.Wait();
        }

        private static NetMQMessage BuildMessage(byte[] destinationIdentity)
        {
            var message = new NetMQMessage();
            message.AppendEmptyFrame();
            message.Append(destinationIdentity);
            return message;
        }

        private void Socket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            for (int i = 0; i < _batchLimit; i++)
            {

                var msg = new NetMQMessage();
                if (e.Socket.TryReceiveMultipartMessage(ref msg))
                {
                    OnMessageReceived(msg);
                }
                else
                {
                    //no more msgs, exit the loop
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
