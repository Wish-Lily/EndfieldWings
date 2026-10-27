using System;
using EndfieldWings.Common.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EndfieldWings.Common.UI
{
	/// <summary>
	/// 主界面接管（仿 CalamityOverhaul 的 MenuOverride）。
	/// 关键点：进入钩子时批次可能是开的，也可能没开（取决于其他 mod 的钩子顺序），
	/// 所以先尝试 End 并记住结果，结束时按原状态恢复，避免批次悬空导致崩溃。
	/// </summary>
	public class SiMenuOverride : ModSystem
	{
		private static Texture2D bg;
		private static bool loaded;
		private static bool faulted;

		private const int VanillaTop = 220;
		private const int VanillaStep = 52;
		private const int VanillaHeight = 44;

		private static readonly string[] Labels = {
			"单人模式", "多人模式", "成就", "创意工坊", "设置", "制作人员", "退出"
		};

		public override void Load()
		{
			// 停用：与 Fargo / InnoVault 的 DrawMenu 批次管理无法稳定共存，启用即崩。
			// 保留代码，等有可靠方案（或引入 InnoVault）再开。
			// On_Main.DrawMenu += DrawMenuHook;
		}

		public override void Unload()
		{
			// On_Main.DrawMenu -= DrawMenuHook;
			bg = null;
			loaded = false;
			faulted = false;
		}

		private static bool Active()
		{
			if (faulted || !Main.gameMenu || Main.menuMode != 0) {
				return false;
			}

			try {
				return ModContent.GetInstance<WingsClientConfig>().UseMenuTakeover
					&& ModContent.GetInstance<SiMenu>().IsSelected;
			}
			catch {
				return false;
			}
		}

		private static void DrawMenuHook(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			if (!Active()) {
				orig(self, gameTime);
				return;
			}

			EnsureAssets();

			SpriteBatch sb = Main.spriteBatch;
			bool hadBatch = TryEnd(sb);
			int clicked = HitOurButton();
			bool doClick = clicked >= 0 && Main.mouseLeft && Main.mouseLeftRelease;

			if (doClick) {
				int oldX = Main.mouseX;
				int oldY = Main.mouseY;
				bool oldLeft = Main.mouseLeft;
				bool oldRelease = Main.mouseLeftRelease;

				Main.mouseX = Main.screenWidth / 2;
				Main.mouseY = VanillaTop + clicked * VanillaStep + VanillaHeight / 2;
				Main.mouseLeft = true;
				Main.mouseLeftRelease = true;

				try {
					orig(self, gameTime);
				}
				catch {
					// 原版跳转失败也不让游戏崩
				}
				finally {
					Main.mouseX = oldX;
					Main.mouseY = oldY;
					Main.mouseLeft = oldLeft;
					Main.mouseLeftRelease = oldRelease;

					if (hadBatch) {
						SafeBegin(sb);
					}
				}

				if (Main.menuMode != 0) {
					DrawCursorSafe(sb, hadBatch);
					return;
				}
			}

			try {
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
				DrawOverlay(sb);
				sb.End();
			}
			catch (Exception) {
				faulted = true;
				try {
					sb.End();
				}
				catch {
				}
			}

			if (hadBatch) {
				SafeBegin(sb);
			}

			DrawCursorSafe(sb, hadBatch);

			Main.mouseLeftRelease = !Main.mouseLeft;
			Main.mouseRightRelease = !Main.mouseRight;
			Main.mouseMiddleRelease = !Main.mouseMiddle;
			Main.mouseXButton1Release = !Main.mouseXButton1;
			Main.mouseXButton2Release = !Main.mouseXButton2;
		}

		/// <summary>尝试关闭当前批次，返回原本是否开着。</summary>
		private static bool TryEnd(SpriteBatch sb)
		{
			try {
				sb.End();
				return true;
			}
			catch {
				return false;
			}
		}

		private static void SafeBegin(SpriteBatch sb)
		{
			try {
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
			}
			catch {
			}
		}

		private static void DrawCursorSafe(SpriteBatch sb, bool hadBatch)
		{
			try {
				if (hadBatch) {
					Main.DrawCursor(Main.DrawThickCursor());
				}
				else {
					SafeBegin(sb);
					Main.DrawCursor(Main.DrawThickCursor());
					sb.End();
				}
			}
			catch {
			}
		}

		private static void EnsureAssets()
		{
			if (loaded) {
				return;
			}

			loaded = true;
			try {
				bg = ModContent.Request<Texture2D>("EndfieldWings/Assets/UI/Theme/ThemeBg",
					AssetRequestMode.ImmediateLoad).Value;
			}
			catch {
				bg = null;
			}
		}

		private static Rectangle ButtonRect(int index)
		{
			Vector2 size = FontAssets.MouseText.Value.MeasureString(Labels[index]) * SiMenuTheme.ButtonTextScale;
			return new Rectangle((int)SiMenuTheme.ButtonX,
				(int)(SiMenuTheme.ButtonTop + index * SiMenuTheme.ButtonStep),
				(int)size.X + 20, (int)size.Y + 4);
		}

		private static int HitOurButton()
		{
			Point mouse = new(Main.mouseX, Main.mouseY);

			for (int i = 0; i < Labels.Length; i++) {
				if (ButtonRect(i).Contains(mouse)) {
					return i;
				}
			}

			return -1;
		}

		private static void DrawOverlay(SpriteBatch spriteBatch)
		{
			if (bg != null) {
				spriteBatch.Draw(bg, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
			}

			Utils.DrawBorderString(spriteBatch, "终末地 · 祀岁翼",
				new Vector2(SiMenuTheme.TitleX, SiMenuTheme.TitleY), SiMenuTheme.TitleColor,
				SiMenuTheme.TitleScale);
			Utils.DrawBorderString(spriteBatch, "EndfieldWings",
				new Vector2(SiMenuTheme.TitleX + 4f, SiMenuTheme.SubtitleY),
				SiMenuTheme.SubtitleColor * 0.8f, SiMenuTheme.SubtitleScale);

			Point mouse = new(Main.mouseX, Main.mouseY);

			for (int i = 0; i < Labels.Length; i++) {
				bool over = ButtonRect(i).Contains(mouse);
				Color color = over ? SiMenuTheme.HoverColor : SiMenuTheme.TextColor;
				Utils.DrawBorderString(spriteBatch, Labels[i],
					new Vector2(SiMenuTheme.ButtonX, SiMenuTheme.ButtonTop + i * SiMenuTheme.ButtonStep),
					color, SiMenuTheme.ButtonTextScale);
			}
		}
	}
}
