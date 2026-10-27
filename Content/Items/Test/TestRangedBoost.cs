using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Test
{
	/// <summary>测试用：远程伤害 +100%。</summary>
	public class TestRangedBoost : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.value = 0;
			Item.rare = ItemRarityID.White;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Ranged) += 1f;
		}

		public override void AddRecipes()
		{
			CreateRecipe().AddIngredient(ItemID.Wood, 1).AddTile(TileID.WorkBenches).Register();
		}
	}
}
