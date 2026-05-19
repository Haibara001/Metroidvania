# -*- coding: utf-8 -*-
"""生成系统总体架构图 — 分层模块化架构"""
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.font_manager import FontProperties
import os, sys

# ---- 中文字体 ----
font_paths = [
    "C:/Windows/Fonts/simhei.ttf",
    "C:/Windows/Fonts/msyh.ttc",
    "C:/Windows/Fonts/simsun.ttc",
]
cn_font = None
for fp in font_paths:
    if os.path.exists(fp):
        cn_font = FontProperties(fname=fp, size=11)
        cn_font_sm = FontProperties(fname=fp, size=9)
        cn_font_lg = FontProperties(fname=fp, size=14)
        cn_font_title = FontProperties(fname=fp, size=18)
        break

if cn_font is None:
    print("Warning: No Chinese font found, falling back to ASCII labels")
    cn_font = FontProperties(size=11)
    cn_font_sm = FontProperties(size=9)
    cn_font_lg = FontProperties(size=14)
    cn_font_title = FontProperties(size=18)

# ---- Canvas ----
fig, ax = plt.subplots(1, 1, figsize=(22, 16), dpi=150)
ax.set_xlim(0, 22)
ax.set_ylim(0, 16)
ax.axis("off")

def draw_box(x, y, w, h, text, color="#E8F0FE", border="#3B5998", font=cn_font,
             text_color="black", linewidth=1.5, alpha=1.0, zorder=2):
    rect = mpatches.FancyBboxPatch(
        (x, y), w, h, boxstyle="round,pad=0.15",
        facecolor=color, edgecolor=border, linewidth=linewidth,
        alpha=alpha, zorder=zorder
    )
    ax.add_patch(rect)
    # Split multi-line text
    lines = text.split("\n")
    total_h = len(lines) * 0.35
    for i, line in enumerate(lines):
        yy = y + h / 2 + total_h / 2 - i * 0.35 - 0.15
        ax.text(x + w / 2, yy, line, ha="center", va="center",
                fontproperties=font, fontsize=font.get_size(),
                color=text_color, zorder=zorder + 1)

def draw_arrow(x1, y1, x2, y2, color="#666666", lw=1.2, style="-", zorder=1):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle="->", color=color, lw=lw,
                               linestyle=style, connectionstyle="arc3,rad=0"),
                zorder=zorder)

def draw_bidirectional(x1, y1, x2, y2, color="#666", lw=1.2, zorder=1):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle="->", color=color, lw=lw,
                               connectionstyle="arc3,rad=0"), zorder=zorder)
    ax.annotate("", xy=(x1, y1), xytext=(x2, y2),
                arrowprops=dict(arrowstyle="->", color=color, lw=lw,
                               connectionstyle="arc3,rad=0"), zorder=zorder)

# ============================================================
# LAYOUT (left to right with vertical spacing):
#   Col 1: 表现层
#   Col 2: 逻辑层 (big container with sub-modules)
#   Col 3: 数据层
# ============================================================

# ---- TITLE ----
ax.text(11, 15.3, "系统总体架构图", ha="center", va="center",
        fontproperties=cn_font_title, fontweight="bold")

# ---- LAYER 3: 数据层 (top) ----
draw_box(14.5, 10.0, 6.5, 4.5,
         "数  据  层\n\n"
         "SaveSystem (单例，DontDestroyOnLoad)\n"
         "SaveData (JSON序列化)\n"
         "EquipmentItemData (ScriptableObject)\n"
         "PlayerPrefs / 文件I/O",
         color="#FFF3CD", border="#D4A017", zorder=2)

# ---- LAYER 2: 逻辑层 (middle, largest) ----
draw_box(0.5, 1.8, 18.5, 11.5,
         "逻  辑  层",
         color="#F5F5F5", border="#AAAAAA", linewidth=1.0, alpha=0.3, zorder=0)

ax.text(9.75, 13.0, "逻  辑  层", ha="center", va="center",
        fontproperties=cn_font_lg, fontweight="bold", color="#333333")

