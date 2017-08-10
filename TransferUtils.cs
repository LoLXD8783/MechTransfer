﻿using MechTransfer.ContainerAdapters;
using MechTransfer.Tiles;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace MechTransfer
{
    public static class TransferUtils
    {
        public static MechTransfer mod;
        private static List<Point16> relayTriggered = new List<Point16>(); //stops infinite recursion

        public static void EatWorldItem(int id, int eatNumber = 1)
        {
            EatItem(ref Main.item[id], eatNumber);

            if (Main.netMode == 2)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, id);
        }

        public static void EatItem(ref Item item, int eatNumber)
        {
            if (item.stack > eatNumber)
                item.stack -= eatNumber;
            else
                item = new Item();
        }

        public static bool StartTransfer(int startX, int startY, Item item)
        {
            relayTriggered.Clear();

            Item clone = item.Clone();
            clone.stack = 1;
            bool result = SearchForTarget(startX, startY, clone);

            relayTriggered.Clear();
            return result;
        }

        private static bool SearchForTarget(int startX, int startY, Item IToTransfer)
        {
            Queue<Point16> searchQ = new Queue<Point16>();
            HashSet<Point16> visited = new HashSet<Point16>();

            searchQ.Enqueue(new Point16(startX, startY));

            while (searchQ.Count > 0)
            {
                Point16 c = searchQ.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    Point16 searchP = new Point16();

                    switch (i)
                    {
                        case 0: searchP = new Point16(c.X + 1, c.Y); break;
                        case 1: searchP = new Point16(c.X - 1, c.Y); break;
                        case 2: searchP = new Point16(c.X, c.Y + 1); break;
                        case 3: searchP = new Point16(c.X, c.Y - 1); break;
                    }

                    if (visited.Contains(searchP))
                        continue;
                    visited.Add(searchP);

                    Tile tile = Main.tile[searchP.X, searchP.Y];
                    if (tile == null || !tile.active())
                        continue;

                    bool addToQ = false;

                    //checking for targets
                    if (tile.type == mod.TileType<TransferRelayTile>() && !relayTriggered.Contains(searchP))
                    {
                        if (tile.frameX == 0)
                        {
                            relayTriggered.Add(searchP);
                            if (SearchForTarget(searchP.X + 1, searchP.Y, IToTransfer))
                            {
                                mod.GetModWorld<MechTransferWorld>().TripWireDelayed(searchP.X, searchP.Y, 2, 1);
                                return true;
                            }
                        }
                        if (tile.frameX == 54)
                        {
                            relayTriggered.Add(searchP);
                            if (SearchForTarget(searchP.X - 1, searchP.Y, IToTransfer))
                            {
                                mod.GetModWorld<MechTransferWorld>().TripWireDelayed(searchP.X - 1, searchP.Y, 2, 1);
                                return true;
                            }
                        }
                    }
                    else if (tile.type == mod.TileType<TransferInjectorTile>() && InjectItem(searchP.X, searchP.Y, IToTransfer))
                    {
                        mod.GetModWorld<MechTransferWorld>().TripWireDelayed(searchP.X, searchP.Y, 1, 1);
                        return true;
                    }
                    else if (tile.type == mod.TileType<TransferOutletTile>())
                    {
                        DropItem(searchP.X, searchP.Y, IToTransfer);
                        mod.GetModWorld<MechTransferWorld>().TripWireDelayed(searchP.X, searchP.Y, 1, 1);
                        return true;
                    }

                    //checking for pipes
                    else if (tile.type == mod.TileType<TransferPipeTile>())
                        addToQ = true;
                    else if (tile.type == mod.TileType<TransferInletTile>() && (tile.frameX == 0 || tile.frameX == 36))
                        addToQ = true;
                    else if (tile.type == mod.TileType<TransferGateTile>() && tile.frameY == 0)
                        addToQ = true;
                    else if (tile.type == mod.TileType<TransferFilterTile>())
                    {
                        int id = mod.GetTileEntity<TransferFilterTileEntity>().Find(searchP.X, searchP.Y);
                        if (id != -1 && (((TransferFilterTileEntity)TileEntity.ByID[id]).ItemId == 0 || IToTransfer.type == ((TransferFilterTileEntity)TileEntity.ByID[id]).ItemId))
                            addToQ = true;
                    }

                    if (addToQ)
                        searchQ.Enqueue(searchP);
                }
            }
            return false;
        }

        public static bool InjectItem(int x, int y, Item item)
        {
            List<ContainerAdapter> found = FindContainerAdjacent(x, y);

            foreach (var container in found)
            {
                if (container.InjectItem(item))
                    return true;
            }
            return false;
        }

        public static void DropItem(int x, int y, Item item)
        {
            int dropTarget = Item.NewItem(x * 16, y * 16, item.width, item.height, item.type);
            item.position = Main.item[dropTarget].position;
            item.velocity.X = 0;
            item.velocity.Y = 0;
            Main.item[dropTarget] = item;
        }

        //search order: Up, Down, Left, Right
        public static List<ContainerAdapter> FindContainerAdjacent(int x, int y)
        {
            List<ContainerAdapter> found = new List<ContainerAdapter>();
            ContainerAdapter c;

            c = FindContainer(x, y - 1);
            if (c != null)
                found.Add(c);
            c = FindContainer(x, y + 1);
            if (c != null)
                found.Add(c);
            c = FindContainer(x - 1, y);
            if (c != null)
                found.Add(c);
            c = FindContainer(x + 1, y);
            if (c != null)
                found.Add(c);

            return found;
        }

        public static ContainerAdapter FindContainer(int x, int y)
        {
            ContainerAdapterDefinition c;
            Tile tile = Main.tile[x, y];
            if (tile != null && tile.active() && mod.ContainerAdapters.TryGetValue(tile.type, out c))
                return c.GetAdapter(x, y);
            return null;
        }

        public static string ItemNameById(int id)
        {
            if (id == 0)
                return "";

            LocalizedText name = Lang.GetItemName(id);
            if (name == LocalizedText.Empty)
                return string.Format("Unknown item #{0}", id);
            return name.Value;
        }
    }
}