using Terraria.ModLoader;

namespace MechTransfer.Tiles.Simple
{
    public abstract class SimpleTile : ModTile
    {
        public virtual void PostLoad()
        {
        }

        public virtual void AddRecipes()
        {
        }

        public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */
        {
            return false;
        }
    }
}