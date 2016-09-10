using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

using u8 = System.Byte;
using u32 = System.UInt32;

namespace makebmsg
{
    [DataContract]
    public class BinaryMessage
    {
        [DataMember(Order = 0)]
        public Version Version;
        [DataMember(Order = 1)]
        public MessageInfo[] Entries;

        public void Save(string path)
        {
            using (FileStream fs = File.Create(path))
            {
                Save(fs);
            }
        }
        public void Save(Stream stream)
        {
            BinaryMessageHeader header = new BinaryMessageHeader();
            header.Magic = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("BMSG"), 0);
            header.MessageCount = (u32)Entries.Length;
            StructWriter.WriteStruct(stream, header);

            header.pMessageInfo = (u32)stream.Position;

            List<BinaryMessageInfo> temp = new List<BinaryMessageInfo>();
            foreach (var info in Entries)
            {
                BinaryMessageInfo entry = new BinaryMessageInfo(info);
                StructWriter.WriteStruct(stream, entry);
                temp.Add(entry);
            }
            var entries = temp.ToArray();
            AlignStream(stream, 0x10);
            header.pLabel = (u32)stream.Position;

            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < temp.Count; i++)
            {
                entries[i].LabelOffset = (u32)stream.Position - header.pLabel;
                writer.Write(Entries[i].Label.ToCharArray());
                stream.WriteByte(0);
                stream.Flush();
            }
            AlignStream(stream, 0x10);
            header.pMessage = (u32)stream.Position;

            for (int i = 0; i < temp.Count; i++)
            {
                entries[i].MessageOffset = (u32)stream.Position - header.pMessage;
                writer.Write(Entries[i].Message.ToCharArray());
                stream.WriteByte(0);
                stream.Flush();
            }
            AlignStream(stream, 0x10);

            header.FileSize = (u32)stream.Length;
            header.Version = Version;

            stream.Position = 0;
            StructWriter.WriteStruct(stream, header);
            foreach (var e in entries)
            {
                StructWriter.WriteStruct(stream, e);
            }
            stream.Flush();
        }
        private static void AlignStream(Stream stream, int alignment)
        {
            while (stream.Position % alignment != 0)
            {
                stream.WriteByte(0);
            }
        }
    }
    public class MessageInfo
    {
        public float ScaleX;
        public float ScaleY;
        public string Label;
        public string Message;

        public MessageInfo()
        {
            ScaleX = ScaleY = 0;
            Label = Message = "";
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BinaryMessageHeader
    {
        public u32 Magic;
        public Version Version;
        public u8 Reserved1;
        public u32 FileSize;
        public u32 MessageCount;
        public u32 pMessageInfo;
        public u32 pLabel;
        public u32 pMessage;
        public u32 Reserved2;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Version
    {
        public u8 Major;
        public u8 Minor;
        public u8 Micro;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BinaryMessageInfo
    {
        public float ScaleX;
        public float ScaleY;
        public u32 LabelOffset;
        public u32 MessageOffset;

        public BinaryMessageInfo(MessageInfo info)
        {
            ScaleX = info.ScaleX;
            ScaleY = info.ScaleY;
            LabelOffset = 0;
            MessageOffset = 0;
        }
    }
}
