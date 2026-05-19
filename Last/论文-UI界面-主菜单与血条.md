# §5.6 UI 界面系统

UI 界面系统是玩家与游戏交互的桥梁，负责呈现游戏状态信息、接收玩家操作指令并提供视觉反馈。本节从主菜单界面、各层级血条 UI 和雪花视觉特效三个方面阐述 UI 系统的设计与实现。

---

## §5.6.1 主菜单界面

主菜单是玩家进入游戏后首先接触的界面，承载着开始游戏、继续游戏和退出游戏三个核心入口。UI_MainMenu 组件挂载于主菜单场景（MainMenu Scene）的 Canvas 对象上，通过 Unity uGUI 按钮的 onClick 事件绑定各功能方法。

**开始游戏（StartGame）**。该方法直接调用 SceneManager.LoadScene(sceneName) 加载主游戏场景，sceneName 通过 Inspector 序列化字段配置（默认值为"MainScene"）。加载过程触发 Unity 的场景卸载/加载管线，旧场景的全部 GameObject 被销毁，新场景的 Awake()→Start() 生命周期依次执行，玩家从初始存档状态开始游戏。

**继续游戏（ContinueGame）**。该方法通过 SaveSystem.GetMostRecentSaveSlot() 获取最近一次存档的槽位编号（返回 -1 表示无存档）。若存在有效槽位，调用 SaveSystem.LoadFromSlotStatic(latestSlot) 从该槽位加载完整游戏状态（包括玩家位置、属性、装备、能力解锁、地图探索进度、已移除场景对象等），随后加载存档中记录的场景名称。若无存档数据，通过 Debug.Log 输出提示信息，玩家需选择"开始游戏"创建新存档。

**退出游戏（QuitGame）**。该方法通过条件编译宏 #if UNITY_EDITOR 区分编辑器环境和独立构建环境的退出行为：在编辑器中调用 EditorApplication.isPlaying = false 停止 Play Mode 运行；在独立构建中调用 Application.Quit() 关闭应用程序。这种双路径处理确保开发者在编辑器中测试时也能正常退出。

主菜单还包含 LoadGame() 方法作为 ContinueGame() 的别名入口，用于多按钮绑定或键盘快捷键触发的统一处理。

---

## §5.6.2 玩家血条 UI

UI_HealthBar 组件挂载于 Player 对象上，通过 [RequireComponent(typeof(PlayerStats))] 强制依赖 PlayerStats 组件。该组件在运行时完全通过代码动态构建 UI 层级，不依赖任何预制体或场景中的预置 Canvas。

**Canvas 构建**。BuildUi() 方法在 Awake() 中执行，依次创建以下 UI 层级：

- HealthBarCanvas（Canvas，ScreenSpaceOverlay 渲染模式，sortingOrder=10）：作为血条 UI 的根 Canvas，叠加于游戏画面之上。CanvasScaler 以 1920×1080 为参考分辨率，采用 ScaleWithScreenSize 缩放模式和 MatchWidthOrHeight(0.5) 匹配策略，确保在不同分辨率下血条的物理尺寸保持一致。
- HPRoot（RectTransform，锚定左上角）：根布局容器，锚点设置为 (0,1)，pivot 为左上角，通过 screenOffset（默认 (20, -20)）相对屏幕左上角偏移，实现血条在屏幕左上角固定显示。

**布局结构**。HPRoot 内部水平排列两个子元素：

- Portrait（头像）：Image 组件，挂载 portraitSprite 精灵图（默认从 Resources.Load\<Sprite\>("WitchHead") 加载），sizeDelta 为 portraitSize × portraitSize（默认 52×52 像素），启用 preserveAspect 保持宽高比。头像位于血条左侧，作为角色身份的视觉标识。
- Bar（血条）：三层嵌套结构实现带边框的圆角血条。最外层 BarBorder（Image，borderColor 填充，充当边框）包含 BarBg（Image，bgColor 填充，offsetMin/Max 向内缩进 border=2px，形成内边距）再包含 HPFill（Image，fillColor 填充）。血条总尺寸为 barWidth × barHeight（默认 220×22 像素），通过右侧 anchorMax.x 控制填充比例实现血量变化。

**圆角精灵程序化生成**。CreateRoundedSprite() 方法在运行时逐像素构建圆角矩形纹理，免除了外部图片资源的依赖。该方法以 cornerRadius（默认 10，Inspector 可调范围 0~11）为核心参数，生成四角均带有像素级圆弧的白色 Sprite。纹理尺寸为 (r×2)×(r×2)（其中 r = cornerRadius×4），通过距离函数 Dist() 判断每个像素是否位于圆角半径内——位于四角圆弧外的像素设为透明（Color.clear），圆弧内的像素设为白色（Color.white）。生成的 Sprite 使用 FullRect 网格类型和 border 矢量边框，配合 Image.Type.Sliced 实现任意尺寸拉伸时圆角不变形。

