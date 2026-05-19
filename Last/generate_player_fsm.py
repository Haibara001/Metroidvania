# -*- coding: utf-8 -*-
"""生成玩家状态转换流程图 — 用 matplotlib 绘制，确保中文正确渲染"""
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.font_manager import FontProperties
import os

# ---- 中文字体 ----
font_paths = [
    "C:/Windows/Fonts/simhei.ttf",
    "C:/Windows/Fonts/msyh.ttc",
]
cn = None
for fp in font_paths:
    if os.path.exists(fp):
        cn = FontProperties(fname=fp, size=9)
        cn_sm = FontProperties(fname=fp, size=7.5)
        cn_md = FontProperties(fname=fp, size=11)
        cn_title = FontProperties(fname=fp, size=16)
        break
assert cn is not None, "No Chinese font found"

# ---- Canvas ----
fig, ax = plt.subplots(figsize=(24, 18), dpi=150)
ax.set_xlim(0, 24)
ax.set_ylim(0, 18)
ax.axis("off")
ax.set_facecolor("white")

def box(x, y, w, h, text, face="#DDDDDD", border="#333333", font=cn, tc="black", lw=1.2, z=2, style="round"):
    if style == "round":
        p = mpatches.FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.12",
            facecolor=face, edgecolor=border, linewidth=lw, zorder=z)
    else:
        p = mpatches.FancyBboxPatch((x, y), w, h, boxstyle="square,pad=0.12",
            facecolor=face, edgecolor=border, linewidth=lw, zorder=z)
    ax.add_patch(p)
    lines = text.split("\n")
    total_h = len(lines) * 0.3
    for i, line in enumerate(lines):
        yy = y + h / 2 + total_h / 2 - i * 0.3 - 0.12
        ax.text(x + w / 2, yy, line, ha="center", va="center",
                fontproperties=font, fontsize=font.get_size(), color=tc, zorder=z + 1)

def arrow(x1, y1, x2, y2, label="", color="#666", lw=1.0, style="solid", z=1, font=cn_sm):
    ls = "-" if style == "solid" else "--"
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
        arrowprops=dict(arrowstyle="->", color=color, lw=lw, linestyle=ls,
            connectionstyle="arc3,rad=0"), zorder=z)
    if label:
        mx, my = (x1 + x2) / 2, (y1 + y2) / 2 + 0.12
        ax.text(mx, my, label, ha="center", va="bottom", fontproperties=font,
                fontsize=font.get_size(), color=color, zorder=z + 1,
                bbox=dict(facecolor="white", edgecolor="none", alpha=0.7, pad=0))

def bidirectional(x1, y1, x2, y2, color="#666", lw=1.0, z=1):
    arrow(x1, y1, x2, y2, color=color, lw=lw, z=z)
    arrow(x2, y2, x1, y1, color=color, lw=lw, z=z)

# ============================================================
# TITLE
# ============================================================
ax.text(12, 17.3, "玩家状态转换流程图", ha="center", va="center",
        fontproperties=cn_title, fontweight="bold")

# ============================================================
# LEGEND (top-right)
# ============================================================
ax.text(20, 17.0, "图例", fontproperties=cn_md, fontweight="bold")
legend_items = [
    (20.0, 16.5, "绿色 = 地面状态", "#82B366"),
    (20.0, 16.1, "蓝色 = 空中状态", "#6C8EBF"),
    (20.0, 15.7, "紫色 = 特殊能力", "#9673A6"),
    (20.0, 15.3, "红色 = 战斗状态", "#B85450"),
    (20.0, 14.9, "橙色 = 受击状态", "#D79B00"),
]
for x, y, text, color in legend_items:
    ax.text(x, y, text, fontproperties=cn, color=color, fontsize=9)

# ============================================================
# CLUSTER: GROUND STATES  (bottom-left)
# ============================================================
# Cluster background
ground_cluster = mpatches.FancyBboxPatch((0.3, 9.2), 6.0, 4.5,
    boxstyle="round,pad=0.2", facecolor="#F0F8F0", edgecolor="#82B366",
    linewidth=1.5, linestyle="--", zorder=0)
