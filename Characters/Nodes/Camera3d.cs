using Godot;

public partial class OutlineCamera : Camera3D
{
    [Export]
    public ShaderMaterial OutlineMaterial;

    public override void _Ready()
    {
        if (OutlineMaterial != null)
        {
            OutlineMaterial.SetShaderParameter("near_plane", Near);
            OutlineMaterial.SetShaderParameter("far_plane", Far);
        }
    }
}
