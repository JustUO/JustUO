namespace Server.Items
{
    public class ScrollBox3 : WoodenBox
    {
        [Constructable]
        public ScrollBox3()
        {
            Movable = true;
            Hue = 1159;

            PlaceItemIn(this, 45, 66, new PowerScroll(SkillName.Imbuing, 110.0));
        }

        public ScrollBox3(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get { return "Reward Scroll Box"; }
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

        private static void PlaceItemIn(Container parent, int x, int y, Item item)
        {
            parent.AddItem(item);
            item.Location = new Point3D(x, y, 0);
        }
    }
}