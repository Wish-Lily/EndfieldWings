# 终末地 · 祀岁翼（EndfieldWings）

适配 tModLoader 1.4.4 的翅膀饰品 mod，主题为《明日方舟：终末地》风格的「祀」。

- 无限飞行，巡航 / 悬浮两种模式，空中双击跳跃切换
- 十二刻印饰品 + 权能轮盘 + 合并 HUD
- 画卷空间（随身 50 格储物，每天翻倍）
- 主界面主题（背景图 + 主题曲，素材需自备）

设计与进度记录见 [docs/00-索引与状态.md](docs/00-索引与状态.md)。

## 仓库说明

- 本仓库**只包含代码和自制资源**
- `Assets/UI/Theme/ThemeBg.png`（角色宣传图）与 `Assets/Music/HuaSiShi.mp3`（《花祀时》）是**第三方素材，不随仓库分发**，详见 [ASSETS.md](ASSETS.md)
- 代码采用 [MIT 许可证](LICENSE)；第三方素材不在授权范围内

## 本地构建

1. 克隆到 tModLoader 的 ModSources 目录：

```
git clone https://github.com/<你的用户名>/EndfieldWings.git
```

放置到：

```
文档\My Games\Terraria\tModLoader\ModSources\EndfieldWings
```

2. 按 [ASSETS.md](ASSETS.md) 放入两个素材文件（缺少也不会崩，只是没有背景和音乐）

3. 启动 tModLoader → **Workshop → Develop Mods**，找到 `EndfieldWings`
4. **Build + Reload** → 启用 → 进游戏

## 目录结构

```
EndfieldWings.cs                 Mod 主类：键位、音乐、代码内本地化
build.txt / description*.txt     构建与商店描述
icon.png / icon_small.png        模组图标
Common/
  Players/                       飞行、权能、刻印、画卷空间
  Authorities/                   13 权能与轮盘数据
  GlobalNPCs/ GlobalProjectiles/ 因果种子、色空弹幕拦截
  Systems/                       形（配方解锁）
  UI/                            权能轮盘、合并 HUD、画卷空间、主界面主题
  Configs/                       客户端设置
Content/
  Items/Accessories/             祀岁翼 + 12 刻印
  Items/Test/                    测试用增幅与武器
Localization/                    中英文（关键文本也在代码里兜底）
Assets/                          贴图、印记图标、主题素材
docs/                            设计与进度文档
```

## 主要功能

| 模块 | 状态 |
|---|---|
| 祀岁翼：无限飞 / 双模式 / 双击切换 | ✅ |
| 12 刻印被动 | ✅ |
| 权能轮盘（O 键） | ✅ |
| 13 权能 | 11 个可用，2 个临时方案（色空、虚实），1 个待定（演化） |
| 合并 HUD（右上角，右键拖动） | ✅ |
| 画卷空间（U 键，每天翻倍 + 单次复制） | ✅ |
| 主界面主题（ModMenu） | ✅ 背景 + 音乐，按钮仍居中 |
| 主界面按钮左移 | ❌ 因与 Fargo/InnoVault 的 DrawMenu 钩子冲突，暂时停用 |
| 岁时 Boss / 领域 | ⬜ 未开始 |

## 已知限制

- 主界面「接管绘制」（左侧按钮）当前禁用，开关无效
- 色空用「拦截弹幕 + 1×1 判定」实现，不是真正缩小碰撞箱
- 虚实：溺水在 Calamity 环境不走 `ImmuneTo`，未拦截
- 演化：暂为无敌
- 多人未同步权能开关与画卷空间
- hjson 可能被 tML 的本地化导出追加坏，关键文本已改为代码内默认值

## 开发

构建命令（游戏需关闭）：

```
cd <tModLoader 安装目录>
dotnet .\tModLoader.dll -build "<ModSources>\EndfieldWings"
```

## 素材声明

主题图与主题曲版权归原权利方，本项目仅本地使用，不提供再分发授权。