**血量刷新与低血量变色**。RefreshBar() 方法从 PlayerStats 读取 CurrentHealth 和 MaxHealth 计算生命值比例（ratio），通过设置 fillRect.anchorMax = new Vector2(ratio, 1f) 动态调整填充区域宽度。当 ratio ≤ lowHpPercent（默认 0.3，即生命值低于 30%）时，fillImage.color 切换为 lowHpColor（亮红色），高于阈值时使用 fillColor（暗红色）。这种视觉反馈在低血量时形成紧迫感，提示玩家采取治疗或回避行动。

**事件驱动更新**。组件在 Start() 中订阅 PlayerStats.StatsChanged 事件，将 RefreshBar 注册为回调。每当玩家属性发生变更（受到伤害、治疗、升级等），StatsChanged 事件自动触发血条刷新，实现了数据层到表现层的解耦。OnDisable() 中取消订阅防止内存泄漏，OnDestroy() 中销毁动态创建的 Canvas 对象进行清理。

---

## §5.6.3 敌人血条 UI

EnemyHealthBar 组件挂载于敌人对象上，通过 [RequireComponent(typeof(Enemy))] 强制依赖 Enemy 组件。与玩家血条使用 uGUI Canvas 不同，敌人血条采用 SpriteRenderer 在世界空间中渲染，使血条跟随敌人位置移动。

**世界空间定位**。血条的根 Transform（HPBar）SetParent 至敌人自身 transform，localPosition 设为零。GetBarWorldPosition() 方法在 LateUpdate 中每帧计算血条的世界坐标：若敌人存在 Renderer 组件，则取 bounds.center.x 为水平中心、bounds.max.y 为顶部，叠加 offset（默认 (0, 0.35, 0)）使血条显示在敌人头顶上方。血条通过 rootTransform.gameObject.SetActive(false/true) 在敌人死亡时自动隐藏，在敌人存活时显示。

**三层 SpriteRenderer 结构**。CreateBar() 方法创建三个子 GameObject 分别作为边框（Border）、背景（Bg）和填充（Fill），每个子对象挂载 SpriteRenderer 组件并共享同一个纯白 Sprite（通过 CreateSolidSprite() 生成 1×1 像素纹理，FilterMode.Point 确保拉伸时无模糊）。三层通过 sortingOrder 递增（border=20, bg=21, fill=22）控制渲染顺序，共用 sortingLayerName="Player" 确保与玩家处于同一渲染层。

- Border：localScale 设置为 (barWidth, barHeight, 1)，使用 borderColor（深紫色边框）填充，形成血条外框。
- Bg：localScale 设置为 (innerBarWidth, innerBarHeight, 1)（即扣除 borderPadding×2 = 0.06f 后的内缩尺寸），使用 bgColor（深色半透明背景）填充。
- Fill：与 Bg 同尺寸，使用 fillColor（暗红色）填充。血量变化通过调整 fillTransform.localScale.x 实现——localScale.x = innerBarWidth × hp，并同步调整 fillTransform.localPosition.x = -(innerBarWidth × (1-hp)) × 0.5 使填充条左对齐收缩。

**性能优化**。LateUpdate() 中使用 lastHp 缓存上一帧的血量百分比，仅当 Mathf.Abs(hp - lastHp) > 0.001f（即血量发生实质变化）时才更新 fillTransform 的 scale 和 position，避免每帧无效的 Transform 写入。fillRenderer.enabled 在 hp ≤ 0 时设为 false，阻止空血条填充层的渲染。

---

## §5.6.4 Boss 全宽血条 UI

BossScreenHealthBar 组件挂载于 Enemy_MiniBoss 对象上，通过 [RequireComponent(typeof(Enemy_MiniBoss))] 强制依赖。与普通敌人血条不同，Boss 血条采用 uGUI ScreenSpaceOverlay 渲染模式，以全宽横幅形式固定在屏幕顶部，类似传统动作游戏的 Boss 血条设计。

**延迟显示机制**。组件在 Awake() 中即完成全部 UI 构建，但 canvasObject 初始设为 SetActive(false) 隐藏。Reveal() 方法由 Enemy_MiniBoss.DetectPlayer() 在首次检测到玩家时调用——该方法同时设置 revealed 标志为 true 并激活 Canvas。这种设计确保在玩家遭遇 Boss 之前屏幕上不显示血条，避免提前剧透 Boss 的存在。

