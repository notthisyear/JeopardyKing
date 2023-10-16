using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JeopardyKing.Communication
{
    internal class UdpBroadcastListener : IDisposable
    {
        #region Private fields
        private const int ListenPort = 5973;
        private readonly Action<byte[]> _eventAction;
        private readonly UdpClient _client;
        private IPEndPoint _broadcastListenEndPoint;
        private bool _disposedValue;
        #endregion

        public UdpBroadcastListener(Action<byte[]> eventAction)
        {
            _eventAction = eventAction;
            _client = new UdpClient(ListenPort);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 60000);
            _broadcastListenEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);

            _ = Task.Run(() => ListenForUdpBroadcasts());
        }

        #region Private methods
        private void ListenForUdpBroadcasts()
        {
            try
            {
                var msg = _client.Receive(ref _broadcastListenEndPoint);
                _eventAction(msg);
            }
            catch (Exception e) when (e is SocketException or ObjectDisposedException)
            { }

            ListenForUdpBroadcasts();
        }
        #endregion

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_client.Client.Connected)
                        _client.Client.Disconnect(reuseSocket: true);
                    _client.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
