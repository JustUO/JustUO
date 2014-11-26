#region Header
//   Vorspire    _,-'/-'/  ContainerExt.cs
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
using System.Linq;

using Server.Items;
#endregion

namespace Server
{
	public static class ContainerExtUtility
	{
		public static bool HasItem<TItem>(
			this Container container, int amount = 1, bool children = true, Func<TItem, bool> predicate = null)
			where TItem : Item
		{
			return predicate == null
					   ? HasItem(container, typeof(TItem), amount, children)
					   : HasItem(container, typeof(TItem), amount, children, i => predicate(i as TItem));
		}

		public static bool HasItem(
			this Container container, Type type, int amount = 1, bool children = true, Func<Item, bool> predicate = null)
		{
			if (container == null || type == null || amount < 1)
			{
				return false;
			}

			long total = 0;

			total =
				container.FindItemsByType(type, true)
						 .Where(i => i != null && !i.Deleted && i.TypeEquals(type, children) && (predicate == null || predicate(i)))
						 .Aggregate(total, (c, i) => c + (long)i.Amount);

			return total >= amount;
		}

		public static bool HasItems(
			this Container container, Type[] types, int[] amounts = null, bool children = true, Func<Item, bool> predicate = null)
		{
			if (container == null || types == null || types.Length == 0)
			{
				return false;
			}

			if (amounts == null)
			{
				amounts = new int[0];
			}

			int count = 0;

			for (int i = 0; i < types.Length; i++)
			{
				var t = types[i];
				int amount = amounts.InBounds(i) ? amounts[i] : 1;

				if (HasItem(container, t, amount, children, predicate))
				{
					++count;
				}
			}

			return count >= types.Length;
		}
	}
}