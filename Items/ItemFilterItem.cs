using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechTransfer.Items
{
    public class ItemFilterItem : ModItem
    {
        public delegate bool MatchConditionn(Item item);

        public int recipeItem = -1;
        public int Rarity = 0;
        public bool expert = false;

        private MatchConditionn matchConditionn;

        protected override bool CloneNewInstances { get { return true; } }

        public ItemFilterItem(MatchConditionn matchConditionn)
        {
            this.matchConditionn = matchConditionn;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = Rarity;
            Item.expert = expert;
        }

        public bool MatchesItem(Item item)
        {
            return matchConditionn(item);
        }

        public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */
        {
            return false;
        }

        public override void AddRecipes()
        {
            if (recipeItem != -1)
            {
                Recipe r = Recipe.Create(Item.type, 1);
                r.AddIngredient(Mod.Find<ModItem>("AnyFilterItem").Type, 1);
                r.AddIngredient(recipeItem, 1);
                r.AddTile(TileID.WorkBenches);
                r.Register();
            }
        }

        //Needed to stop ModLoader from assigning a default display name
        public override void AutoStaticDefaults()
        {
            TextureAssets.Item[Item.type].Value = ModContent.GetTexture(Texture);
        }
    }
}