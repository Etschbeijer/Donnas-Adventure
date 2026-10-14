using Godot;
using System;
using System.Collections.Generic;

public partial class ParticleBurst : Node3D
{
    readonly List<GpuParticles3D> particles = new();
    int remainingEmitters;

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is GpuParticles3D emitter)
            {
                particles.Add(emitter);
                emitter.Finished += OnEmitterFinished;
            }
        }
    }

    public void Emit()
    {
        remainingEmitters = particles.Count;
        if (remainingEmitters == 0)
        {
            QueueFree();
            return;
        }

        double longestLifetime = 0;
        foreach (GpuParticles3D emitter in particles)
        {
            emitter.Restart();
            emitter.Emitting = true;
            longestLifetime = Math.Max(longestLifetime, emitter.Lifetime / Math.Max(emitter.SpeedScale, 0.01));
        }

        // Culled particles may never report Finished. Keep a generous fallback
        // so offscreen bursts cannot accumulate, and pause it with the scene.
        using var cleanupTimer = GetTree().CreateTimer(longestLifetime * 3, processAlways: false);
        cleanupTimer.Connect(SceneTreeTimer.SignalName.Timeout, new Callable(this, Node.MethodName.QueueFree));
    }

    void OnEmitterFinished()
    {
        if (--remainingEmitters == 0)
            QueueFree();
    }
}
