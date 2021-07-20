using Chatting_Client.Session;
using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chatting_Client
{
	class Program
	{
		static void Main(string[] args)
		{
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();

			connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); });

			//사용자 이름을 넘겨서 서버가 받게 하고 싶음

			//연결로그 뜬 이후에 진행되도록 1초 텀을 둠
			Thread.Sleep(1000);
			Console.Write("사용자 이름을 입력하세요:");
			string playerName = Console.ReadLine();

			while (true)
			{
				try
				{
					//단일 클라에서는 굳이 더미클라방식으로 테스트를 할 필요가 없음
					string msg = Console.ReadLine();

					C_Chat chatPacket = new C_Chat();

					chatPacket.playername = playerName;
					chatPacket.chat = msg;

					ArraySegment<byte> segment = chatPacket.Write();

					//버퍼에 패킷직렬화까지 끝 마친상태에서 Send함수 호출
					SessionManager.InstanceSession.Send(segment);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(250);//0.25초
			}
		}
	}
}
