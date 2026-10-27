# 14 · DragonLens 学习笔记

> 仓库：https://github.com/ScalarVector1/DragonLens
> 定位：调试 / 管理员工具集（不是玩法 mod），代码组织很适合抄。

## 一、规模与结构

约 370 个文件、120 个 `.cs`，按职责分层：

```
Core/
  Systems/ToolSystem/        Tool 抽象 + ToolHandler + 热键
  Systems/ToolbarSystem/     工具栏
  Systems/ThemeSystem/       主题（颜色 / 图标 / 边框提供者）
  Loaders/UILoading/         UILoader + DraggableUIState 基类
  Systems/PermissionHandler  多人管理员权限
Content/
  Tools/Spawners/            物品 / NPC / 弹幕 / buff / 尘 / 图格 / 音效
  Tools/Despawners/          清除上述实体
  Tools/Gameplay/            Godmode / Noclip / 时间 / 天气 / 难度 / 出生点 / 磁铁 / 喷漆
  Tools/Map/                 揭示地图 / 隐藏 / 传送
  Tools/Visualization/       碰撞箱 / 自由相机 / 补光 / 锁定相机
  Tools/Multiplayer/         玩家管理 / 背包管理
  Tools/Developer/           资源管理 / 暂停 / 着色器编译
  GUI/                       Browser / FieldEditor / Toolbar / Theme
  Filters/ Sorts/            筛选与排序
```

## 二、核心设计

### 1. Tool 抽象（`Core/Systems/ToolSystem/Tool.cs`）

- 继承 `ModType`，是**单例**；tML 自动注册、自动生成一个 Keybind
- 关键成员：`IconKey`、`DisplayName`、`Description`、`HasRightClick`、`OnActivate`、`OnRightClick`、`SaveData` / `LoadData`、`SendPacket` / `RecievePacket`、`NetSend`
- 图标绘制默认由 `ThemeHandler.GetIcon(IconKey)` 取，子类可重写（高亮/发光）

### 2. ToolHandler（`ToolHandler.cs`）

- `ModContent.GetContent<Tool>()` 枚举全部工具
- `ToolPlayer.ProcessTriggers` 统一处理热键与右键
- 数据持久化：`Main.SavePath/DragonLensLayouts/ToolData`，TagCompound 按工具名分块
- 网络：包第一段是 `"ModName/ToolName"`，路由到对应 Tool 的 `RecievePacket`

### 3. 工具栏与主题

- `ToolbarSystem` 画一排按钮，开启的工具高亮
- `ThemeSystem` 拆成 `ThemeColorProvider` / `ThemeIconProvider` / `ThemeBoxProvider`，可以整套换皮（Default / HERO 等）

### 4. UILoader / DraggableUIState

- `UILoader.GetUIState<T>()` 懒加载单例 UIState，避免自己管理
- `DraggableUIState` 提供拖动、`InsertionIndex` 插到 “Vanilla: Mouse Text” 层之前

### 5. Browser（`Content/GUI/Browser.cs`）

- 通用浏览器：`UIGrid` + 搜索栏 + 筛选面板 + 排序 + 按钮大小滑块 + 滚动条
- 子类只实现 `PopulateGrid`，把 `BrowserButton` 塞进网格
- 列表/网格双模式，筛选按钮可折叠

### 6. FieldEditor

- 反射式字段编辑器：int / float / string / bool / Vector2 / Rectangle / Item / NPC / Player / Projectile / Point16 / Color
- `ObjectEditor` 组合多个 FieldEditor，用来实时改实体属性

### 7. 权限

- `PermissionHandler.CanUseTools(player)`：单人直接 true，多人看 host 权限
- 每个工具可 `ResetForNonAdmin`，非管理员进服时强制关闭

## 三、两个可直接借鉴的实现

### Godmode（`Content/Tools/Gameplay/Godmode.cs`）

```csharp
public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
    => Godmode.godMode && PermissionHandler.CanUseTools(Player);
```

外加 `PostUpdate` 回满血、`PreKill` 拦死亡。**说明 `ImmuneTo` 恒 true 能挡住所有伤害**——
我们之前虚实失败，问题在“识别来源”，不在钩子本身。

### SpawnPoint（`Content/Tools/Gameplay/SpawnPoint.cs`）

```csharp
Main.spawnTileX = (int)((Player.position.X - 8f + Player.width / 2f) / 16f);
Main.spawnTileY = (int)((Player.position.Y + Player.height) / 16f);
```

这就是戌绘卷「设置出生点」的标准写法，比我们之前用 `Center` 更准（按脚底）。

### Hitboxes（`Content/Tools/Visualization/Hitboxes.cs`）

- `On_Main.DrawInterface` 事件挂钩，先画碰撞箱再调 `orig`
- `HitboxOption` 支持 none / box / 完整等模式，逐类实体开关

## 四、对我们的用法

| 我们的需求 | 可借鉴 |
|---|---|
| 权能轮盘 | Tool 化：12 个权能各一个 Tool（图标 + 热键 + 高亮），或保留现有轮盘，加 Toolbar 快捷 |
| Buff 免疫勾选 UI（逍遥/寅诗壶） | Browser + 复选框列表 + `SaveData` 持久化 |
| 画卷空间（大背包） | Browser/UIGrid + `Player` 存档；参考 `InventoryManager` 的 UI 组织 |
| 虚实选择性免疫 | Godmode 证明 `ImmuneTo` 可行；需要正确的 `DamageSource` 识别（可加日志打印） |
| 色空缩小判定 | 参考 `ModifyHitByProjectile` 拦截 + 小判定圈，而不是改 `Player.width/height` |
| 开发期调试 | Hitboxes / FreeCamera / Pause 这类工具很值得先做一个精简版 |

## 五、结论

DragonLens 的「Tool 单例 + Handler + Toolbar + Browser + FieldEditor」是一套成熟的**工具型 UI 框架**，
我们要做的权能面板、buff 选择、画卷空间，都可以按这个骨架拆，而不是继续堆在 `ModSystem.PostDrawInterface` 里。
