#region References

using System;
using System.Collections;
using Server.Items;
using Server.Mobiles;

#endregion

namespace Server.Engines.XmlSpawner2
{
	public class ReferralRewardItems
	{
		public int Cost; // cost of the reward in credits
		public Type RewardType; // this will be used to create an instance of the reward
		public string Name; // used to describe the reward in the gump
		public int ItemID; // used for display purposes
		public object[] RewardArgs; // arguments passed to the reward constructor
		public string Description; // Used for the seperate item gump.
		public int Hue; // Used for item image hue

		private static readonly ArrayList ReferralRewardList = new ArrayList();

		public static ArrayList RewardsList
		{
			get { return ReferralRewardList; }
		}

		public ReferralRewardItems(Type reward, string name, int cost, int id, object[] args, string description, int hue)
		{
			RewardType = reward;
			Cost = cost;
			ItemID = id;
			Name = name;
			RewardArgs = args;
			Description = description;

			Hue = hue;
		}

		public ReferralRewardItems(Type reward, string name, int cost, int id, object[] args, string description)
		{
			RewardType = reward;
			Cost = cost;
			ItemID = id;
			Name = name;
			RewardArgs = args;
			Description = description;

			Hue = 0;
		}

		public ReferralRewardItems(Type reward, string name, int cost, int id, object[] args)
		{
			RewardType = reward;
			Cost = cost;
			ItemID = id;
			Name = name;
			RewardArgs = args;
			Description = "";

			Hue = 0;
		}

		public ReferralRewardItems(Type reward, string name, int cost, int id, object[] args, int hue)
		{
			RewardType = reward;
			Cost = cost;
			ItemID = id;
			Name = name;
			RewardArgs = args;
			Description = "";

			Hue = hue;
		}

