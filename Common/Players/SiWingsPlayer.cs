using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EndfieldWings.Common.Players
{
	/// <summary>
	/// 祀岁翼的飞行控制器。
	///
	/// 模式 0「巡航」：飞行手感交给原版翅膀逻辑，这里只负责无限飞行时长；
	/// 模式 1「悬浮」：接管垂直和水平速度，松开方向键立刻停住，变向没有惯性。
	///
	/// 空中连按两次跳跃切换模式。
	/// 逻辑放在 PreUpdateMovement 里：它在「按速度移动位置」之前执行，
	/// 改完速度当帧就生效，不会被原版重力或翅膀逻辑覆盖。
	/// </summary>
	public class SiWingsPlayer : ModPlayer
	{
		public const int Cruise = 0;
		public const int Hover = 1;

		// 双击判定的窗口（帧）。60 帧 = 1 秒，15 帧大约是 0.25 秒。
		private const int DoubleTapWindow = 15;

		// ---- 模式 1 的速度参数（手感先给一组，后面按需要调）----
		public const float HoverAscendSpeed = 6.5f;
		public const float HoverDescendSpeed = 6.5f;
		public const float HoverHorizontalSpeed = 8f;

		/// <summary>本帧是否装备了祀岁翼，由饰品在 UpdateAccessory 里置位。</summary>
		public bool WingsEquipped;

		/// <summary>合并 HUD 的位置（-1 表示默认位置）。</summary>
		public float HudX = -1f;
		public float HudY = -1f;

		public override void SaveData(TagCompound tag)
		{
			tag["HudX"] = HudX;
			tag["HudY"] = HudY;
		}

		public override void LoadData(TagCompound tag)
		{
			if (tag.ContainsKey("HudX")) {
				HudX = tag.GetFloat("HudX");
			}

			if (tag.ContainsKey("HudY")) {
				HudY = tag.GetFloat("HudY");
			}
		}

		/// <summary>当前飞行模式，见 Cruise / Hover。</summary>
		public int FlightMode = Cruise;

		/// <summary>模式 1 是否正在接管悬停。</summary>
		public bool HoverEngaged;

		private bool _prevJump;
		private int _doubleTapTimer;

		public override void ResetEffects()
		{
			WingsEquipped = false;
		}

		public override void PreUpdateMovement()
		{
			Player p = Player;

			// 只驱动本地玩家，避免多人游戏里改写其他玩家的速度
			if (p.whoAmI != Main.myPlayer) {
				return;
			}

			if (!WingsEquipped || p.mount.Active || p.dead) {
				HoverEngaged = false;
				_prevJump = p.controlJump;
				return;
			}

			// 无限飞行：每帧把翅膀能量补满
			p.wingTime = p.wingTimeMax;

			UpdateModeSwitch(p);

			if (FlightMode == Hover) {
				UpdateHoverFlight(p);
			}
			else {
				HoverEngaged = false;
			}

			ApplySpecialMechanics(p);
		}

		/// <summary>空中连按两次跳跃 → 切换模式。</summary>
		private void UpdateModeSwitch(Player p)
		{
			bool pressed = p.controlJump && !_prevJump;
			_prevJump = p.controlJump;

			// 只在空中判定，避免在地面上连续起跳时误切换
			if (pressed && !IsStanding(p)) {
				if (_doubleTapTimer > 0) {
					FlightMode = FlightMode == Cruise ? Hover : Cruise;
					HoverEngaged = false;
					_doubleTapTimer = 0;

					Main.NewText(
						Language.GetTextValue(FlightMode == Hover ? EndfieldWings.ModeHoverKey : EndfieldWings.ModeCruiseKey),
						FlightMode == Hover ? new Color(255, 214, 140) : new Color(120, 220, 255));
					return;
				}

				_doubleTapTimer = DoubleTapWindow;
				return;
			}

			if (_doubleTapTimer > 0) {
				_doubleTapTimer--;
			}
		}

		/// <summary>模式 1：默认悬停，上下左右即时响应。</summary>
		private void UpdateHoverFlight(Player p)
		{
			// 站在地面 / 平台上就交还控制权，避免在地面上变成「贴地飞行」
			if (IsStanding(p)) {
				HoverEngaged = false;
				return;
			}

			HoverEngaged = true;

			// 垂直：按下 → 下降；按上 → 上升；都不按 → 定住
			// 注意这里不响应跳跃键：跳跃只用来切换模式，避免刚切到悬浮就自动上浮
			if (p.controlDown) {
				p.velocity.Y = HoverDescendSpeed;
			}
			else if (p.controlUp) {
				p.velocity.Y = -HoverAscendSpeed;
			}
			else {
				p.velocity.Y = 0f;
			}

			// 水平：直接给速度，没有加速过程，松手立刻停
			if (p.controlLeft) {
				p.velocity.X = -HoverHorizontalSpeed;
			}
			else if (p.controlRight) {
				p.velocity.X = HoverHorizontalSpeed;
			}
			else {
				p.velocity.X = 0f;
			}
		}

		/// <summary>
		/// 判断玩家脚下是否踩着实体方块或平台。
		/// 原版没有公开的 onGround 属性，这里直接查脚底那一格物块。
		/// 注意不能靠 velocity.Y == 0 判断：重力和碰撞的先后顺序会让站地上的玩家
		/// 在 PreUpdateMovement 阶段仍然带着一点向下的速度。
		/// </summary>
		private static bool IsStanding(Player p)
		{
			int feetY = (int)((p.position.Y + p.height + 1f) / 16f);
			int leftX = (int)(p.position.X / 16f);
			int rightX = (int)((p.position.X + p.width - 1f) / 16f);

			for (int x = leftX; x <= rightX; x++) {
				if (!WorldGen.InWorld(x, feetY, 1)) {
					continue;
				}

				Tile tile = Main.tile[x, feetY];
				if (tile.HasTile && (Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType])) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 特殊机制预留位。
		/// 之后要加的东西（冲刺、落地冲击波、空中攻击强化、悬浮时回蓝……）都写在这里，
		/// 它每帧会在飞行逻辑之后被调用一次。
		/// </summary>
		private void ApplySpecialMechanics(Player p)
		{
			// TODO: 待定
		}
	}
}
