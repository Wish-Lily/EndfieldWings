using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories.Shichen
{
	/// <summary>
	/// 子武剑 · 子时（朔·武）
	/// 十二刻印之一。被动：攻击 20% 概率无视防御。
	/// </summary>
	public class ZiWuSword : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;
		}

		/// <summary>十二刻印之一。被动：攻击 20% 概率无视防御。</summary>
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ShichenPlayer>().IgnoreDefenseChance = 0.2f;
		}

	}
}
