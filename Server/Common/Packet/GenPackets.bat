START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml

XCOPY /Y GenPackets.cs "../../DummyClinet/Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"