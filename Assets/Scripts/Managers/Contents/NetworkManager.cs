using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public class NetworkManager
{
	ServerSession _session = new ServerSession();
	public GameObject WaitingUI {  get; set; }

	public void Send(IMessage packet)
	{
		_session.Send(packet);
	}

	public void Init()
	{
		// DNS (Domain Name System)
		string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);

        //#if UNITY_EDITOR
        //		IPHostEntry ipHost = Dns.GetHostEntry(host);
        //#else
        //		IPHostEntry ipHost = Dns.GetHostEntry("220.69.209.153");
        //#endif
        //IPHostEntry ipHost = Dns.GetHostEntry("DDuKi.iptime.org");
        //IPHostEntry ipHost = Dns.GetHostEntry("CGlabHospital.iptime.org");
        IPAddress ipAddr = ipHost.AddressList[ipHost.AddressList.Length - 1];
		IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

		Connector connector = new Connector();

		connector.Connect(endPoint,
			() => { 
				Debug.Log("연결되었습니다."); 
				return _session; 
			},
			1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}
	}

}
