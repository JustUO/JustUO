using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Engines.Harvest;
using Server.Items;
using Server.Regions;

namespace Server.Mobiles
{
    [CorpseName("an Iron beetle corpse")]
    public class IronBeetle : BaseCreature
    {
        private static readonly int[] m_MountainAndCaveTiles =
        {
            113, 114, 115, 116, 117, 118, 119, 120,
            220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
            230, 231, 236, 237, 238, 239, 240, 241, 242, 243,
            244, 245, 246, 247, 252, 253, 254, 255, 256, 257,
            258, 259, 260, 261, 262, 263, 268, 269, 270, 271,
            272, 273, 274, 275, 276, 277, 278, 279, 286, 287,
            288, 289, 290, 291, 292, 293, 294, 296, 296, 297,
            321, 322, 323, 324, 467, 468, 469, 470, 471, 472,
            473, 474, 476, 477, 478, 479, 480, 481, 482, 483,
            484, 485, 486, 487, 492, 493, 494, 495, 543, 544,
            545, 546, 547, 548, 549, 550, 551, 552, 553, 554,
            555, 556, 557, 558, 559, 560, 561, 562, 563, 564,
            565, 566, 567, 568, 569, 570, 571, 572, 573, 574,
            575, 576, 577, 578, 579, 581, 582, 583, 584, 585,
            586, 587, 588, 589, 590, 591, 592, 593, 594, 595,
            596, 597, 598, 599, 600, 601, 610, 611, 612, 613,
            1010, 1741, 1742, 1743, 1744, 1745, 1746, 1747, 1748, 1749,
            1750, 1751, 1752, 1753, 1754, 1755, 1756, 1757, 1771, 1772,
            1773, 1774, 1775, 1776, 1777, 1778, 1779, 1780, 1781, 1782,
            1783, 1784, 1785, 1786, 1787, 1788, 1789, 1790, 1801, 1802,
            1803, 1804, 1805, 1806, 1807, 1808, 1809, 1811, 1812, 1813,
            1814, 1815, 1816, 1817, 1818, 1819, 1820, 1821, 1822, 1823,
            1824, 1831, 1832, 1833, 1834, 1835, 1836, 1837, 1838, 1839,
            1840, 1841, 1842, 1843, 1844, 1845, 1846, 1847, 1848, 1849,
            1850, 1851, 1852, 1853, 1854, 1861, 1862, 1863, 1864, 1865,
            1866, 1867, 1868, 1869, 1870, 1871, 1872, 1873, 1874, 1875,
            1876, 1877, 1878, 1879, 1880, 1881, 1882, 1883, 1884, 1981,
            1982, 1983, 1984, 1985, 1986, 1987, 1988, 1989, 1990, 1991,
            1992, 1993, 1994, 1995, 1996, 1997, 1998, 1999, 2000, 2001,
            2002, 2003, 2004, 2028, 2029, 2030, 2031, 2032, 2033, 2100,
            2101, 2102, 2103, 2104, 2105,
            0x453B, 0x453C, 0x453D, 0x453E, 0x453F, 0x4540, 0x4541,
            0x4542, 0x4543, 0x4544, 0x4545, 0x4546, 0x4547, 0x4548,
            0x4549, 0x454A, 0x454B, 0x454C, 0x454D, 0x454E, 0x454F
        };

        public bool m_Mine;

        [Constructable]
        public IronBeetle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an Iron beetle";
            Body = 714;
            BaseSoundID = 397;

            SetStr(831);
            SetDex(74, 80);
            SetInt(45);

            SetHits(801);
            SetMana(45);

            SetDamage(15, 20);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 55, 60);
            SetResistance(ResistanceType.Fire, 20, 25);
            SetResistance(ResistanceType.Cold, 20, 25);
            SetResistance(ResistanceType.Poison, 35, 40);
            SetResistance(ResistanceType.Energy, 50, 60);

            SetSkill(SkillName.Anatomy, 85, 90);
            SetSkill(SkillName.MagicResist, 30.1, 35);
            SetSkill(SkillName.Tactics, 91.5, 92);
            SetSkill(SkillName.Wrestling, 99.8, 100);
            SetSkill(SkillName.Blacksmith, 120.0);
            SetSkill(SkillName.Mining, 50);
            Skills.Mining.Cap = 120;

            Fame = 2000;
            Karma = -2000;

            VirtualArmor = 38;

            Tamable = true;
            ControlSlots = 4;
            MinTameSkill = 71.1;

