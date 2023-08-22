namespace UnityEngine.Purchasing
{
	internal static class VersionCheck
	{
		internal struct Version
		{
			public int major;

			public int minor;

			public int patch;
		}

		public static bool GreaterThanOrEqual(string versionA, string versionB)
		{
			return !LessThan(versionA, versionB);
		}

		public static bool GreaterThan(string versionA, string versionB)
		{
			return !LessThanOrEqual(versionA, versionB);
		}

		public static bool LessThan(string versionA, string versionB)
		{
			Version version = Parse(versionA);
			Version version2 = Parse(versionB);
			if (version.major > version2.major)
			{
				return false;
			}
			if (version.major < version2.major)
			{
				return true;
			}
			if (version.minor > version2.minor)
			{
				return false;
			}
			if (version.minor < version2.minor)
			{
				return true;
			}
			if (version.patch > version2.patch)
			{
				return false;
			}
			if (version.patch < version2.patch)
			{
				return true;
			}
			return false;
		}

		public static bool LessThanOrEqual(string versionA, string versionB)
		{
			return LessThan(versionA, versionB) || !LessThan(versionB, versionA);
		}

		public static bool Equal(string versionA, string versionB)
		{
			return !LessThan(versionA, versionB) && !LessThan(versionB, versionA);
		}

		public static int MajorVersion(string version)
		{
			return PartialVersion(version, 0);
		}

		public static int MinorVersion(string version)
		{
			return PartialVersion(version, 1);
		}

		public static int PatchVersion(string version)
		{
			return PartialVersion(version, 2);
		}

		public static Version Parse(string version)
		{
			Version result = default(Version);
			result.major = MajorVersion(version);
			result.minor = MinorVersion(version);
			result.patch = PatchVersion(version);
			return result;
		}

		private static int PartialVersion(string version, int index)
		{
			string[] array = version.Split('a', 'b', 'f', 'p');
			string text = array[0];
			int result = 0;
			string[] array2 = text.Split('.');
			if (array2.Length < index + 1)
			{
				return result;
			}
			int.TryParse(array2[index], out result);
			return result;
		}
	}
}
