# 16 · ImproveGame 学习与画卷空间实现

> 仓库：https://github.com/487666123/ImproveGame （分支 `1.4.4`，约 1100 文件 / 557 个 .cs）

## 一、它是什么

功能非常多的 QoL（生活质量）mod：超级虚空袋（SuperVault）、极限存储（ExtremeStorage）、自动垃圾桶、自动钓鱼、前缀编辑、出生点、液体存储、配方浏览等。

## 二、架构要点

```
UIFramework/           自研 UI 框架：AutoCreateGUI、BaseBody、SUIPanel、
                       SUIText、SUISwitch、SUIButton、ModItemGrid、动画计时器
UI/BigBagGUI.cs        超级虚空袋（大背包）：打开时要求 Main.playerInventory
UI/ExtremeStorage/     极限存储：多箱子网络、筛选、工具按钮、分页
Packets/               每个功能一组网络包（BigBagPackets、NetStorager…）
Common/ModSystems/     ExtremeStorageCore 等系统
Common/ModPlayers/     DataPlayer、ExtremeStoragePlayer、ImprovePlayer
```

### BigBagGUI 的关键做法

- `Enabled` 里检查 `Main.playerInventory`，背包关掉就收起
- 打开/关闭用 `AnimationTimer`（`StartTimer`）做过渡
- 内容是一个 `ModItemGrid`（物品格子网格）
- 顶部有「参与合成 / 优先抓取 / 自动抓取」开关，底部有快捷放入、补充、排序按钮
- 服务端/客户端通过 `BigBagPackets` 同步

### 极限存储

- 把多个箱子当成一个网络，按物品分组（武器/护甲/药水/方块…）
- 筛选器、工具按钮、长按切换等
- 网络包按操作拆分（添加/移除/交换/同步）

## 三、我们借鉴了什么

画卷空间（戌绘卷 / 意）做成了**轻量版大背包**：

| ImproveGame | 我们 |
|---|---|
| 打开要求背包界面 | 同样 `Main.playerInventory` |
| 物品网格 + 点击交换 | 50 格网格，点击与鼠标物品互换 |
| 自研 UI 框架 | 沿用我们已有的手写绘制（`PostDrawInterface`） |
| 网络同步 | 暂不做（单人优先） |
| 快捷放入 / 排序 | 暂不做 |

## 四、我们的实现

```
Common/Players/ScrollSpacePlayer.cs   50 格 Item[]，ItemIO 存档
Common/UI/ScrollSpaceUI.cs            网格绘制 + 点击交换 + U 键
EndfieldWings.cs                      注册 ScrollSpace 键位（默认 U）
```

规则：

- 装备**戌绘卷**，或翅膀开启**意**，即可使用画卷空间
- **U 键**开关；打开时自动拉出背包界面
- 关闭背包 / Esc 时一起收起
- 物品存在玩家存档里，摘下戌绘卷不会丢
- 点击格子：与鼠标上的物品互换（空手拿起 / 放下 / 交换）

## 五、每日翻倍（意）

- 触发：`Main.dayTime` 从 `false` 变 `true` 的那一帧（夜晚 → 白天 = 新的一天）
- 条件：**「意」开启**（装备祀岁翼 + 轮盘里「夕」亮着）
- 效果：画卷空间每格 `stack × 2`，不超过该物品的 `maxStack`
- **可堆叠物品**（材料、匣子、能堆叠的宝藏袋）：每天天亮都翻倍
- **不可堆叠物品**（武器、饰品、不能堆叠的袋子）：复制一份，但**每次放入只复制一次**；
  取出再放回会重置标记，可以再次复制
- **本模组自己的物品（权能类）不复制**（按 `item.ModItem.Mod.Name` 判断）
- 复制出来的物品需要空槽；标记会随存档保存
- 可叠加：不取出的物品，第二天再 ×2（第 3 天 ×4，以此类推）
- 关闭「意」时，天亮不触发翻倍
- 读档/进世界的首帧只记录昼夜状态，不会立刻翻倍
- 实现：`ScrollSpacePlayer.PostUpdate` + `DoubleStacks()`

## 六、限制（待做）

- 没有排序、快捷放入、搜索
- 没有多人同步（联机时各自独立）
- 面板位置固定居中，未做拖动
- 物品图标按整张贴图缩放，个别多帧图标可能显示异常

## 六、结论

ImproveGame 的 UI 框架太重，不适合整体照搬；但**「大背包 = 玩家身上的 Item[] + 网格 UI + 点击交换 + 存档」**这个骨架很清晰，画卷空间按这个思路已经能跑。
