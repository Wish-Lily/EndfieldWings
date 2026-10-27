using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 亥食灶 · 亥时（余·食）
	/// 十二刻印之一。被动：每秒恢复自身 0.5% 最大生命值。
	/// </summary>
	public class HaiShiStove : ModItem
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
			player.GetModPlayer<ShichenPlayer>().PercentRegen = true;
		}

	}
}
