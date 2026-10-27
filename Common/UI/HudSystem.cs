using System;
using EndfieldWings.Common.Configs;
using EndfieldWings.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace EndfieldWings.Common.UI
{
	/// <summary>
	/// 合并 HUD：权能入口 + 领域状态，右上角小地图左侧，可右键拖动。
	/// </summary>
	public class HudSystem : ModSystem
	{
		private const int Width = 236;
		private const int Height = 112;

		private static Texture2D themeBg;
		private static Texture2D icon;
		private static bool loaded;

		private static bool dragging;
		private static Vector2 dragOffset;

		private static WingsClientConfig Config => ModContent.GetInstance<WingsClientConfig>();

		public override void Unload()
		{
			themeBg = null;
			icon = null;
			loaded = false;
		}

		private static void EnsureLoaded()
		{
			if (loaded) {
				return;
			}

			loaded = true;
			try {
				themeBg = ModContent.Request<Texture2D>("EndfieldWings/Assets/UI/Theme/ThemeBg",
					AssetRequestMode.ImmediateLoad).Value;
				icon = ModContent.Request<Texture2D>("EndfieldWings/Assets/UI/Seal_Si",
					AssetRequestMode.ImmediateLoad).Value;
			}
			catch {
				// 资源缺失时 HUD 仍然工作，只是没有背景
			}
		}

		private static Rectangle HudRect(Player player)
		{
			SiWingsPlayer wings = player.GetModPlayer<SiWingsPlayer>();
			float x = wings.HudX;
			float y = wings.HudY;

			if (x < 0f || y < 0f) {
				// 默认：右上角小地图左侧
				x = Main.screenWidth - Width - 300f;
				y = 56f;
			}

			x = MathHelper.Clamp(x, 4f, Main.screenWidth - Width - 4f);
			y = MathHelper.Clamp(y, 4f, Main.screenHeight - Height - 4f);
			return new Rectangle((int)x, (int)y, Width, Height);
		}

		public override void PostUpdateInput()
		{
			if (Main.dedServ || Main.gameMenu) {
				return;
			}

			WingsClientConfig config = Config;
			Player player = Main.LocalPlayer;

			if (!config.HudVisible || !player.GetModPlayer<SiWingsPlayer>().WingsEquipped) {
				dragging = false;
				return;
			}

			Rectangle rect = HudRect(player);
			Point mouse = new(Main.mouseX, Main.mouseY);
			bool over = rect.Contains(mouse);

			if (over) {
				player.mouseInterface = true;
			}

			// 右键拖动
			if (config.HudDraggable) {
				if (Main.mouseRight && over && !dragging) {
					dragging = true;
					dragOffset = new Vector2(mouse.X - rect.X, mouse.Y - rect.Y);
				}

				if (dragging) {
					SiWingsPlayer wings = player.GetModPlayer<SiWingsPlayer>();
					if (Main.mouseRight) {
						wings.HudX = MathHelper.Clamp(mouse.X - dragOffset.X, 4f,
							Main.screenWidth - Width - 4f);
						wings.HudY = MathHelper.Clamp(mouse.Y - dragOffset.Y, 4f,
							Main.screenHeight - Height - 4f);
					}
					else {
						dragging = false;
					}
				}
			}

			// 左键点图标：开关权能轮盘
			Rectangle iconRect = new(rect.X + 8, rect.Y + 8, 44, 44);
			if (iconRect.Contains(mouse) && Main.mouseLeft && Main.mouseLeftRelease) {
				AuthorityWheelUI.Open = !AuthorityWheelUI.Open;
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (Main.dedServ || Main.gameMenu) {
				return;
			}

			WingsClientConfig config = Config;
			Player player = Main.LocalPlayer;

			if (!config.HudVisible || !player.GetModPlayer<SiWingsPlayer>().WingsEquipped) {
				return;
			}

			EnsureLoaded();
			Rectangle rect = HudRect(player);

			// 主题背景
			if (themeBg != null) {
				float scale = Math.Max(rect.Width / (float)themeBg.Width, rect.Height / (float)themeBg.Height);
				Vector2 origin = new(themeBg.Width / 2f, themeBg.Height / 2f);
				spriteBatch.Draw(themeBg, rect.Center.ToVector2(), null, Color.White * 0.75f, 0f,
					origin, scale, SpriteEffects.None, 0f);
			}

			// 暗化 + 边框
			Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(pixel, rect, new Color(12, 10, 16) * 0.45f);
			spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), new Color(210, 190, 150) * 0.8f);
			spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), new Color(210, 190, 150) * 0.5f);

			// 图标
			if (icon != null) {
				spriteBatch.Draw(icon, new Vector2(rect.X + 8, rect.Y + 8), null, Color.White,
					0f, Vector2.Zero, 44f / icon.Width, SpriteEffects.None, 0f);
			}

			AuthorityPlayer authority = player.GetModPlayer<AuthorityPlayer>();
			int on = 0;
			for (int i = 0; i < AuthorityPlayer.Count; i++) {
				if (authority.IsOn(i)) {
					on++;
				}
			}

			Utils.DrawBorderString(spriteBatch, $"权能 {on}/{AuthorityPlayer.Count}",
				new Vector2(rect.X + 60, rect.Y + 10), new Color(240, 232, 210), 0.95f);
			Utils.DrawBorderString(spriteBatch, "岁时：未激活",
				new Vector2(rect.X + 60, rect.Y + 34), new Color(180, 200, 220), 0.8f);
			Utils.DrawBorderString(spriteBatch, "右键拖动 · 左键图标开轮盘",
				new Vector2(rect.X + 10, rect.Bottom - 28), new Color(200, 190, 175) * 0.8f, 0.7f);
		}
	}
}

