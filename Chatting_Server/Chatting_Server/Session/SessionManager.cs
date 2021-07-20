using System;
using System.Collections.Generic;
using System.Text;

namespace Chatting_Server.Session
{
    public class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;

        //반드시 락을 쥔 상태에서 참조해야 함
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        object Lock = new object();

        public ClientSession Generate()
        {
            lock (Lock)
            {
                int sessionId = ++_sessionId;

                ClientSession session = new ClientSession();
                session.SessionId = sessionId;
                _sessions.Add(_sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (Lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }

        }

        public void Remove(ClientSession session)
        {
            lock (Lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}
