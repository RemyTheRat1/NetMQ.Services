using System;

namespace NetMQ.Services.UnitTests
{
    public class TestFixture : IDisposable
    {
        public void Dispose()
        {
            NetMQConfig.Cleanup(false);
        }
    }
}
