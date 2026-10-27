using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 午商梭 · 午时（绩·商）
	/// 十二刻印之一。被动：免疫环境伤害与爆炸伤害；NPC 处购买价格减半。
	/// </summary>
	public class WuShangShuttle : ModItem
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
			player.GetModPlayer<ShichenPlayer>().Merchant = true;
		}

	}
}
