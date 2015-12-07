using Server.Mobiles;

namespace Server.Items
{
    public class PrismOfLightAdmissionTicket : PeerlessKey
    {
        [Constructable]
        public PrismOfLightAdmissionTicket()
            : base(0x14EF)
        {
            Weight = 1.0;
        }

        public PrismOfLightAdmissionTicket(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1074340; }
        } // Prism of Light Admission Ticket

        public override int Lifespan
        {
            get { return 437200; }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1074841); // Double click to transport out of the Prism of Light dungeon
            list.Add(1075269); // Destroyed when dropped
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                if (from.Region.IsPartOf("Prism of Light"))
                    Teleport(from);
                else
                    from.SendLocalizedMessage(1074840);
                        // This ticket can only be used while you are in the Prism of Light dungeon.
            }
        }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            var ret = base.DropToWorld(from, p);

            if (ret)
                DestroyItem(from);

            return ret;
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            var ret = base.DropToMobile(from, target, p);

            if (ret)
                DestroyItem(from);

            return ret;
        }

        #region Enhance Client
        public override bool DropToItem(Mobile from, Item target, Point3D p, byte gridloc)
        {
            var ret = base.DropToItem(from, target, p, gridloc);

            if (ret && Parent != from.Backpack)
                DestroyItem(from);

            return ret;
        }
        #endregion

        public virtual void DestroyItem(Mobile from)
        {
            from.SendLocalizedMessage(500424); // You destroyed the item.
            Teleport(from);
            Decay();
        }

        public virtual void Teleport(Mobile from)
        {
            if (from.Region != null && from.Region.IsPartOf("Prism of Light"))
            {
                // teleport pets
                var region = from.Region.GetRegion("Prism of Light");

                if (region != null)
                {
                    var mobiles = region.GetMobiles();

                    foreach (var m in mobiles)
                        if (m is BaseCreature && ((BaseCreature) m).ControlMaster == from)
                            m.MoveToWorld(new Point3D(3785, 1107, 20), Map);
                }

                // teleport player
                from.MoveToWorld(new Point3D(3785, 1107, 20), Map);
            }
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
}