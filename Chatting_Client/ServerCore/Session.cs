using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            //받은 데이터버퍼에서 헤더와 패킷부분만 확인하는 부분
            while (true)
            {
                if (buffer.Count < HeaderSize) break;

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break;

                //실제로 컨텐츠단에서 처리 할 함수
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }//WHILE

            //얼마나 처리했는지 리턴
            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int disconnected = 0;
        RecvBuffer recvBuffer = new RecvBuffer(65535);

        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        
        object Lock = new object();
        //패킷 모아 보내기와 비슷한 개념
        Queue<ArraySegment<byte>> sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        #region 컨텐츠 쪽에서 오버라이딩 해야할 함수 목록
        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);
        #endregion

        void Clear()
        {
            lock (Lock)
            {
                sendQueue.Clear();
                pendingList.Clear();
            }
        }

        //리스너와 커넥터에서 여기로 넘어옴
        public void Start(Socket socket)
        {
            //소켓 연결 및 송 수신 이벤트 연결
            _socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        void Disconnect()
        {
            //이미 연결이 끊어졌는지 먼저 원자적으로 확인
            if (Interlocked.Exchange(ref disconnected, 1) == 1) return;

            //연결이 끊어졌을때 컨텐츠단에서 실행시킬부분
            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            Clear();
        }

        #region SEND
        //보낼 데이터를 버퍼로 받아서 인자로 넘겨주고 그걸 여기서 받아서 보냄
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (Lock)
            {
                sendQueue.Enqueue(sendBuff);
                if (pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        void RegisterSend()
        {
            //연결이 끊어졌는데 데이터를 보내려고 하는경우
            if (disconnected == 1) return;

            //sendArgs.BufferList=sendList----> 이런식으로 처리하면 이상하게 에러가 남
            while (sendQueue.Count > 0)
            {
                pendingList.Add(sendQueue.Peek());
                sendQueue.Dequeue();
            }//WHILE

            sendArgs.BufferList = pendingList;

            try
            {
                bool pending = _socket.SendAsync(sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, sendArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed{e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            //여기서 락을 쥐는 이유를 반드시 생각해볼 것
            lock (Lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        //pendingList = null;
                        pendingList.Clear();
                        sendArgs.BufferList = null;

                        //컨텐츠단에서 처리 할 함수
                        OnSend(sendArgs.BytesTransferred);

                        //멀티쓰레드 환경이므로 샌드 큐가 다른 쓰레드에 의해서 비어있지 않을 경우도 고려해줘야 함
                        if (sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed{e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }//LOCK
        }
        #endregion

        #region RECV
        void RegisterRecv()
        {
            //이미 연결이 끊어졌는지 확인
            if (disconnected == 1) return;

            recvBuffer.Clean();
            ArraySegment<byte> segment = recvBuffer.WriteSegment;
            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(recvArgs);
                //운좋게 바로 데이터를 받아서 처리할 수 있는 경우
                if (pending == false)
                {
                    OnRecvCompleted(null, recvArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed{e}");
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //recvBuffer에서 받을 수 있는 데이터양보다 더 많이 받았을때
                    if (recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLen = OnRecv(recvBuffer.ReadSegment);
                    if (processLen < 0 || recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    if (recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }


                    RegisterRecv();

                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed{e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
