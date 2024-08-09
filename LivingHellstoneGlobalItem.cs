using MechTransfer.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace MechTransfer
{
    internal class LivingHellstoneGlobalItem : GlobalItem
    {
        private int burnTime = 0;

        public override bool InstancePerEntity { get { return true; } }
        protected override bool CloneNewInstances { get { return true; } }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (item.type == Mod.PlaceItemType<LivingHellstoneTile>())
                return;

            foreach (var p in Collision.GetEntityEdgeTiles(item))
            {
                if (p.X > 0 && p.X < Main.maxTilesX && p.Y > 0 && p.Y < Main.maxTilesY &&
                    Main.tile[p.X, p.Y].HasTile && Main.tile[p.X, p.Y].TileType == ModContent.TileType<LivingHellstoneTile>())
                {
                    if (++burnTime > 0)
                    {
                        Dust.NewDust(item.position, item.width, item.height, 6);
                    }
                    if (burnTime > 60)
                    {
                        item.active = false;
                    }
                    return;
                }
            }
            burnTime = 0;
        }
    }
}