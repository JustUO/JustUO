using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class AcidSac : Item
    {
        [Constructable]
        public AcidSac()
            : base(0x0C67)
        {
            Stackable = true;
            Weight = 1.0;
            Hue = 648;
        }

        public AcidSac(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1111654; }
        } // acid sac

        public override void OnDoubleClick(Mobile from)
        {
            from.SendLocalizedMessage(1111656); // What do you wish to use the acid on?

            from.Target = new InternalTarget(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }

        private class InternalTarget : Target
        {
            private Item wall;
            private Item wallandvine;
            private readonly Item m_Item;

            public InternalTarget(Item item)
                : base(2, false, TargetFlags.None)
            {
                m_Item = item;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                var pm = from as PlayerMobile;

                if (m_Item.Deleted)
                    return;

                if (targeted is AddonComponent)
                {
                    var addoncomponent = (AddonComponent) targeted;

                    if (addoncomponent is MagicVinesComponent || addoncomponent is StoneWallComponent ||
                        addoncomponent is DungeonWallComponent)
                    {
                        var Xs = addoncomponent.X;

                        if (addoncomponent is MagicVinesComponent)
                            Xs += -1;

                        if (addoncomponent.Addon is StoneWallAndVineAddon)
                        {
                            wall = new SecretStoneWallNS();
                            wallandvine = new StoneWallAndVineAddon();
                        }
                        else if (addoncomponent.Addon is DungeonWallAndVineAddon)
                        {
                            wall = new SecretDungeonWallNS();
                            wallandvine = new DungeonWallAndVineAddon();
                        }

                        wall.MoveToWorld(new Point3D(Xs, addoncomponent.Y, addoncomponent.Z), addoncomponent.Map);

                        addoncomponent.Delete();

                        m_Item.Consume();

                        wall.PublicOverheadMessage(0, 1358, 1111662);
                            // The acid quickly burns through the writhing wallvines, revealing the strange wall.

                        Timer.DelayCall(TimeSpan.FromSeconds(15.0), delegate
                        {
                            this.wallandvine.MoveToWorld(this.wall.Location, this.wall.Map);

                            this.wall.Delete();
                            this.wallandvine.PublicOverheadMessage(0, 1358, 1111663);
                                // The vines recover from the acid and, spreading like tentacles, reclaim their grip over the wall.
                        });
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1111657); // The acid swiftly burn through it.
                    m_Item.Consume();
                }
            }
        }
    }

    public class AncientPotteryFragments : Item
    {
        [Constructable]
        public AncientPotteryFragments()
            : this(1)
        {
        }

        [Constructable]
        public AncientPotteryFragments(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public AncientPotteryFragments(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112990; }
        } // Ancient Pottery fragments

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class BouraPelt : Item
    {
        [Constructable]
        public BouraPelt()
            : this(1)
        {
        }

        [Constructable]
        public BouraPelt(int amount)
            : base(0x5742)
        {
            Stackable = true;
            Amount = amount;
        }

        public BouraPelt(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113355; }
        } // boura pelt

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class ClawSlasherVeils : Item
    {
        [Constructable]
        public ClawSlasherVeils()
            : this(1)
        {
        }

        [Constructable]
        public ClawSlasherVeils(int amount)
            : base(0x2DB8)
        {
            Stackable = true;
            Amount = amount;
        }

        public ClawSlasherVeils(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1031704; }
        } // Claw of Slasher of Veils

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class CongealedSlugAcid : Item
    {
        [Constructable]
        public CongealedSlugAcid()
            : this(1)
        {
        }

        [Constructable]
        public CongealedSlugAcid(int amount)
            : base(0x5742)
        {
            Stackable = true;
            Amount = amount;
        }

        public CongealedSlugAcid(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112901; }
        } // Congealed Slug Acid

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class EnchantEssence : Item
    {
        [Constructable]
        public EnchantEssence()
            : this(1)
        {
        }

        [Constructable]
        public EnchantEssence(int amount)
            : base(0x2DB2)
        {
            Stackable = true;
            Amount = amount;
        }

        public EnchantEssence(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1031698; }
        } // Enchaned Essence

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class FairyDragonWing : Item
    {
        [Constructable]
        public FairyDragonWing()
            : this(1)
        {
        }

        [Constructable]
        public FairyDragonWing(int amount)
            : base(0x5726)
        {
            Stackable = true;
            Amount = amount;
        }

        public FairyDragonWing(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112899; }
        } // Fairy Dragon Wing

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class LeatherWolfSkin : Item
    {
        [Constructable]
        public LeatherWolfSkin()
            : this(1)
        {
        }

        [Constructable]
        public LeatherWolfSkin(int amount)
            : base(0x3189)
        {
            Stackable = true;
            Amount = amount;
        }

        public LeatherWolfSkin(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112906; }
        } // leather wolf skin

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class MagicalResidue : Item
    {
        [Constructable]
        public MagicalResidue()
            : this(1)
        {
        }

        [Constructable]
        public MagicalResidue(int amount)
            : base(0x2DB1)
        {
            Stackable = true;
            Amount = amount;
        }

        public MagicalResidue(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1031697; }
        } // Magical Residue

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class PileInspectedIngots : Item
    {
        [Constructable]
        public PileInspectedIngots()
            : this(1)
        {
        }

        [Constructable]
        public PileInspectedIngots(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public PileInspectedIngots(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class RelicFragment : Item
    {
        [Constructable]
        public RelicFragment()
            : this(1)
        {
        }

        [Constructable]
        public RelicFragment(int amount)
            : base(0x2DB3)
        {
            Stackable = true;
            Amount = amount;
        }

        public RelicFragment(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1031699; }
        } // Relic Fragment

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class SearedFireAntGoo : Item
    {
        [Constructable]
        public SearedFireAntGoo()
            : this(1)
        {
        }

        [Constructable]
        public SearedFireAntGoo(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public SearedFireAntGoo(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112902; }
        } // Seared Fire Ant Goo

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class StygianDragonHead : Item
    {
        [Constructable]
        public StygianDragonHead()
            : this(1)
        {
        }

        [Constructable]
        public StygianDragonHead(int amount)
            : base(0x2DB4)
        {
            Stackable = true;
            Amount = amount;
        }

        public StygianDragonHead(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1031700; }
        } // Stygian Dragon Head

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class TatteredAncientScroll : Item
    {
        [Constructable]
        public TatteredAncientScroll()
            : this(1)
        {
        }

        [Constructable]
        public TatteredAncientScroll(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public TatteredAncientScroll(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112991; }
        } // Tattered Remnants of an Ancient Scroll

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class UndamagedIronBeetleScale : Item
    {
        [Constructable]
        public UndamagedIronBeetleScale()
            : this(1)
        {
        }

        [Constructable]
        public UndamagedIronBeetleScale(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public UndamagedIronBeetleScale(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112905; }
        } // Undamaged Iron Beetle Scale

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class UndeadGargHorn : Item
    {
        [Constructable]
        public UndeadGargHorn()
            : this(1)
        {
        }

        [Constructable]
        public UndeadGargHorn(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public UndeadGargHorn(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112903; }
        } // Undamaged Undead Gargoyle Horns

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class UndeadGargMedallion : Item
    {
        [Constructable]
        public UndeadGargMedallion()
            : this(1)
        {
        }

        [Constructable]
        public UndeadGargMedallion(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public UndeadGargMedallion(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112907; }
        } // Undead Gargoyle Medallion

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class UntransTome : Item
    {
        [Constructable]
        public UntransTome()
            : this(1)
        {
        }

        [Constructable]
        public UntransTome(int amount)
            : base(0x2F5F)
        {
            Stackable = true;
            Amount = amount;
        }

        public UntransTome(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112992; }
        } // Untranslated Ancient Tome

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}