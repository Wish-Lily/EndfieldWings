using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Test
{
	/// <summary>测试用：伤害 100 的枪，方便验证相合。</summary>
	public class TestGun : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 20;
			Item.damage = 100;
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item11;
			Item.shoot = ProjectileID.Bullet;
			Item.shootSpeed = 12f;
			Item.value = 0;
			Item.rare = ItemRarityID.White;
		}

		public override void AddRecipes()
		{
			CreateRecipe().AddIngredient(ItemID.Wood, 1).AddTile(TileID.WorkBenches).Register();
		}
	}
}
