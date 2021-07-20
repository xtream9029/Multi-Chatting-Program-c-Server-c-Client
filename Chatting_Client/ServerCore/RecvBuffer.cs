using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> buffer;
        int writePos;
        int readPos;

        public RecvBuffer(int bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return writePos - readPos; } }
        public int FreeSize { get { return buffer.Count - writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(buffer.Array,buffer.Offset+readPos,DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(buffer.Array, buffer.Offset + writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            //readPos==writePos일때
            if (dataSize == 0)
            {
                readPos = 0;
                writePos = 0;
            }

            Array.Copy(buffer.Array, buffer.Offset + readPos, buffer.Array, buffer.Offset, dataSize);
            readPos = 0;
            writePos = dataSize;
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize) return false;

            readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize) return false;

            writePos += numOfBytes;
            return true;
        }

    }
}
