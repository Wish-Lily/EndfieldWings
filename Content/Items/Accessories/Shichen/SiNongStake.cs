using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 巳农桩 · 巳时（黍·农）
	/// 十二刻印之一。命中种「因」（每敌最多 2 层），叠满后 5 秒结果：
	/// 造成池内 60% 伤害（单体），并回复 3% 的结算伤害。
	/// </summary>
	public class SiNongStake : ModItem
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
			player.GetModPlayer<ShichenPlayer>().SeedHarvest = true;
		}

	}
}
