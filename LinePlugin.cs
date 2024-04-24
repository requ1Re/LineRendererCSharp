#if TOOLS
using Godot;

[Tool]
public partial class LinePlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var script = GD.Load<Script>("res://addons/LineRendererCSharp/LineRenderer.cs");
        var texture = GD.Load<Texture2D>("res://addons/LineRendererCSharp/line_render_icon.svg");
        AddCustomType("LineRenderer3D", "MeshInstance3D", script, texture);
    }

    public override void _ExitTree()
    {
	    RemoveCustomType("LineRenderer3D");
    }
}
#endif