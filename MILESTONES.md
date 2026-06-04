# Milestones — FlappyBirdByClaude

Each milestone is self-contained and can be Play-tested immediately after completion.

---

## M1 — Bird Falls and Flaps

**Goal**: A bird appears on screen, falls due to gravity, and jumps when you press Space or click.

**Deliverables**
- `Player` GameObject: `SpriteRenderer` (`yellowbird-midflap`), `Rigidbody2D` (Dynamic, Freeze Rotation Z), `CircleCollider2D`
- `PlayerController.cs`: gravity active, `AddForce` on Space/LMB input

**Test**: Press Play → bird falls. Press Space → bird jumps up. Repeat several times.

---

## M2 — Bird Rotates with Velocity

**Goal**: Bird tilts upward on flap and nose-dives as it falls — purely visual.

**Deliverables**
- `PlayerController.cs`: read `Rigidbody2D.velocity.y`, lerp `transform.rotation` between +30° (rising) and −90° (falling)

**Test**: Press Play → flap once → bird tilts up then smoothly rotates down.

---

## M3 — Bird Animates Wings

**Goal**: Three-frame wing-flap animation loops while the game is running.

**Deliverables**
- `PlayerController.cs`: frame timer cycles through `downflap / midflap / upflap` sprites at 0.1 s per frame

**Test**: Press Play → wings visibly cycle. Animation is smooth, not flickering.

---

## M4 — Infinite Scrolling Background and Ground

**Goal**: Background and ground scroll left forever with no visible seam.

**Deliverables**
- `Scroller.cs`: moves a pair of tiles left; teleports the trailing tile to the front when it exits screen-left
- `Background` object (2 tiles, speed 0.5 u/s)
- `Ground` object (2 tiles, speed 3.0 u/s, `BoxCollider2D` on each tile, tagged `"Ground"`)

**Test**: Press Play → background and ground scroll seamlessly. Stare at the seam for 10 s — no pop or gap visible.

---

## M5 — Bird Dies on Ground

**Goal**: Hitting the ground stops the bird and logs "Game Over" to the console.

**Deliverables**
- `PlayerController.cs`: `OnCollisionEnter2D` with tag `"Ground"` → freeze physics, `Debug.Log("GameOver")`

**Test**: Press Play → do nothing → bird falls and hits ground → Console shows "GameOver". Bird does not clip through.

---

## M6 — Pipe Spawner

**Goal**: Pipes spawn off-screen right, scroll left, and despawn off-screen left.

**Deliverables**
- `Pipe.prefab`: PipeTop (flipped), PipeBottom, `BoxCollider2D` on each, tagged `"Pipe"`
- `PipeSpawner.cs`: spawns a pair every 1.8 s at `X = +8`, randomizes gap center Y in `[−1.5, +1.5]`, gap size 3.5 u
- Pipe pair carries a `Scroller`-style self-scroll + self-destruct at `X = −8`

**Test**: Press Play → pipes spawn regularly, scroll left, disappear. Gap positions vary between spawns.

---

## M7 — Bird Dies on Pipe

**Goal**: Colliding with a pipe also triggers game over.

**Deliverables**
- `PlayerController.cs`: `OnCollisionEnter2D` with tag `"Pipe"` → same death logic as ground

**Test**: Press Play → fly into a pipe → Console shows "GameOver". Fly through the gap → no false trigger.

---

## M8 — Game State Machine

**Goal**: Clean state transitions replace the `Debug.Log` stubs. Bird is inert until first input.

**Deliverables**
- `GameManager.cs`: `enum GameState { Idle, Playing, GameOver }`, singleton
- **Idle**: bird bobs gently (sine wave), pipes not spawning, scrollers at half speed
- **Playing**: full gravity, pipes spawn, scrollers full speed — entered on first flap input
- **GameOver**: physics frozen, pipe spawner stopped — entered on collision

**Test**
- Launch → bird bobs, nothing moves at full speed, no pipes.
- Press Space → game starts, bird falls, pipes appear.
- Hit pipe/ground → everything stops cleanly.

---

## M9 — Score Counter (Console)

**Goal**: Score increments by 1 each time the bird passes through a pipe gap.

**Deliverables**
- Score trigger: `BoxCollider2D` (IsTrigger) centered in each pipe gap, tagged `"ScoreTrigger"`
- `ScoreManager.cs`: `OnTriggerEnter2D` → `score++`, `Debug.Log($"Score: {score}")`

**Test**: Press Play, fly through 3 gaps → Console prints "Score: 1", "Score: 2", "Score: 3". Brushing past the pipe without entering gap does not score.

---

