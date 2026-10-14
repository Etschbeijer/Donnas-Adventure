The graphics changes reduce work per rendered frame. The application has no new
FPS limit, and its existing V-Sync settings are unchanged.

- Water uses a 96-by-256 subdivision grid instead of 400-by-400. This changes
  the mesh from 321,602 triangles to 49,858, while keeping vertex waves and
  animated texture normals.
- The current water material uses one normal layer because both existing
  texture resources and scroll directions are identical. The shader defaults
  to two layers for other materials. Enable `use_second_normal_layer` when
  configuring a distinct second texture or direction.
- The water shader reuses its depth sample.
- The cloud shader reuses each ray step's height-curve sample and zero-offset
  shadow sample. Cloud resolution, ray steps, and shadow steps are unchanged.
- TAA remains enabled for distant edges and moving shader detail. 2x MSAA is
  disabled to avoid running both AA methods. Resolution, lighting, shadows, glow, and outlines
  retain their existing settings.

Run the benchmark with the Godot .NET executable after importing and building:

```powershell
godot --path . --windowed --rendering-driver vulkan --script res://Tests/graphics_benchmark.gd -- current
```

Use a temporary `APPDATA` directory for isolated save data. Replace `godot`
with your executable path if necessary. This test requires a rendering device;
`--headless` cannot provide GPU timing measurements.

The benchmark renders a fixed level-one camera into a 1920-by-1080 viewport,
warms up for 120 frames, and collects 240 GPU timing samples. It disables
V-Sync and frame limiting within the test process only. Results and a rendered
image are written to `.godot/graphics-current.json` and
`.godot/graphics-current.png`. Gameplay and animation are frozen for consistent
geometry; sky and water shader animation remain active during measurements.

GPU frame time measures rendering cost, not electrical power. With uncapped
rendering, spare GPU capacity can increase frame rate instead of lowering total
power consumption. Compare runs on the same hardware and power settings, and
use a power sensor or meter if actual watts are needed.

Observed on Intel Arc Pro Graphics with Vulkan / Forward+ at 1920x1080:

| Paired run | Original median GPU time | Optimized median GPU time |
| --- | ---: | ---: |
| 1 | 40.09 ms | 35.86 ms |
| 2 | 39.89 ms | 36.00 ms |
| Average of run medians | 39.99 ms | 35.93 ms |

This is a 10.2% decrease in GPU frame time at the measured viewpoint. It is a
local rendering-cost comparison, not a whole-game FPS or electrical-power
measurement. Original settings and shader sources were restored in memory for
each baseline process; optimized sources remained on disk throughout the paired
tests. Each run collected 240 samples after warming up.
