using Godot;
using System;

[Tool]
public partial class LoopRail : Node3D
{
    //Path of the feet in local space. The ring leans sideways by LateralOffset so the lead-out
    //lands beside the lead-in. PlayerCharacter reads the path through SampleTransform and
    //ExitTransform and does all movement itself.

    const int RingSamples = 1024;
    const int LeadSamples = 16;
    const int VisualSegments = 64;
    const string VisualsName = "Visuals";

    float radius = 5f;
    float leadIn = 6f;
    float leadOut = 12f;
    float lateralOffset = 2f;
    float playerOriginHeight = 0.45f;

    [Export]
    public float Radius { get => radius; set { radius = value; markDirty(); } }
    [Export]
    public float LeadIn { get => leadIn; set { leadIn = value; markDirty(); } }
    [Export]
    public float LeadOut { get => leadOut; set { leadOut = value; markDirty(); } }
    [Export]
    public float LateralOffset { get => lateralOffset; set { lateralOffset = value; markDirty(); } }
    [Export]
    public float MinSpeed { get; set; } = 10f;
    [Export]
    public float PlayerOriginHeight { get => playerOriginHeight; set { playerOriginHeight = value; markDirty(); } }

    public float Length
    {
        get
        {
            ensureTable();
            return cumulative[cumulative.Length - 1];
        }
    }

    Vector3[] points;
    Vector3[] ups;
    float[] cumulative;
    bool tableDirty = true;

    Node3D visuals;

    public override void _Ready()
    {
        rebuildVisuals();
    }

    private void markDirty()
    {
        tableDirty = true;

        if (IsInsideTree())
        {
            rebuildVisuals();
        }
    }

    private void ensureTable()
    {
        if (!tableDirty && cumulative != null)
        {
            return;
        }

        float r = Mathf.Max(radius, 0.1f);
        float lead = Mathf.Max(leadIn, 0.1f);
        float lout = Mathf.Max(leadOut, 0.1f);
        int count = (LeadSamples + 1) + RingSamples + LeadSamples;

        points = new Vector3[count];
        ups = new Vector3[count];
        cumulative = new float[count];

        int index = 0;

        for (int j = 0; j <= LeadSamples; j++)
        {
            points[index] = new Vector3(-lateralOffset / 2f, 0f, -lead * j / LeadSamples);
            ups[index] = Vector3.Up;
            index++;
        }

        for (int i = 1; i <= RingSamples; i++)
        {
            float theta = Mathf.Tau * i / RingSamples;
            float x = -lateralOffset / 2f + lateralOffset * (theta - Mathf.Sin(theta)) / Mathf.Tau;
            float y = r * (1f - Mathf.Cos(theta));
            float z = -lead - r * Mathf.Sin(theta);

            points[index] = new Vector3(x, y, z);
            ups[index] = new Vector3(0f, Mathf.Cos(theta), Mathf.Sin(theta));
            index++;
        }

        for (int j = 1; j <= LeadSamples; j++)
        {
            points[index] = new Vector3(lateralOffset / 2f, 0f, -lead - lout * j / LeadSamples);
            ups[index] = Vector3.Up;
            index++;
        }

        cumulative[0] = 0f;
        for (int k = 1; k < count; k++)
        {
            cumulative[k] = cumulative[k - 1] + points[k].DistanceTo(points[k - 1]);
        }

        tableDirty = false;
    }

    //Local transform with the origin on the feet point.
    private Transform3D sampleFeetLocal(float distance)
    {
        ensureTable();

        float total = cumulative[cumulative.Length - 1];
        distance = Mathf.Clamp(distance, 0f, total);

        int i = Array.BinarySearch(cumulative, distance);
        if (i < 0)
        {
            i = ~i - 1;
        }
        i = Mathf.Clamp(i, 0, cumulative.Length - 2);

        float span = cumulative[i + 1] - cumulative[i];
        float t = span > 0f ? (distance - cumulative[i]) / span : 0f;

        Vector3 position = points[i].Lerp(points[i + 1], t);
        Vector3 up = ups[i].Lerp(ups[i + 1], t);
        Vector3 tangent = (points[i + 1] - points[i]).Normalized();

        Vector3 y = (up - tangent * up.Dot(tangent)).Normalized();
        Vector3 x = y.Cross(tangent);

        return new Transform3D(new Basis(x, y, tangent), position);
    }

    //Global transform for the player origin at the given distance along the rail.
    public Transform3D SampleTransform(float distance)
    {
        var local = sampleFeetLocal(distance);
        local.Origin += local.Basis.Y * playerOriginHeight;

        return GlobalTransform * local;
    }

    //Upright transform at the end of the rail. Basis Z is the horizontal travel direction.
    public Transform3D ExitTransform()
    {
        var end = SampleTransform(Length);

        Vector3 dir = end.Basis.Z;
        dir.Y = 0f;
        dir = dir.Normalized();

        Vector3 y = Vector3.Up;
        Vector3 x = y.Cross(dir);

        return new Transform3D(new Basis(x, y, dir), end.Origin);
    }

    public void OnEntryBodyEntered(Node3D body)
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        if (body is not PlayerCharacter player)
        {
            return;
        }

        var horizontal = new Vector3(player.PlayerVelocity.X, 0f, player.PlayerVelocity.Z);

        if (horizontal.Dot(-GlobalTransform.Basis.Z) > 0.5f)
        {
            player.TryStartRail(this);
        }
    }

    private void rebuildVisuals()
    {
        if (IsInstanceValid(visuals))
        {
            RemoveChild(visuals);
            visuals.QueueFree();
        }

        var stale = GetNodeOrNull(VisualsName);
        if (stale != null)
        {
            RemoveChild(stale);
            stale.QueueFree();
        }

        visuals = new Node3D { Name = VisualsName };
        AddChild(visuals, false, InternalMode.Front);

        var library = GD.Load<MeshLibrary>("res://MeshLibraries/Blocks.tres");
        var mesh = library?.GetItemMesh(0);
        if (mesh == null)
        {
            return;
        }

        ensureTable();

        int ringStart = LeadSamples;
        int step = RingSamples / VisualSegments;

        for (int k = 0; k < VisualSegments; k++)
        {
            float d0 = cumulative[ringStart + k * step];
            float d1 = cumulative[ringStart + (k + 1) * step];
            var sample = sampleFeetLocal((d0 + d1) / 2f);

            var basis = new Basis(
                sample.Basis.X * 1.6f,
                sample.Basis.Y * 0.15f,
                sample.Basis.Z * ((d1 - d0) / 2f * 1.08f));

            visuals.AddChild(new MeshInstance3D
            {
                Mesh = mesh,
                Transform = new Transform3D(basis, sample.Origin - sample.Basis.Y * 0.15f)
            });
        }
    }
}
