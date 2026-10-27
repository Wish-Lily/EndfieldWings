using Terraria.Localization;
using Terraria.ModLoader;

namespace EndfieldWings
{
	// 每个 mod 有且只有一个 Mod 类。
	// 内部名（build.txt 里的 displayName 是显示名）对应 EndfieldWings。
	public class EndfieldWings : Mod
	{
		/// <summary>打开 / 关闭权能轮盘（默认 O，可在设置里改）。</summary>
		public static ModKeybind WheelKey { get; private set; }

		/// <summary>打开 / 关闭画卷空间（默认 U）。</summary>
		public static ModKeybind ScrollKey { get; private set; }

		public static string ModeCruiseKey { get; private set; }
		public static string ModeHoverKey { get; private set; }

		/// <summary>主题音乐槽位（0 表示不可用）。</summary>
		public static int ThemeMusicSlot { get; private set; }

		public override void Load()
		{
			WheelKey = KeybindLoader.RegisterKeybind(this, "AuthorityWheel", "O");
			ScrollKey = KeybindLoader.RegisterKeybind(this, "ScrollSpace", "U");

			// 音乐：tML 会自动登记 Assets/Music 下的文件，不能再手动 AddMusic（会抛重复 key）
			try {
				ThemeMusicSlot = MusicLoader.GetMusicSlot(this, "Assets/Music/HuaSiShi");
			}
			catch {
				ThemeMusicSlot = 0;
			}

			// 关键文本在代码里注册默认值：即使 hjson 没打进包也能显示
			Language.GetOrRegister($"Mods.{Name}.Keybinds.AuthorityWheel.DisplayName", () => "权能轮盘");
			Language.GetOrRegister($"Mods.{Name}.Keybinds.ScrollSpace.DisplayName", () => "画卷空间");
			Language.GetOrRegister($"Mods.{Name}.Messages.ModeCruise", () => "祀岁翼 · 巡航模式");
			Language.GetOrRegister($"Mods.{Name}.Messages.ModeHover", () => "祀岁翼 · 悬浮模式");

			// 配置界面文字由 [Label]/[Tooltip] 属性提供，不再依赖 hjson

			// 物品名（防止本地化文件损坏时回退成英文）
			(string key, string name)[] itemNames = {
				("SiWings", "祀岁翼"), ("ZiWuSword", "子武剑"), ("ChouMouChess", "丑谋棋"),
				("YinShiPot", "寅诗壶"), ("MaoLvBell", "卯律钟"), ("ChenWenBook", "辰文书"),
				("SiNongStake", "巳农桩"), ("WuShangShuttle", "午商梭"), ("WeiJianRuler", "未建尺"),
				("ShenZhuWheel", "申铸轮"), ("YouLiaoScale", "酉疗秤"), ("XuHuiScroll", "戌绘卷"),
				("HaiShiStove", "亥食灶")
			};

			foreach ((string key, string name) in itemNames) {
				Language.GetOrRegister($"Mods.{Name}.Items.{key}.DisplayName", () => name);
			}

			ModeCruiseKey = $"Mods.{Name}.Messages.ModeCruise";
			ModeHoverKey = $"Mods.{Name}.Messages.ModeHover";
		}

		public override void Unload()
		{
			WheelKey = null;
			ScrollKey = null;
			ModeCruiseKey = null;
			ModeHoverKey = null;
			ThemeMusicSlot = 0;
		}
	}
}
