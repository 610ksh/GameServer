
using System;
using System.Collections.Generic;
using System.Text;

// 상속, 실질적인 생성 객체
class PlayerInfoReq
{
    public byte testByte;
    public long playerId; // 8(Int64)
    public string name;

    #region Skill
    // 모든 skill 객체들은 아래에 해당하는 정보를 담고 있음.
    public struct SkillInfo
    {
        public int id;
        public short level;
        public float duration;

        public bool Write(Span<byte> s, ref ushort count)
        {
            bool success = true;
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
            count += sizeof(int);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
            count += sizeof(short);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
            count += sizeof(float);

            return success;
        }

        public void Read(ReadOnlySpan<byte> s, ref ushort count)
        {
            id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
            count += sizeof(int);
            level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
            count += sizeof(short);
            duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
            count += sizeof(float);
        }
    }

    // skill에 대한 객체생성. 스킬 종류가 늘어날때마다 List에 Add로 추가. 
    public List<SkillInfo> skills = new List<SkillInfo>();
    #endregion

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        // default
        count += sizeof(ushort);
        count += sizeof(ushort);

        // testByte
        this.testByte = segment.Array[segment.Offset + count];
        count += sizeof(byte);

        //
        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count)); // 범위를 짚어줌. 몇바이트인지도 지정
        count += sizeof(long);

        // string
        ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        count += nameLen;

        // skill list
        skills.Clear(); // 안전하게 초기화
        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);

        for (int i = 0; i < skillLen; i++)
        {
            SkillInfo skill = new SkillInfo();
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

        // packet header
        // [][][][][][][][][][]
        // size는 마지막에 최종적으로 확정되기 때문에 맨 마지막에 count 변수로 처리한다.
        // success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
        count += sizeof(ushort);

        // testByte
        segment.Array[segment.Offset + count] = this.testByte;
        count += sizeof(byte);

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
        count += sizeof(long);

        // string
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
        count += nameLen;

        // skill list
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count); // 스킬 개수만큼(Count) 확보
        count += sizeof(ushort);

        // List<SkillInfo> skills를 돌면서 처리
        foreach (SkillInfo skill in skills)
            success &= skill.Write(s, ref count);

        // 최종 헤더 사이즈 부분 넘겨줌
        success &= BitConverter.TryWriteBytes(s, count); // packet.size = count

        if (success == false)
            return null; // 유의 위의 Array가 null로 셋팅되어 넘어가는걸 의도.

        return SendBufferHelper.Close(count);
    }
}

public enum PacketID
{
    PlayerInfoReq = 1,
    PlayerInfoOk = 2,
}