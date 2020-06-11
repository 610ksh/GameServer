using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,

    }

    interface IPacket
    {
        ushort Protocol { get; }
        void Read(ArraySegment<byte> segment);
        ArraySegment<byte> Write();
    }


    class PlayerInfoReq : IPacket
    {
        public byte testByte;
        public long playerId;
        public string name;

        public class Skill
        {
            public int id;
            public short level;
            public float duration;

            public class Attribute
            {
                public int att;

                public void Read(ReadOnlySpan<byte> s, ref ushort count)
                {
                    this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);
                }

                public bool Write(Span<byte> s, ref ushort count)
                {
                    bool success = true;
                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.att);
                    count += sizeof(int);
                    return success;
                }
            }

            public List<Attribute> attributes = new List<Attribute>();

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
                attributes.Clear();
                ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);

                for (int i = 0; i < attributeLen; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(s, ref count);
                    attributes.Add(attribute);
                }
            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)attributes.Count);
                count += sizeof(ushort);

                foreach (Attribute attribute in attributes)
                    success &= attribute.Write(s, ref count);

                return success;
            }
        }

        public List<Skill> skills = new List<Skill>();

        public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }

        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);

            this.testByte = (byte)segment.Array[segment.Offset + count];
            count += sizeof(byte);
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);
                skills.Add(skill);
            }
        }


        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096); // 버퍼 공간확보

            ushort count = 0; // 자료형 중요함.
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);

            segment.Array[segment.Offset + count] = (byte)this.testByte;
            count += sizeof(byte);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);

            foreach (Skill skill in skills)
                success &= skill.Write(s, ref count);


            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected(Server) : {endPoint}"); // Session 소켓의 정보임.
            Console.WriteLine("OnConnected 성공함. 이전 함수 Session.Start() 수행하고 실행됨.");
            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size); // knight 객체의 hp 데이터를 바이트로 저장.
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            //Send(sendBuff); // Send to client


            Thread.Sleep(1000); // 1초 대기
            Disconnect(); // close to server
        }

        // sealed 되어서 OnRecv는 사용x 대신 아래것을 사용.
        // 클라에서 보낸 패킷을 까봐서 해석하는곳.
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count); // 2바이트 이후
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer); // deserialization 역직렬화
                        Console.WriteLine($"PlayerInfoReq : {p.playerId}  {p.name}");

                        foreach (PlayerInfoReq.Skill skill in p.skills)
                        {
                            Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
                        }
                    }
                    break;
            }

            Console.WriteLine($"RecvPacketId : {id}, Size {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected is failed : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}