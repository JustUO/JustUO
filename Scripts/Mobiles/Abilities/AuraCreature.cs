// Created by Peoharen

using System;

//using Server.Spells.BlueMagic;

namespace Server.Mobiles
{
    public class AuraCreature : BaseCreature
    {
        public DateTime m_AuraDelay;
        private string m_AuraMessage = "";
        private ResistanceType m_AuraType = ResistanceType.Physical;

        public AuraCreature(AIType aitype, FightMode fightmode, int spot, int meleerange, double passivespeed,
            double activespeed)
            : base(aitype, fightmode, spot, meleerange, passivespeed, activespeed)
        {
            AuraPoison = null;
            m_AuraDelay = DateTime.UtcNow;
            /*
            Default is ?
            AuraMessage = "The intense cold is damaging you!";
            AuraType = ResistanceType.Fire;
            MinAuraDelay = 5;
            MaxAuraDelay = 15;
            MinAuraDamage = 15;
            MaxAuraDamage = 25;
            AuraRange = 3;
            */
        }

        public AuraCreature(Serial serial)
            : base(serial)
        {
            AuraPoison = null;
        }

        public override void OnThink()
        {
            if (DateTime.UtcNow > m_AuraDelay)
            {
                DebugSay("Auraing");
                Ability.Aura(this, MinAuraDamage, MaxAuraDamage, m_AuraType, AuraRange, AuraPoison, m_AuraMessage);

                m_AuraDelay = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(MinAuraDelay, MaxAuraDelay));
            }

            base.OnThink();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(MinAuraDelay);
            writer.Write(MaxAuraDelay);
            writer.Write(MinAuraDamage);
            writer.Write(MaxAuraDamage);
            writer.Write(AuraRange);
            writer.Write((int) m_AuraType);
            Poison.Serialize(AuraPoison, writer);
            writer.Write(m_AuraMessage);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
            MinAuraDelay = reader.ReadInt();
            MaxAuraDelay = reader.ReadInt();
            MinAuraDamage = reader.ReadInt();
            MaxAuraDamage = reader.ReadInt();
            AuraRange = reader.ReadInt();
            m_AuraType = (ResistanceType) reader.ReadInt();
            AuraPoison = Poison.Deserialize(reader);
            m_AuraMessage = reader.ReadString();
            m_AuraDelay = DateTime.UtcNow;
        }

        #region publicprops

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinAuraDelay { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxAuraDelay { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinAuraDamage { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxAuraDamage { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public new int AuraRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ResistanceType AuraType
        {
            get { return m_AuraType; }
            set { m_AuraType = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison AuraPoison { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string AuraMessage
        {
            get { return m_AuraMessage; }
            set { m_AuraMessage = value; }
        }

        #endregion
    }
}