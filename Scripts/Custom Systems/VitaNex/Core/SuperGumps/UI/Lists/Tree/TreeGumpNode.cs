#region Header
//   Vorspire    _,-'/-'/  TreeGumpNode.cs
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

namespace VitaNex.SuperGumps.UI
{
	public class TreeGumpNode : IEquatable<TreeGumpNode>, IEquatable<string>
	{
		public TreeGumpNode Parent { get; private set; }

		public string Name { get; private set; }
		public string FullName { get; private set; }

		public bool HasParent { get { return Parent != null; } }

		public bool IsRoot { get { return !HasParent; } }
		public bool IsEmpty { get { return String.IsNullOrWhiteSpace(FullName); } }

		public int Depth { get { return GetParents().Count(); } }

		public TreeGumpNode(string path)
		{
			FullName = path ?? String.Empty;

			var parents = FullName.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

			if (parents.Length == 0)
			{
				Name = FullName;
				return;
			}

			Name = parents.LastOrDefault() ?? FullName;

			if (parents.Length > 1)
			{
				Parent = new TreeGumpNode(String.Join("|", parents.Take(parents.Length - 1)));
			}
		}

		public bool IsChildOf(TreeGumpNode d)
		{
			if (d == null)
			{
				return false;
			}

			TreeGumpNode p = Parent;

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

		public IEnumerable<TreeGumpNode> GetParents()
		{
			TreeGumpNode c = this;

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
			return (obj is string && Equals((string)obj)) || (obj is TreeGumpNode && Equals((TreeGumpNode)obj));
		}

		public virtual bool Equals(TreeGumpNode other)
		{
			return !ReferenceEquals(other, null) && Equals(other.FullName);
		}

		public virtual bool Equals(string other)
		{
			return Insensitive.Equals(FullName, other);
		}

		public static bool operator ==(TreeGumpNode l, TreeGumpNode r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(TreeGumpNode l, TreeGumpNode r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}

		public static bool operator >(TreeGumpNode l, TreeGumpNode r)
		{
			return !ReferenceEquals(l, null) && !ReferenceEquals(r, null) && r.IsChildOf(l);
		}

		public static bool operator <(TreeGumpNode l, TreeGumpNode r)
		{
			return !ReferenceEquals(l, null) && !ReferenceEquals(r, null) && l.IsChildOf(r);
		}

		public static bool operator >=(TreeGumpNode l, TreeGumpNode r)
		{
			return l > r || l == r;
		}

		public static bool operator <=(TreeGumpNode l, TreeGumpNode r)
		{
			return l < r || l == r;
		}

		public static implicit operator TreeGumpNode(string path)
		{
			return new TreeGumpNode(path);
		}

		public static implicit operator string(TreeGumpNode path)
		{
			return ReferenceEquals(path, null) ? String.Empty : path.FullName;
		}
	}
}