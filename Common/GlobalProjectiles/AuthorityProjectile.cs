using EndfieldWings.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace EndfieldWings.Common.GlobalProjectiles
{
	/// <summary>
	/// 色空：弹幕只有命中玩家中心 10×10 的小圈才算受伤，圈外完全无伤。
	/// </summary>
	public class AuthorityProjectile : GlobalProjectile
	{
		public override bool CanHitPlayer(Projectile projectile, Player target)
		{
			AuthorityPlayer authority = target.GetModPlayer<AuthorityPlayer>();

			// 6 虚实：陷阱弹幕直接不碰撞
			if (authority.Active(6) && projectile.trap) {
				return false;
			}

			// 3 色空：只有命中中心 10×10 小圈才算受伤
			if (!authority.Active(3)) {
				return true;
			}

			return projectile.Hitbox.Intersects(authority.SmallBox());
		}
	}
}
