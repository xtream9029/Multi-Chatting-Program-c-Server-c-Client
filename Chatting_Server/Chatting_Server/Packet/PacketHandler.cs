using Chatting_Server;
using Chatting_Server.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    //서버쪽에서 클라의 패킷을 처리하는 클래스
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null) return;

        GameRoom room = clientSession.Room;

        room.Push(()=> room.BroadCast(clientSession,chatPacket.chat,chatPacket.playername));
        
        //clientSession.Room.BroadCast(clientSession, chatPacket.chat);
    }
}
