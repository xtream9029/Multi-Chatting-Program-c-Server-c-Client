using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ServerCore;

namespace Chatting_Server.Session
{
    public class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            //현재 하나의 클라이언트가 연결된 이후에 메시지를 보내고
            //다른 클라이언트를 서버에 연결하였을때 제대로 동작하지 않는 버그가 발생하고 있음

            Console.WriteLine($"OnConnected : {endPoint}");

         
            //게임룸 입장
            Program.Room.Push(()=> Program.Room.Enter(this));

            //Program.Room.Enter(this);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if (Room != null)
            {
                GameRoom room = Room;


                //Room.Leave(this);
                room.Push(() => room.Leave(this));
                Room = null;
            }

            Console.WriteLine($"Disconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //로그를 너무 많이 찍으면 서버 부하걸림
            //Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}
