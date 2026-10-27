using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 戌绘卷 · 戌时（夕·绘）
	/// 十二刻印之一。画卷空间（大背包）待实现。
	/// </summary>
	public class XuHuiScroll : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			// 画卷空间：需要一个独立背包 UI + 存档 + 多人同步，后续实现
		}

	}
}
