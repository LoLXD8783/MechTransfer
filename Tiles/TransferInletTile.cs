﻿using MechTransfer.Items;
using MechTransfer.Tiles.Simple;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MechTransfer.Tiles
{
    public class TransferInletTile : SimpleTETile<TransferInletTileEntity>, ITransferPassthrough
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
			ModContent.GetInstance<ChestPlacementFix>().AddNoChestTile(Type);

            AddMapEntry(MapColors.Input, GetPlaceItem(0).DisplayName);

            ModContent.GetInstance<TransferAgent>().passthroughs.Add(Type, this);
            ModContent.GetInstance<TransferPipeTile>().connectedTiles.Add(Type);

            base.SetStaticDefaults();
        }

        protected override void SetTileObjectData()
        {
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(1, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;

            base.SetTileObjectData();
        }

        public bool ShouldPassthrough(Point16 location, Item item)
        {
            Tile tile = Main.tile[location.X, location.Y];
            return (tile.TileFrameX == 0 || tile.TileFrameX == 36);
        }

        public override void PostLoad()
        {
            PlaceItems[0] = SimplePrototypeItem.MakePlaceable(Mod, "TransferInletItem", Type, 32, 14);
        }

        public override void AddRecipes()
        {
            Recipe r = Recipe.Create(PlaceItems[0].Item.type, 1);
            r.AddIngredient(ModContent.ItemType<PneumaticActuatorItem>(), 1);
            r.AddIngredient(ItemID.InletPump, 1);
            r.AddTile(TileID.WorkBenches);
            r.Register();

            LoadBlacklist();
        }

        private void LoadBlacklist()
        {
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.Heart);
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.CandyApple);
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.CandyCane);

            TransferInletTileEntity.PickupBlacklist.Add(ItemID.Star);
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.SugarPlum);
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.SoulCake);

            TransferInletTileEntity.PickupBlacklist.Add(ItemID.NebulaPickup1);
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.NebulaPickup2);
            TransferInletTileEntity.PickupBlacklist.Add(ItemID.NebulaPickup3);

            TransferInletTileEntity.PickupBlacklist.Add(ItemID.DD2EnergyCrystal);

            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                ModItem item = ItemLoader.GetItem(i);
                if (item != null &&
                   (item.GetType().GetMethod("ItemSpace").DeclaringType != typeof(ModItem) ||
                   item.GetType().GetMethod("OnPickup").DeclaringType != typeof(ModItem)))
                {
                    TransferInletTileEntity.PickupBlacklist.Add(item.Item.type);
                }
            }
        }
    }
}