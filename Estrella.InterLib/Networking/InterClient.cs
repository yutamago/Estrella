using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Estrella.InterLib.Encryption;
using Estrella.Util;

namespace Estrella.InterLib.Networking
{
    public class InterClient
    {
        private const int MaxReceiveBuffer = 0x4000; //16kb
        private readonly byte[] _mReceiveBuffer;
        private readonly ConcurrentQueue<ByteArraySegment> _mSendSegments;

        private int _mDisconnected;
        private bool _mHeader = true;
        private byte[] _mIvRecv;
        private bool _mIVs;
        private byte[] _mIvSend;
        private int _mReceiveLength;
        private int _mReceiveStart;
        private int _mReceivingPacketLength;
        private int _mSending;

        public InterClient(Socket socket)
        {
            _mSendSegments = new ConcurrentQueue<ByteArraySegment>();
            Socket = socket;
            Host = ((IPEndPoint) Socket.RemoteEndPoint).Address.ToString();
            // mReceiveBuffer = new byte[4];
            _mReceiveBuffer = new byte[MaxReceiveBuffer];
            _mReceivingPacketLength = 4; // Header length
            Assigned = false;
            Start();
            SendIVs();
        }

        public bool Assigned { get; set; }

        public Socket Socket { get; private set; }
        public string Host { get; private set; }
        public event EventHandler<InterPacketReceivedEventArgs> OnPacket;
        public event EventHandler<SessionCloseEventArgs> OnDisconnect;

        public void Start()
        {
            var rnd = new Random();
            _mIvRecv = new byte[16];
            _mIvSend = new byte[16];
            rnd.NextBytes(_mIvSend);

            BeginReceive();
        }

        public void Disconnect()
        {
            if (Interlocked.CompareExchange(ref _mDisconnected, 1, 0) == 0)
            {
                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }

                if (OnDisconnect != null)
                {
                    OnDisconnect(this, SessionCloseEventArgs.ConnectionTerminated); //TODO: split
                }
            }
        }

        private void BeginReceive()
        {
            if (_mDisconnected != 0) return;
            var args = new SocketAsyncEventArgs();
            args.Completed += EndReceive;
            args.SetBuffer(_mReceiveBuffer, _mReceiveStart, _mReceivingPacketLength - _mReceiveStart);
            try
            {
                if (!Socket.ReceiveAsync(args))
                {
                    EndReceive(this, args);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error at BeginReceive: {0}", ex.ToString());
                Disconnect();
            }
        }

        private void EndReceive(object sender, SocketAsyncEventArgs pArguments)
        {
            if (_mDisconnected != 0) return;
            if (pArguments.BytesTransferred <= 0)
            {
                Disconnect();
                return;
            }

            _mReceiveLength += pArguments.BytesTransferred;

            if (_mReceivingPacketLength == _mReceiveLength)
            {
                if (_mHeader) //parse headers
                {
                    _mReceivingPacketLength = BitConverter.ToInt32(_mReceiveBuffer, 0);
                    _mReceiveLength = 0;
                    _mReceiveStart = 0;
                    _mHeader = false;
                    // mReceiveBuffer = new byte[mReceivingPacketLength];
                }
                else
                {
                    //parse packets
                    var packetData = new byte[_mReceivingPacketLength];
                    Buffer.BlockCopy(_mReceiveBuffer, 0, packetData, 0, _mReceivingPacketLength);
                    if (!_mIVs)
                    {
                        var packet = new InterPacket(packetData);
                        if (packet.OpCode == InterHeader.Ivs)
                        {
                            Log.WriteLine(LogLevel.Info, "IV data received");
                            packet.ReadBytes(_mIvRecv);
                            _mIVs = true;
                        }
                        else
                        {
                            Log.WriteLine(LogLevel.Info, "Got wrong packet.");
                            Disconnect();
                        }
                    }
                    else
                    {
                        packetData = InterCrypto.DecryptData(_mIvRecv, packetData);
                        if (OnPacket != null)
                        {
                            var packet = new InterPacket(packetData);
                            OnPacket(this, new InterPacketReceivedEventArgs(packet, this));
                        }
                    }

                    //we reset this packet
                    _mReceivingPacketLength = 4;
                    _mReceiveLength = 0;
                    _mReceiveStart = 0;
                    _mHeader = true;
                    // mReceiveBuffer = new byte[4];
                }
            }
            else
            {
                _mReceiveStart += _mReceivingPacketLength;
            }

            BeginReceive();
            pArguments.Dispose();
        }

        private void SendIVs()
        {
            var packet = new InterPacket(InterHeader.Ivs);
            packet.WriteBytes(_mIvSend);
            SendPacket(packet, false);
        }

        public void SendInterPass(string what)
        {
            var packet = new InterPacket(InterHeader.Auth);
            packet.WriteStringLen(what);
            SendPacket(packet);
        }

        public void Send(byte[] pBuffer)
        {
            if (_mDisconnected != 0) return;
            _mSendSegments.Enqueue(new ByteArraySegment(pBuffer));
            if (Interlocked.CompareExchange(ref _mSending, 1, 0) == 0)
            {
                BeginSend();
            }
        }

        public void SendPacket(InterPacket pPacket, bool crypto = true)
        {
            var data = new byte[pPacket.Length + 4];
            Buffer.BlockCopy(crypto ? InterCrypto.EncryptData(_mIvSend, pPacket.ToArray()) : pPacket.ToArray(), 0, data,
                4, pPacket.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(pPacket.Length), 0, data, 0, 4);
            Send(data);
        }

        private void BeginSend()
        {
            var args = new SocketAsyncEventArgs();
            if (_mSendSegments.TryPeek(out var segment))
            {
                args.Completed += EndSend;
                args.SetBuffer(segment.Buffer, segment.Start, segment.Length);
                // args.SetBuffer(segment.Buffer, segment.Start, Math.Min(segment.Length, 1360));
                try
                {
                    if (!Socket.SendAsync(args))
                    {
                        EndSend(this, args);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    Log.WriteLine(LogLevel.Exception, "Error at BeginSend: {0}", ex.ToString());
                    Disconnect();
                }
            }
        }

        private void EndSend(object sender, SocketAsyncEventArgs pArguments)
        {
            if (_mDisconnected != 0) return;

            if (pArguments.BytesTransferred <= 0)
            {
                Disconnect();
                return;
            }

            if (_mSendSegments.TryPeek(out var segment))
            {
                if (segment.Advance(pArguments.BytesTransferred))
                {
                    _mSendSegments.TryDequeue(out var seg); //we try to get it out
                }

                if (_mSendSegments.Count > 0)
                {
                    BeginSend();
                }
                else
                {
                    _mSending = 0;
                }
            }

            pArguments.Dispose();
        }
    }
}