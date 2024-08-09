﻿using MechTransfer.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace MechTransfer
{
    internal class MechTransferAssemblerWorld : ModSystem
    {
        private Texture2D pixel;

        public override void OnWorldLoad()/* tModPorter Suggestion: Also override OnWorldUnload, and mirror your worldgen-sensitive data initialization in PreWorldGen */
        {
            if (!Main.dedServ)
            {
                pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.TransformationMatrix);

            //These should be recalculated for view space, but they don't really hurt anything like this, so...
            int screenLeft = (int)((Main.screenPosition.X) / 16f - 1f);
            int screenRight = (int)((Main.screenPosition.X + (float)Main.screenWidth) / 16f) + 2;
            int screenTop = (int)((Main.screenPosition.Y) / 16f - 1f);
            int screenBottom = (int)((Main.screenPosition.Y + (float)Main.screenHeight) / 16f) + 5;
            if (screenLeft < 0)
                screenLeft = 0;
            if (screenRight > Main.maxTilesX)
                screenRight = Main.maxTilesX;
            if (screenTop < 0)
                screenTop = 0;
            if (screenBottom > Main.maxTilesY)
                screenBottom = Main.maxTilesY;

            for (int x = screenLeft; x < screenRight; x++)
            {
                for (int y = screenTop; y < screenBottom; y++)
                {
                    if (Main.tile[x, y] != null && Main.tile[x, y].HasTile)
                    {
                        if (Main.tile[x, y].TileType == ModContent.TileType<TransferInjectorTile>() || Main.tile[x, y].TileType == ModContent.TileType<TransferExtractorTile>() || Main.tile[x, y].TileType == ModContent.TileType<StackExtractorTile>() || Main.tile[x, y].TileType == ModContent.TileType<TransferAssemblerTile>())
                        {
                            DrawTransition(x, y - 1, Mod.GetTexture("Tiles/Transitions/Top"));
                            DrawTransition(x, y + 1, Mod.GetTexture("Tiles/Transitions/Bottom"));
                            DrawTransition(x - 1, y, Mod.GetTexture("Tiles/Transitions/Left"));
                            DrawTransition(x + 1, y, Mod.GetTexture("Tiles/Transitions/Right"));
                        }

                        if (WiresUI.Settings.DrawWires && Main.tile[x, y].TileType == ModContent.TileType<TransferAssemblerTile>())
                        {
                            DrawRectFast(x * 16 - 80 - (int)Main.screenPosition.X, y * 16 - 80 - (int)Main.screenPosition.Y, 176, 176);
                        }
                    }
                }
            }

            Main.spriteBatch.End();
        }

        private void DrawRectFast(int left, int top, int height, int width)
        {
            if (Main.LocalPlayer.gravDir == -1)
                top = Main.screenHeight - top - height;

            Main.spriteBatch.Draw(pixel, new Rectangle(left, top, width, 2), null, Color.LightSeaGreen);
            Main.spriteBatch.Draw(pixel, new Rectangle(left, top + height, width, 2), null, Color.LightSeaGreen);
            Main.spriteBatch.Draw(pixel, new Rectangle(left, top, 2, height), null, Color.LightSeaGreen);
            Main.spriteBatch.Draw(pixel, new Rectangle(left + width, top, 2, height), null, Color.LightSeaGreen);
        }

        private void DrawTransition(int x, int y, Texture2D texture)
        {
            if (ModContent.GetInstance<TransferAgent>().IsContainer(x, y))
            {
                if (Main.LocalPlayer.gravDir == 1)
                    Main.spriteBatch.Draw(texture, new Vector2(x * 16 - Main.screenPosition.X, y * 16 - Main.screenPosition.Y), Lighting.GetColor(x, y));
                else
                    Main.spriteBatch.Draw(texture, new Vector2(x * 16 - Main.screenPosition.X, Main.screenHeight - y * 16 + Main.screenPosition.Y - 16), null, Lighting.GetColor(x, y), 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
            }
        }
    }
}