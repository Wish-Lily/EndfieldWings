using System;
using EndfieldWings.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EndfieldWings.Common.UI
{
	/// <summary>
	/// 画卷空间：50 格随身储物。U 键开关（需要装备戌绘卷或开启「意」）。
	/// 点击格子与鼠标上的物品交换。
	/// </summary>
	public class ScrollSpaceUI : ModSystem
	{
		public static bool Open;

		private const int Cols = 10;
		private const int Rows = 5;
		private const int Slot = 44;
		private const int Pad = 4;
		private const int TitleH = 28;

		private const int PanelW = Cols * (Slot + Pad) + Pad;
		private const int PanelH = Rows * (Slot + Pad) + Pad + TitleH;

		private static int hoverIndex = -1;

		public override void Unload()
		{
			Open = false;
			hoverIndex = -1;
		}

		private static Rectangle PanelRect()
		{
			return new Rectangle((Main.screenWidth - PanelW) / 2, (Main.screenHeight - PanelH) / 2, PanelW, PanelH);
		}

		private static Rectangle SlotRect(int index)
		{
			Rectangle panel = PanelRect();
			int col = index % Cols;
			int row = index / Cols;
			return new Rectangle(
				panel.X + Pad + col * (Slot + Pad),
				panel.Y + TitleH + Pad + row * (Slot + Pad),
				Slot, Slot);
		}

		public override void PostUpdateInput()
		{
			if (Main.dedServ || Main.gameMenu) {
				return;
			}

			Player player = Main.LocalPlayer;
			ScrollSpacePlayer space = player.GetModPlayer<ScrollSpacePlayer>();

			if (!space.HasScrollSpace()) {
				Open = false;
				return;
			}

			if (EndfieldWings.ScrollKey?.JustPressed == true) {
				Open = !Open;
				if (Open) {
					Main.playerInventory = true;
				}
			}

			if (!Open) {
				hoverIndex = -1;
				return;
			}

			// 关掉背包就一起收起来
			if (!Main.playerInventory) {
				Open = false;
				return;
			}

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) {
				Open = false;
				return;
			}

			bool clicked = Main.mouseLeft && Main.mouseLeftRelease;
			Point mouse = new(Main.mouseX, Main.mouseY);
			hoverIndex = -1;

			for (int i = 0; i < ScrollSpacePlayer.Slots; i++) {
				if (!SlotRect(i).Contains(mouse)) {
					continue;
				}

				hoverIndex = i;
				player.mouseInterface = true;

				if (clicked) {
					// 与鼠标上的物品互换
					Item temp = Main.mouseItem;
					Main.mouseItem = space.Items[i];
					space.Items[i] = temp;
					// 重新放入的物品可以再次复制
					space.OnSlotChanged(i);
				}

				break;
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (Main.dedServ || Main.gameMenu || !Open) {
				return;
			}

			Player player = Main.LocalPlayer;
			ScrollSpacePlayer space = player.GetModPlayer<ScrollSpacePlayer>();
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle panel = PanelRect();

			// 背板
			spriteBatch.Draw(pixel, panel, new Color(16, 14, 20) * 0.88f);

			// 标题栏
			spriteBatch.Draw(pixel, new Rectangle(panel.X, panel.Y, panel.Width, TitleH),
				new Color(60, 48, 40) * 0.95f);
			Utils.DrawBorderString(spriteBatch, "画卷空间", new Vector2(panel.X + 10, panel.Y + 4),
				new Color(232, 226, 208), 0.9f);

			for (int i = 0; i < ScrollSpacePlayer.Slots; i++) {
				Rectangle rect = SlotRect(i);
				Color bg = i == hoverIndex ? new Color(90, 74, 56) : new Color(32, 28, 36);
				spriteBatch.Draw(pixel, rect, bg * 0.9f);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), Color.Black * 0.5f);

				Item item = space.Items[i];
				if (item == null || item.IsAir) {
					continue;
				}

				Texture2D tex = TextureAssets.Item[item.type].Value;
				float scale = Math.Min(1f, 32f / Math.Max(tex.Width, tex.Height));
				spriteBatch.Draw(tex, rect.Center.ToVector2(), null, Color.White, 0f,
					tex.Size() / 2f, scale, SpriteEffects.None, 0f);

				if (item.stack > 1) {
					Utils.DrawBorderString(spriteBatch, item.stack.ToString(),
						new Vector2(rect.X + 4, rect.Y + 24), Color.White, 0.75f);
				}
			}

			// 悬停提示物品名
			if (hoverIndex >= 0) {
				Item hovered = space.Items[hoverIndex];
				if (hovered != null && !hovered.IsAir) {
					Utils.DrawBorderString(spriteBatch, hovered.Name,
						new Vector2(panel.X, panel.Bottom + 4), Color.White, 0.9f);
				}
			}
		}
	}
}
