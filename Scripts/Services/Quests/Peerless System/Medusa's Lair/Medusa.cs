using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a medusa corpse")]
    public class Medusa : BaseSABosses, ICarvable
    {
        public static List<Mobile> AffectedMobiles = new List<Mobile>();
        //private readonly DateTime m_Delay;
        private int m_Scales;
        private DateTime m_GazeDelay;
        private DateTime m_DelayOne;
        private DateTime m_DelayTwo;
        [Constructable]
        public Medusa()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Medusa";
            Body = 728;

            SetStr(1235, 1391);
            SetDex(128, 139);
            SetInt(537, 664);

            SetHits(170000);

            SetDamage(21, 28);

            SetDamageType(ResistanceType.Physical, 60);
            SetDamageType(ResistanceType.Fire, 20);
            SetDamageType(ResistanceType.Energy, 20);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 55, 65);
            SetResistance(ResistanceType.Cold, 55, 65);
            SetResistance(ResistanceType.Poison, 80, 90);
            SetResistance(ResistanceType.Energy, 60, 75);

            SetSkill(SkillName.Anatomy, 110.6, 116.1);
            SetSkill(SkillName.EvalInt, 100.0, 114.4);
            SetSkill(SkillName.Magery, 100.0);
            SetSkill(SkillName.Meditation, 118.2, 127.8);
            SetSkill(SkillName.MagicResist, 120.0);
            SetSkill(SkillName.Tactics, 111.9, 134.5);
            SetSkill(SkillName.Wrestling, 119.7, 128.9);

            Fame = 22000;
            Karma = -22000;

            VirtualArmor = 60;

            PackItem(new Arrow(Utility.RandomMinMax(600, 700)));

            IronwoodCompositeBow Bow = new IronwoodCompositeBow();
            Bow.Movable = false;
            AddItem(Bow);

            m_Scales = Utility.Random(5) + 1;
        }

        public Medusa(Serial serial)
            : base(serial)
        {
        }

        public override Type[] UniqueSAList
        {
            get
            {
                return new Type[] { typeof(Slither), typeof(IronwoodCompositeBow) };
            }
        }
        public override Type[] SharedSAList
        {
            get
            {
                return new Type[] { typeof(DemonBridleRing), typeof(PetrifiedSnake), typeof(StoneDragonsTooth), typeof(SummonersKilt), typeof(Venom) };
            }
        }
        public override bool IgnoreYoungProtection
        {
            get
            {
                return true;
            }
        }
        public override int Scales
        {
            get
            {
                return (Utility.Random(5) + 1);
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.MedusaLight;
            }
        }
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool GivesSAArtifact
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override bool BardImmune
        {
            get
            {
                return false;
            }
        }
        public override bool AllureImmune
        {
            get
            {
                return true;
            }
        }
        public override bool Unprovokable
        {
            get
            {
                return false;
            }
        }
        public override bool AreaPeaceImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return (0.8 >= Utility.RandomDouble() ? Poison.Deadly : Poison.Lethal);
            }
        }
        public static Mobile FindRandomMedusaTarget(Mobile from)
        {
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile m in from.GetMobilesInRange(12))
            {
                if (m != null && m != from && !(m is MedusaClone) && !(m is StoneMonster) && !(AffectedMobiles.Contains(m)) && !(m is BaseCreature))
                {
                    Item[] items = m.Backpack.FindItemsByType(typeof(GorgonLens));

                    if (items.Length == 0)
                    {
                        AffectedMobiles.Remove(m);
                        list.Add(m);
                    }
                    else
                    {
                        foreach (GorgonLens gorg in items)
                        {
                            AffectedMobiles.Add(m);
                            gorg.ConsumeUse(from);
                            list.Remove(m);
                            m.SendLocalizedMessage(1112599);  //Your Gorgon Lens deflect Medusa's petrifying gaze!

                            break;
                        }
                    }
                }

                if (m != null && m != from && !(m is MedusaClone) && !(m is StoneMonster) && !(AffectedMobiles.Contains(m)) && (m is BaseCreature))
                {
                    AffectedMobiles.Remove(m);
                    list.Add(m);
                }
            }

            if (list.Count == 0)
                return null;
            if (list.Count == 1)
                return list[0];

            return list[Utility.Random(list.Count)];
        }

        public override int GetIdleSound()
        {
            return 1557;
        }

        public override int GetAngerSound()
        {
            return 1554;
        }

        public override int GetHurtSound()
        {
            return 1556;
        }

        public override int GetDeathSound()
        {
            return 1555;
        }

        public void Carve(Mobile from, Item item)
        {
            if (m_Scales > 0 && from.Backpack != null)
            {
                new Blood(0x122D).MoveToWorld(Location, Map);
                from.AddToBackpack(new MedusaDarkScales(Utility.Random(3) + 1));
                from.SendLocalizedMessage(1114098); // You cut away some scales and put them in your backpack.
                Combatant = from;
                --m_Scales;
            }
        }

        public override void OnThink()
        {
            base.OnThink();

            if (DateTime.UtcNow > m_DelayOne)
            {
                AffectedMobiles.Clear();

                m_DelayOne = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 180));
            }

            if (DateTime.UtcNow > m_GazeDelay)
            {
                Mobile target = FindRandomMedusaTarget(this);
                //  Mobile target = Ability.FindRandomMedusaTarget(this);

                Map map = Map;

                if (map == null)
                    return;

                DoSpecialAbility(this);

                if ((target != null) && !(target is MedusaClone) && !(target is StoneMonster))
                {
                    BaseCreature clone = new MedusaClone(target);

                    bool validLocation = false;
                    Point3D loc = Location;

                    for (int j = 0; !validLocation && j < 10; ++j)
                    {
                        int x = X + Utility.Random(10) - 1;
                        int y = Y + Utility.Random(10) - 1;
                        int z = map.GetAverageZ(x, y);

                        if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                            loc = new Point3D(x, y, Z);
                        else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                            loc = new Point3D(x, y, z);
                    }

                    Effects.SendLocationEffect(loc, target.Map, 0x37B9, 10, 5);
                    clone.Frozen = clone.Blessed = true;
                    clone.SolidHueOverride = 761;
                    target.Frozen = true;
                    target.SolidHueOverride = 761;
                    clone.MoveToWorld(loc, target.Map);
                    target.SendLocalizedMessage(1112768); // You have been turned to stone!!!

                    new GazeTimer(target, clone).Start();
                    m_GazeDelay = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));

                    AffectedMobiles.Add(target);
                }
            }
        }

        public void DoSpecialAbility(Mobile target)
        {
            if (0.8 >= Utility.RandomDouble())
                SpawnStone(target);

            if (0.05 >= Utility.RandomDouble())
                FreeStone(target);
        }

        public void SpawnStone(Mobile target)
        {
            Map map = Map;

            if (map == null)
                return;

            if (DateTime.UtcNow > m_DelayTwo)
            {
                int stones = 0;

                foreach (Mobile m in GetMobilesInRange(40))
                {
                    if (m is StoneMonster)
                        ++stones;
                }

                if (stones >= 3)
                {
                    return;
                }
                else
                {
                    BaseCreature stone = new StoneMonster();

                    bool validLocation = false;
                    Point3D loc = Location;

                    for (int j = 0; !validLocation && j < 10; ++j)
                    {
                        int x = X + Utility.Random(10) - 1;
                        int y = Y + Utility.Random(10) - 1;
                        int z = map.GetAverageZ(x, y);

                        if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                            loc = new Point3D(x, y, Z);
                        else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                            loc = new Point3D(x, y, z);
                    }

                    stone.MoveToWorld(loc, target.Map);
                    stone.Frozen = stone.Blessed = true;
                    stone.SolidHueOverride = 761;
                    stone.Combatant = null;
                }

                m_DelayTwo = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 150));
            }
        }

        public void FreeStone(Mobile target)
        {
            Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60)), delegate()
            {
                List<Mobile> list = new List<Mobile>();

                foreach (Mobile mob in GetMobilesInRange(40))
                {
                    if (mob is StoneMonster)
                        list.Add(mob);
                }

                if (0 != list.Count)
                {
                    int j = Utility.Random(list.Count);
                    Mobile stone1 = list.ToArray()[j];

                    stone1.Frozen = stone1.Blessed = false;
                    stone1.SolidHueOverride = -1;

                    foreach (Mobile targ in stone1.GetMobilesInRange(40))
                    {
                        if (targ != null && targ.Player)
                        {
                            targ.SendLocalizedMessage(1112767); // Medusa releases one of the petrified creatures!!
                            stone1.Combatant = targ;
                        }
                    }
                }
            });
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 5);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.025)
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        c.DropItem(new MedusaBlood());
                        break;
                    case 1:
                        c.DropItem(new MedusaStatue());
                        break;
                }
            }
        }

        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.MortalStrike;
        }

        public override void OnAfterDelete()
        {
            DeleteMedusaClone(this);

            for (int i = 0; i < AffectedMobiles.Count; i++)
                if (AffectedMobiles[i] != null)
                    AffectedMobiles.Clear();

            base.OnAfterDelete();
        }

        public override bool OnBeforeDeath()
        {
            DeleteMedusaClone(this);

            for (int i = 0; i < AffectedMobiles.Count; i++)
                if (AffectedMobiles[i] != null)
                    AffectedMobiles.Clear();

            return base.OnBeforeDeath();
        }

        public override void OnDelete()
        {
            DeleteMedusaClone(this);

            for (int i = 0; i < AffectedMobiles.Count; i++)
                if (AffectedMobiles[i] != null)
                    AffectedMobiles.Clear();

            base.OnDelete();
        }

        public void DeleteMedusaClone(Mobile target)
        {
            ArrayList list = new ArrayList();

            foreach (Mobile clone in GetMobilesInRange(40))
            {
                if (clone is MedusaClone || clone is StoneMonster)
                    list.Add(clone);
            }

            foreach (Mobile clone in list)
            {
                clone.Delete();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write((int)m_Scales);
            writer.Write(AffectedMobiles);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            reader.ReadInt();
            AffectedMobiles = reader.ReadStrongMobileList();

            for (int i = 0; i < AffectedMobiles.Count; i++)
                if (AffectedMobiles[i] != null)
                    AffectedMobiles.Clear();
        }

        public class GazeTimer : Timer
        {
            private readonly Mobile target;
            private readonly Mobile clone;
            private int m_Count;
            public GazeTimer(Mobile m, Mobile mc)
                : base(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20))
            {
                target = m;
                clone = mc;
                m_Count = 0;
            }

            protected override void OnTick()
            {
                ++m_Count;

                if (m_Count == 1 && target != null)
                {
                    target.Frozen = false;
                    target.SolidHueOverride = -1;

                    if (AffectedMobiles.Contains(target))
                        AffectedMobiles.Remove(target);
                }
                else if (m_Count == 2 && clone != null)
                {
                    clone.SolidHueOverride = -1;
                    clone.Frozen = clone.Blessed = false;

                    foreach (Mobile m in clone.GetMobilesInRange(12))
                    {
                        if ((m != null) && (m.Player) && !(m is MedusaClone) && !(m is StoneMonster))
                        {
                            m.SendLocalizedMessage(1112767); // Medusa releases one of the petrified creatures!!
                            m.Send(new RemoveMobile(clone));
                            m.Send(new MobileIncoming(m, clone));
                            clone.Combatant = m;
                        }
                    }
                }
                else
                    Stop();
            }
        }
    }

    public class MedusaClone : BaseCreature, IFreezable
    {
        public MedusaClone(Mobile m)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            SolidHueOverride = 33;
            Clone(m);
        }

        public MedusaClone(Serial serial)
            : base(serial)
        {
        }

        public override bool DeleteCorpseOnDeath
        {
            get
            {
                return true;
            }
        }
        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override bool AlwaysMurderer
        {
            get
            {
                return !Frozen;
            }
        }
        public void Clone(Mobile m)
        {
            if (m == null)
            {
                Delete();
                return;
            }

            Body = m.Body;

            Str = m.Str;
            Dex = m.Dex;
            Int = m.Int;

            Hits = m.HitsMax;

            Hue = m.Hue;
            Female = m.Female;

            Name = m.Name;
            NameHue = m.NameHue;

            Title = m.Title;
            Kills = m.Kills;

            HairItemID = m.HairItemID;
            HairHue = m.HairHue;

            FacialHairItemID = m.FacialHairItemID;
            FacialHairHue = m.FacialHairHue;

            BaseSoundID = m.BaseSoundID;

            for (int i = 0; i < m.Skills.Length; ++i)
            {
                Skills[i].Base = m.Skills[i].Base;
                Skills[i].Cap = m.Skills[i].Cap;
            }

            for (int i = 0; i < m.Items.Count; i++)
            {
                if (m.Items[i].Layer != Layer.Backpack && m.Items[i].Layer != Layer.Mount && m.Items[i].Layer != Layer.Bank)
                    AddItem(CloneItem(m.Items[i]));
            }
        }

        public Item CloneItem(Item item)
        {
            Item cloned = new Item(item.ItemID);
            cloned.Layer = item.Layer;
            cloned.Name = item.Name;
            cloned.Hue = item.Hue;
            cloned.Weight = item.Weight;
            cloned.Movable = false;

            return cloned;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Frozen)
                DisplayPaperdollTo(from);
            else
                base.OnDoubleClick(from);
        }

        public void OnRequestedAnimation(Mobile from)
        {
            if (Frozen)
            {
                //if (Core.SA)
                //from.Send(new UpdateStatueAnimationSA(this, 31, 5));
                //else
                from.Send(new UpdateStatueAnimation(this, 1, 31, 5));
            }
        }

        public override void OnDelete()
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 15, 5042);

            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            Delete();
        }
    }
}

namespace Server.Commands
{
    public class AddCloneCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("addclone", AccessLevel.Seer, new CommandEventHandler(AddClone_OnCommand));
        }

        [Description("")]
        public static void AddClone_OnCommand(CommandEventArgs e)
        {
            BaseCreature clone = new MedusaClone(e.Mobile);
            clone.Frozen = clone.Blessed = true;
            clone.MoveToWorld(e.Mobile.Location, e.Mobile.Map);
        }
    }
}