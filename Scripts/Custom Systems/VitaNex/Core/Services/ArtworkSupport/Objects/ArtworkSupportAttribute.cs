#region Header
//   Vorspire    _,-'/-'/  ArtworkSupportAttribute.cs
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

using Server;
#endregion

namespace VitaNex
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ArtworkSupportAttribute : Attribute
	{
		public ClientVersion Version { get; private set; }
		public Pair<int, int>[] ItemIDs { get; private set; }

		public ArtworkSupportAttribute(int oldItemID, int newItemID)
			: this(ArtworkSupport.DefaultVersion, oldItemID, newItemID)
		{ }

		public ArtworkSupportAttribute(int[] oldItemIDs, int newItemID)
			: this(ArtworkSupport.DefaultVersion, oldItemIDs, newItemID)
		{ }

		public ArtworkSupportAttribute(string version, int oldItemID, int newItemID)
			: this(new ClientVersion(version), oldItemID, newItemID)
		{ }

		public ArtworkSupportAttribute(string version, int[] oldItemIDs, int newItemID)
			: this(new ClientVersion(version), oldItemIDs, newItemID)
		{ }

		public ArtworkSupportAttribute(ClientVersion version, int oldItemID, int newItemID)
			: this(version, new[] {Pair.Create(oldItemID, newItemID)})
		{ }

		public ArtworkSupportAttribute(ClientVersion version, int[] oldItemIDs, int newItemID)
			: this(
				version,
				oldItemIDs != null && oldItemIDs.Length > 0
					? oldItemIDs.Select(id => Pair.Create(id, newItemID)).ToArray()
					: new Pair<int, int>[0])
		{ }

		private ArtworkSupportAttribute(ClientVersion version, Pair<int, int>[] itemIDs)
		{
			Version = version ?? ArtworkSupport.DefaultVersion;
			ItemIDs = itemIDs ?? new Pair<int, int>[0];
		}
	}
}