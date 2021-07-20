using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper 
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null) CurrentBuffer.Value=new SendBuffer(ChunkSize);
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
        //[    ][usedSize][    ][    ][    ][    ][    ][    ][    ]
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

            return new ArraySegment<byte>(buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, _usedSize, usedSize);//이 부분이 이해안감
            _usedSize += usedSize;
            return segment;
        }
    }
}
