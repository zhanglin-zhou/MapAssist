using MapAssist.Helpers;
using MapAssist.Settings;
using MapAssist.Structs;
using System;

namespace MapAssist.Types
{
    public class UnitItem : UnitAny
    {
        public ItemData ItemData { get; private set; }
        public Npc VendorOwner { get; set; } = Npc.Invalid;
        public Item Item => (Item)TxtFileNo;
        public ItemMode ItemMode => (ItemMode)Struct.Mode;
        public DateTime FoundTime { get; private set; } = DateTime.Now;

        public UnitItem(IntPtr pUnit) : base(pUnit)
        {
        }

        public new UnitItem Update()
        {
            base.Update();

            if (MapAssistConfiguration.Loaded.ItemLog.Enabled)
            {
                using (var processContext = GameManager.GetProcessContext())
                {
                    ItemData = processContext.Read<ItemData>(Struct.pUnitData);

                    if ((IsDropped && !IsIdentified) || IsInStore)
                    {
                        Items.LogItem(this, processContext.ProcessId);
                    }
                }
            }

            return this;
        }

        public bool IsIdentified
        {
            get => ItemData.ItemQuality >= ItemQuality.MAGIC && (ItemData.ItemFlags & ItemFlags.IFLAG_IDENTIFIED) == ItemFlags.IFLAG_IDENTIFIED;
        }

        public bool IsDropped
        {
            get => ItemModeMapped == ItemModeMapped.Ground;
        }

        public bool IsInStore
        {
            get => ItemModeMapped == ItemModeMapped.Vendor;
        }

        public bool IsPlayerHolding
        {
            get
            {
                switch (ItemModeMapped)
                {
                    case ItemModeMapped.Belt:
                    case ItemModeMapped.Inventory:
                    case ItemModeMapped.Cube:
                    case ItemModeMapped.Stash:
                    case ItemModeMapped.Player:
                    case ItemModeMapped.Mercenary:
                        return true;
                }
                return false;
            }
        }

        public ItemModeMapped ItemModeMapped
        {
            get
            {
                switch (ItemMode)
                {
                    case ItemMode.INBELT: return ItemModeMapped.Belt;
                    case ItemMode.DROPPING: return ItemModeMapped.Ground;
                    case ItemMode.ONGROUND: return ItemModeMapped.Ground;
                    case ItemMode.SOCKETED: return ItemModeMapped.Socket;
                    case ItemMode.EQUIP:
                        if (ItemData.dwOwnerID != uint.MaxValue) return ItemModeMapped.Player;
                        else return ItemModeMapped.Mercenary;
                }

                if (ItemData.dwOwnerID == uint.MaxValue && (ItemData.ItemFlags & ItemFlags.IFLAG_INSTORE) == ItemFlags.IFLAG_INSTORE) return ItemModeMapped.Vendor;
                if (ItemData.dwOwnerID != uint.MaxValue && ItemData.InvPage == InvPage.EQUIP) return ItemModeMapped.Trade; // Other player's trade window

                switch (ItemData.InvPage)
                {
                    case InvPage.INVENTORY: return ItemModeMapped.Inventory;
                    case InvPage.TRADE: return ItemModeMapped.Trade;
                    case InvPage.CUBE: return ItemModeMapped.Cube;
                    case InvPage.STASH: return ItemModeMapped.Stash;
                }

                return ItemModeMapped.Unknown; // Items that appeared in the trade window before will appear here
            }
        }

        public string ItemHash()
        {
            return Item + "/" + Position.X + "/" + Position.Y;
        }
    }
}
