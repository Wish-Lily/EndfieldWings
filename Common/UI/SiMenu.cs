using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace EndfieldWings.Common.UI
{
	/// <summary>主界面主题：祀的宣传图 + 《花祀时》。</summary>
	public class SiMenu : ModMenu
	{
		public override void SetStaticDefaults()
		{
			// 让主界面默认选中本主题：直接改 tML 记录的「上次选择的菜单」。
			// 反射失败也不影响手动切换。
			try {
				var field = typeof(MenuLoader).GetField("LastSelectedModMenu",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
				field?.SetValue(null, FullName);
			}
			catch {
			}
		}

		public override string DisplayName => "祀岁翼 · 花祀时";

		public override int Music {
			get {
				try {
					return MusicLoader.GetMusicSlot(Mod, "Assets/Music/HuaSiShi");
				}
				catch {
					return -1;
				}
			}
		}

		public override ModSurfaceBackgroundStyle MenuBackgroundStyle =>
			ModContent.GetInstance<SiMenuBackgroundStyle>();

		/// <summary>这个主题不画 tModLoader 的 logo。</summary>
		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter,
			ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			return false;
		}
	}

	/// <summary>主界面背景：整屏绘制宣传图。</summary>
	public class SiMenuBackgroundStyle : ModSurfaceBackgroundStyle
	{
		private static Texture2D texture;

		public override void ModifyFarFades(float[] fades, float transitionSpeed)
		{
			for (int i = 0; i < fades.Length; i++) {
				if (i == Slot) {
					fades[i] = MathHelper.Clamp(fades[i] + transitionSpeed, 0f, 1f);
				}
				else {
					fades[i] = MathHelper.Clamp(fades[i] - transitionSpeed, 0f, 1f);
				}
			}
		}

		public override int ChooseFarTexture()
		{
			return -1;
		}

		public override int ChooseMiddleTexture()
		{
			return -1;
		}

		public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
		{
			if (texture == null) {
				try {
					texture = ModContent.Request<Texture2D>("EndfieldWings/Assets/UI/Theme/ThemeBg",
						AssetRequestMode.ImmediateLoad).Value;
				}
				catch {
					return true;
				}
			}

			spriteBatch.Draw(texture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
			return false;
		}
	}
}
