using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a Tyball Shadow corpse")]
    public class TyballsShadow : BaseCreature
    {
        [Constructable]
        public TyballsShadow()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.15, 0.4)
        {
            Body = 0x190;
            Hue = 0x4001;
            Female = false;
            Name = "Tyball's Shadow";

            AddItem(new Robe(2406));

            SetStr(400, 450);
            SetDex(210, 250);
            SetInt(310, 330);

            SetHits(2800, 3000);

            SetDamage(20, 25);

            SetDamageType(ResistanceType.Physical, 100);
            SetDamageType(ResistanceType.Energy, 25);
            SetDamageType(ResistanceType.Poison, 20);
            SetDamageType(ResistanceType.Energy, 20);

            SetResistance(ResistanceType.Physical, 100);
            SetResistance(ResistanceType.Fire, 70);
            SetResistance(ResistanceType.Cold, 70);
            SetResistance(ResistanceType.Poison, 70);
            SetResistance(ResistanceType.Energy, 70);

            SetSkill(SkillName.Magery, 100.0);
            SetSkill(SkillName.MagicResist, 120.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 100.0);

            Fame = 20000;
            Karma = -20000;
            VirtualArmor = 65;
        }

        public TyballsShadow(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get { return true; }
        }

        public override bool Unprovokable
        {
            get { return true; }
        }

        public override bool Uncalmable
        {
            get { return true; }
        }

        public override bool AlwaysMurderer
        {
            get { return true; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
        }

        public override void OnDeath(Container c)
        {
            if (Map == Map.TerMur)
            {
                var rights = GetLootingRights(DamageEntries, HitsMax);
                var toGive = new List<Mobile>();

                for (var i = rights.Count - 1; i >= 0; --i)
                {
                    var ds = rights[i];
                    if (ds.m_HasRight)
                        toGive.Add(ds.m_Mobile);
                }

                if (toGive.Count > 0)
                    toGive[Utility.Random(toGive.Count)].AddToBackpack(new YellowKey1());

                /*else
                c.DropItem(new YellowKey1());*/

                if (Utility.RandomDouble() < 0.10)
                    c.DropItem(new ShroudOfTheCondemned());
            }
            base.OnDeath(c);
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