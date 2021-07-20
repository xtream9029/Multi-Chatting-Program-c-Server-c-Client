using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chatting_Server;
using Chatting_Server.Session;
using ServerCore;

namespace Chatting_Server
{
	class Program
	{
		static Listener _listener = new Listener();

		public static GameRoom Room = new GameRoom();


		static void Main(string[] args)
		{
			//PacketManager.Instance.Register();

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			while (true)
			{
				//주 쓰레드
				Room.Push(() => Room.Flush());
				Thread.Sleep(250);//0.25초
			}
		}
	}
}
