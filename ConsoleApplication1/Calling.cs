using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public struct CallFrameListNode
    {
        public DateTime CreatedTime;
        public ulong FrameId;
        public long NextCallFrame;
    }

    public struct CallFrameListDescriptor
    {
        public long CallFrameListHead;

        public TimeSpan FrameLife;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Luid
    {
        [FieldOffset(0)] public Guid Whole;

        [FieldOffset(0)] public long High;

        [FieldOffset(8)] public long Low;
    }

    public static class CallManager
    {
        public static unsafe void Advise<T>(this Ipc scope, string method, Action<T> proxy)
            where T : struct 
        {
            CallFrameListDescriptor* descriptor = null;//scope.GetObject<CallFrameListDescriptor>("CallFrameListDescriptor:" + method);
            CallFrameListNode* node = null; //scope.GetObject<CallFrameListNode>("CallFrameListNode:" + descriptor->CallFrameListHead);

            //Seek to the end of the frame list
            while (Interlocked.CompareExchange(ref node->NextCallFrame, 0, 0) != 0)
            {
                node = null; //scope.GetObject<CallFrameListNode>("CallFrameListNode:" + node->NextCallFrame);
            }

            //The current value of node is the one before any calls we'd process
            new Thread(() =>
            {
                while (true) { 
                    //while there's no node to process, wait patiently
                    while (Interlocked.CompareExchange(ref node->NextCallFrame, 0, 0) == 0)
                    {
                        Thread.Sleep(1);
                    }

                    //Move to the unprocessed frame
                    node = null; //scope.GetObject<CallFrameListNode>("CallFrameListNode:" + node->NextCallFrame);
                    
                    //Process it
                    Ipc<T> frameData = scope.GetObject<T>("CallFrame:" + node->FrameId);
                    proxy(frameData.Value);
                }
            }).Start();
        }
    }

    public static class CallFrameManager
    {
        public static unsafe void QueueCall<T>(this Ipc scope, string method, T frameContent)
            where T : struct 
        {
            CallFrameListDescriptor* descriptor = null;//scope.GetObject<CallFrameListDescriptor>("CallFrameListDescriptor:" + method);
            CallFrameListNode* node = null; //scope.GetObject<CallFrameListNode>("CallFrameListNode:" + descriptor->CallFrameListHead);
            Luid id = new Luid {Whole = Guid.NewGuid()};
            MemoryMappedFile data;

            //Get a space to store the frame in
            do
            {
                try
                {
                    data = MemoryMappedFile.CreateNew($"{scope.Name}:CallFrames:{id.Low}", Marshal.SizeOf<T>(), MemoryMappedFileAccess.ReadWrite);
                    //Write frameContent into data
                    break;
                }
                catch
                {
                    id = new Luid { Whole = Guid.NewGuid() };
                }
            } while (true);

            //Acquire the tail of the call frame list
            while (Interlocked.CompareExchange(ref node->NextCallFrame, id.Low, 0) != 0)
            {
                node = null; //scope.GetObject<CallFrameListNode>("CallFrameListNode:" + node->NextCallFrame);
            }

            node = null; //scope.GetObject<CallFrameListNode>("CallFrameListNode:" + id.Low);
            //Write the current time to the call frame so it may be reclaimed at some point
            //write 

            Interlocked.Exchange(ref descriptor->CallFrameListHead, id.Low);
        }
    }
}
