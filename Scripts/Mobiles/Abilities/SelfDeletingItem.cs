// Created by Peoharen for the Mobile Abilities Package.

using System;

namespace Server.Items
{
    public class SelfDeletingItem : Item
    {
        [Constructable]
        public SelfDeletingItem(int id, string name, int duration)
            : base(8391)
        {
            Weight = 1.0;
            ItemID = id;
            Name = "name";
            Movable = false;

            Timer.DelayCall(TimeSpan.FromSeconds(duration), Expire);
        }

        public SelfDeletingItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();

            Timer.DelayCall(TimeSpan.FromSeconds(5), Expire);
        }

        private void Expire()
        {
            if (Deleted)
                return;

            Delete();
        }
    }
}