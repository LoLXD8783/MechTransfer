using MechTransfer.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MechTransfer
{
    internal class MechTransferMechanic : GlobalNPC
    {
        public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
        {
            if (type == NPCID.Mechanic)
            {
                shop.item[nextSlot++].SetDefaults(ModContent.ItemType<PneumaticActuatorItem>());
                shop.item[nextSlot++].SetDefaults(Mod.Find<ModItem>("AnyFilterItem").Type);
            }
        }
    }
}