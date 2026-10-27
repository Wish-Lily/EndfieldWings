using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace EndfieldWings.Common.Configs
{
	/// <summary>祀岁翼的客户端设置。</summary>
	[Label("祀岁翼")]
	public class WingsClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("显示合并 HUD")]
		[Tooltip("关闭后不显示右上角的权能 / 岁时面板")]
		[DefaultValue(true)]
		public bool HudVisible;

		[Label("允许右键拖动 HUD")]
		[Tooltip("关闭后 HUD 固定在当前位置")]
		[DefaultValue(true)]
		public bool HudDraggable;

		[Label("主界面接管（实验）")]
		[Tooltip("标题页自绘背景与左侧按钮；不稳定时请关闭")]
		[DefaultValue(false)]
		public bool UseMenuTakeover;
	}
}
