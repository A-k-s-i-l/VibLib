
using System.Net.Sockets;
using System.Net;
using System;
namespace VibLib
{

	/// <summary>
	/// Sends UDP messages containing float values to AQEBridge on local machine.
	/// </summary>
	public class UdpSender
	{
		private UdpClient udpClient;
		private IPEndPoint endPoint;

		public UdpSender(string serverIp, int serverPort)
		{
			udpClient = new UdpClient();
			endPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
		}

		public void SendMessage(float message)
		{
			byte[] data = BitConverter.GetBytes(message);
			udpClient.Send(data, data.Length, endPoint);
		}

		public void Close()
		{
			udpClient.Close();
		}
	}
}
