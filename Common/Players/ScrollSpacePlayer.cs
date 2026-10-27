using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EndfieldWings.Common.Players
{
	/// <summary>
	/// 画卷空间：50 格随身储物，数据存在玩家身上，摘下戌绘卷也不会丢。
	///
	/// 每天天亮（意开启时）：
	/// - 可堆叠物品：数量 ×2（封顶 maxStack）
	/// - 武器 / 饰品等不可堆叠物：复制一份，**每次放入只复制一次**；
	///   取出再放回会重置，可以再次复制
	/// - 本模组自己的物品（权能类）不复制
	/// </summary>
	public class ScrollSpacePlayer : ModPlayer
	{
		public const int Slots = 50;

		public Item[] Items = new Item[Slots];

		/// <summary>该槽位是否已经在本次放入后复制过。</summary>
		public bool[] Copied = new bool[Slots];

		private bool lastDayTime;
		private bool dayInitialized;

		public override void PostUpdate()
		{
			if (!dayInitialized) {
				lastDayTime = Main.dayTime;
				dayInitialized = true;
				return;
			}

			bool dawn = Main.dayTime && !lastDayTime;
			lastDayTime = Main.dayTime;

			if (dawn && Player.GetModPlayer<AuthorityPlayer>().Active(10)) {
				ProcessDawn();
			}
		}

		private void ProcessDawn()
		{
			List<int> occupied = new();
			for (int i = 0; i < Items.Length; i++) {
				if (Items[i] != null && !Items[i].IsAir) {
					occupied.Add(i);
				}
			}

			foreach (int i in occupied) {
				Item item = Items[i];
				if (item == null || item.IsAir) {
					continue;
				}

				// 可堆叠：每天翻倍
				if (item.maxStack > 1) {
					if (item.stack < item.maxStack) {
						int doubled = item.stack * 2;
						item.stack = doubled > item.maxStack ? item.maxStack : doubled;
					}

					continue;
				}

				// 不可堆叠（武器 / 饰品 / 袋子）：每次放入只复制一次
				if (Copied[i] || !CanCopy(item)) {
					continue;
				}

				int empty = FindEmptySlot();
				if (empty < 0) {
					continue;
				}

				Items[empty] = item.Clone();
				Items[empty].stack = 1;
				Copied[empty] = true;
				Copied[i] = true;
			}
		}

		/// <summary>本模组自己的物品不复制。</summary>
		private static bool CanCopy(Item item)
		{
			return item.ModItem == null || item.ModItem.Mod.Name != "EndfieldWings";
		}

		private int FindEmptySlot()
		{
			for (int i = 0; i < Items.Length; i++) {
				if (Items[i] == null || Items[i].IsAir) {
					return i;
				}
			}

			return -1;
		}

		/// <summary>槽位内容变化时调用：重置“已复制”标记，允许再次复制。</summary>
		public void OnSlotChanged(int index)
		{
			if (index >= 0 && index < Copied.Length) {
				Copied[index] = false;
			}
		}

		public override void Initialize()
		{
			Items = new Item[Slots];
			Copied = new bool[Slots];
			for (int i = 0; i < Slots; i++) {
				Items[i] = new Item();
				Items[i].SetDefaults(0, true);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["ScrollItems"] = Items.Select(ItemIO.Save).ToList();

			StringBuilder builder = new();
			for (int i = 0; i < Copied.Length; i++) {
				builder.Append(Copied[i] ? '1' : '0');
			}

			tag["ScrollCopied"] = builder.ToString();
		}

		public override void LoadData(TagCompound tag)
		{
			if (!tag.ContainsKey("ScrollItems")) {
				return;
			}

			IList<TagCompound> list = tag.GetList<TagCompound>("ScrollItems");
			for (int i = 0; i < Items.Length; i++) {
				Items[i] = i < list.Count ? ItemIO.Load(list[i]) : new Item();
			}

			if (tag.ContainsKey("ScrollCopied")) {
				string flags = tag.GetString("ScrollCopied");
				for (int i = 0; i < Copied.Length && i < flags.Length; i++) {
					Copied[i] = flags[i] == '1';
				}
			}
		}

		/// <summary>是否拥有画卷空间：装备戌绘卷，或翅膀开启「意」。</summary>
		public bool HasScrollSpace()
		{
			if (Player.GetModPlayer<AuthorityPlayer>().Active(10)) {
				return true;
			}

			int type = ModContent.ItemType<Content.Items.Accessories.Shichen.XuHuiScroll>();
			for (int i = 3; i < Player.armor.Length; i++) {
				if (Player.armor[i].type == type) {
					return true;
				}
			}

			return false;
		}
	}
}
