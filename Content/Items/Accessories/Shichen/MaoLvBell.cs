using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 卯律钟 · 卯时（均·律）
	/// 十二刻印之一。被动：自身受伤判定框缩小三分之一。
	/// </summary>
	public class MaoLvBell : ModItem
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
			player.GetModPlayer<ShichenPlayer>().SmallHitbox = true;
		}

	}
}
