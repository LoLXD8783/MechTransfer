using Terraria.ModLoader;

namespace MechTransfer.Tiles.Simple
{
    public abstract class SimplePlaceableTile : SimpleTile
    {
        public ModItem PlaceItem { get; protected set; }

        public override void SetStaticDefaults()
        {
            ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = PlaceItem.Item.type;
        }
    }
}