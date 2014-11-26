#region Header
//   Vorspire    _,-'/-'/  SuperGump_Tree.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Linq;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public List<SuperGump> Children { get; private set; }

		public bool HasChildren { get { return (Children != null && Children.Count > 0); } }

		protected void AddChild(SuperGump child)
		{
			if (child == null)
			{
				return;
			}

			//child._Parent = this;

			if (Children.Contains(child))
			{
				return;
			}

			Children.Add(child);
			OnChildAdded(child);
		}

		protected void RemoveChild(SuperGump child)
		{
			if (child == null)
			{
				return;
			}

			//child._Parent = null;

			if (Children.Remove(child))
			{
				OnChildRemoved(child);
			}
		}

		public bool HasChild(SuperGump child)
		{
			return HasChild(child, false);
		}

		public bool HasChild(SuperGump child, bool tree)
		{
			return child != null && Children.Any(c => c == child || (tree && c.HasChild(child, true)));
		}

		public bool IsChildOf(SuperGump parent)
		{
			return IsChildOf(parent, false);
		}

		public bool IsChildOf(SuperGump parent, bool tree)
		{
			if (parent == null)
			{
				return false;
			}

			var p = Parent;

			while (p != null)
			{
				if (p == parent)
				{
					return true;
				}

				p = !tree || !(p is SuperGump) ? null : ((SuperGump)p).Parent;
			}

			return false;
		}

		protected virtual void OnChildAdded(SuperGump child)
		{ }

		protected virtual void OnChildRemoved(SuperGump child)
		{ }
	}
}