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
using System.Collections.Generic;
using System.Linq;

using Server;
#endregion

namespace VitaNex.IO
{
	public class VirtualDirectory : IEquatable<VirtualDirectory>, IEquatable<string>
	{
		public VirtualDirectory Parent { get; private set; }

		public string Name { get; private set; }
		public string FullName { get; private set; }

		public bool HasParent { get { return Parent != null; } }

		public bool IsRoot { get { return !HasParent; } }
		public bool IsEmpty { get { return String.IsNullOrWhiteSpace(FullName); } }

		public int Depth { get { return GetParents().Count(); } }

		public VirtualDirectory(string path)
		{
			FullName = path != null ? IOUtility.GetSafeDirectoryPath(path).TrimEnd(IOUtility.SEPARATOR) : String.Empty;

			var parents = FullName.Split(new[] {IOUtility.SEPARATOR}, StringSplitOptions.RemoveEmptyEntries);

			if (parents.Length == 0)
			{
				Name = FullName;
				return;
			}

			Name = parents.LastOrDefault() ?? FullName;

			if (parents.Length > 1)
			{
				Parent = new VirtualDirectory(String.Join("/", parents.Take(parents.Length - 1)));
			}
		}

		public bool IsChildOf(VirtualDirectory d)
		{
			if (d == null)
			{
				return false;
			}

			VirtualDirectory p = Parent;

			while (p != null)
			{
				if (p == d)
				{
					return true;
				}

				p = p.Parent;
			}

			return false;
		}

		public IEnumerable<VirtualDirectory> GetParents()
		{
			VirtualDirectory c = this;

			while (c.HasParent)
			{
				c = c.Parent;

				yield return c;
			}
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = FullName.Length;
				hash = (hash * 397) ^ FullName.ToLower().GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			return (obj is string && Equals((string)obj)) || (obj is VirtualDirectory && Equals((VirtualDirectory)obj));
		}

		public virtual bool Equals(VirtualDirectory other)
		{
			return !ReferenceEquals(other, null) && Equals(other.FullName);
		}

		public virtual bool Equals(string other)
		{
			return Insensitive.Equals(
				FullName, other != null ? IOUtility.GetSafeDirectoryPath(other).TrimEnd(IOUtility.SEPARATOR) : String.Empty);
		}

		public static bool operator ==(VirtualDirectory l, VirtualDirectory r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(VirtualDirectory l, VirtualDirectory r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}

		public static implicit operator VirtualDirectory(string path)
		{
			return new VirtualDirectory(path);
		}

		public static implicit operator string(VirtualDirectory path)
		{
			return ReferenceEquals(path, null) ? String.Empty : path.FullName;
		}
	}
}