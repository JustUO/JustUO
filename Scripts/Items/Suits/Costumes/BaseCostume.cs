using System;

namespace Server.Items
{
    [Flipable(0x19BC, 0x19BD)]
    public class BaseCostume : BaseShield, IDyable
    {
        private int m_Hue = -1;
        private bool m_SaveDisplayGuildTitle = true;
        private int m_SaveHueMod = -1;
        private int m_SaveNameHue = -1;
        public bool m_Transformed;
        private Mobile m_Wearer;

        public BaseCostume() : base(0x19BC)
        {
            CostumeBody = 0;
            Name = "Generic Costume";
            Resource = CraftResource.None;
            Attributes.SpellChanneling = 1;
            Layer = Layer.FirstValid;
            Weight = 3.0;
        }

        public BaseCostume(Serial serial) : base(serial)
        {
            CostumeBody = 0;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Transformed
        {
            get { return m_Transformed; }
            set { m_Transformed = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CostumeBody { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CostumeHue
        {
            get { return m_Hue; }
            set { m_Hue = value; }
        }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            if (RootParent is Mobile && @from != RootParent)
                return false;

            Hue = sender.DyedHue;
            return true;
        }

        private void EnMask(Mobile from)
        {
            m_Wearer = from;
            from.SendMessage("You put on your spooky costume!");

            m_SaveNameHue = from.NameHue;
            m_SaveDisplayGuildTitle = from.DisplayGuildTitle;
            m_SaveHueMod = from.HueMod;
            from.BodyMod = CostumeBody;
            from.NameHue = 39;
            from.HueMod = m_Hue;
            from.DisplayGuildTitle = false;
            Transformed = true;
        }

        private void DeMask(Mobile from)
        {
            from.SendMessage("You dicide to quit being so spooky.");

            from.BodyMod = 0;
            from.NameHue = m_SaveNameHue;
            from.HueMod = m_SaveHueMod;
            from.DisplayGuildTitle = m_SaveDisplayGuildTitle;
            Transformed = false;
        }

        public override void OnAdded(IEntity parent)
        {
            if (parent is Mobile) m_Wearer = (Mobile) parent;
            base.OnAdded(parent);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Parent != from)
            {
                from.SendMessage("The costume must be equiped to be used.");
            }

            else if (@from.Mounted)
            {
                from.SendMessage("You cannot be mounted while wearing your costume!");
            }

            else if (from.BodyMod != 0 && !Transformed)
            {
                from.SendMessage("You are already costumed!");
            }

            else if (Transformed == false)
            {
                EnMask(from);
            }
            else
            {
                DeMask(from);
            }
        }

        public override void OnRemoved(IEntity o)
        {
            if (Transformed) DeMask(m_Wearer);
            m_Wearer = null;

            if (o is Mobile && ((Mobile) o).Kills >= 5)
            {
                ((Mobile) o).Criminal = true;
            }

            if (o is Mobile && ((Mobile) o).GuildTitle != null)
            {
                ((Mobile) o).DisplayGuildTitle = m_SaveDisplayGuildTitle;
            }

            base.OnRemoved(o);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);
            writer.Write(CostumeBody);
            writer.Write(m_Hue);
            writer.Write(m_SaveNameHue);
            writer.Write(m_SaveDisplayGuildTitle);
            writer.Write(m_SaveHueMod);

            if (m_Wearer == null)
                writer.Write(Serial.MinusOne.Value);
            else
                writer.Write(m_Wearer.Serial.Value);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            if (version == 1)
            {
                CostumeBody = reader.ReadInt();
                m_Hue = reader.ReadInt();
                m_SaveNameHue = reader.ReadInt();
                m_SaveDisplayGuildTitle = reader.ReadBool();
                m_SaveHueMod = reader.ReadInt();
                Serial WearerSerial = reader.ReadInt();

                if (WearerSerial.IsMobile)
                    m_Wearer = World.FindMobile(WearerSerial);

                else
                    m_Wearer = null;
            }
        }
    }
}