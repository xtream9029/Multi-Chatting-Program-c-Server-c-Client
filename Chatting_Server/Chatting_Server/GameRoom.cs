using Chatting_Server.Session;
using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Chatting_Server
{
    public class GameRoom :IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        //패킷 모아보내는 함수
        public void Flush()
        {
            foreach (ClientSession s in _sessions)
            {
                s.Send(_pendingList);
            }
            _pendingList.Clear();

        }


        public void BroadCast(ClientSession session,string chat,string name)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{name}:{chat}";
            //packet.chat = $"{packet.playername}:{chat}";
            ArraySegment<byte> segment = packet.Write();


            _pendingList.Add(segment);

            //foreach (ClientSession s in _sessions)
            //{
            //    s.Send(segment);
            //}
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }

    }
}
