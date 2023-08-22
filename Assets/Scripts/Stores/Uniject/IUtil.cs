using System;
using System.Collections;
using UnityEngine;

namespace Uniject
{
	internal interface IUtil
	{
		RuntimePlatform platform { get; }

		bool isEditor { get; }

		string persistentDataPath { get; }

		string cloudProjectId { get; }

		string deviceUniqueIdentifier { get; }

		string unityVersion { get; }

		string userId { get; }

		string gameVersion { get; }

		ulong sessionId { get; }

		DateTime currentTime { get; }

		string deviceModel { get; }

		string deviceName { get; }

		DeviceType deviceType { get; }

		string operatingSystem { get; }

		int screenWidth { get; }

		int screenHeight { get; }

		float screenDpi { get; }

		string screenOrientation { get; }

		T[] GetAnyComponentsOfType<T>() where T : class;

		object InitiateCoroutine(IEnumerator start);

		object GetWaitForSeconds(int seconds);

		void InitiateCoroutine(IEnumerator start, int delayInSeconds);

		void RunOnMainThread(Action runnable);

		void AddPauseListener(Action<bool> runnable);

		bool IsClassOrSubclass(Type potentialBase, Type potentialDescendant);
	}
}