**布局结构**。BuildUi() 构建的 UI 层级如下：

- BossHealthBarCanvas（Canvas，sortingOrder=200，比普通 UI 更高以覆盖其他界面元素）：CanvasScaler 同样以 1920×1080 为参考分辨率。
- BossHealthRoot（RectTransform，锚定底部中央）：锚点 (0.5, 0)，pivot (0.5, 0)，通过 screenOffset（默认 (0, 36)）距离屏幕底部 36 像素。sizeDelta 为 rootSize（默认 2000×64），宽度超出参考分辨率以确保大屏幕下的视觉连续感。

BossHealthRoot 内部水平排列两个元素：

- BossName（Text）：位于最左侧，leftPadding=20px 偏移，占 nameWidth=180px 宽度。使用 nameFont（可在 Inspector 指定字体）渲染 nameFontSize=32 的加粗 Boss 名称文本。nameText.text 从 boss.BossDisplayName 属性读取，若未配置则使用 GameObject 名称。文本颜色为 nameColor（金色），对齐方式为 MiddleLeft。
- 血条区域：位于名称右侧，总宽 = rootSize.x - leftPadding - nameWidth - nameGap - rightPadding。同样采用三层嵌套结构（border → bg → fill），通过 fillRect.anchorMax.x 控制血量比例。与玩家血条使用 Sliced 圆角精灵不同，Boss 血条直接使用纯色 Image 填充，视觉风格更为简洁醒目以适配全宽布局。

**状态同步**。LateUpdate() 中每帧检查 boss.IsDead()——若 Boss 已死亡则 SetActive(false) 隐藏血条。血量变化通过比较 hp 与 lastHp 的差异（容差 0.001f）判断是否需要更新 fillRect.anchorMax，同样实现了变化时才更新的性能优化策略。nameText.text 每帧同步更新，确保 Inspector 中修改 bossDisplayName 后实时反映。

---

## §5.6.5 雪花视觉特效

UI_SnowEffect 组件通过纯 UI 方式实现全屏雪花飘落特效，无需任何外部纹理资源或粒子系统。该组件挂载于 UI Canvas 下的 GameObject 上，通过 [RequireComponent(typeof(RectTransform))] 强制依赖 RectTransform，运行时可动态挂载到任意 UI 层级（如背包面板或场景 HUD）。

**雪花粒子数据结构**。每片雪花由内部类 Snowflake 描述，包含以下动态属性：rect（RectTransform 引用，控制位置和旋转）、image（Image 组件引用，控制颜色和透明度）、speed（下落速度）、size（尺寸）、swayAmplitude 和 swayFrequency（水平摇摆幅度和频率）、swayOffset（摇摆相位偏移，用于差异化各雪花的摇摆节奏）、rotationSpeed（自转速度）、baseX（基准水平位置）、y（当前垂直位置）。所有属性在雪花重置时通过随机数独立生成，确保每片雪花的运动轨迹各不相同。

**程序化精灵生成**。EnsureSnowSprite() 静态方法利用 Texture2D.whiteTexture（Unity 内置的 1×1 白色纹理）通过 Sprite.Create() 生成纯白 Sprite，存储于静态字段 snowSprite 供所有雪花实例共享。这种静态共享策略避免了每片雪花独立创建纹理的内存开销——无论雪花数量多少，全局仅存在一份 Sprite 和 Texture 资源。

**运动与摇摆模型**。Update() 中每帧遍历所有雪花执行以下运动计算：

- 垂直下落：flake.y -= flake.speed × deltaTime，speed 在 fallSpeedRange（默认 40~120 像素/秒）范围内随机取值。雪花超出 Canvas 底部边界（minY - flake.size）后自动重置至顶部（maxY + 随机偏移），形成无限循环的降雪效果。
- 水平摇摆：x = flake.baseX + Mathf.Sin(time × flake.swayFrequency + flake.swayOffset) × flake.swayAmplitude。每片雪花以独立的频率（swayFrequencyRange 默认 0.6~1.8）和幅度（swayAmplitudeRange 默认 8~30 像素）沿正弦曲线水平摆动，swayOffset 提供相位差异化使多片雪花不同步摇摆。当雪花漂移至 Canvas 水平边界外时自动回绕至对侧。
- 自转：flake.rect.Rotate(0, 0, rotationSpeed × deltaTime)，rotationSpeed 在 rotationSpeedRange（默认 -40°~40°/秒）内随机取值，正负值分别对应顺时针和逆时针旋转。

