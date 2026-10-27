namespace EndfieldWings.Common.Authorities
{
	/// <summary>
	/// 十二权能的展示信息（轮盘顺序：朔、望、令、均、颉、黍、绩、易、年、方、夕、余）。
	/// </summary>
	public static class AuthorityInfo
	{
		public static readonly string[] Names = {
			"朔", "望", "令", "均", "颉", "黍", "绩", "易", "年", "方", "夕", "余"
		};

		public static readonly string[] Effects = {
			"最终伤害 +10%，50% 破防",
			"最终伤害减免 10%，50% 闪避",
			"绝对免疫所有负面",
			"受伤判定缩成一点",
			"免死一次：回满血 + 清屏（365 秒）",
			"12 层种子，满层或 3 秒后爆发",
			"免疫接触/环境/陷阱伤害；花费返还 99%",
			"造成的伤害额外视为近战/远程/魔法/召唤伤害",
			"视为处于所有制造环境",
			"攻击附带 5% 最大生命",
			"画卷空间（特殊复制背包）",
			"每秒恢复 5% 最大生命"
		};
	}
}
