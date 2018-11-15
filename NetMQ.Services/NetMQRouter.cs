using NetMQ.Sockets;

namespace NetMQ.Services
{
    public class NetMQRouter
    {
        private const int EXPECTED_FRAME_COUNT = 4;
        private const int DESTINATION_IDENTITY_FRAME = 2;
        private const int MSG_PAYLOAD_FRAME = 3;

        private RouterSocket _routerSocket;
        private NetMQPoller _routerPoller;
        private int _batchLimit;

        public bool IsRunning { get; private set; }

        public void Start(int routerPort, int batchLimit = 500)
        {
            if (IsRunning)
            {
                return;
            }

            _batchLimit = batchLimit;
            _routerSocket = new RouterSocket($"@tcp://*:{routerPort}");
            _routerPoller = new NetMQPoller { _routerSocket };
            _routerSocket.ReceiveReady += Router_ReceiveReady;

            _routerPoller.RunAsync();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            _routerPoller.Stop();
            _routerSocket.Close();
            IsRunning = false;
        }

        protected virtual void ForwardMessage(NetMQMessage msg)
        {
            //1st frame is the destination identity             
            //2nd frame is an empty delimiter
            //3rd frame is the msg payload            
            if (msg.FrameCount != EXPECTED_FRAME_COUNT)
            {
                return;
            }

            //forward the msg on to its destination
            var routedMsg = new NetMQMessage();
            routedMsg.Append(msg[DESTINATION_IDENTITY_FRAME]);
            routedMsg.AppendEmptyFrame();
            routedMsg.Append(msg[MSG_PAYLOAD_FRAME]);
            _routerSocket.SendMultipartMessage(routedMsg);
        }

        private void Router_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            if (e.Socket == null)
            {
                return;
            }

            //receive up to the _batchLimit at once to improve poller performance
            for (int i = 0; i < _batchLimit; i++)
            {
                var msg = new NetMQMessage();
                if (e.Socket.TryReceiveMultipartMessage(ref msg))
                {
                    ForwardMessage(msg);
                }
                else
                {
                    //no more msgs, exist the loop
                    break;
                }
            }

        }
    }
}