ax.add_patch(ground_cluster)
ax.text(3.3, 13.4, "地面状态层", ha="center", fontproperties=cn_md,
        color="#336633", zorder=1)

# GroundedState base
box(1.0, 10.0, 3.5, 1.0, "PlayerGroundedState\n(中间基类: 重置空中次数)", "#E8F5E0", "#82B366", font=cn_sm)
# Idle
box(1.0, 11.5, 1.6, 1.2, "Idle\n待机", "#D5E8D4", "#82B366")
# Move
box(2.9, 11.5, 1.6, 1.2, "Move\n移动", "#D5E8D4", "#82B366")

# GroundBase -> Idle/Move (invis, just structure)
arrow(2.75, 11.0, 1.8, 11.5, color="#aaa", lw=0.5)
arrow(2.75, 11.0, 3.7, 11.5, color="#aaa", lw=0.5)
# Idle <-> Move
bidirectional(2.6, 12.1, 2.9, 12.1, color="#82B366")

# ============================================================
# CLUSTER: AIR STATES (top-left)
# ============================================================
air_cluster = mpatches.FancyBboxPatch((0.3, 0.8), 6.5, 8.0,
    boxstyle="round,pad=0.2", facecolor="#F5F8FC", edgecolor="#6C8EBF",
    linewidth=1.5, linestyle="--", zorder=0)
ax.add_patch(air_cluster)
ax.text(3.55, 8.5, "空中状态", ha="center", fontproperties=cn_md,
        color="#335577", zorder=1)

# Jump
box(0.8, 6.8, 1.6, 1.2, "Jump\n跳跃", "#DAE8FC", "#6C8EBF")
# Air
box(3.8, 6.8, 2.0, 1.2, "Air\n空中\n(80%速度控制)", "#DAE8FC", "#6C8EBF")
# WallSlide
box(0.8, 3.0, 1.8, 1.4, "WallSlide\n墙滑\n(70%缓降)", "#DAE8FC", "#6C8EBF")
# WallJump
box(4.5, 3.0, 1.8, 1.4, "WallJump\n墙跳\n(0.4s计时)", "#DAE8FC", "#6C8EBF")

# ============================================================
# CLUSTER: ABILITY (purple, right-top)
# ============================================================
ability_cluster = mpatches.FancyBboxPatch((13.5, 8.5), 7.0, 8.5,
    boxstyle="round,pad=0.2", facecolor="#FBF5FC", edgecolor="#9673A6",
    linewidth=1.5, linestyle="--", zorder=0)
ax.add_patch(ability_cluster)
ax.text(17.0, 16.7, "特殊能力", ha="center", fontproperties=cn_md,
        color="#553366", zorder=1)

# Dash
box(14.0, 14.8, 2.5, 1.3, "Dash\n冲刺\n(无敌帧+穿层)", "#E1D5E7", "#9673A6")
# EchoCast
box(17.0, 14.8, 2.5, 1.3, "EchoCast\n回声施放", "#E1D5E7", "#9673A6")
# EchoSwap
box(15.5, 10.5, 2.5, 1.3, "EchoSwap\n回声交换\n(传送+爆炸)", "#E1D5E7", "#9673A6")

# ============================================================
# CLUSTER: COMBAT (red, middle-right)
# ============================================================
combat_cluster = mpatches.FancyBboxPatch((7.5, 12.8), 5.5, 4.5,
    boxstyle="round,pad=0.2", facecolor="#FFF5F5", edgecolor="#B85450",
    linewidth=1.5, linestyle="--", zorder=0)
ax.add_patch(combat_cluster)
ax.text(10.25, 17.0, "战斗状态", ha="center", fontproperties=cn_md,
        color="#663333", zorder=1)