            QLPoints = 20;

            var pack = Backpack;
            if (pack != null) pack.Delete();
            pack = new StrongBackpack();
            pack.Movable = false;
            AddItem(pack);

            AddItem(new Gold(600));
            PackItem(new IronOre(5));
        }

        public IronBeetle(Serial serial) : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextUse { get; set; }

        public override bool SubdueBeforeTame
        {
            get { return true; }
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override FoodType FavoriteFood
        {
            get { return FoodType.Meat; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.Gems);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.1)
                c.DropItem(new UndamagedIronBeetleScale());

            if (Utility.RandomDouble() < 0.15)
                c.DropItem(new IBShovel());
        }

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            return DeathMoveResult.MoveToCorpse;
        }

        public override bool AllowEquipFrom(Mobile from)
        {
            return (ControlMaster != null && ControlMaster == from);
        }

        public override bool CanBeRenamedBy(Mobile from)
        {
            return (ControlMaster != null && ControlMaster == from);
        }

        public override bool CanPaperdollBeOpenedBy(Mobile from)
        {
            return false;
        }

        public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
        {
            return PackAnimal.CheckAccess(this, from);
        }

        public override bool CheckNonlocalLift(Mobile from, Item item)
        {
            return PackAnimal.CheckAccess(this, from);
        }

        public override bool OnBeforeDeath()
        {
            if (!base.OnBeforeDeath()) return false;
            PackAnimal.CombineBackpacks(this);
            return base.OnBeforeDeath();
        }

        public override bool IsSnoop(Mobile from)
        {
            if (PackAnimal.CheckAccess(this, from)) return false;
            return base.IsSnoop(from);
        }

        public override bool OnDragDrop(Mobile from, Item item)
        {
            Skills.Mining.Cap = 120;
            if (CheckFeed(from, item)) return true;
            if (PackAnimal.CheckAccess(this, from))
            {
                if (item is BaseOre)
                {
                    var m_Ore = item as BaseOre;

                    var toConsume = m_Ore.Amount;

                    if (toConsume > 30000)
                    {
                        toConsume = 30000;
                    }
                    else if (m_Ore.Amount < 2)
                    {
                        from.SendLocalizedMessage(501989);
                            // You burn away the impurities but are left with no useable metal.
                        m_Ore.Delete();
                        return true;
                    }

                    var ingot = m_Ore.GetIngot();
                    ingot.Amount = toConsume*2;

                    m_Ore.Consume(toConsume);

                    Hue = m_Ore.Hue;

                    from.PlaySound(0x57);

                    AddToBackpack(item);
                    AddToBackpack(ingot);
                    return true;
                }
            }

            return base.OnDragDrop(from, item);
        }

        public override void OnSpeech(SpeechEventArgs args)
        {
            var said = args.Speech.ToLower();
            var from = args.Mobile;
            var bmine = Name + " " + "mine";
            var bmine2 = bmine.ToLower();
            var bbreak = Name + " " + "takebreak";
            var bbreak2 = bbreak.ToLower();
            var bbreak3 = Name + " " + "take break";
            var bbreak4 = bbreak3.ToLower();
            if (said == Name.ToLower())
            {
                if (from == ControlMaster)
                {
                    Say("Yes?");
                }
                else
                {
                    Say("You are not my master.");
                }
            }

            else if ((said == "mine" || said == bmine2) && from == ControlMaster)
            {
                m_Mine = true;
                ControlOrder = OrderType.Stay;
            }
            else if ((said == "takebreak" || said == "take break" || said == bbreak2 || said == bbreak4) &&
                     from == ControlMaster) m_Mine = false;
            base.OnSpeech(args);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            PackAnimal.GetContextMenuEntries(this, from, list);
        }

        public override void OnThink()
        {
            if (Region.IsPartOf(typeof (HouseRegion))) NextUse = DateTime.Now + TimeSpan.FromSeconds(2.5);
            else if (Alive && m_Mine && NextUse <= DateTime.Now && Loyalty > 50)
            {
                var backpack = Backpack;
                if (backpack == null)
                {
                    NextUse = DateTime.Now + TimeSpan.FromSeconds(2.5);
                    base.OnThink();
                    return;
                }
                if (backpack.TotalWeight >= backpack.MaxWeight || backpack.TotalItems >= backpack.MaxItems)
                {
                    NextUse = DateTime.Now + TimeSpan.FromSeconds(2.5);
                    base.OnThink();
                    return;
                }
                var shovel = (Shovel) backpack.FindItemByType(typeof (Shovel));
                //Shovel shovel = (Shovel)backpack.FindItemByType(typeof(Shovel));
                if (shovel != null)
                {
                    if (DoDisposeOre(X, Y, Z, Map, this))
                    {
                        shovel.UsesRemaining -= 1;
                        if (shovel != null && !shovel.Deleted && shovel.UsesRemaining <= 0) shovel.Delete();
                    }
                }
                else if (shovel != null)
                {
                    if (DoDisposeOre(X, Y, Z, Map, this))
                    {
                        shovel.UsesRemaining -= 1;
                        if (shovel != null && !shovel.Deleted && shovel.UsesRemaining <= 0) shovel.Delete();
                    }
                }
                NextUse = DateTime.Now + TimeSpan.FromSeconds(2.5);
            }
            base.OnThink();
        }

        public bool DoDisposeOre(int x, int y, int z, Map map, Mobile from)
        {
            if (!IsMiningTile(x, y, map))

            {
                Say("I can not mine here");
                return false;
            }
            var bank = Mining.System.OreAndStone.GetBank(map, x, y);
            if (bank == null)
            {
                Say("I can not mine here");
                return false;
            }
            if (bank.Current <= 0) return false;
            var vein = bank.DefaultVein;
            if (vein == null)
            {
                Say("I can not mine here");
                return false;
            }
            var def = Mining.System.OreAndStone;
            var res = vein.PrimaryResource;
            var ore = Mining.System.Construct(res.Types[0], null) as BaseOre;
            if (ore == null) return false;
            if (ore.Resource > CraftResource.Iron)
            {
                var minskill = 0.0;
                var minskill2 = 0.0;
                var maxskill = 0.0;
                var skillbase = Skills.Mining.Base;
                if (Skills.Mining.Base < 120.0) Say("skill = {0}", Convert.ToString(skillbase));
                switch (ore.Resource)
                {
                    case CraftResource.Iron:
                    {
                        minskill = 00.0;
                        minskill2 = 00.0;
                        maxskill = 100.0;
                    }
                        break;
                    case CraftResource.DullCopper:
                    {
                        minskill = 60.0;
                        minskill2 = 25.0;
                        maxskill = 105.0;
                    }
                        break;
                    case CraftResource.ShadowIron:
                    {
                        minskill = 65.0;
                        minskill2 = 30.0;
                        maxskill = 110.0;
                    }
                        break;
                    case CraftResource.Copper:
                    {
                        minskill = 70.0;
                        minskill2 = 35.0;
                        maxskill = 115.0;
                    }
                        break;
                    case CraftResource.Gold:
                    {
                        minskill = 75.0;
                        minskill2 = 40.0;
                        maxskill = 120.0;
                    }
                        break;
                    case CraftResource.Agapite:
                    {
                        minskill = 80.0;
                        minskill2 = 45.0;
                        maxskill = 120.0;
                    }
                        break;
                    case CraftResource.Verite:
                    {
                        minskill = 85.0;
                        minskill2 = 50.0;
                        maxskill = 120.0;
                    }
                        break;
                    case CraftResource.Valorite:
                    {
                        minskill = 90.0;
                        minskill2 = 55.0;
                        maxskill = 120.0;
                    }
                        break;
                }
                if (Utility.RandomDouble() <= 0.30 || skillbase < minskill)
                {
                    ore = new IronOre();
                    minskill = 00.0;
                    minskill2 = 00.0;
                    maxskill = 100.0;
                }
                if (!(from.CheckSkill(SkillName.Mining, minskill2, maxskill)))
                {
                    ore.Delete();
                    return false;
                }
            }
            ore.Amount = (map == Map.Felucca ? 2 : 1);
            if (from != null) from.AddToBackpack(ore);
            else ore.Delete();
            bank.Consume(ore.Amount, this);
            Hue = ore.Hue;

            return true;
        }

        private bool IsMiningTile(int X, int Y, Map map)
        {
            var list = map.Tiles.GetLandTile(X, Y);
            //Only for testing
            //Console.WriteLine( "Check Tiles {0}", list );
            //Console.WriteLine( "StaticTile {0}", list.ID );
            for (var l = 0; l < m_MountainAndCaveTiles.Length; l++)
            {
                if (m_MountainAndCaveTiles[l] == list.ID) return true;
            }
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}