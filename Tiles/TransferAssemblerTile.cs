﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MechTransfer.Tiles
{
    public class TransferAssemblerTile : ModTile
    {
        private ItemInventory inventory = new ItemInventory();

        private Dictionary<int, int[]> tileRemap = new Dictionary<int, int[]>() {
            { 302,  new int[]{ 17 } },
            { 77,  new int[]{ 17 } },
            { 133,  new int[]{ 17, 77 } },
            { 134,  new int[]{ 16 } },
            { 354,  new int[]{ 14 } },
            { 469,  new int[]{ 14 } },
            { 355,  new int[]{ 13, 14 } },
        };

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TransferAssemblerTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);

            drop = mod.ItemType("TransferAssemblerItem");
            AddMapEntry(new Color(200, 200, 200));
        }

        public override void HitWire(int i, int j)
        {
            if (Main.netMode == 1)
                return;

            inventory.Clear();
            foreach (var c in TransferUtils.FindContainerAdjacent(i, j))
            {
                inventory.RegisterContainer(c);
            }

            int filterId = mod.GetTileEntity<TransferAssemblerTileEntity>().Find(i, j);
            if (filterId == -1)
                return;
            TransferAssemblerTileEntity entity = (TransferAssemblerTileEntity)TileEntity.ByID[filterId];

            List<Recipe> candidates = new List<Recipe>();
            for (int r = 0; r < Recipe.maxRecipes && !Main.recipe[r].createItem.IsAir; r++)
            {
                if (Main.recipe[r].createItem.type == entity.ItemId && TryMakeRecipe(Main.recipe[r], entity))
                {
                    break;
                }
            }
            inventory.Clear();
        }

        private bool TryMakeRecipe(Recipe recipe, TransferAssemblerTileEntity entity)
        {
            for (int i = 0; i < Recipe.maxRequirements && !recipe.requiredItem[i].IsAir; i++)
            {
                if (!inventory.TryTakeIngredient(recipe, recipe.requiredItem[i]))
                {
                    entity.Status = TransferAssemblerTileEntity.StatusKind.MissingItem;
                    entity.MissingItemType = recipe.requiredItem[i].type;
                    return false;
                }
            }

            bool alchemy = false;
            if (!SearchStation(recipe, entity.Position.X, entity.Position.Y, ref alchemy))
            {
                entity.Status = TransferAssemblerTileEntity.StatusKind.MissingStation;
                return false;
            }

            Item clone = recipe.createItem.Clone();

            if (!clone.IsAir)
            {
                //these can potentially cause issues in some cases, might have to remove them
                RecipeHooks.OnCraft(clone, recipe);
                ItemLoader.OnCraft(clone, recipe);
            }

            if (!TransferUtils.InjectItem(entity.Position.X, entity.Position.Y, clone))
            {
                entity.Status = TransferAssemblerTileEntity.StatusKind.MissingSpace;
                return true; //returning with success, so we don't try alternate recipes
            }

            inventory.Commit(alchemy);
            entity.Status = TransferAssemblerTileEntity.StatusKind.Success;

            return true;
        }

        private bool SearchStation(Recipe recipe, int x, int y, ref bool alchemy)
        {
            bool[] tileOk = new bool[Recipe.maxRequirements];
            bool waterOk = !recipe.needWater;
            bool honeyOk = !recipe.needHoney;
            bool lavaOk = !recipe.needLava;
            bool snowOk = !recipe.needSnowBiome;

            for (int i = x - 5; i <= x + 5; i++)
            {
                for (int j = y - 5; j <= y + 5; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile != null && tile.active())
                    {
                        for (int z = 0; z < Recipe.maxRequirements && recipe.requiredTile[z] != -1; z++)
                        {
                            if (recipe.requiredTile[z] == tile.type)
                                tileOk[z] = true;

                            if (tileRemap.ContainsKey(tile.type) && tileRemap[tile.type].Contains(recipe.requiredTile[z]))
                                tileOk[z] = true;

                            ModTile modTile = TileLoader.GetTile(tile.type);
                            if (modTile != null && modTile.adjTiles.Contains(recipe.requiredTile[z]))
                                tileOk[z] = true;

                            //easier than reimplementing the zone finding logic
                            if (tile.type == TileID.SnowBlock || tile.type == TileID.IceBlock || tile.type == TileID.HallowedIce || tile.type == TileID.FleshIce || tile.type == TileID.CorruptIce)
                                snowOk = true;

                            if (tile.type == TileID.AlchemyTable && recipe.alchemy)
                                alchemy = true;

                            //can't access TileLoader.HookAdjTiles, so if a mod uses that, it won't work
                        }
                    }

                    if (tile != null && tile.liquid > 200)
                    {
                        if (tile.liquidType() == 0)
                            waterOk = true;
                        if (tile.liquidType() == 2)
                            honeyOk = true;
                        if (tile.liquidType() == 1)
                            lavaOk = true;
                    }
                }
            }

            if (!waterOk || !honeyOk || !lavaOk || !snowOk)
                return false;

            for (int i = 0; i < Recipe.maxRequirements && recipe.requiredTile[i] != -1; i++)
            {
                if (!tileOk[i])
                    return false;
            }

            return true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            mod.GetTileEntity<TransferAssemblerTileEntity>().Kill(i, j);
        }

        public override void RightClick(int i, int j)
        {
            if (!Main.LocalPlayer.HeldItem.IsAir)
            {
                int id = mod.GetTileEntity<TransferAssemblerTileEntity>().Find(i, j);
                if (id != -1)
                    ((TransferAssemblerTileEntity)TileEntity.ByID[id]).ItemId = Main.LocalPlayer.HeldItem.type;
            }
        }

        public override void MouseOverFar(int i, int j)
        {
            DisplayTooltip(i, j);
        }

        public override void MouseOver(int i, int j)
        {
            DisplayTooltip(i, j);
        }

        public void DisplayTooltip(int i, int j)
        {
            int id = mod.GetTileEntity<TransferAssemblerTileEntity>().Find(i, j);
            if (id == -1)
                return;
            TransferAssemblerTileEntity entity = (TransferAssemblerTileEntity)TileEntity.ByID[id];

            string statusText = "";
            switch (entity.Status)
            {
                case TransferAssemblerTileEntity.StatusKind.Ready:
                    statusText = "[c/FFFF00:Ready]"; break;
                case TransferAssemblerTileEntity.StatusKind.Success:
                    statusText = "[c/00FF00:Success]"; break;
                case TransferAssemblerTileEntity.StatusKind.MissingItem:
                    statusText = string.Format("[c/FF0000:Missing ingredient ({0})]", TransferUtils.ItemNameById(entity.MissingItemType)); break;
                case TransferAssemblerTileEntity.StatusKind.MissingStation:
                    statusText = "[c/FF0000:Missing crafting station]"; break;
                case TransferAssemblerTileEntity.StatusKind.MissingSpace:
                    statusText = "[c/FF0000:Inventory full]"; break;
            }

            string itemText = "";
            if (entity.ItemId == 0)
            {
                itemText = "Not set";
                Main.LocalPlayer.showItemIcon2 = drop;
            }
            else
            {
                itemText = TransferUtils.ItemNameById(entity.ItemId);
                Main.LocalPlayer.showItemIcon2 = entity.ItemId;
            }

            Main.LocalPlayer.showItemIconText = string.Format("      Crafting: {0}\n{1}", itemText, statusText);
            Main.LocalPlayer.showItemIcon = true;
        }
    }
}