		public static void Initialize()
		{
		    if (!TellAFriend.Enabled)
		    {
		        return;
		    }

		    //Add your own using the examples below.

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealHorse), "Ethereal Horse", 2, 0x20DD, null,
		        "An ethereal horse."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealLlama), "Ethereal Llama", 2, 0x20F6, null,
		        "An ethereal llama."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealOstard), "Ethereal Ostard", 2, 0x2135, null,
		        "An ethereal ostard."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Crocodile Statuette", 2, 0x20DA,
		        new object[] {MonsterStatuetteType.Crocodile}, "A crocodile statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Daemon Statuette", 2, 0x20D3,
		        new object[] {MonsterStatuetteType.Daemon}, "A daemon statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Dragon Statuette", 2, 0x20D6,
		        new object[] {MonsterStatuetteType.Dragon}, "A dragon statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Earth Elemental Statuette", 2,
		        0x20D7,
		        new object[] {MonsterStatuetteType.EarthElemental},
		        "A earth elemental statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Ettin Statuette", 2, 0x20D8,
		        new object[] {MonsterStatuetteType.Ettin}, "An ettin statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Gargoyle Statuette", 2, 0x20D9,
		        new object[] {MonsterStatuetteType.Gargoyle}, "A gargoyle statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Gorilla Statuette", 2, 0x20F5,
		        new object[] {MonsterStatuetteType.Gorilla}, "A gorilla statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Lich Statuette", 2, 0x20F8,
		        new object[] {MonsterStatuetteType.Lich}, "A lich statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Lizardman Statuette", 2, 0x20DE,
		        new object[] {MonsterStatuetteType.Lizardman}, "A lizardman statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Ogre Statuette", 2, 0x20DF,
		        new object[] {MonsterStatuetteType.Ogre}, "An ogre statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Orc Statuette", 2, 0x20E0,
		        new object[] {MonsterStatuetteType.Orc}, "An orc statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Ratman Statuette", 2, 0x20E3,
		        new object[] {MonsterStatuetteType.Ratman}, "A ratman statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Skeleton Statuette", 2, 0x20E7,
		        new object[] {MonsterStatuetteType.Skeleton}, "A skeleton statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Troll Statuette", 2, 0x20E9,
		        new object[] {MonsterStatuetteType.Troll}, "A troll statuette that makes sounds when walked by."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (BlackDyeTub), "Black Dye Tub", 3, 0xFAB, null,
		        "A pure black dye tub.", 01));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (SpecialDyeTub), "Special Dye Tub", 3, 0xFAB, null,
		        "New colors to use on cloth that are not available on a normal dye tub."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (FurnitureDyeTub), "Furniture Dye Tub", 3, 0xFAB, null,
		        "Used to dye furniture."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (LeatherDyeTub), "Leather Dye Tub", 4, 0xFAB, null,
		        "Dye leather armor a variety of colors."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (FlamingHeadDeed), "Flaming Head Deed", 4, 0x14F0, null,
		        "A skull that you place in your house that shoots fire."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (BannerDeed), "Banner Deed", 4, 0x14F0, null,
		        "A variety of banner styles to spruce up your home."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (PottedCactusDeed), "Potted Cactus Deed", 4, 0x14F0,
		        null,
		        "One of six possible varieties of cactus plants in a pot."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RunebookDyeTub), "Runebook Dye Tub", 4, 0xFAB, null,
		        "Remember the contents of a Runebook easier with color coding."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (DecorativeShieldDeed), "Decorative Shield Deed", 4,
		        0x14F0,
		        null, "A decorative shield for your home."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (HangingSkeletonDeed), "Hanging Skeleton Deed", 4,
		        0x14F0, null,
		        "A choice of many styles of hanging skeletons to place in your home."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (StoneAnkhDeed), "Stone Ankh Deed", 5, 0x14F0, null,
		        "A decorative ankh to place in your home."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (TreeStumpDeed), "Tree Stump Deed", 5, 0x14F0, null,
		        "A stump from a cut-down tree, faces two directions, with and without an axe."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Cow Statuette", 5, 0x2103,
		        new object[] {MonsterStatuetteType.Cow}, "A cow statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Zombie Statuette", 5, 0x20EC,
		        new object[] {MonsterStatuetteType.Zombie}, "A zombie statuette that makes sounds when walked by."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MonsterStatuette), "Llama Statuette", 5, 0x20F6,
		        new object[] {MonsterStatuetteType.Llama}, "A llama statuette that makes sounds when walked by."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RewardRobe), "Bronze Robe", 6, 0x1F03,
		        new object[] {0x972, 1041287}, "Reward Robes come blessed and add 2 physical resist to your character.",
		        0x972));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RewardRobe), "Copper Robe", 6, 0x1F03,
		        new object[] {0x96D, 1041289}, "Reward Robes come blessed and add 2 physical resist to your character.",
		        0x96D));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RewardRobe), "Agapite Robe", 6, 0x1F03,
		        new object[] {0x979, 1041291}, "Reward Robes come blessed and add 2 physical resist to your character.",
		        0x979));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RewardRobe), "Golden Robe", 6, 0x1F03,
		        new object[] {0x8A5, 1041293}, "Reward Robes come blessed and add 2 physical resist to your character.",
		        0x8A5));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RewardRobe), "Verite Robe", 6, 0x1F03,
		        new object[] {0x89F, 1041295}, "Reward Robes come blessed and add 2 physical resist to your character.",
		        0x89F));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (RewardRobe), "Valorite Robe", 6, 0x1F03,
		        new object[] {0x8AB, 1041297}, "Reward Robes come blessed and add 2 physical resist to your character.",
		        0x8AB));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (BloodyPentagramDeed), "Bloody Pentagram Deed", 7,
		        0x14F0, null,
		        "A bloody pentagram to place on the floor of your home."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (WallBannerDeed), "Wall Banner Deed", 7, 0x14F0, null,
		        "An attractive, large banner which can be hung on a wall and dyed with 15 different design choices.."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (AnkhOfSacrificeDeed), "Ankh of Sacrifice Deed", 7,
		        0x14F0,
		        null, "A house addon in the form of a bloodied Ankh that can be used to self-resurrect one per hour."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (MiningCartDeed), "Mining Cart Deed", 7, 0x14F0, null,
		        "A house add-on that randomly generates 10 ingots or 5 gems once a day."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealRidgeback), "Ethereal Ridgeback", 8, 0x2615,
		        null,
		        "An ethereal ridgeback."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealKirin), "Ethereal Kirin", 8, 0x25A0, null,
		        "An ethereal kirin."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealUnicorn), "Ethereal Unicorn", 8, 0x25CE, null,
		        "An ethereal unicorn."));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealSwampDragon), "Ethereal Swamp Dragon", 8,
		        0x2619, null,
		        "An ethereal swamp dragon."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (LuckyNecklace), "Lucky Necklace", 10, 0x1088, null,
		        "A blessed necklace with 200 luck.", 1153));
		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealBeetle), "Ethereal Beetle", 10, 0x260F, null,
		        "An ethereal beetle with no account age requirements."));

		    ReferralRewardList.Add(new ReferralRewardItems(typeof (EtherealHiryu), "Ethereal Hiryu", 20, 0x276A, null,
		        "An ethereal hiryu."));
		}
	}
}