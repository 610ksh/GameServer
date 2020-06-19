using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 패킷을 서버쪽에서 만들어서 바로 핸들러를 통해 처리하는게 아니라
 * 큐에 넣어 잠시 저장한뒤 그것을 유니티쪽에서 꺼내는 역할.
 */
public class PacketQueue
{
    // getter를 추가하고, 싱글톤
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> _packetQueue = new Queue<IPacket>();
    object _lock = new object(); // for lock

    public void Push(IPacket packet)
    {
        lock(_lock)
        {
            _packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock(_lock)
        {
            if (_packetQueue.Count == 0)
                return null;
            return _packetQueue.Dequeue();
        }
    }

    public List<IPacket> PopAll()
    {
        List<IPacket> list = new List<IPacket>();

        lock(_lock)
        {
            while (_packetQueue.Count > 0)
                list.Add(_packetQueue.Dequeue());
        }
        return list;
    }

}