# PrimaryAttack
box(7.8, 14.0, 2.4, 1.8, "PrimaryAttack\n近战三段连击\n(combCounter\n 0 / 1 / 2)", "#F8CECC", "#B85450")
# RangedAttack
box(10.5, 14.0, 2.2, 1.5, "RangedAttack\n远程攻击\n(投射物)", "#F8CECC", "#B85450")

# ============================================================
# DAMAGED (orange, standalone)
# ============================================================
box(9.5, 10.0, 2.2, 1.5, "Damaged\n受击\n(击退+硬直)", "#FFE6CC", "#D79B00")

# ============================================================
# TRANSITIONS
# ============================================================

# === Idle <-> Move ===
bidirectional(2.6, 12.0, 3.7, 12.0, color="#82B366", lw=0.8)
ax.text(3.15, 12.05, "xInput!=0", ha="center", fontproperties=cn_sm, color="#82B366", fontsize=6.5)
ax.text(3.15, 11.7, "xInput==0", ha="center", fontproperties=cn_sm, color="#82B366", fontsize=6.5)

# === Idle/Move -> Jump ===
arrow(1.8, 11.3, 1.2, 8.0, color="#6C8EBF")
arrow(3.7, 11.3, 4.8, 8.0, color="#6C8EBF")
ax.text(2.1, 9.5, "Space", fontproperties=cn_sm, color="#6C8EBF", fontsize=7)

# === Idle/Move -> Air (walk off ledge) ===
arrow(1.8, 10.0, 4.2, 7.5, color="#888", lw=0.8)
arrow(3.7, 10.0, 4.8, 7.5, color="#888", lw=0.8)
ax.text(2.6, 8.3, "!IsGroundDetected", fontproperties=cn_sm, color="#888", fontsize=6.5)

# === Idle/Move -> PrimaryAttack ===
arrow(2.5, 12.5, 8.0, 14.5, color="#B85450")
ax.text(5.5, 13.5, "Mouse0", fontproperties=cn_sm, color="#B85450", fontsize=7)

# === Jump -> Air ===
arrow(1.2, 6.6, 3.8, 7.0, label="vel.y < 0", color="#6C8EBF")

# === Air -> Jump (double jump) ===
arrow(4.8, 7.6, 2.2, 7.0, label="Space\n+DoubleJump", color="#6C8EBF")

# === Air -> WallSlide ===
arrow(2.0, 6.6, 1.5, 4.4, label="IsWallDetected\n+WallJump", color="#6C8EBF")

# === Air -> Idle ===
arrow(4.5, 7.0, 2.5, 12.0, label="IsGroundDetected", color="#888", lw=0.8)

# === WallSlide <-> Air ===
arrow(2.6, 4.4, 3.8, 6.8, color="#888", lw=0.8)
ax.text(3.5, 5.5, "!IsWallDetected", fontproperties=cn_sm, color="#888", fontsize=6.5)

# === WallSlide -> WallJump ===
arrow(1.5, 4.4, 5.2, 4.0, label="Space", color="#6C8EBF")

# === WallJump -> Air ===
arrow(5.8, 4.4, 5.0, 7.0, label="timer\n>0.4s", color="#6C8EBF")

# === DASH transitions (purple dashed) ===
# From Idle/Move to Dash
arrow(2.0, 12.8, 14.5, 15.0, color="#9673A6", style="dashed")
ax.text(8.5, 14.3, "LeftShift + Dash (可解锁)", fontproperties=cn_sm, color="#9673A6", fontsize=7)

# From Air to Dash
arrow(5.5, 7.2, 14.5, 14.8, color="#9673A6", style="dashed")
ax.text(9.0, 11.2, "LeftShift + AirDash (可解锁)", fontproperties=cn_sm, color="#9673A6", fontsize=7)

# From WallSlide to Dash
arrow(2.0, 4.0, 14.0, 14.5, color="#9673A6", style="dashed")

