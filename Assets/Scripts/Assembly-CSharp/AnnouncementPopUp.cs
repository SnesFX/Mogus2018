using System;
using System.Collections;
using Hazel;
using Hazel.Udp;
using UnityEngine;

public class AnnouncementPopUp : MonoBehaviour
{
	private enum AnnounceState
	{
		Fetching = 0,
		Failed = 1,
		Success = 2
	}

	private const uint AnnouncementVersion = 1u;

	private UdpClientConnection connection;

	private static AnnounceState AskedForUpdate;

	public TextRenderer AnnounceText;

	private Announcement announcementUpdate;

	public void Init()
	{
		if (AskedForUpdate != 0)
		{
			return;
		}
		if (SaveManager.LastAnnouncement.DateFetched == DateTime.Now.DayOfYear)
		{
			AskedForUpdate = AnnounceState.Success;
			return;
		}
		Announcement lastAnnouncement = SaveManager.LastAnnouncement;
		connection = new UdpClientConnection(new NetworkEndPoint("18.218.20.65", 22024));
		connection.DataReceived += Connection_DataReceived;
		connection.Disconnected += Connection_Disconnected;
		try
		{
			MessageWriter messageWriter = MessageWriter.Get();
			messageWriter.WritePacked(1u);
			messageWriter.WritePacked(lastAnnouncement.Id);
			connection.ConnectAsync(messageWriter.ToByteArray(true));
			messageWriter.Recycle();
		}
		catch
		{
			AskedForUpdate = AnnounceState.Failed;
		}
	}

	private void Connection_Disconnected(object sender, DisconnectedEventArgs e)
	{
		AskedForUpdate = AnnounceState.Failed;
		connection.Dispose();
		connection = null;
	}

	private void Connection_DataReceived(object sender, DataReceivedEventArgs e)
	{
		if (e.Bytes.Length > 4)
		{
			MessageReader messageReader = MessageReader.Get(e.Bytes);
			announcementUpdate = default(Announcement);
			announcementUpdate.DateFetched = DateTime.Now.DayOfYear;
			announcementUpdate.Id = messageReader.ReadPackedUInt32();
			announcementUpdate.AnnounceText = messageReader.ReadString();
		}
		AskedForUpdate = AnnounceState.Success;
		try
		{
			connection.Dispose();
			connection = null;
		}
		catch
		{
		}
	}

	public IEnumerator Show()
	{
		float timer = 0f;
		while (AskedForUpdate == AnnounceState.Fetching && connection != null && timer < 5f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		if (AskedForUpdate != AnnounceState.Success)
		{
			Announcement lastAnnouncement = SaveManager.LastAnnouncement;
			if (lastAnnouncement.Id == 0)
			{
				AnnounceText.Text = "Couldn't get announcement.";
			}
			else
			{
				AnnounceText.Text = "Couldn't get announcement. Last Known:\r\n" + lastAnnouncement.AnnounceText;
			}
		}
		else if (announcementUpdate.Id != SaveManager.LastAnnouncement.Id)
		{
			if (SaveManager.LastAnnouncement.DateFetched != DateTime.Now.DayOfYear)
			{
				base.gameObject.SetActive(true);
			}
			if (announcementUpdate.Id == 0)
			{
				announcementUpdate = SaveManager.LastAnnouncement;
				announcementUpdate.DateFetched = DateTime.Now.DayOfYear;
			}
			SaveManager.LastAnnouncement = announcementUpdate;
			AnnounceText.Text = announcementUpdate.AnnounceText;
		}
		while (base.gameObject.activeSelf)
		{
			yield return null;
		}
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (connection != null)
		{
			connection.Dispose();
			connection = null;
		}
	}
}
