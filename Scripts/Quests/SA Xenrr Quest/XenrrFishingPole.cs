namespace Server.Items
{
    public class XenrrFishingPole : FishingPole
    {
        private int m_BodyInit;

        [Constructable]
        public XenrrFishingPole()
        {
            LootType = LootType.Blessed;
            Weight = 1;
        }

        public XenrrFishingPole(Serial serial) : base(serial)
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
            get { return 1095066; }
        }

        public override bool OnEquip(Mobile from)
        {
            BodyInit = from.BodyValue;
            from.BodyValue = 723;

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