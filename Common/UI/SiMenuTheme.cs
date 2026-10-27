using Microsoft.Xna.Framework;

namespace EndfieldWings.Common.UI
{
	/// <summary>主界面主题的配色与布局常量。</summary>
	public static class SiMenuTheme
	{
		public static readonly Color TitleColor = new(240, 232, 210);
		public static readonly Color SubtitleColor = new(210, 200, 180);
		public static readonly Color TextColor = new(230, 224, 206);
		public static readonly Color HoverColor = new(255, 214, 150);
		public static readonly Color DimColor = new(160, 150, 138);

		// 左侧按钮列
		public const float ButtonX = 92f;
		public const float ButtonTop = 230f;
		public const float ButtonStep = 54f;
		public const float ButtonTextScale = 1.05f;

		public const float TitleX = 92f;
		public const float TitleY = 130f;
		public const float TitleScale = 1.5f;

		public const float SubtitleY = 182f;
		public const float SubtitleScale = 0.9f;
	}
}
