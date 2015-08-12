 /*
** Allows staff to quickly switch between player and their assigned staff levels by equipping or removing the cloak
** Also allows instant teleportation to a specified destination when double-clicked by the staff member.
*/

namespace Server.Items
{
    public class BarreraakRing : GoldRing
    {
        private int m_BodyInit;

        [Constructable]
        public BarreraakRing()
        {
            LootType = LootType.Blessed;
            Weight = 1;
        }

        public BarreraakRing(Serial serial) : base(serial)
        {
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int BodyInit
        {
            get { return m_BodyInit; }
            set
            {
                m_BodyInit = value;
                InvalidateProperties();
            }
        }

        public override int LabelNumber
        {
            get { return 1095049; }
        }

        public override bool OnEquip(Mobile from)
        {
            BodyInit = from.BodyValue;
            from.BodyValue = 334;

            return base.OnEquip(from);
        }

        public override void OnRemoved(IEntity parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile && !Deleted)
            {
                var m = (Mobile) parent;

                m.BodyValue = BodyInit;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
            writer.Write(m_BodyInit);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
            m_BodyInit = reader.ReadInt();
        }
    }
}