# 15 · CheatSheet 与 HEROsMod 学习笔记

> - https://github.com/JavidPack/CheatSheet （分支 `1.4.4`，约 137 文件 / 55 个 .cs）
> - https://github.com/JavidPack/HEROsMod （分支 `1.4.4`，约 255 文件 / 98 个 .cs）
>
> 结论先说：**这两个都不如 DragonLens 适合参考**，但各有可抄的小点。

## 一、CheatSheet

### 定位
单机/联机都能用的作弊菜单，热键唤出底部快捷栏。

### 结构

```
CheatSheet.cs            Mod 主类：注册热键、初始化所有窗口、权限表
CheatSheetSystem.cs      空壳，PostSetupRecipes 时 SetupUI()
CheatSheetPlayer.cs      额外饰品槽 + 热键处理
CustomUI/                UISlideWindow / UIHotbar（滑入滑出动画）
Menus/                   Hotbar、ItemBrowser、NPCBrowser、RecipeBrowser、
                         PaintTools、QuickTeleport、NPCButcherer…
```

### 关键实现

1. **自定义 UI 框架**：`CheatSheet.UI` 里有一套 `UIWindow` / `UIImage` / `UIView` / `UICheckbox` / `UIScrollBar` / `UIScrollView` / `UITextbox`，不是 tML 的 `UIState`，代码偏老但能跑。
2. **滑入式快捷栏**：`UISlideWindow` 用 `MathHelper.SmoothStep` 在 `shownPosition` / `hiddenPosition` 之间插值，`selected` 属性控制显隐。
3. **额外饰品槽**：`CheatSheetPlayer.UpdateEquips` 里对额外槽调用
   `Player.GrantPrefixBenefits` + `Player.GrantArmorBenefits` + `Player.ApplyEquipFunctional`，
   这是“绕过装备栏给效果”的标准写法。
4. **浏览器**：ItemBrowser / NPCBrowser 都是 `UIWindow` + 搜索框 + 复选框过滤 + 滚动列表。
5. **HEROs 权限整合**：`herosPermissions` 字典 + 每个功能一个权限字符串，HEROsMod 装了就用它的权限门禁。

### 可抄

- 滑入式窗口动画（`SmoothStep` + 目标位置插值）
- 搜索 + 复选框过滤的浏览器（做 buff 免疫勾选面板时直接用）
- 额外饰品槽的 `ApplyEquipFunctional` 写法（如果以后要做“权能额外槽”）

## 二、HEROsMod

### 定位
多人服务器的管理员权限系统 + 管理工具集。

### 结构

```
HEROsMod.cs                      Mod 主类：初始化服务、注册按键
HEROsModServices/HEROsModService.cs   服务基类（UI + 热键图标 + 权限）
HEROsModServices/*Service.cs           GodMode / ItemBrowser / MobSpawner / MapRevealer…
HEROsModNetwork/                       Network / LoginService / Group / Region
HEROsModModPlayer.cs                   玩家包装（索引、组、用户名）
```

### 关键实现

1. **Service 基类**：每个功能是一个 `HEROsModService`，自带 `HotbarIcon`、`IsInHotbar`、`HasPermissionToUse`、`AddUIView`，统一挂到 `MasterView.gameScreen`。
2. **权限组**：`Group` 维护 `Dictionary<string, bool> Permissions`，`PermissionInfo` 列表定义所有权限名（GodMode、SpawnBosses、RevealMap…）。
3. **网络**：`LoginService` 登录、`Network` 处理请求（如 `RequestGodMode`），服务端权威。
4. **GodMode**：静态 `Enabled`，右键切换 BuddhaMode，图标透明度反映状态；多人下左键发请求而不是直接改状态。

### 可抄

- 服务化拆分（每个功能一个类，而不是全塞进一个 ModSystem）
- 权限名常量化的做法（如果以后做多人，可以按这个给权能加权限）
- 图标透明度表示开关状态

## 三、三 mod 对比

| | DragonLens | CheatSheet | HEROsMod |
|---|---|---|---|
| 定位 | 调试工具集 | 作弊菜单 | 管理员权限系统 |
| UI 框架 | tML `UIState` + 自研 UILoader | 自研 `CheatSheet.UI` | 自研 `UIKit` |
| 模块化 | ⭐⭐⭐ Tool 单例 + Handler | ⭐⭐ 窗口类 | ⭐⭐ Service 基类 |
| 浏览器 | ⭐⭐⭐ Browser + Filter + Sort | ⭐⭐ 搜索 + 复选框 | ⭐⭐ ItemBrowser |
| 持久化 | TagCompound 统一 | 分散 | 网络配置 |
| 多人 | 有权限，偏单机 | 依赖 HEROs | ⭐⭐⭐ 组/权限/网络 |
| 代码质量 | 现代、注释多 | 老代码多、TODO 多 | 老代码多 |
| 适合我们参考 | ⭐⭐⭐ | ⭐⭐ | ⭐ |

## 四、对我们的结论

1. **UI 框架继续以 DragonLens 为准**：`UIState` + `UILoader` + `Browser`（UIGrid/搜索/筛选）。
2. **CheatSheet 可取**：
   - 滑入式窗口动画（权能轮盘/画卷空间打开时可以用）
   - 搜索 + 复选框过滤（buff 免疫勾选面板）
   - `ApplyEquipFunctional` 额外饰品槽写法
3. **HEROsMod 暂不需要**：除非以后做多人服务器权限（“谁能用 13 权能”）。
4. 这两个 mod 的代码都比 DragonLens 老，**不建议整体照搬**。

## 五、回到我们的项目

这次学习没有改动代码。当前最有价值的下一步仍然是：

1. 回归测试相合 ×16（200 / 1600）
2. 用 DragonLens 的 Browser 思路做「buff 免疫勾选」和「画卷空间」
3. 虚实、色空、演化三个暂停项等新方案
