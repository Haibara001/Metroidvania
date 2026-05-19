# §5.X 回声技能系统

回声技能（Echo）是本游戏 Metroidvania 玩法中最具特色的核心机制之一，融合了分身诱敌、战术传送和环境解谜三种功能。该系统的设计围绕着"施放—留存—交换"的操作循环展开，由 WitchEcho 回声分身、EchoCast/EchoSwap 两种玩家状态和 EchoPassage 回声通道三类组件协同实现。

---

## §5.X.1 回声技能概述

回声技能的操作入口为 Q 键，由 Player.CheckForEchoSwapInput() 方法统一管理。该方法首先检查玩家是否已解锁 PlayerAbilityType.EchoSwap 能力——若未解锁则直接返回，确保技能在能力门控系统中被正确限制。已解锁状态下，根据当前是否存在活跃的 WitchEcho 实例（activeEcho != null）自动选择执行路径：

- **无活跃分身 → EchoCast（回声施放）**：玩家在原地召唤一个 WitchEcho 回声分身，消耗本次施放机会。
- **有活跃分身 → EchoSwap（回声交换）**：玩家瞬间传送至分身位置，分身同时触发范围爆炸伤害。

两种操作均受 echoSwapCooldown（默认 0.35 秒）冷却限制，防止连续按键导致的误操作。

---

## §5.X.2 WitchEcho 回声分身

WitchEcho 类是回声系统的核心实体，同时实现 IDamageable 和 IAggroTarget 两个接口，使其既能承受伤害又能作为敌人的仇恨目标。

**初始化配置**。Initialize(Player, float, float, float) 方法在创建时由 Player.CreateEchoFromAnimation() 调用，接收拥有者引用、存活时间（duration，默认 4 秒）、爆炸半径（burstRadius，默认 2.5 单位）和爆炸伤害（burstDamage，默认 20 点）四个参数。初始化过程依次执行：

(1) 将分身 GameObject 的 layer 设置为拥有者玩家相同的层级，确保物理碰撞检测的一致性。

(2) 配置 CircleCollider2D 触发器——半径设为 Mathf.Max(0.3f, damageRadius × 0.35f)，用于检测敌人与分身的接触。

(3) 复制拥有者玩家子对象的 SpriteRenderer 外观——克隆 sprite 精灵图、flipX 朝向、sortingLayerID 渲染层和 sortingOrder 排序值（-1 使分身渲染在玩家后方），叠加 echoTint 色调（默认浅粉紫色 RGBA(1, 0.75, 0.9, 0.7)）实现半透明的分身视觉效果，与玩家本体形成视觉区分。

**仇恨吸引机制**。WitchEcho 实现的 IAggroTarget 接口中，AggroPriority 返回 aggroPriority（默认 10），远高于玩家的优先级 0。在 Enemy_Normal.DetectPlayer() 的优先级+距离双重标准目标选择算法中，分身因更高的 AggroPriority 值会被敌人优先锁定。同时 CanBeTargeted 属性在 burstTriggered 为 true（分身已爆炸/销毁）时返回 false，防止敌人在分身消失后仍尝试攻击无效目标。这种设计使分身具备天然的战术诱敌功能——玩家可在危险区域施放 EchoCast 吸引火力后安全撤退。

**花瓣爆发（Petal Burst）**。WitchEcho 在以下三种情况下触发 TriggerPetalBurst()：

- **计时耗尽**：Update() 中 timer 递减至 ≤ 0 时自动触发。
- **受到伤害**：TakeDamage(float) 被调用时立即触发，分身承受一次攻击后爆炸。
- **玩家交换**：Player.SwapWithEchoFromAnimation() 中主动调用触发。

TriggerPetalBurst() 通过 burstTriggered 标志确保仅执行一次。DealBurstDamage() 方法以分身位置为圆心、burstRadius 为半径执行 OverlapCircleAll 物理检测，使用 HashSet\<IDamageable\> 去重后对范围内每个有效目标调用 TakeDamage(burstDamage)。伤害判定的排除列表包括：(1) 分身自身（damageable == this）；(2) 拥有者玩家（damageable == owner），防止回声爆炸误伤玩家。

爆炸后依次调用 owner.NotifyEchoDestroyed(this) 清空玩家的 activeEcho 引用，随后 Destroy(gameObject) 销毁分身对象。

---

## §5.X.3 EchoCast 回声施放

PlayerEchoCastState 是施放回声的玩家状态，通过 animBoolName="EchoCast" 触发对应的施法动画。

**状态流程**。Enter() 阶段：调用 player.ZeroVelocity() 停止玩家移动，在施法期间锁定位置；播放 sfxEchoCast 音效提供听觉反馈。Update() 阶段：每帧持续 ZeroVelocity() 确保静止；等待 triggerCalled 标志——该标志由 PlayerAnimationTriggers 在施法动画关键帧通过 Animation Event 回调 AnimationFinishTrigger() 设置，确保回声分身在动画的恰当时机（而非状态进入瞬间）生成。

