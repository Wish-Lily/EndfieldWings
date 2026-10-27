using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace EndfieldWings.Common.Systems
{
	/// <summary>
	/// 形：佩戴祀岁翼并开启「年」时，临时取消所有配方的制造环境要求；
	/// 关掉后恢复原样。
	/// </summary>
	public class RecipeUnlockSystem : ModSystem
	{
		private static int[][] originalTiles;
		private static bool unlocked;

		public override void PostSetupRecipes()
		{
			originalTiles = new int[Main.recipe.Length][];
			for (int i = 0; i < Main.recipe.Length; i++) {
				originalTiles[i] = Main.recipe[i].requiredTile.ToArray();
			}
		}

		public override void PostUpdateEverything()
		{
			if (Main.dedServ) {
				return;
			}

			Player player = Main.LocalPlayer;
			bool want = player != null && player.active
				&& player.GetModPlayer<AuthorityPlayer>().Active(8);

			if (want == unlocked) {
				return;
			}

			unlocked = want;
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				recipe.requiredTile.Clear();
				if (!want) {
					recipe.requiredTile.AddRange(originalTiles[i]);
				}
			}
		}
	}
}
