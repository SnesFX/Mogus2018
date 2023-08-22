namespace Hazel.Udp
{
	internal enum UdpSendOption : byte
	{
		Hello = 8,
		Disconnect = 9,
		Acknowledgement = 10,
		Fragment = 11
	}
}
