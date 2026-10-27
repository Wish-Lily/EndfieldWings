using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 寅诗壶 · 寅时（令·诗）
	/// 十二刻印之一。被动：免疫常规负面 buff。
	/// （具体逻辑在 ShichenPlayer.ResetEffects 里按装备检测）
	/// </summary>
	public class YinShiPot : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;
		}
	}
}
