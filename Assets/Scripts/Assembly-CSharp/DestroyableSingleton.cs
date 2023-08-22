using UnityEngine;

public class DestroyableSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	public bool DontDestroy;

	public static bool InstanceExists
	{
		get
		{
			return (Object)_instance;
		}
	}

	public static T Instance
	{
		get
		{
			lock (_lock)
			{
				if (!(Object)_instance)
				{
					_instance = Object.FindObjectOfType<T>();
					if (!(Object)_instance)
					{
						GameObject gameObject = new GameObject("(singleton) " + typeof(T).ToString());
						_instance = gameObject.AddComponent<T>();
					}
				}
				return _instance;
			}
		}
	}

	public virtual void Start()
	{
		if (!(Object)_instance)
		{
			_instance = this as T;
			if (DontDestroy)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}
		else if (_instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else if (DontDestroy)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public virtual void OnDestroy()
	{
		if (!DontDestroy)
		{
			lock (_lock)
			{
				_instance = (T)null;
			}
		}
	}
}
