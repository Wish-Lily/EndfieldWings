using System;
using EndfieldWings.Common.Authorities;
using EndfieldWings.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace EndfieldWings.Common.UI
{
	/// <summary>
	/// 佩戴祀岁翼时右下角的权能图标，点开后是十二权能轮盘。
	/// 轮盘从右上 45° 起顺时针排列：朔、望、令、均、颉、黍、绩、易、年、方、夕、余。
	/// 左键点印记开关权能；O 键或图标开关轮盘；右键 / Esc 关闭。
	/// </summary>
	public class AuthorityWheelUI : ModSystem
	{
		public static bool Open;

		private const int Count = 12;
		private const float Radius = 200f;
		private static readonly float StartAngle = -MathHelper.PiOver4;

		private static Texture2D wheelBg;
		private static Texture2D[] seals;
		private static bool loaded;

		private static float openProgress;
		private static int hoverIndex = -1;
		private static bool hoverCenter;

		public override void Unload()
		{
			wheelBg = null;
			seals = null;
			loaded = false;
			Open = false;
			openProgress = 0f;
		}

		private static void EnsureLoaded()
		{
			if (loaded) {
				return;
			}

			loaded = true;
			wheelBg = TryLoad("EndfieldWings/Assets/UI/AuthorityWheelBg");

			seals = new Texture2D[Count];
			for (int i = 0; i < Count; i++) {
				seals[i] = TryLoad($"EndfieldWings/Assets/UI/Seal_{i:00}");
			}
		}

		private static Texture2D TryLoad(string path)
		{
			try {
				return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
			}
			catch {
				return null;
			}
		}

		private static Vector2 Center => new(Main.screenWidth / 2f, Main.screenHeight / 2f);

		private static Vector2 SealPosition(int index)
		{
			float angle = StartAngle + MathHelper.TwoPi * index / Count;
			return Center + angle.ToRotationVector2() * Radius;
		}

		private static int HitTest(Vector2 mouse)
		{
			for (int i = 0; i < Count; i++) {
				if (Vector2.Distance(mouse, SealPosition(i)) <= 34f) {
					return i;
				}
			}

			return -1;
		}

		public override void PostUpdateInput()
		{
			if (Main.dedServ || Main.gameMenu) {
				return;
			}

			Player player = Main.LocalPlayer;
			if (!player.GetModPlayer<SiWingsPlayer>().WingsEquipped) {
				Open = false;
				openProgress = 0f;
				return;
			}

			// O 键开关（DragonLens 的 Tool 热键思路）
			if (EndfieldWings.WheelKey?.JustPressed == true) {
				Open = !Open;
			}

			Vector2 mouse = new(Main.mouseX, Main.mouseY);
			bool clicked = Main.mouseLeft && Main.mouseLeftRelease;
			bool rightClicked = Main.mouseRight && Main.mouseRightRelease;

			// 动画进度
			float target = Open ? 1f : 0f;
			openProgress = MathHelper.Lerp(openProgress, target, 0.18f);
			if (Math.Abs(openProgress - target) < 0.01f) {
				openProgress = target;
			}

			if (!Open) {
				hoverIndex = -1;
				hoverCenter = false;
				return;
			}

			if (Main.keyState.IsKeyDown(Keys.Escape) || rightClicked) {
				Open = false;
				return;
			}

			AuthorityPlayer authority = player.GetModPlayer<AuthorityPlayer>();

			// 中心：演化开关
			if (Vector2.Distance(mouse, Center) <= 48f) {
				player.mouseInterface = true;
				hoverCenter = true;
				if (clicked) {
					authority.EvolutionOn = !authority.EvolutionOn;
				}

				return;
			}

			hoverCenter = false;
			hoverIndex = HitTest(mouse);
			if (hoverIndex >= 0) {
				player.mouseInterface = true;
				if (clicked) {
					authority.Toggle(hoverIndex);
				}

				return;
			}

			// 点轮盘外关闭
			if (clicked) {
				Open = false;
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (Main.dedServ || Main.gameMenu) {
				return;
			}

			Player player = Main.LocalPlayer;
			if (!player.GetModPlayer<SiWingsPlayer>().WingsEquipped) {
				return;
			}

			EnsureLoaded();

			if (openProgress <= 0.02f || wheelBg == null || seals == null) {
				return;
			}

			AuthorityPlayer authority = player.GetModPlayer<AuthorityPlayer>();
			Vector2 center = Center;
			float scale = 0.85f + 0.15f * openProgress;
			float alpha = openProgress;
			Color tint = Color.White * alpha;

			// 背景
			Vector2 bgOrigin = new(wheelBg.Width / 2f, wheelBg.Height / 2f);
			spriteBatch.Draw(wheelBg, center, null, tint, 0f, bgOrigin, 0.82f * scale,
				SpriteEffects.None, 0f);

			// 十二印记
			for (int i = 0; i < Count; i++) {
				if (seals[i] == null) {
					continue;
				}

				bool on = authority.IsOn(i);
				Color color = (on ? Color.White : new Color(80, 80, 90)) * alpha;
				float sealScale = (on ? 1.05f : 0.92f) * scale;
				if (i == hoverIndex) {
					sealScale *= 1.12f;
					color = Color.Lerp(color, new Color(255, 230, 160) * alpha, 0.55f);
				}

				Vector2 origin = new(seals[i].Width / 2f, seals[i].Height / 2f);
				spriteBatch.Draw(seals[i], SealPosition(i), null, color, 0f, origin, sealScale,
					SpriteEffects.None, 0f);
			}

			// 中心：演化开关
			if (authority.EvolutionOn) {
				Color glow = new Color(255, 226, 150) * (0.35f * alpha);
				spriteBatch.Draw(wheelBg, center, null, glow, 0f, bgOrigin, 0.5f * scale,
					SpriteEffects.None, 0f);
			}

			// 悬停信息
			if (hoverCenter) {
				Utils.DrawBorderString(spriteBatch, "演化", new Vector2(center.X - 30f, center.Y + Radius + 48f),
					new Color(255, 230, 160), 1.1f);
				Utils.DrawBorderString(spriteBatch,
					authority.EvolutionOn ? "开启：免疫一切伤害（点击关闭）" : "关闭：点击开启",
					new Vector2(center.X - 150f, center.Y + Radius + 74f), Color.White, 0.9f);
			}
			else if (hoverIndex >= 0) {
				string title = AuthorityInfo.Names[hoverIndex];
				string effect = AuthorityInfo.Effects[hoverIndex];
				Vector2 textPos = new(center.X - 160f, center.Y + Radius + 48f);
				Utils.DrawBorderString(spriteBatch, title, textPos, new Color(255, 230, 160), 1.1f);
				Utils.DrawBorderString(spriteBatch, effect, textPos + new Vector2(0f, 26f), Color.White, 0.95f);
			}
		}
	}
}
