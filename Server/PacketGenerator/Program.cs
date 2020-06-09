using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true, // 주석 무시
                IgnoreWhitespace = true // space 무시
            };

            // xml 파일 파서
            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                // 헤더를 건너뜀.
                r.MoveToContent();

                // xml에서 순서대로 읽어들임. bool 반환. 끝까지.
                while (r.Read())
                {
                    // Element : 열린부분, EndElement : 닫힌부분(</~>)
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                    //Console.WriteLine(r.Name + " " + r["name"] + " " + r.Depth);
                }
            }

        }
        public static void ParsePacket(XmlReader r)
        {
            // 만약 닫힌 부분이 들어오면 return
            if (r.NodeType == XmlNodeType.EndElement)
                return;
            // 파싱한 Name이 소문자로 packet이 아니라면 return
            if (r.Name.ToLower() != "packet")
                return;

            // packet의 tag "name" 뽑아오기 = PlayerInfoReq
            string packetName = r["name"];
            
            // 만약 "name" 부분이 비었다면, 없다면 return
            if(string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }
            
            // Member를 파서
            ParserMembers(r);
        }

        public static void ParserMembers(XmlReader r)
        {
            // PlayerInfoReq 추출
            string packetName = r["name"];

            // 다음에 읽어들일 멤버들은 depth가 +1 임
            int depth = r.Depth + 1; // depth if packet is 1
            
            // 다음 xml 순서를 읽어들임
            while(r.Read())
            {
                // 내부 멤버가 없다면 break;
                if (r.Depth != depth)
                    break;

                // 멤버의 name tag를 파싱
                string memberName = r["name"];
                // name이 없다면 return
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                // 멤버의 Tpye 파싱. 모두 소문자로 받음
                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":

                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}