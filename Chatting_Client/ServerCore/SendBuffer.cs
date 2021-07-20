using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    //왜 굳이 샌드버퍼핼퍼클래스를 이용해서 샌드를 하는지 생각해볼것
    public class SendBufferHelper 
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        
        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null) CurrentBuffer.Value=new SendBuffer(ChunkSize);

            //요구하는량이 가지고 있는 양보다 많을 경우 그냥 새로할당해 줌
            if (reserveSize > CurrentBuffer.Value.FreeSize) CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        byte[] buffer;
        int _usedSize = 0;

        public SendBuffer(int chunkSize)
        {
            buffer = new byte[chunkSize];
        }

        public int FreeSize { get { return buffer.Length - _usedSize; } }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize) return null;

            return new ArraySegment<byte>(buffer,_usedSize,reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