# Sub-modules in logic layer (grid layout)
sub_modules = [
    # Row 1
    ("玩家模块\nPlayer\nPlayerStateMachine\n12个PlayerState\nWitchEcho\nParallaxBackground",
     1.0, 8.5, 3.8, 4.0, "#D5E8D4", "#82B366"),
    ("敌人模块\nEnemy / Enemy_Normal\nEnemy_Bug / Enemy_MiniBoss\nEnemyStateMachine\n各敌人State (×5)"
     "\nBossPhaseAura",
     5.2, 8.5, 3.8, 4.0, "#F8CECC", "#B85450"),
    ("战斗模块\nIDamageable\nIAggroTarget\nWitchThrowing (投射物)\n攻击判定 / 伤害缓冲\n接触伤害",
     9.4, 8.5, 3.8, 4.0, "#DAE8FC", "#6C8EBF"),
    ("探索模块\nTilemap (多层)\nUndiscoveredAreaManager\nAbilityGate / AbilityPickup\nEchoPassage\nDiscoverableArea",
     13.6, 8.5, 3.8, 4.0, "#E1D5E7", "#9673A6"),

    # Row 2
    ("成长模块\nPlayerStats\nPlayerProgression\nPlayerInventory\nPlayerEquipment\nExperiencePickup",
     1.0, 3.2, 3.8, 4.0, "#D5E8D4", "#82B366"),
    ("UI模块\nUI_Inventory\nUI_HealthBar / EnemyHealthBar\nBossScreenHealthBar\nDamagePopup\n"
     "UI_AbilityNotification\nUI_SnowEffect",
     5.2, 3.2, 3.8, 4.0, "#F8CECC", "#B85450"),
    ("音频模块\nBGMManager\nSFXManager\n(5通道AudioSource池化)",
     9.4, 3.2, 3.8, 3.2, "#DAE8FC", "#6C8EBF"),
    ("特效模块\nGridMagicAmbient\n(45光点+10光线)",
     13.6, 3.2, 3.8, 2.5, "#E1D5E7", "#9673A6"),
]

for text, x, y, w, h, face, border in sub_modules:
    draw_box(x, y, w, h, text, color=face, border=border,
             font=cn_font_sm, linewidth=1.2)

# ---- LAYER 1: 表现层 (bottom) ----
draw_box(0.5, 0.5, 18.5, 2.0,
         "表  现  层        "
         "Scene系统  │  Sprite Renderer  │  UI Canvas  │  Animator  │  Camera  │  Input Manager  │  Audio Listener",
         color="#D9EAD3", border="#6AA84F", zorder=2)

# ---- LAYER 0: 用户 (bottom-most) ----
draw_box(7.5, -0.8, 5.0, 0.8, "玩家输入 (键盘/鼠标)", color="#EEEEEE", border="#999999",
         font=cn_font_sm, linewidth=1.0, zorder=2)

# ---- ARROWS between layers ----
# User → 表现层
draw_arrow(10, 0.2, 10, 0.5, color="#666", lw=1.5)
# 表现层 → 逻辑层
draw_bidirectional(3, 2.5, 3, 3.2, color="#666", lw=1.3)
draw_bidirectional(6, 2.5, 6, 3.2, color="#666", lw=1.3)
draw_bidirectional(9, 2.5, 9, 3.2, color="#666", lw=1.3)
draw_bidirectional(12, 2.5, 12, 3.2, color="#666", lw=1.3)
draw_bidirectional(15, 2.5, 15, 3.2, color="#666", lw=1.3)
# 逻辑层 → 数据层
draw_bidirectional(16.5, 7.5, 16.5, 10.0, color="#D4A017", lw=1.3)
draw_bidirectional(18.5, 9.5, 18.5, 12.0, color="#D4A017", lw=1.3)

# ---- Intra-layer arrows (事件驱动通信) ----
# Player ↔ Combat
draw_bidirectional(4.8, 11.0, 5.2, 11.0, color="#82B366", lw=1.0)
# Player ↔ Progression
draw_bidirectional(3.5, 8.5, 2.5, 7.2, color="#82B366", lw=1.0)
# Enemy ↔ Combat
draw_bidirectional(9.0, 11.0, 9.4, 11.0, color="#B85450", lw=1.0)

# ---- LEGEND ----
legend_items = [
    (15.0, 13.5, ">  事件驱动通信", "#666666"),
    (15.0, 13.05, ">  数据读写", "#D4A017"),
    (15.0, 12.6, ">  模块间调用", "#82B366"),
]
for x, y, text, color in legend_items:
    ax.text(x, y, text, fontproperties=cn_font_sm, color=color, fontsize=9)

# ---- Event description box ----
draw_box(15.0, 6.8, 5.5, 5.5,
         "事件通信机制\n\n"
         "StatsChanged\n"
         "ProgressionChanged\n"
         "EquipmentChanged\n"
         "InventoryChanged\n"
         "DeathFinalized\n"
         "sceneLoaded",
         color="#FFF9E6", border="#C0A040", font=cn_font_sm, linewidth=1.0)

# ---- OUTPUT ----
out_path = os.path.join(os.path.dirname(__file__), "系统总体架构图.png")
fig.savefig(out_path, dpi=150, bbox_inches="tight", facecolor="white",
            edgecolor="none")
print(f"Saved: {out_path}")
plt.close()