**视觉配置**。每片雪花的 Image.color 由 snowTint 基础色调叠加随机透明度构成——透明度在 alphaRange（默认 0.35~0.9）范围内随机取值，使远近雪花呈现不同的通透感，增强景深层次。snowflakeCount（默认 48 片）和所有运动参数范围均通过 Inspector 序列化字段暴露，开发者可在编辑器内实时调参。组件提供 [ContextMenu("Rebuild Snowflakes")]，支持在编辑器中一键重建所有雪花以预览调参效果。

**时间缩放独立**。组件通过 ignoreTimeScale 开关（默认 true）控制时间计算方式——启用时使用 Time.unscaledDeltaTime 和 Time.unscaledTime 驱动动画，在游戏暂停（如打开背包面板、Time.timeScale=0）期间雪花仍然飘落。这与 GridMagicAmbient 环境粒子的设计理念一致：保持 UI 层环境氛围的连续性，避免暂停破坏视觉沉浸感。

**生命周期管理**。OnEnable() 中调用 RebuildSnowflakes() 创建 snowflakeCount 片雪花并随机初始化所有属性，OnDisable() 中调用 ClearSnowflakes() 遍历销毁所有雪花 GameObject——在运行时使用 Destroy()，在编辑模式下使用 DestroyImmediate() 以避免内存泄漏。OnValidate() 方法对各参数范围进行合法性钳制（如 count ≥ 1、范围最小值 ≤ 最大值、alphaRange 钳制在 [0,1]），确保 Inspector 输入不会导致运行时异常。

---

## §5.6.6 能力拾取与通知系统

AbilityPickup 组件负责场景中能力道具的交互拾取，使玩家通过触碰触发区域并按交互键获取新能力。该组件支持与 EquipmentPickup 共存于同一 GameObject，实现"拾取一件道具同时获得能力与装备"的复合拾取效果。

**触发检测**。组件通过 OnTriggerEnter2D 和 OnTriggerExit2D 管理 nearbyPlayer 引用。当玩家 Collider 进入道具的 Trigger 碰撞体时，通过 GetComponent 和 GetComponentInParent 获取 Player 组件并存储引用；玩家离开时清空引用。这种近场检测机制配合 Update() 中的按键监听，实现了玩家靠近后按交互键（默认 E 键）即可拾取的交互模式。

**拾取流程**。Collect() 方法在玩家按下交互键时执行：(1) 调用 nearbyPlayer.UnlockAbility(abilityToUnlock) 解锁目标能力——若能力已解锁则返回 false 中止拾取，防止重复解锁；(2) 设置 collected 标志为 true，阻止后续重复拾取；(3) 通过 SFXManager 播放 pickupSFX 音效提供听觉反馈；(4) 调用 UI_AbilityNotification.Show(abilityToUnlock, abilityDescription) 弹出能力解锁通知；(5) 若配置了 pickupVisual，将其 SetActive(false) 隐藏视觉模型；(6) 通过 SaveSystem.MarkSceneObjectRemoved(this) 将自身标记为已移除；(7) 根据 destroyAfterPickup 和是否与 EquipmentPickup 共存决定销毁策略——若共享存在则仅禁用 AbilityPickup 脚本（enabled = false）保留 GameObject 供 EquipmentPickup 继续使用，否则 Destroy 或 SetActive(false) 移除道具。

**存档恢复支持**。ApplyCollectedState() 方法在加载存档时由外部调用，将 collected 置为 true 并隐藏视觉，使已拾取的道具在存档读档后不会重复出现。CollectFromEquipment() 方法供 EquipmentPickup 在装备拾取时同步触发能力解锁，实现了两种拾取类型的联动。

**UI_AbilityNotification 通知弹窗**。该组件采用单例模式（instance 静态字段），提供静态方法 Show(PlayerAbilityType, string) 供全局调用。若场景中不存在实例则动态创建 GameObject 并挂载组件，确保在任何时机调用均可正常显示。

通知面板的 Canvas 采用 ScreenSpaceOverlay 渲染模式，sortingOrder=20，锚定屏幕顶部中央（pivot 为顶部），通过 screenOffset（默认 (0, -100)）向下偏移。面板结构包含三个层次：

