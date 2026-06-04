# Game Design Document — Flappy Bird Clone
**Project**: FlappyBirdByClaude  
**Engine**: Unity 2022.3.62f1 LTS  
**Platform**: PC (Mouse / Keyboard)  
**Genre**: Endless arcade

---

## 1. Gameplay

### 1.1 Core Loop
The player controls a bird that falls continuously due to gravity. Tapping/clicking makes the bird flap upward. The goal is to fly through gaps between pipes for as long as possible without hitting anything.

### 1.2 Controls
| Input | Action |
|-------|--------|
| Left Mouse Button | Flap |
| Space Bar | Flap |
| Left Mouse Button / Space (Game Over screen) | Restart |

### 1.3 Bird Physics
| Parameter | Value |
|-----------|-------|
| Gravity Scale | 3.0 |
| Flap Force (AddForce Y) | 7.0 |
| Max fall velocity (clamp) | −10.0 |
| Max rise velocity (clamp) | +10.0 |
| Rotation max up | +30° |
| Rotation max down | −90° |
| Rotation speed | 300°/s lerp |

The bird rotates upward on flap and rotates downward progressively as it falls — purely visual, does not affect hitbox.

### 1.4 Bird Animation
Three-frame sprite loop using `Animator` (no Animator Controller needed — driven by script):

| Frame | Sprite | Duration |
|-------|--------|----------|
| 0 | `yellowbird-midflap.png` | 0.1 s |
| 1 | `yellowbird-upflap.png` | 0.1 s |
| 2 | `yellowbird-midflap.png` | 0.1 s |
| 3 | `yellowbird-downflap.png` | 0.1 s |

Animation plays only during **Playing** state. Frozen on the midflap frame during **Idle** and **GameOver**.

### 1.5 Pipes
- Pipe pairs spawn off-screen right at `X = +8`.
- Both pipes scroll left at **PipeSpeed** units/second.
- Despawn when they reach `X = −8`.
- Gap between top pipe and bottom pipe: **3.5 units**.
- Gap center Y randomized each spawn: range `[−1.5, +1.5]`.
- Spawn interval: **1.8 s** (decreases with difficulty, see §6).

Each pipe pair contains:
- Top pipe — `pipe-green.png` flipped 180° on Y
- Bottom pipe — `pipe-green.png` normal
- Score trigger — invisible `BoxCollider2D` (IsTrigger) between the two pipes, same width as pipe

### 1.6 Background
- `background-day.png` rendered on two side-by-side GameObjects.
- Scrolls left at **BackgroundSpeed = 0.5** units/second (parallax, slower than pipes).
- When the left tile exits screen-left it teleports to screen-right → seamless loop.

### 1.7 Ground
- `base.png` rendered on two tiles at the bottom of the screen (`Y = −4.0`).
- Scrolls left at **PipeSpeed** (same speed as pipes for consistency).
- Same seamless-loop technique as background.
- Has a `BoxCollider2D` (not trigger) — touching it ends the game.

### 1.8 Camera
- Orthographic, size **5**, fixed at `(0, 0, −10)`. Never moves.

---

## 2. Scoring

### 2.1 How Points Are Earned
- +1 point each time the bird's center passes through a pipe's score trigger collider.
- The trigger uses `OnTriggerEnter2D` on the bird; the pipe trigger is tagged **"ScoreTrigger"**.

### 2.2 Best Score
- Best score persisted via `PlayerPrefs` key `"BestScore"`.
- Updated and saved immediately when current score exceeds best score.

### 2.3 Score Display
- Score shown during gameplay using digit sprites (`0.png`–`9.png`), centered at the top of the screen.
- Each digit is a separate `Image` in a horizontal `HorizontalLayoutGroup`.
- During **GameOver** screen, both current score and best score are displayed using the same digit sprite system.

---

## 3. UI

### 3.1 State-by-State UI

#### Idle State
- `message.png` displayed at center screen.
- Score display hidden.
- Game Over panel hidden.

#### Playing State
- `message.png` hidden.
- Score display (digit sprites) visible at top center.
- Game Over panel hidden.

#### Game Over State
- `gameover.png` banner shown at upper-center.
- Score panel shows current score + best score using digit sprites.
- "Tap to restart" hint (TextMeshPro text) at bottom.
- A brief **death animation** plays before the panel appears:
  - Bird falls off-screen (0.5 s delay before showing panel).

### 3.2 Canvas Setup
| Canvas | Render Mode | Sort Order |
|--------|-------------|------------|
| GameCanvas | Screen Space – Overlay | 0 |

**Hierarchy under GameCanvas:**
```
GameCanvas
├── ScoreDisplay          ← HorizontalLayoutGroup of digit Images
├── IdlePanel
│   └── MessageImage      ← message.png
└── GameOverPanel
    ├── GameOverImage     ← gameover.png
    ├── CurrentScore      ← digit Images
    ├── BestScore         ← digit Images
    └── RestartHint       ← TMP text "Press Space or Click to Restart"
```

---

## 4. Audio

### 4.1 Clip Map
| Clip | Trigger |
|------|---------|
| `wing.ogg` | Every flap input (Playing state only) |
| `point.ogg` | Every +1 score |
| `hit.ogg` | First frame of collision (pipe or ground) |
| `die.ogg` | 0.3 s after `hit` — when bird begins to fall off-screen |
| `swoosh.ogg` | Game Over panel slides in |