# Dash -> Idle / Air / WallSlide
arrow(15.5, 14.6, 4.0, 11.8, color="#888", lw=0.8)
ax.text(9.8, 13.6, "timer end + IsGroundDetected", fontproperties=cn_sm, color="#888", fontsize=6.5)
arrow(15.5, 14.6, 5.0, 7.5, color="#888", lw=0.8)
ax.text(10.5, 10.8, "timer end\n+ !IsGroundDetected", fontproperties=cn_sm, color="#888", fontsize=6.5)

# === PrimaryAttack -> Idle ===
arrow(9.0, 13.8, 3.5, 12.2, label="动画结束", color="#B85450")

# === PrimaryAttack -> Air ===
arrow(8.5, 13.8, 4.5, 7.5, color="#888", lw=0.8)
ax.text(6.8, 10.5, "if 离地", fontproperties=cn_sm, color="#888", fontsize=6.5)

# === RangedAttack -> Idle ===
arrow(11.5, 13.8, 3.8, 12.2, label="动画结束", color="#B85450")

# === Idle/Move -> RangedAttack ===
arrow(3.2, 12.8, 11.0, 14.5, color="#B85450", style="dashed")
ax.text(7.2, 13.8, "Mouse1 + RangedAttack unlocked", fontproperties=cn_sm, color="#B85450", fontsize=6.5)

# === ECHO transitions ===
# Idle/Move/Air -> EchoCast
arrow(2.8, 12.5, 17.5, 15.0, color="#E1D5E7", style="dashed")
ax.text(10.5, 14.0, "Q (无分身时)", fontproperties=cn_sm, color="#9673A6", fontsize=6.5)
# EchoCast -> Idle
arrow(17.5, 14.6, 4.0, 12.0, color="#888", lw=0.8)

# Idle/Move/Air -> EchoSwap
arrow(3.0, 12.0, 16.0, 11.0, color="#E1D5E7", style="dashed")
ax.text(9.0, 11.8, "Q (有分身时)", fontproperties=cn_sm, color="#9673A6", fontsize=6.5)
# EchoSwap -> Idle
arrow(16.0, 10.5, 4.0, 11.8, color="#888", lw=0.8)

# === DAMAGED transitions ===
# From various states to Damaged
arrow(2.0, 11.5, 9.5, 10.8, color="#D79B00", style="solid")
ax.text(5.5, 11.5, "TakeDamage", fontproperties=cn_sm, color="#D79B00", fontsize=7, fontweight="bold")
arrow(4.5, 7.0, 9.5, 10.5, color="#D79B00")
arrow(1.0, 6.8, 9.5, 10.2, color="#D79B00")
# Damaged -> Idle/Move
arrow(10.5, 10.0, 3.5, 11.8, label="计时结束", color="#D79B00")

# ============================================================
# INPUT legend (bottom-left)
# ============================================================
input_box = mpatches.FancyBboxPatch((8.0, 0.5), 8.0, 2.0,
    boxstyle="round,pad=0.15", facecolor="#FAFAFA", edgecolor="#BBBBBB",
    linewidth=1.0, zorder=0)
ax.add_patch(input_box)
ax.text(12.0, 2.2, "输入映射表", ha="center", fontproperties=cn_md, fontweight="bold")
inputs = [
    (8.5, 1.7, "A / D 或 ← / →   =  水平移动", "#444"),
    (8.5, 1.3, "Space   =  跳跃  │  LeftShift   =  冲刺  │  Mouse0   =  近战攻击", "#444"),
    (8.5, 0.9, "Mouse1   =  远程攻击  │  Q   =  回声施放/交换  │  Tab   =  背包", "#444"),
]
for x, y, text, color in inputs:
    ax.text(x, y, text, fontproperties=cn_sm, color=color, fontsize=7.5)

# ============================================================
# SAVE
# ============================================================
out_path = os.path.join(os.path.dirname(__file__), "玩家状态转换流程图.png")
fig.savefig(out_path, dpi=150, bbox_inches="tight", facecolor="white", edgecolor="none")
print(f"Saved: {out_path}")
print(f"Size: {os.path.getsize(out_path)} bytes")
plt.close()
