START D:/c#ServerPratice/ChattingProgram/Chatting_Server/PacketGenerator/bin/Debug/netcoreapp3.1/PacketGenerator.exe D:/c#ServerPratice/ChattingProgram/Chatting_Server/PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "D:/c#ServerPratice/ChattingProgram/Chatting_Client/Chatting_Client/Packet"
XCOPY /Y GenPackets.cs "D:/c#ServerPratice/ChattingProgram/Chatting_Server/Chatting_Server/Packet"
XCOPY /Y ClientPacketManager.cs "D:/c#ServerPratice/ChattingProgram/Chatting_Client/Chatting_Client/Packet"
XCOPY /Y ServerPacketManager.cs "D:/c#ServerPratice/ChattingProgram/Chatting_Server/Chatting_Server/Packet"