## M10 — Score Display (Digit Sprites)

**Goal**: Live score shown on screen using the number sprites.

**Deliverables**
- `GameCanvas` → `ScoreDisplay`: `HorizontalLayoutGroup` of `Image` components
- `UIManager.cs`: converts integer score to digit-sprite sequence, updates on each score change
- `ScoreManager.cs` fires a `UnityEvent<int>` or calls `UIManager` directly

**Test**: Press Play → fly through pipes → score at top of screen increments correctly. Single digit, double digit, triple digit all render correctly.

---

## M11 — Game Over Screen

**Goal**: Dying shows the game-over banner, current score, and best score.

**Deliverables**
- `GameOverPanel` under `GameCanvas`: `gameover.png` image + two digit-sprite rows (current / best)
- `PlayerPrefs` save/load for best score in `ScoreManager.cs`
- `UIManager.cs`: show panel on `GameOver` state, hide on `Playing`

**Test**
- Get score 5, die → panel shows 5 + best 5.
- Restart, get score 3, die → panel shows 3 (current) + 5 (best).
- Restart, get score 8, die → panel shows 8 (current) + 8 (best, updated).

---

## M12 — Idle Screen

**Goal**: `message.png` shown at start, hidden when gameplay begins.

**Deliverables**
- `IdlePanel` under `GameCanvas`: `message.png` centered
- `UIManager.cs`: show on `Idle`, hide on transition to `Playing`

**Test**: Launch → message visible, bird bobs. Press Space → message disappears instantly.

---

## M13 — Restart Flow

**Goal**: Pressing Space or clicking on the Game Over screen resets everything cleanly.

**Deliverables**
- `GameManager.cs`: restart input only accepted in `GameOver` state
- Full reset: bird position/velocity, score, pipes destroyed, scrollers reset, `Idle` state entered

**Test**: Play → die → press Space → game resets to Idle. Play again → die again → resets again. No leftover pipes or wrong score after two consecutive runs.

---

## M14 — Audio

**Goal**: All 5 sound effects trigger at the correct moments.

**Deliverables**
- `AudioSource` on `GameManager`, all 5 clips assigned
- `wing.ogg` — on flap (Playing state only)
- `point.ogg` — on score increment
- `hit.ogg` — on collision
- `die.ogg` — 0.3 s after hit
- `swoosh.ogg` — when Game Over panel appears

**Test**: Play a full run with headphones. Confirm each sound triggers once at the right moment. Verify wing sound does not play during Idle bob.

---

## M15 — Difficulty Scaling

**Goal**: Game gets harder as the score increases.

**Deliverables**
- `DifficultyStep[]` array in `GameManager.cs` (5 thresholds: 0 / 10 / 20 / 30 / 40)
- Each step sets `PipeSpeed`, `SpawnInterval`, `GapSize`
- `ScoreManager` notifies `GameManager` on score change; `GameManager` updates `PipeSpawner` and `Scroller` speed

| Score | Speed | Interval | Gap |
|-------|-------|----------|-----|
| 0 | 3.0 | 1.8 s | 3.5 |
| 10 | 3.5 | 1.6 s | 3.2 |
| 20 | 4.0 | 1.4 s | 3.0 |
| 30 | 4.5 | 1.3 s | 2.8 |
| 40 | 5.0 | 1.2 s | 2.6 |

**Test**: Cheat score to 9, 19, 29, 39 → verify pipes visibly speed up and gap narrows at each threshold.

---

## M16 — Polish Pass

**Goal**: Game feels complete and ship-ready.

**Deliverables**
- Bird collision uses `CircleCollider2D` sized slightly smaller than sprite (forgiveness hitbox)
- Death animation: bird continues falling off-screen before Game Over panel appears (0.5 s delay)
- Pipes use correct layer matrix (`Player` ↔ `Obstacle` only)
- Game Over panel entrance uses `swoosh.ogg` + simple scale-in tween
- Score display centered correctly for 1, 2, and 3-digit numbers

**Test**: Full playthrough end-to-end. Death feels fair, sounds correct, UI crisp, no visual glitches.

---

## Dependency Order

```
M1 → M2 → M3          (bird visuals)
M4                     (world visuals, independent)
M1 + M4 → M5          (ground death)
M6 → M7               (pipes + pipe death)
M5 + M7 → M8          (state machine)
M8 + M6 → M9          (score logic)
M9 → M10              (score UI)
M10 → M11 → M12 → M13 (full UI flow)
M13 → M14             (audio — needs full flow)
M8 + M9 → M15         (difficulty — needs state + score)
M13 + M14 + M15 → M16 (polish — everything done)
```
