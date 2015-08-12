using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests.Ninja
{
    public class EminosKatanaChest : WoodenChest
    {
        [Constructable]
        public EminosKatanaChest()
        {
            Movable = false;
            ItemID = 0xE42;

            GenerateTreasure();
        }

        public EminosKatanaChest(Serial serial)
            : base(serial)
        {
        }

        public override bool IsDecoContainer
        {
            get { return false; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            var player = from as PlayerMobile;

            if (player != null && player.InRange(GetWorldLocation(), 2))
            {
                var qs = player.Quest;

                if (qs is EminosUndertakingQuest)
                {
                    if (EminosUndertakingQuest.HasLostEminosKatana(from))
                    {
                        Item katana = new EminosKatana();

                        if (!player.PlaceInBackpack(katana))
                        {
                            katana.Delete();
                            player.SendLocalizedMessage(1046260);
                                // You need to clear some space in your inventory to continue with the quest.  Come back here when you have more space in your inventory.
                        }
                    }
                    else
                    {
                        var obj = qs.FindObjective(typeof (HallwayWalkObjective));

                        if (obj != null && !obj.Completed)
                        {
                            Item katana = new EminosKatana();

                            if (player.PlaceInBackpack(katana))
                            {
                                GenerateTreasure();
                                obj.Complete();
                            }
                            else
                            {
                                katana.Delete();
                                player.SendLocalizedMessage(1046260);
                                    // You need to clear some space in your inventory to continue with the quest.  Come back here when you have more space in your inventory.
                            }
                        }
                    }
                }
            }

            base.OnDoubleClick(from);
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            return false;
        }

        public override bool CheckItemUse(Mobile from, Item item)
        {
            return item == this;
        }

        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            var player = from as PlayerMobile;

            if (player != null && player.Quest is EminosUndertakingQuest)
            {
                var obj = player.Quest.FindObjective(typeof (HallwayWalkObjective)) as HallwayWalkObjective;

                if (obj != null)
                {
                    if (obj.StolenTreasure)
                        from.SendLocalizedMessage(1063247);
                            // The guard is watching you carefully!  It would be unwise to remove another item from here.
                    else
                        return true;
                }
            }

            return false;
        }

        public override void OnItemLifted(Mobile from, Item item)
        {
            var player = from as PlayerMobile;

            if (player != null && player.Quest is EminosUndertakingQuest)
            {
                var obj = player.Quest.FindObjective(typeof (HallwayWalkObjective)) as HallwayWalkObjective;

                if (obj != null)
                    obj.StolenTreasure = true;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();
        }

        private void GenerateTreasure()
        {
            for (var i = Items.Count - 1; i >= 0; i--)
                Items[i].Delete();

            for (var i = 0; i < 75; i++)
            {
                switch (Utility.Random(3))
                {
                    case 0:
                        DropItem(new GoldBracelet());
                        break;
                    case 1:
                        DropItem(new GoldRing());
                        break;
                    case 2:
                        DropItem(Loot.RandomGem());
                        break;
                }
            }
        }
    }
}