**分身创建**。triggerCalled 触发后调用 player.CreateEchoFromAnimation()：

(1) 计算生成位置：默认以 echoSpawnOffset（默认 (0, -0.5)）偏移玩家当前位置，使分身出现在玩家脚下而非完全重叠。

(2) EchoPassage 通道检测：若玩家处于 EchoPassage 的触发范围内（由 OnTriggerEnter2D 设置 nearbyEchoPassage 引用），则调用 passage.TryGetDestination(out Vector3) 获取 exitPoint 位置，直接在该位置生成分身而非玩家脚底。此机制用于回声解谜——玩家可将分身"投送"至栅栏对面或高台等无法直接到达的位置，随后通过 EchoSwap 传送过去。

(3) 实例化 GameObject 并挂载 WitchEcho 组件，调用 Initialize() 完成配置，赋值给 activeEcho 字段。

状态完成后通过 GetDefaultLocomotionState() 返回默认移动状态（地面→Idle，空中→Air）。

---

## §5.X.4 EchoSwap 回声交换

PlayerEchoSwapState 是交换传送的玩家状态，通过 animBoolName="EchoSwap" 触发传送动画。

**状态流程**。与 EchoCast 类似，Enter() 阶段 ZeroVelocity 锁定移动并播放 sfxEchoSwap 音效。Update() 中等待 triggerCalled 后执行 player.SwapWithEchoFromAnimation()：

(1) 有效性检查：若 activeEcho 为 null（分身在动画播放期间已被破坏），直接返回，状态切换至默认移动状态，不执行传送。

(2) 传送：通过 activeEcho.GetSwapPosition() 获取分身世界坐标，直接将 player.transform.position 设置为该坐标，实现瞬间位置交换。

(3) 爆炸触发：调用 activeEcho.TriggerPetalBurst() 手动引爆分身，在传送完成的同时对分身周围敌人造成 burstDamage 伤害。传送+爆炸的组合使 EchoSwap 兼具位移和范围攻击双重效果——玩家可在分身吸引敌群聚集后交换过去，利用爆炸清场。

---

## §5.X.5 EchoPassage 回声通道

EchoPassage 组件挂载于场景中的特定触发器对象，是实现回声解谜的环境元素。该组件仅暴露一个配置字段——exitPoint（Transform 类型），指向通道的目标出口位置。

**交互机制**。Player 类通过 OnTriggerEnter2D/OnTriggerExit2D 检测 EchoPassage 碰撞体，进入时存储 nearbyEchoPassage 引用，离开时清空。当玩家在 EchoPassage 范围内按下 Q 键执行 EchoCast 时，CreateEchoFromAnimation() 检测到 nearbyEchoPassage != null，优先使用 passage.TryGetDestination() 返回的出口坐标而非玩家脚下偏移位置生成分身。

**关卡设计应用**。EchoPassage 的设计服务于 Metroidvania 类型的探索解谜需求：场景中可配置栅栏、墙壁或高台等物理障碍物，在障碍物两侧分别放置 EchoPassage 入口触发器和 exitPoint。玩家站在入口处施放 EchoCast→分身出现在障碍物另一侧→EchoSwap 传送到分身位置，从而绕过物理障碍实现跨越。这种"入口施放—出口生成—交换穿越"的三步解谜模式是 Metroidvania 游戏能力门控设计的典型范例。

---

## §5.X.6 回声系统设计总结

回声技能系统的设计体现了以下核心设计原则：

**操作一致性**。施放和交换共享同一个输入键（Q 键），由系统根据当前游戏状态（是否有活跃分身）自动区分行为，避免引入额外的按键复杂度。PlayerStateMachine 中 echoCastState 和 echoSwapState 均为独立的 PlayerState，遵循与其他 10 个玩家状态相同的 Enter/Update/Exit 生命周期模式，保持了状态机架构的统一性。

**战术深度**。WitchEcho 通过 IAggroTarget 接口的高仇恨优先级（10 > 玩家的 0）实现分身诱敌；花瓣爆发机制使分身在消失时贡献最后的范围伤害；玩家可在分身存续期间（4 秒）自主选择交换时机（立即传送或等待敌群聚集后传送+爆炸），赋予战斗高度的策略性。

**探索解谜**。EchoPassage 回声通道将战斗技能延伸至探索领域，在不增加额外能力类型的前提下，利用现有回声机制实现障碍跨越，是 Metroidvania 类型"一技多用"设计哲学的体现。

**视觉辨识**。分身通过 echoTint 半透明紫色色调、渲染在玩家后方的排序层级和复制的玩家 Sprite 实现独立且可辨识的视觉呈现，确保玩家在战斗中能快速区分本体与分身。
