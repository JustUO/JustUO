using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Zosilem : MondainQuester
    {
        [Constructable]
        public Zosilem()
            : base("Zosilem", "the Alchemist")
        {
        }

        public Zosilem(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (DabblingontheDarkSide)
                };
            }
        }

        public override void InitBody()
        {
            HairItemID = 0x2044; //
            HairHue = 1153;
            FacialHairItemID = 0x204B;
            FacialHairHue = 1153;
            Body = 666;
            Blessed = true;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Boots());
            AddItem(new LongPants(0x6C7));
            AddItem(new FancyShirt(0x6BB));
            AddItem(new Cloak(0x59));
        }

        public override bool OnDragDrop(Mobile from, Item item1)
        {
            if (item1 is PotionKeg)
            {
                var m_Pot1 = item1 as PotionKeg;

                if (m_Pot1.Type == PotionEffect.RefreshTotal)
                {
                    from.SendMessage("OHHHH YESSS !!!");

                    var toConsume = m_Pot1.Amount;

                    if ((m_Pot1.Amount < 2) && (m_Pot1.Amount > 0))
                    {
                        from.SendMessage(
                            "You have converted 1 Keg of Total Refreshment Potion in a Inspected Keg of Total Refreshment");
                        m_Pot1.Delete();
                        from.AddToBackpack(new InspectedKegofTotalRefreshment());

                        return true;
                    }
                    @from.SendMessage("You can only convert 1 Keg of Total Refreshment Potion at a time !");
                }
                else if (m_Pot1.Type == PotionEffect.PoisonGreater)
                {
                    from.SendMessage("OHHHH YESSS !!!");

                    var toConsume = m_Pot1.Amount;

                    if ((m_Pot1.Amount < 2) && (m_Pot1.Amount > 0))
                    {
                        from.SendMessage(
                            "You have converted 1 Keg of Greater Poison Potion in a Inspected Keg of Greater Poison");
                        m_Pot1.Delete();
                        from.AddToBackpack(new InspectedKegofGreaterPoison());

                        return true;
                    }
                    @from.SendMessage("You can only convert 1 Keg of Greater Poison Potion at a time !");
                }
            }

            if (item1 is GoldIngot)
            {
                var m_Ing1 = item1 as BaseIngot;

                var toConsume = m_Ing1.Amount;

                if ((m_Ing1.Amount > 19) && (m_Ing1.Amount < 21))
                {
                    from.SendMessage("You have converted 20 Gold Ingot in a Pile of Inspected Gold Ingots");
                    m_Ing1.Delete();
                    from.AddToBackpack(new PileofInspectedGoldIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 Gold Ingot at a time !");
            }

            if (item1 is DullCopperIngot)
            {
                var m_Ing2 = item1 as BaseIngot;

                var toConsume = m_Ing2.Amount;

                if ((m_Ing2.Amount > 19) && (m_Ing2.Amount < 21))
                {
                    from.SendMessage("You have converted 20 DullCopper Ingot in a Pile of Inspected DullCopper Ingots");
                    m_Ing2.Delete();
                    from.AddToBackpack(new PileofInspectedDullCopperIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 DullCopper Ingot at a time !");
            }

            if (item1 is ShadowIronIngot)
            {
                var m_Ing3 = item1 as BaseIngot;

                var toConsume = m_Ing3.Amount;

                if ((m_Ing3.Amount > 19) && (m_Ing3.Amount < 21))
                {
                    from.SendMessage("You have converted 20 ShadowIron Ingot in a Pile of Inspected ShadowIron Ingots");
                    m_Ing3.Delete();
                    from.AddToBackpack(new PileofInspectedShadowIronIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 ShadowIron Ingot at a time !");
            }

            if (item1 is CopperIngot)
            {
                var m_Ing4 = item1 as BaseIngot;

                var toConsume = m_Ing4.Amount;

                if ((m_Ing4.Amount > 19) && (m_Ing4.Amount < 21))
                {
                    from.SendMessage("You have converted 20 Copper Ingot in a Pile of Inspected Copper Ingots");
                    m_Ing4.Delete();
                    from.AddToBackpack(new PileofInspectedCopperIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 Copper Ingot at a time !");
            }

            if (item1 is BronzeIngot)
            {
                var m_Ing5 = item1 as BaseIngot;

                var toConsume = m_Ing5.Amount;

                if ((m_Ing5.Amount > 19) && (m_Ing5.Amount < 21))
                {
                    from.SendMessage("You have converted 20 Bronze Ingot in a Pile of Inspected Bronze Ingots");
                    m_Ing5.Delete();
                    from.AddToBackpack(new PileofInspectedBronzeIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 Bronze Ingot at a time !");
            }

            if (item1 is AgapiteIngot)
            {
                var m_Ing6 = item1 as BaseIngot;

                var toConsume = m_Ing6.Amount;

                if ((m_Ing6.Amount > 19) && (m_Ing6.Amount < 21))
                {
                    from.SendMessage("You have converted 20 Agapite Ingot in a Pile of Inspected Bronze Ingots");
                    m_Ing6.Delete();
                    from.AddToBackpack(new PileofInspectedAgapiteIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 Agapite Ingot at a time !");
            }

            if (item1 is VeriteIngot)
            {
                var m_Ing7 = item1 as BaseIngot;

                var toConsume = m_Ing7.Amount;

                if ((m_Ing7.Amount > 19) && (m_Ing7.Amount < 21))
                {
                    from.SendMessage("You have converted 20 Verite Ingot in a Pile of Inspected Verite Ingots");
                    m_Ing7.Delete();
                    from.AddToBackpack(new PileofInspectedVeriteIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 Verite Ingot at a time !");
            }

            if (item1 is ValoriteIngot)
            {
                var m_Ing8 = item1 as BaseIngot;

                var toConsume = m_Ing8.Amount;

                if ((m_Ing8.Amount > 19) && (m_Ing8.Amount < 21))
                {
                    from.SendMessage("You have converted 20 Valorite Ingot in a Pile of Inspected Valorite Ingots");
                    m_Ing8.Delete();
                    from.AddToBackpack(new PileofInspectedValoriteIngots());

                    return true;
                }
                @from.SendMessage("You can only convert 20 Verite Ingot at a time !");
            }

            return base.OnDragDrop(from, item1);
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