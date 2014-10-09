#region Header
//   Vorspire    _,-'/-'/  VirtualDirectory.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
#endregion

namespace VitaNex.IO
{
	public class VirtualDirectory : IEquatable<VirtualDirectory>, IEquatable<string>
	{
		public VirtualDirectory Parent { get; private set; }

		public string Name { get; private set; }
		public string FullName { get; private set; }

		public VirtualDirectory(string path)
		{
			FullName = path != null ? IOUtility.GetSafeDirectoryPath(path).TrimEnd(IOUtility.SEPARATOR) : String.Empty;

			var parents = FullName.Split(new[] {IOUtility.SEPARATOR}, StringSplitOptions.RemoveEmptyEntries);

			if (parents.Length == 0)
			{
				Name = FullName;
				return;
			}

			Name = parents[parents.Length - 1];

			if (parents.Length <= 1)
			{
				return;
			}

			string parent = String.Empty;

			parents.ForRange(0, parents.Length - 1, (i, s) => parent = s + IOUtility.SEPARATOR + parent);

			Parent = new VirtualDirectory(parent);
		}

		public override int GetHashCode()
		{
			return FullName.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null))
			{
				return false;
			}

			if (obj is string)
			{
				return FullName == (string)obj;
			}

			if (obj is VirtualDirectory)
			{
				return Equals((VirtualDirectory)obj);
			}

			return base.Equals(obj);
		}

		public bool Equals(string other)
		{
			return !ReferenceEquals(other, null) && FullName == other;
		}

		public bool Equals(VirtualDirectory other)
		{
			return !ReferenceEquals(other, null) && FullName == other.FullName;
		}

		public override string ToString()
		{
			return FullName;
		}

		public static implicit operator VirtualDirectory(string path)
		{
			return new VirtualDirectory(path);
		}

		public static implicit operator string(VirtualDirectory path)
		{
			return path.FullName;
		}
	}
}