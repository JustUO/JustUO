//================================================//
// Based on winecrafting grounds created by	  //
// dracana, modded by Manu from Splitterwelt.com  //
// for use with carpets					  //
// Desc: For players to place carpets in their	  //
//       houses.  Especially useful for players   //
//       with non-custom housing.
//  Modified for 2.0 by Draco Van Peeble
//================================================//

using Server.Gumps;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class VariableCarpetAddon : BaseAddon
    {
        public override BaseAddonDeed Deed
        {
            get { return new VariableCarpetAddonDeed(); }
        }

        public override void OnDoubleClick(Mobile from)
        {
            var house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsCoOwner(from))
            {
                if (from.InRange(GetWorldLocation(), 3))
                {
                    from.SendGump(new ConfirmRemovalGumpVariableCarpet(this));
                }
                else
                {
                    from.SendLocalizedMessage(500295); // You are too far away to do that.
                }
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

        #region Constructors

        [Constructable]
        public VariableCarpetAddon(VariableCarpetType type, int width, int height) : this((int) type, width, height)
        {
        }

        public VariableCarpetAddon(int type, int width, int height)
        {
            var info = VariableCarpetInfo.GetInfo(type);

            AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.Top).ItemID), 0, 0, 0);
            AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.Right).ItemID), width, 0, 0);
            AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.Left).ItemID), 0, height, 0);
            AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.Bottom).ItemID), width, height, 0);

            var w = width - 1;
            var h = height - 1;

            for (var y = 1; y <= h; ++y)
                AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.West).ItemID), 0, y, 0);

            for (var x = 1; x <= w; ++x)
                AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.North).ItemID), x, 0, 0);

            for (var y = 1; y <= h; ++y)
                AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.East).ItemID), width, y, 0);

            for (var x = 1; x <= w; ++x)
                AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.South).ItemID), x, height, 0);

            for (var x = 1; x <= w; ++x)
                for (var y = 1; y <= h; ++y)
                    AddComponent(new AddonComponent(info.GetItemPart(GroundPosition.Center).ItemID), x, y, 0);
        }

        public VariableCarpetAddon(Serial serial) : base(serial)
        {
        }

        #endregion
    }

    public enum VariableCarpetType
    {
        BlueStructureBorder,
        BluePlainBorder,
        BlueYellowBorder,
        RedStructureBorder,
        RedPlainBorder,
        YellowStructureBorder
    }

    public enum GroundPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        West,
        North,
        East,
        South,
        Center
    }

    public class VariableCarpetInfo
    {
        #region VariableCarpetInfo definitions

        private static readonly VariableCarpetInfo[] m_Infos =
        {
/* BlueStructureBorder */		new VariableCarpetInfo(new[]
{
    new GroundItemPart(0xAC3, GroundPosition.Top, 44, 0),
    new GroundItemPart(0xAC2, GroundPosition.Bottom, 44, 68),
    new GroundItemPart(0xAC4, GroundPosition.Left, 0, 28),
    new GroundItemPart(0xAC5, GroundPosition.Right, 88, 28),
    new GroundItemPart(0xAF6, GroundPosition.West, 22, 12),
    new GroundItemPart(0xAF7, GroundPosition.North, 66, 12),
    new GroundItemPart(0xAF8, GroundPosition.East, 66, 46),
    new GroundItemPart(0xAF9, GroundPosition.South, 22, 46),
    new GroundItemPart(0xABD, GroundPosition.Center, 44, 24)
}),
/* BluePlainBorder */			new VariableCarpetInfo(new[]
{
    new GroundItemPart(0xAC3, GroundPosition.Top, 44, 0),
    new GroundItemPart(0xAC2, GroundPosition.Bottom, 44, 68),
    new GroundItemPart(0xAC4, GroundPosition.Left, 0, 28),
    new GroundItemPart(0xAC5, GroundPosition.Right, 88, 28),
    new GroundItemPart(0xAF6, GroundPosition.West, 22, 12),
    new GroundItemPart(0xAF7, GroundPosition.North, 66, 12),
    new GroundItemPart(0xAF8, GroundPosition.East, 66, 46),
    new GroundItemPart(0xAF9, GroundPosition.South, 22, 46),
    new GroundItemPart(0xABE, GroundPosition.Center, 44, 24)
}),
/* BlueYellowBorder */			new VariableCarpetInfo(new[]
{
    new GroundItemPart(0xAD3, GroundPosition.Top, 44, 0),
    new GroundItemPart(0xAD2, GroundPosition.Bottom, 44, 68),
    new GroundItemPart(0xAD4, GroundPosition.Left, 0, 28),
    new GroundItemPart(0xAD5, GroundPosition.Right, 88, 28),
    new GroundItemPart(0xAD6, GroundPosition.West, 22, 8),
    new GroundItemPart(0xAD7, GroundPosition.North, 66, 8),
    new GroundItemPart(0xAD8, GroundPosition.East, 66, 46),
    new GroundItemPart(0xAD9, GroundPosition.South, 22, 46),
    new GroundItemPart(0xAD1, GroundPosition.Center, 44, 24)
}),
/* RedStructureBorder 	*/		new VariableCarpetInfo(new[]
{
    new GroundItemPart(0xACA, GroundPosition.Top, 44, 0),
    new GroundItemPart(0xAC9, GroundPosition.Bottom, 44, 68),
    new GroundItemPart(0xACB, GroundPosition.Left, 0, 28),
    new GroundItemPart(0xACC, GroundPosition.Right, 88, 28),
    new GroundItemPart(0xACD, GroundPosition.West, 22, 10),
    new GroundItemPart(0xACE, GroundPosition.North, 66, 12),
    new GroundItemPart(0xACF, GroundPosition.East, 66, 46),
    new GroundItemPart(0xAD0, GroundPosition.South, 22, 46),
    new GroundItemPart(0xAC7, GroundPosition.Center, 44, 24)
}),
/* RedPlainBorder */			new VariableCarpetInfo(new[]
{
    new GroundItemPart(0xACA, GroundPosition.Top, 44, 0),
    new GroundItemPart(0xAC9, GroundPosition.Bottom, 44, 68),
    new GroundItemPart(0xACB, GroundPosition.Left, 0, 28),
    new GroundItemPart(0xACC, GroundPosition.Right, 88, 28),
    new GroundItemPart(0xACD, GroundPosition.West, 22, 10),
    new GroundItemPart(0xACE, GroundPosition.North, 66, 12),
    new GroundItemPart(0xACF, GroundPosition.East, 66, 46),
    new GroundItemPart(0xAD0, GroundPosition.South, 22, 46),
    new GroundItemPart(0xAC8, GroundPosition.Center, 44, 24)
}),
/* YellowStructureBorder */		new VariableCarpetInfo(new[]
{
    new GroundItemPart(0xADC, GroundPosition.Top, 44, 0),
    new GroundItemPart(0xADB, GroundPosition.Bottom, 44, 68),
    new GroundItemPart(0xADD, GroundPosition.Left, 0, 28),
    new GroundItemPart(0xADE, GroundPosition.Right, 88, 28),
    new GroundItemPart(0xADF, GroundPosition.West, 22, 8),
    new GroundItemPart(0xAE0, GroundPosition.North, 66, 8),
    new GroundItemPart(0xAE1, GroundPosition.East, 66, 46),
    new GroundItemPart(0xAE2, GroundPosition.South, 22, 46),
    new GroundItemPart(0xADA, GroundPosition.Center, 44, 24)
})
        };

        #endregion

        public VariableCarpetInfo(GroundItemPart[] entries)
        {
            Entries = entries;
        }

        public GroundItemPart[] Entries { get; private set; }

        public static VariableCarpetInfo[] Infos
        {
            get { return m_Infos; }
        }

        public GroundItemPart GetItemPart(GroundPosition pos)
        {
            var i = (int) pos;

            if (i < 0 || i >= Entries.Length)
                i = 0;

            return Entries[i];
        }

        public static VariableCarpetInfo GetInfo(int type)
        {
            if (type < 0 || type >= m_Infos.Length)
                type = 0;

            return m_Infos[type];
        }
    }

    public class GroundItemPart
    {
        public GroundItemPart(int itemID, GroundPosition info, int offsetX, int offsetY)
        {
            ItemID = itemID;
            GroundPosition = info;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public int ItemID { get; private set; }
        public GroundPosition GroundPosition { get; private set; }
        // For Gump Rendering
        public int OffsetX { get; private set; }
        // For Gump Rendering
        public int OffsetY { get; private set; }
    }

    public class ConfirmRemovalGumpVariableCarpet : Gump
    {
        private readonly VariableCarpetAddon m_VariableCarpetAddon;

        public ConfirmRemovalGumpVariableCarpet(VariableCarpetAddon VariableCarpetaddon)
            : base(50, 50)
        {
            m_VariableCarpetAddon = VariableCarpetaddon;

            AddBackground(0, 0, 450, 260, 9270);

            AddAlphaRegion(12, 12, 426, 22);
            AddTextEntry(13, 13, 379, 20, 32, 0, @"Warning!");

            AddAlphaRegion(12, 39, 426, 209);

            AddHtml(15, 50, 420, 185, "<BODY>" +
                                      "<BASEFONT COLOR=YELLOW>You are about to remove this carpet!<BR><BR>" +
                                      "<BASEFONT COLOR=YELLOW>If it is removed, a deed will be placed " +
                                      "<BASEFONT COLOR=YELLOW>in your backpack.<BR><BR>" +
                                      "<BASEFONT COLOR=YELLOW>Are you sure that you want to remove this carpet?<BR><BR>" +
                                      "</BODY>", false, false);

            AddButton(13, 220, 0xFA5, 0xFA6, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(47, 222, 150, 20, 1052072, 0x7FFF, false, false); // Continue

            //AddButton(200, 245, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
            //AddHtmlLocalized(47, 247, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL
            AddButton(350, 220, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(385, 222, 100, 20, 1060051, 0x7FFF, false, false); // CANCEL
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0)
                return;

            var from = sender.Mobile;

            from.AddToBackpack(new VariableCarpetAddonDeed());
            m_VariableCarpetAddon.Delete();

            from.SendMessage("Carpet removed");
        }
    }
}