- AccentLine（顶部装饰线）：2px 高的横向线条，使用 accentColor（紫色调）填充，起视觉分割和品牌标识作用。
- Title（标题文本）：使用 TextMeshProUGUI 渲染，fontSize=14，居中显示"NEW ABILITY UNLOCKED"，颜色为 accentColor，位于面板中上部。
- AbilityName（能力名称）：TextMeshProUGUI 渲染，fontSize=20，加粗，根据 ability 参数通过 GetAbilityName() 映射为对应名称——Dash→"Shadow Dash"、DoubleJump→"Double Jump"、WallJump→"Wall Jump"、AirDash→"Air Dash"、EchoSwap→"Echo Swap"、RangedAttack→"Ranged Attack"。
- Description（描述文本）：TextMeshProUGUI 渲染，fontSize=12，透明度 0.7，显示传入的 description 参数，若为空则隐藏。

**淡入淡出动画**。ShowRoutine() 协程控制通知的生命周期：SetVisible(true) 激活面板并设置 canvasGroup.alpha=1，保持 displayDuration（默认 2.5 秒）后进入 fadeDuration（默认 0.8 秒）的线性淡出阶段——每帧按 (elapsed/fadeDuration) 比例递减 alpha 直至 0，最后 SetVisible(false) 隐藏面板。CanvasGroup 组件控制透明度，可同时影响面板内所有子 UI 元素。

---

## §5.6.7 经验值磁铁拾取系统

ExperiencePickup 组件实现了经验值与金币拾取物的自动磁铁吸附效果。该组件同时用于敌人死亡奖励和经验道具的生成，通过 Configure(int experience, int gold) 方法在实例化后进行数值配置。

**延迟吸附机制**。组件在 Awake() 中设置 delayTimer = autoCollectDelay（默认 0.2 秒），在计时器归零前拾取物保持静止——这段短暂延迟防止了拾取物生成瞬间即被吸收导致的视觉跳变，给予玩家观察掉落位置的窗口。

**磁铁吸引逻辑**。延迟结束后，Update() 调用 FindClosestPlayerInRange() 搜索范围内最近的 PlayerProgression 实例——通过 FindObjectsOfType<PlayerProgression>() 获取场景中所有 PlayerProgression，取距离 ≤ attractionRange（默认 100 单位）且最近者作为吸附目标。找到目标后，拾取物以变速飞向玩家：

- 速度加速：currentFlySpeed 通过 Mathf.MoveTowards 从 minFlySpeed（默认 4 单位/秒）向 maxFlySpeed（默认 12 单位/秒）以 acceleration（默认 18 单位/秒²）递增，产生逐渐加速的视觉惯性。
- 位置逼近：transform.position 通过 Vector3.MoveTowards 以当前速度向玩家位置移动。

**吸收判定**。当拾取物与玩家距离 ≤ absorbDistance（默认 0.35 单位）时触发 Absorb()：调用 targetProgression.GainExperience(experienceAmount) 增加经验值，若有金币则调用 AddGold()，播放 pickupSFX，随后根据 destroyAfterPickup 决定是否销毁 GameObject。

**编辑器可视化**。OnDrawGizmosSelected() 以金黄色半透明线框球体绘制 attractionRange 范围，辅助开发者在 Scene 视图中直观调整吸附半径参数。

---

## §5.6.8 三种血条设计对比

项目中的三种血条分别服务于不同的信息传达需求，设计策略各有侧重：

| 特性 | UI_HealthBar（玩家） | EnemyHealthBar（普通敌人） | BossScreenHealthBar（Boss） |
|------|---------------------|---------------------------|----------------------------|
| 渲染方式 | uGUI Canvas (Overlay) | SpriteRenderer (World Space) | uGUI Canvas (Overlay) |
| 位置 | 屏幕左上角固定 | 跟随敌人头顶 | 屏幕底部中央固定 |
| 尺寸 | 220×22 px | 1.2×0.14 世界单位 | 2000×64 px |
| 显示时机 | 始终显示 | 始终显示 | 首次遭遇时 Reveal() |
| 隐藏条件 | 无 | 敌人死亡 | Boss 死亡 |
| 头像 | 有（WitchHead） | 无 | 无（以名称文本代替） |
| 圆角 | 程序化圆角（Sliced） | 纯色矩形 | 纯色矩形 |
| 低血量变色 | 有（<30% 亮红） | 无 | 无 |
| 更新机制 | StatsChanged 事件 | LateUpdate 变化检测 | LateUpdate 变化检测 |

玩家血条附带角色头像和低血量变色，强调角色扮演的代入感和生存紧迫感。普通敌人血条采用世界空间跟随，使玩家在战斗中无需移动视线即可感知敌人血量。Boss 血条以全宽横幅形式突出 Boss 战的仪式感和威胁等级，名称文本提供角色标识，延迟显示避免剧透。
