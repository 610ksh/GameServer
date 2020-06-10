using System;
using System.IO; // for File IO
using System.Xml; // for XML

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;
        static ushort packetId; // 0 초기화
        static string packetEnums;
        
        static void Main(string[] args)
        {
            string pdlPath = "../PDL.xml"; // default path

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true, // 주석 무시
                IgnoreWhitespace = true // space 무시
            };

            // 메인함수로 입력값을 받았으면 경로에 넣어주기
            if (args.Length >= 1)
                pdlPath = args[0];

            // xml 파일 파서
            using (XmlReader r = XmlReader.Create(pdlPath, settings))
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

                // 파일 Text 만들기
                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                // File IO, 해당 string으로 파일생성
                File.WriteAllText("GenPackets.cs", fileText);
            }

        }

        ///
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
            Tuple<string, string, string> t = ParserMembers(r);

            // packetFormat 파싱한거 가져오기
            genPackets += string.Format(PacketFormat.packetFormat,
                packetName, t.Item1, t.Item2, t.Item3);
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";
        }


        // 멤버들 파싱
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static Tuple<string, string, string> ParserMembers(XmlReader r)
        {
            // PlayerInfoReq 추출
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

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
                    return null;
                }

                // 해당 변수들을 만나면 처리하고 매번 개행하라
                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                // 멤버의 Type 파싱. 모두 소문자로 받음
                string memberType = r.Name.ToLower();

                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            // 코드 간격을 예쁘게 정렬하기 위한 코드
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }


        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }
            Tuple<string, string, string> t = ParserMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1,
                t.Item2,
                t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        // 첫글자 대문자 만들기
        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        // 모두 소문자 만들기
        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}