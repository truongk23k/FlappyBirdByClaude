# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Flappy Bird clone built in Unity 2022.3.62f1 (LTS) as a 2D game. All visual and audio assets are already imported; the game logic scripts and scene setup still need to be implemented.

## Unity MCP Integration

This project uses `com.ivanmurzak.unity.mcp` (v0.79.0) — a plugin that connects the Unity Editor to Claude Code via an MCP server. Use the Unity skills (listed in `.claude/skills/`) to manipulate the Editor directly instead of editing `.unity` or `.asset` files by hand.

**Prerequisite**: `unity-mcp-cli` must be installed globally and the Unity Editor must be open with the project loaded.

```powershell
# Install once
npm install -g unity-mcp-cli

# All tool calls go through this pattern (use --input-file for reliable JSON quoting in PowerShell)
unity-mcp-cli run-tool <tool-name> --input-file args.json
```

Key skill categories available in `.claude/skills/`:
- `gameobject-*` — create, modify, destroy, find GameObjects and their components
- `assets-*` — find, create, modify, move Unity assets
- `scene-*` — open, save, query scene state
- `script-*` — read and update C# scripts
- `screenshot-*` — capture game/scene view for visual verification

## Architecture

### Scene: `Assets/Scenes/SampleScene.unity`
Currently contains only a Main Camera (orthographic, size 5, position `0,0,-10`). All game objects need to be created and wired up.

### Scripts: `Assets/Scripts/`
Only `PlayerController.cs` exists (currently an empty `MonoBehaviour` stub attached to the Player Capsule). All other game systems need to be scripted here.

### Assets
- **Sprites**: `Assets/Flappy_Bird_assets by kosresetr55/Game Objects/` — bird (3 animation frames), background-day, base (ground), pipe-green
- **Audio**: `Assets/Flappy_Bird_assets by kosresetr55/Sound Effects/` — wing, point, hit, die, swoosh (WAV + OGG)
- **UI Sprites**: `Assets/Flappy_Bird_assets by kosresetr55/UI/` — gameover, message, digit sprites 0–9
- **Prefabs**: `Assets/Prefabs/` — empty, needs to be populated

### Packages
- `com.unity.feature.2d` 2.0.1 — core 2D support (Physics2D, Sprite rendering, UGUI)
- `com.unity.textmeshpro` 3.0.7 — UI text
- `com.unity.test-framework` 1.1.33 — unit tests

## Game Systems To Implement

| System | Notes |
|--------|-------|
| Player (bird) | Gravity + tap/click to flap; 3-frame wing animation |
| Pipes | Spawn off-screen right, scroll left, randomize gap Y, despawn off-screen left |
| Collision | Bird vs pipe or ground → game over |
| Scoring | Increment when bird passes a pipe pair |
| UI | Score display (digit sprites), game-over screen, restart |
| Audio | Trigger wing/point/hit/die/swoosh clips at correct game events |
| Game state | Idle → Playing → GameOver state machine |
