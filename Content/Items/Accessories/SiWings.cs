using EndfieldWings.Common.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EndfieldWings.Content.Items.Accessories
{
	// AutoloadEquip(EquipType.Wings) 会让 tML 自动去找同目录下的
	// SiWings_Wings.png 作为穿戴在背后的翅膀贴图（86x248，4 帧竖排）。
	[AutoloadEquip(EquipType.Wings)]
	public class SiWings : ModItem
	{
		public override void SetStaticDefaults()
		{
			// 翅膀数值：水平/悬停对齐「天界星盘」，上升加速度/下落缓冲/飞行时长用回原值
			// flyTime             3600 帧；这里每帧补满，等于无限飞
			// flySpeedOverride    82 mph ≈ 16 px/tick
			// accelerationMultiplier 16×（星盘的水平加速度倍率）
			// hasHoldDownHoverFeatures 悬停（飞行时按住下键）
			// hoverFlySpeedOverride  悬停时 41 mph ≈ 8 px/tick，倍率 4.5×
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
				flyTime: 60 * 60,
				flySpeedOverride: 16f,
				accelerationMultiplier: 16f,
				hasHoldDownHoverFeatures: true,
				hoverFlySpeedOverride: 8f,
				hoverAccelerationMultiplier: 4.5f);
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Master;
			Item.accessory = true;

			// 名字固定为中文（本地化偶发不生效时兜底）
			Item.SetNameOverride("祀岁翼");
		}

		/// <summary>名字颜色随时间循环变化。</summary>
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			float hue = (Main.GlobalTimeWrappedHourly * 0.35f) % 1f;
			Color color = Main.hslToRgb(hue, 0.85f, 0.68f);
			foreach (TooltipLine line in tooltips) {
				if (line.Name == "ItemName") {
					line.OverrideColor = color;
				}
			}
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			// 告诉飞行控制器：本帧装备了祀岁翼
			player.GetModPlayer<SiWingsPlayer>().WingsEquipped = true;

			// ===== 数值加成（待定，先放一组占位值）=====
			player.GetDamage(DamageClass.Generic) += 0.08f; // 全伤害 +8%
			player.GetCritChance(DamageClass.Generic) += 5f; // 暴击率 +5%
			player.endurance += 0.02f;                       // 伤害减免 +2%
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			// 模式 0「巡航」的上升参数：
			// ascentWhenFalling 下落时按住跳跃的缓冲（回到 0.85）
			// ascentWhenRising  上升中按住跳跃的额外速度
			// maxCanAscendMultiplier 低段速度倍率
			// maxAscentMultiplier 最高上升速度倍率（4.5×，星盘）
			// constantAscend     每帧上升加速度（回到 0.135）
			ascentWhenFalling = 0.85f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 4.5f;
			constantAscend = 0.135f;
		}

	}
}