### 4.2 Implementation
- Single `AudioSource` component on the **GameManager** GameObject, `PlayOneShot` for all clips.
- No BGM — faithful to the original.
- Audio muted during **Idle** state (no sounds until first flap).

---

## 5. Game States

```
         ┌──────────────────────────────────────┐
         │                                      │
         ▼                                      │
      [IDLE] ──── first input ────► [PLAYING] ──┘ (loop)
                                       │
                                  collision
                                       │
                                       ▼
                                  [GAMEOVER]
                                       │
                                  restart input
                                       │
                                       ▼
                                    [IDLE]
```

### 5.1 IDLE
- Bird hovers with gentle sine-wave bob (`Y += sin(Time) * 0.3`).
- Pipes not spawning, background/ground scrolling at half speed.
- Waiting for first input.

### 5.2 PLAYING
- Full gravity active on bird.
- Pipe spawner running.
- Score triggers active.
- All scrolling at full speed.

### 5.3 GAMEOVER
- Physics frozen (`Rigidbody2D.simulated = false`) after bird exits screen.
- Pipe spawner stopped; existing pipes continue scrolling briefly then stop.
- Score saved to `PlayerPrefs`.
- UI panel shown after 0.5 s delay.
- Waiting for restart input.

---

## 6. Difficulty

Difficulty scales linearly with score — no discrete "levels".

| Score Range | Pipe Speed | Spawn Interval | Gap Size |
|-------------|------------|----------------|----------|
| 0 – 9 | 3.0 u/s | 1.8 s | 3.5 u |
| 10 – 19 | 3.5 u/s | 1.6 s | 3.2 u |
| 20 – 29 | 4.0 u/s | 1.4 s | 3.0 u |
| 30 – 39 | 4.5 u/s | 1.3 s | 2.8 u |
| 40+ | 5.0 u/s | 1.2 s | 2.6 u |

Speed and interval values are looked up from a `DifficultyStep[]` array in `GameManager`. Gap size is passed to `PipeSpawner` each spawn.

---

## 7. GameObject & Scene Hierarchy

```
SampleScene
├── Main Camera            (Orthographic, size 5)
├── GameManager            (GameManager.cs, AudioSource)
├── Background
│   ├── BG_Left            (SpriteRenderer: background-day)
│   └── BG_Right           (SpriteRenderer: background-day)
├── Ground
│   ├── Ground_Left        (SpriteRenderer: base, BoxCollider2D)
│   └── Ground_Right       (SpriteRenderer: base, BoxCollider2D)
├── Player                 (SpriteRenderer, Rigidbody2D, CircleCollider2D,
│                           PlayerController.cs)
├── PipeSpawner            (PipeSpawner.cs — empty transform at X=+8)
└── GameCanvas             (Canvas, CanvasScaler, GraphicRaycaster)
    ├── ScoreDisplay
    ├── IdlePanel
    └── GameOverPanel
```

---

## 8. Prefabs

| Prefab | Components |
|--------|------------|
| `Pipe.prefab` | Empty root → PipeTop (SpriteRenderer, BoxCollider2D), PipeBottom (SpriteRenderer, BoxCollider2D), ScoreTrigger (BoxCollider2D isTrigger) |

---

## 9. Scripts

| Script | Responsibility |
|--------|---------------|
| `GameManager.cs` | State machine, difficulty lookup, restart, audio playback |
| `PlayerController.cs` | Flap physics, rotation, animation frames, death detection |
| `PipeSpawner.cs` | Timed spawn, randomize gap Y, scroll + despawn |
| `ScoreManager.cs` | Increment score, update best score, notify UIManager |
| `UIManager.cs` | Show/hide panels, render digit sprites for scores |
| `Scroller.cs` | Reusable infinite scroll (used by Background tiles and Ground tiles) |

---

## 10. Folder Structure

```
Assets/
├── Flappy_Bird_assets by kosresetr55/
│   ├── Game Objects/          ← source sprites (bird, bg, base, pipe)
│   ├── Sound Efects/          ← audio clips (wing, point, hit, die, swoosh)
│   └── UI/
│       └── Numbers/           ← digit sprites 0–9
├── Prefabs/
│   └── Pipe.prefab
├── Scenes/
│   └── SampleScene.unity
└── Scripts/
    ├── GameManager.cs
    ├── PlayerController.cs
    ├── PipeSpawner.cs
    ├── ScoreManager.cs
    ├── UIManager.cs
    └── Scroller.cs
```

---

## 11. Technical Notes

- **Collision layers**: Bird on layer `Player`, Pipes + Ground on layer `Obstacle`. Matrix: Player↔Obstacle = true, all others = false.
- **Tag**: Ground tagged `"Ground"`, Pipe tagged `"Pipe"`, score trigger tagged `"ScoreTrigger"`.
- **Sprite Pixels Per Unit**: All game sprites imported at **100 PPU**. Numbers at **100 PPU**.
- **Physics 2D**: Gravity `(0, −9.81)`, bird `Rigidbody2D` — Dynamic, Freeze Rotation Z = true (rotation handled in script).
- **Note on audio folder typo**: The folder is named `Sound Efects` (missing second 'f') — asset paths must use this exact name.
