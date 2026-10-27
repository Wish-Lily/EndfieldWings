using System;
using EndfieldWings.Common.GlobalNPCs;
using EndfieldWings.Content.Items.Accessories.Shichen;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Common.Players
{
	/// <summary>
	/// 十二刻印共用的玩家状态（弱化版权能）。
	///
	/// 子武剑：攻击 20% 概率无视防御
	/// 丑谋棋：受击 20% 概率闪避
	/// 寅诗壶：免疫常规负面，特殊负面时间减半
	/// 卯律钟：受伤判定框缩小三分之一
	/// 辰文书：免死一次（回 25% 血，600 秒一次）
	/// 酉疗秤：攻击附带对方 0.5% 最大生命值
	/// 亥食灶：每秒恢复自身 0.5% 最大生命值
	/// 午商梭：免疫环境/爆炸伤害 + 商店半价
	/// 未建尺：伤害同时视为近战与魔法
	/// 戌绘卷：设置出生点
	/// </summary>
	public class ShichenPlayer : ModPlayer
	{
		public float IgnoreDefenseChance;
		public float DodgeChance;
		public bool SmallHitbox;
		public bool DeathSave;
		public bool MaxHpOnHit;
		public bool PercentRegen;
		public bool Merchant;
		public bool DualClass;
		public bool SeedHarvest;
		public bool CraftingMaster;

		private int deathSaveCooldown;
		private int regenTimer;
		private int baseWidth = -1;
		private int baseHeight = -1;
		private bool shrunk;

		public override void ResetEffects()
		{
			IgnoreDefenseChance = 0f;
			DodgeChance = 0f;
			SmallHitbox = false;
			DeathSave = false;
			MaxHpOnHit = false;
			PercentRegen = false;
			Merchant = false;
			DualClass = false;
			SeedHarvest = false;
			CraftingMaster = false;

			// 寅诗壶：从源头免疫常规负面（不含药水病 / 窒息 / 微光 / 模组负面）
			if (HasYinShiPot()) {
				int count = Math.Min(Player.buffImmune.Length, Main.debuff.Length);
				for (int i = 0; i < count; i++) {
					if (!Main.debuff[i]) {
						continue;
					}

					bool special = i == BuffID.PotionSickness || i == BuffID.Suffocation
						|| i == BuffID.Shimmer || i >= BuffID.Count;
					if (!special) {
						Player.buffImmune[i] = true;
					}
				}
			}
		}

		private bool HasYinShiPot()
		{
			int type = ModContent.ItemType<YinShiPot>();
			for (int i = 3; i < Player.armor.Length; i++) {
				if (Player.armor[i].type == type) {
					return true;
				}
			}

			return false;
		}

		public override void PostUpdateEquips()
		{
			if (SmallHitbox) {
				if (!shrunk) {
					baseWidth = Player.width;
					baseHeight = Player.height;
					shrunk = true;
				}

				Player.width = Math.Max(2, baseWidth * 2 / 3);
				Player.height = Math.Max(2, baseHeight * 2 / 3);
			}
			else if (shrunk) {
				Player.width = baseWidth;
				Player.height = baseHeight;
				shrunk = false;
				baseWidth = -1;
				baseHeight = -1;
			}

			if (Merchant) {
				// 环境伤害：摔落、岩浆、溺水
				Player.noFallDmg = true;
				Player.lavaImmune = true;
				Player.breath = Player.breathMax;
			}

			if (CraftingMaster) {
				// 视为同时处于所有制造环境
				for (int i = 0; i < Player.adjTile.Length; i++) {
					Player.adjTile[i] = true;
				}

				Player.adjWater = true;
				Player.adjHoney = true;
				Player.adjLava = true;
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			if (!DualClass) {
				return;
			}

			// 近战武器吃魔法加成，魔法武器吃近战加成
			StatModifier other;
			if (item.DamageType == DamageClass.Magic) {
				other = Player.GetDamage(DamageClass.Melee);
			}
			else if (item.DamageType == DamageClass.Melee) {
				other = Player.GetDamage(DamageClass.Magic);
			}
			else {
				return;
			}

			damage += other.Additive - 1f;
			damage *= other.Multiplicative;
			damage.Flat += other.Flat;
		}

		/// <summary>商店购买后半价返还（替代没有的 discount 字段）</summary>
		public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
		{
			if (!Merchant) {
				return;
			}

			long value = item.GetStoreValue() / 2;
			if (value <= 0) {
				return;
			}

			int[] coins = { ItemID.PlatinumCoin, ItemID.GoldCoin, ItemID.SilverCoin, ItemID.CopperCoin };
			long[] denom = { 1000000L, 10000L, 100L, 1L };
			for (int i = 0; i < coins.Length; i++) {
				long count = value / denom[i];
				value %= denom[i];
				while (count > 0) {
					int give = (int)Math.Min(count, 999L);
					Player.QuickSpawnItem(Player.GetSource_Misc("WuShangShuttle"), coins[i], give);
					count -= give;
				}
			}
		}

		public override void PostUpdate()
		{
			if (deathSaveCooldown > 0) {
				deathSaveCooldown--;
			}

			if (PercentRegen) {
				regenTimer++;
				if (regenTimer >= 60) {
					regenTimer = 0;
					int heal = Math.Max(1, (int)(Player.statLifeMax2 * 0.005f));
					Player.statLife = Math.Min(Player.statLifeMax2, Player.statLife + heal);
				}
			}
			else {
				regenTimer = 0;
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (IgnoreDefenseChance > 0f && Main.rand.NextFloat() < IgnoreDefenseChance) {
				modifiers.DefenseEffectiveness *= 0f;
			}

			if (MaxHpOnHit) {
				modifiers.FinalDamage.Flat += target.lifeMax * 0.005f;
			}
		}

		public override bool FreeDodge(Player.HurtInfo info)
		{
			return DodgeChance > 0f && Main.rand.NextFloat() < DodgeChance;
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genDust, ref PlayerDeathReason damageSource)
		{
			if (!DeathSave || deathSaveCooldown > 0) {
				return true;
			}

			deathSaveCooldown = 600 * 60;
			Player.statLife = Math.Max(1, Player.statLifeMax2 / 4);
			Player.immune = true;
			Player.immuneTime = 120;
			return false;
		}
	}
}
