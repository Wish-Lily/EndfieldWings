using System;
using EndfieldWings.Common.GlobalNPCs;
using EndfieldWings.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EndfieldWings.Common.Players
{
	/// <summary>
	/// 十二权能（完整版）的实现，只有装备祀岁翼且轮盘里开启时生效。
	/// 顺序与轮盘一致：0 朔 / 1 望 / 2 令 / 3 均 / 4 颉 / 5 黍 / 6 绩 / 7 易 / 8 年 / 9 方 / 10 夕 / 11 余。
	/// 岁·演化不在轮盘里，装备祀岁翼时常驻。
	/// </summary>
	public class AuthorityPlayer : ModPlayer
	{
		public const int Count = 12;
		public const int DefaultMask = (1 << Count) - 1;

		/// <summary>12 位开关，默认全开。</summary>
		public int Mask = DefaultMask;

		/// <summary>形：累计制造次数。</summary>
		public int CraftCount;

		/// <summary>意：见过的地貌位图。</summary>
		public int BiomeMask;

		/// <summary>意义：免死冷却（帧）。</summary>
		public int MeaningCooldown;

		/// <summary>演化：轮盘中心开关（默认关闭）。</summary>
		public bool EvolutionOn;

		private int regenTimer;

		public bool IsOn(int index)
		{
			return index >= 0 && index < Count && (Mask & (1 << index)) != 0;
		}

		public void Set(int index, bool on)
		{
			if (index < 0 || index >= Count) {
				return;
			}

			if (on) {
				Mask |= 1 << index;
			}
			else {
				Mask &= ~(1 << index);
			}
		}

		public void Toggle(int index)
		{
			if (index >= 0 && index < Count) {
				Mask ^= 1 << index;
			}
		}

		public override void ResetEffects()
		{
			// 逍遥：从源头让负面 buff 无法被施加（沙子/微光是持续施加的，清除无效）
			if (Active(2)) {
				ApplyFullDebuffImmunity();
			}
		}

		private void ApplyFullDebuffImmunity()
		{
			int count = Math.Min(Player.buffImmune.Length, Main.debuff.Length);
			for (int i = 0; i < count; i++) {
				if (Main.debuff[i] || i == BuffID.PotionSickness
					|| i == BuffID.Suffocation || i == BuffID.Shimmer) {
					Player.buffImmune[i] = true;
				}
			}
		}

		/// <summary>装备祀岁翼 + 轮盘开启。</summary>
		public bool Active(int index)
		{
			return IsOn(index) && HasWings;
		}

		/// <summary>是否装备祀岁翼（直接扫装备栏，避免时序问题）。</summary>
		public bool HasWings {
			get {
				if (Player.GetModPlayer<SiWingsPlayer>().WingsEquipped) {
					return true;
				}

				int wingType = ModContent.ItemType<SiWings>();
				for (int i = 3; i < Player.armor.Length; i++) {
					if (Player.armor[i].type == wingType) {
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>演化：轮盘中心开关 + 装备翅膀。</summary>
		public bool ActiveEvolution => EvolutionOn && HasWings;

		public override void SaveData(TagCompound tag)
		{
			tag["AuthorityMask"] = Mask;
			tag["AuthorityCraftCount"] = CraftCount;
			tag["AuthorityBiomeMask"] = BiomeMask;
			tag["AuthorityEvolution"] = EvolutionOn;
		}

		public override void LoadData(TagCompound tag)
		{
			if (tag.ContainsKey("AuthorityMask")) {
				Mask = tag.GetInt("AuthorityMask");
			}

			if (tag.ContainsKey("AuthorityCraftCount")) {
				CraftCount = tag.GetInt("AuthorityCraftCount");
			}

			if (tag.ContainsKey("AuthorityBiomeMask")) {
				BiomeMask = tag.GetInt("AuthorityBiomeMask");
			}

			if (tag.ContainsKey("AuthorityEvolution")) {
				EvolutionOn = tag.GetBool("AuthorityEvolution");
			}
		}

		public override void PostUpdateEquips()
		{
			// 3 色空：受伤判定缩成一个点
			// 注意：直接改 width/height 会让碰撞箱错位，表现为人物被方块顶起来（自动上浮）。
			// 暂时停用，等找到不改碰撞箱的做法（例如只拦伤害）再打开。
			// if (Active(3)) { ... }

			// 6 虚实：环境伤害免疫（不提供无限氧气）
			if (Active(6)) {
				Player.noFallDmg = true;
				Player.lavaImmune = true;
			}

			// 1 势坤：最终伤害减免 10% 在 ModifyHurt 里做
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (Active(1)) {
				modifiers.FinalDamage *= 0.9f;
			}

			// 6 虚实：环境/爆炸/接触伤害清零（仍有 1 点保底，靠 ImmuneTo 兜底）
			if (Active(6)) {
				PlayerDeathReason source = modifiers.DamageSource;
				bool noAttacker = source.SourceNPCIndex < 0 && source.SourceProjectileType <= 0
					&& source.SourcePlayerIndex < 0;
				if (noAttacker || IsExplosion(source) || IsContact(source)) {
					modifiers.FinalDamage *= 0f;
				}
			}
		}

		public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
		{
			// 岁·演化（轮盘中心）：暂时做成完全无敌
			if (ActiveEvolution) {
				return true;
			}

			// 3 色空：被 NPC 接触时，只有命中中心小圈才算
			if (Active(3) && damageSource.SourceNPCIndex >= 0) {
				int index = damageSource.SourceNPCIndex;
				if (index >= 0 && index < Main.maxNPCs) {
					NPC npc = Main.npc[index];
					if (npc.active && !npc.Hitbox.Intersects(SmallBox())) {
						return true;
					}
				}
			}

			if (!Active(6)) {
				return false;
			}

			// 6 虚实：接触伤害（NPC 碰撞）直接免疫
			if (damageSource.SourceNPCIndex >= 0) {
				return true;
			}

			// 陷阱伤害交给 GlobalProjectile 的 projectile.trap 拦截

			// 环境伤害（坠落、岩浆、荆棘等）；溺水在 Calamity 环境里走不到这个钩子
			if (damageSource.SourceOtherIndex >= 0) {
				return true;
			}

			return false;
		}

		/// <summary>色空的中心判定圈（1×1，只有正中中心才算命中）。</summary>
		public Rectangle SmallBox()
		{
			return new Rectangle((int)Player.Center.X, (int)Player.Center.Y, 1, 1);
		}

		public override void PostUpdate()
		{
			// 逍遥：再清一次，挡住本帧稍后才施加的窒息/闪光
			if (Active(2)) {
				PurgeDebuffs();
			}

			// 8 形：放在 PostUpdate 再设一次，避免被原版的相邻工作站刷新覆盖
			if (Active(8)) {
				for (int i = 0; i < Player.adjTile.Length; i++) {
					Player.adjTile[i] = true;
				}

				Player.adjWater = true;
				Player.adjHoney = true;
				Player.adjLava = true;
			}

			if (MeaningCooldown > 0) {
				MeaningCooldown--;
			}

			// 11 生：每秒回 5% 最大生命
			if (Active(11)) {
				regenTimer++;
				if (regenTimer >= 60) {
					regenTimer = 0;
					int heal = Math.Max(1, (int)(Player.statLifeMax2 * 0.05f));
					Player.statLife = Math.Min(Player.statLifeMax2, Player.statLife + heal);
				}
			}
			else {
				regenTimer = 0;
			}

			UpdateBiomes();
		}

		private void UpdateBiomes()
		{
			int bit = -1;
			if (Player.ZoneDesert) {
				bit = 0;
			}
			else if (Player.ZoneJungle) {
				bit = 1;
			}
			else if (Player.ZoneSnow) {
				bit = 2;
			}
			else if (Player.ZoneCorrupt) {
				bit = 3;
			}
			else if (Player.ZoneCrimson) {
				bit = 4;
			}
			else if (Player.ZoneHallow) {
				bit = 5;
			}
			else if (Player.ZoneGlowshroom) {
				bit = 6;
			}
			else if (Player.ZoneBeach) {
				bit = 7;
			}
			else if (Player.ZoneMeteor) {
				bit = 8;
			}
			else if (Player.ZoneDungeon) {
				bit = 9;
			}
			else if (Player.ZoneUndergroundDesert) {
				bit = 10;
			}

			if (bit >= 0) {
				BiomeMask |= 1 << bit;
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			// 8 形：制造次数提升近战 / 远程
			if (Active(8) && (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.Ranged)) {
				damage += Math.Min(0.30f, CraftCount * 0.005f);
			}

			// 10 意：地貌解锁提升魔法 / 召唤
			if (Active(10) && (item.DamageType == DamageClass.Magic || item.DamageType == DamageClass.Summon)) {
				int biomes = BitCount(BiomeMask);
				damage += Math.Min(0.30f, biomes * 0.03f);
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			// 0 开乾：最终伤害 +10%，50% 无视防御
			if (Active(0)) {
				modifiers.FinalDamage *= 1.1f;
				if (Main.rand.NextFloat() < 0.5f) {
					modifiers.DefenseEffectiveness *= 0f;
				}
			}

			// 7 相合：额外视为四类，四类加成都再乘一遍（×32 版本）
			if (Active(7)) {
				float product = Player.GetDamage(DamageClass.Melee).Additive
					* Player.GetDamage(DamageClass.Ranged).Additive
					* Player.GetDamage(DamageClass.Magic).Additive
					* Player.GetDamage(DamageClass.Summon).Additive;
				if (product > 0f) {
					modifiers.FinalDamage *= product;
				}
			}

			// 9 死：附带 5% 最大生命
			if (Active(9)) {
				modifiers.FinalDamage.Flat += target.lifeMax * 0.05f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// 5 因果（完整版），或巳农桩（弱化版）
			bool authority = Active(5);
			bool seal = Player.GetModPlayer<ShichenPlayer>().SeedHarvest;
			if ((!authority && !seal) || target.life <= 0) {
				return;
			}

			ShichenNPC seed = target.GetGlobalNPC<ShichenNPC>();
			if (authority) {
				// 完整版：第一层就开始 3 秒计时，满 12 层立刻爆
				seed.SeedMax = 12;
				seed.SeedImmediate = true;
				if (seed.SeedTimer <= 0) {
					seed.SeedTimer = 180;
				}

				if (seed.SeedStacks < 12) {
					seed.SeedStacks++;
				}
			}
			else {
				// 巳农桩：叠满 2 层后计时 5 秒
				seed.SeedMax = 2;
				seed.SeedImmediate = false;
				if (seed.SeedStacks < 2) {
					seed.SeedStacks++;
				}

				if (seed.SeedStacks >= 2 && seed.SeedTimer <= 0) {
					seed.SeedTimer = 300;
				}
			}

			seed.SeedPool += damageDone;
			seed.SeedOwner = Player.whoAmI;
		}

		public override bool FreeDodge(Player.HurtInfo info)
		{
			// 1 势坤：50% 闪避
			return Active(1) && Main.rand.NextFloat() < 0.5f;
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genDust, ref PlayerDeathReason damageSource)
		{
			// 4 意义：免死 + 回满血 + 清屏
			if (Active(4) && MeaningCooldown <= 0) {
				MeaningCooldown = 365 * 60;
				Player.statLife = Player.statLifeMax2;
				Player.immune = true;
				Player.immuneTime = 120;
				ClearHostileProjectiles();
				return false;
			}

			// 岁·演化：暂停（天顶秒杀的具体实现待确认）
			// if (Active(0) && !converting && damage >= Player.statLifeMax2 * 10.0) { ... }

			// 岁·演化（轮盘中心）：无敌兜底
			if (ActiveEvolution) {
				Player.statLife = Player.statLifeMax2;
				Player.immune = true;
				Player.immuneTime = 60;
				return false;
			}

			return true;
		}

		public override void PostUpdateBuffs()
		{
			// 2 逍遥：绝对免疫所有负面
			if (Active(2)) {
				PurgeDebuffs();
			}
		}

		/// <summary>移除全部负面，并挡住会反复施加的窒息 / 闪光。</summary>
		private void PurgeDebuffs()
		{
			for (int i = Player.MaxBuffs - 1; i >= 0; i--) {
				int type = Player.buffType[i];
				if (type <= 0) {
					continue;
				}

				if (Main.debuff[type] || type == BuffID.PotionSickness
					|| type == BuffID.Suffocation || type == BuffID.Shimmer) {
					Player.DelBuff(i);
				}
			}

			Player.potionDelay = 0;
			ApplyFullDebuffImmunity();
		}

		public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
		{
			// 6 虚实：花费返还 99%（向上取整）
			if (!Active(6)) {
				return;
			}

			long value = (long)Math.Ceiling(item.GetStoreValue() * 0.99);
			GiveCoins(value);
		}

		private void GiveCoins(long value)
		{
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
					Player.QuickSpawnItem(Player.GetSource_Misc("AuthorityXuShi"), coins[i], give);
					count -= give;
				}
			}
		}

		private static bool IsContact(PlayerDeathReason source)
		{
			return source.SourceNPCIndex >= 0;
		}

		private static bool IsExplosion(PlayerDeathReason source)
		{
			int type = source.SourceProjectileType;
			if (type <= 0) {
				return false;
			}

			return type == ProjectileID.Bomb
				|| type == ProjectileID.BouncyBomb
				|| type == ProjectileID.StickyBomb
				|| type == ProjectileID.Dynamite
				|| type == ProjectileID.BouncyDynamite
				|| type == ProjectileID.StickyDynamite
				|| type == ProjectileID.Grenade
				|| type == ProjectileID.BouncyGrenade
				|| type == ProjectileID.StickyGrenade
				|| type == ProjectileID.GrenadeI
				|| type == ProjectileID.GrenadeII
				|| type == ProjectileID.GrenadeIII
				|| type == ProjectileID.GrenadeIV
				|| type == ProjectileID.RocketI
				|| type == ProjectileID.RocketII
				|| type == ProjectileID.RocketIII
				|| type == ProjectileID.RocketIV
				|| type == ProjectileID.Explosives;
		}

		private static void ClearHostileProjectiles()
		{
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.hostile) {
					proj.Kill();
				}
			}
		}

		private static int BitCount(int value)
		{
			int count = 0;
			while (value != 0) {
				value &= value - 1;
				count++;
			}

			return count;
		}
	}
}
