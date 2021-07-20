using System;
using System.Collections.Generic;
using System.Text;
using Chatting_Client.Session;

namespace Chatting_Client.Session
{
    public class SessionManager
    {
        static SessionManager _session = new SessionManager();
        static ServerSession oneSession = new ServerSession();

        public static SessionManager Instance { get { return _session; } }
        public static ServerSession InstanceSession => oneSession;
        object Lock = new object();

        public ServerSession Generate()
        {
            lock (Lock)
            {
                ServerSession session = new ServerSession();
                //_sessions.Add(session);
                oneSession = session;
                return session;
            }
        }

    }
}
