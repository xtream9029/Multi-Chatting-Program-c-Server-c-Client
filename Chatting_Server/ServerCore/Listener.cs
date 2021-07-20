using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class Listener
    {
        Socket listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint,Func<Session> sessionFactory,int register=10,int backlog=100)
        {
            //TCP 연결
            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _sessionFactory += sessionFactory;
            listenSocket.Bind(endPoint);
            listenSocket.Listen(backlog);

            //문지기수 증가
            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }

        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender,SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();

                //본격적인 데이터 송 수신 시작
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnAcceptCompleted Failed{args.SocketError.ToString()}");
            }

            RegisterAccept(args);
        }
    }
}
