using EndfieldWings.Common.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Common.GlobalNPCs
{
	/// <summary>
	/// 因果的「因」标记（完整版 5 层 / 巳农桩 2 层）。
	/// 完整版：池子 × 150%，范围伤害，回 10%；
	/// 巳农桩：池子 × 60%，单体，回 3%。
	/// </summary>
	public class ShichenNPC : GlobalNPC
	{
		public int SeedStacks;
		public float SeedPool;
		public int SeedTimer;
		public int SeedMax;
		public bool SeedImmediate;
		public int SeedOwner = -1;

		public override bool InstancePerEntity => true;

		public override void PostAI(NPC npc)
		{
			if (SeedPool <= 0f || SeedMax <= 0) {
				return;
			}

			// 满层立即结算（因果完整版）
			if (SeedImmediate && SeedStacks >= SeedMax) {
				Bloom(npc);
				return;
			}

			if (SeedTimer > 0) {
				SeedTimer--;
				if (SeedTimer <= 0) {
					Bloom(npc);
				}
			}
		}

		private void Bloom(NPC npc)
		{
			if (!npc.active) {
				SeedPool = 0f;
				SeedStacks = 0;
				return;
			}

			Player owner = SeedOwner >= 0 && SeedOwner < Main.maxPlayers
				? Main.player[SeedOwner]
				: null;
			AuthorityPlayer authority = owner != null && owner.active
				? owner.GetModPlayer<AuthorityPlayer>()
				: null;
			bool full = authority != null && authority.Active(5);

			float damageMul = full ? 1.5f : 0.6f;
			float healMul = full ? 0.10f : 0.03f;
			int damage = (int)(SeedPool * damageMul);
			SeedPool = 0f;
			SeedStacks = 0;
			SeedMax = 0;
			SeedImmediate = false;

			if (damage < 1) {
				return;
			}

			// 结果特效：绿色 / 金色墨点，方便辨认
			for (int i = 0; i < 28; i++) {
				Vector2 offset = Main.rand.NextVector2Circular(46f, 46f);
				int dustType = i % 2 == 0 ? DustID.GoldCoin : DustID.GreenTorch;
				Dust dust = Dust.NewDustPerfect(npc.Center + offset, dustType, Vector2.Zero,
					0, default, 1.3f);
				dust.noGravity = true;
			}

			if (owner != null && owner.active) {
				owner.statLife = System.Math.Min(owner.statLifeMax2,
					owner.statLife + System.Math.Max(1, (int)(damage * healMul)));
			}

			if (full) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC other = Main.npc[i];
					if (!other.active || !other.CanBeChasedBy() || other.friendly) {
						continue;
					}

					if (Vector2.DistanceSquared(other.Center, npc.Center) > 120f * 120f) {
						continue;
					}

					other.StrikeNPC(new NPC.HitInfo {
						Damage = damage,
						Knockback = 0f,
						HitDirection = 0
					}, false, false);
				}
			}
			else {
				npc.StrikeNPC(new NPC.HitInfo {
					Damage = damage,
					Knockback = 0f,
					HitDirection = 0
				}, false, false);
			}
		}
	}
}
