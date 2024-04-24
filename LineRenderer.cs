using Godot;
using System;

public partial class LineRenderer : MeshInstance3D {
    [Export]
    public Vector3[] points = [new Vector3(0, 0, 0), new Vector3(0, 5, 0)];

    [Export]
    public float start_thickness = 0.1f;

    [Export]
    public float end_thickness = 0.1f;

    [Export]
    public int corner_resolution = 5;

    [Export]
    public int cap_resolution = 5;

    [Export]
    public bool draw_caps = true;

    [Export]
    public bool draw_corners = true;

    [Export]
    public bool use_global_coords = true;

    [Export]
    public bool tile_texture = true;

    private Camera3D camera;
    private Vector3 cameraOrigin;

    public override void _EnterTree()
    {
        Mesh = new ImmediateMesh();
    }

    public override void _Ready()
    {
        // Initialization code here
    }

    public override void _PhysicsProcess(double delta)
    {
        ImmediateMesh immediateMesh = (ImmediateMesh)Mesh;
        if (points.Length < 2)
        {
            return;
        }

        camera = GetViewport().GetCamera3D();
        if (camera == null)
        {
            return;
        }
        cameraOrigin = ToLocal(camera.GlobalTransform.Origin);

        float progressStep = 1.0f / points.Length;
        float progress = 0;
        float thickness = Mathf.Lerp(start_thickness, end_thickness, progress);
        float nextThickness = Mathf.Lerp(start_thickness, end_thickness, progress + progressStep);

        immediateMesh.ClearSurfaces();
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 A = points[i];
            Vector3 B = points[i + 1];

            if (use_global_coords)
            {
                A = ToLocal(A);
                B = ToLocal(B);
            }

            Vector3 AB = B - A;
            Vector3 orthogonalABStart = (cameraOrigin - ((A + B) / 2)).Cross(AB).Normalized() * thickness;
            Vector3 orthogonalABEnd = (cameraOrigin - ((A + B) / 2)).Cross(AB).Normalized() * nextThickness;

            Vector3 AtoABStart = A + orthogonalABStart;
            Vector3 AfromABStart = A - orthogonalABStart;
            Vector3 BtoABEnd = B + orthogonalABEnd;
            Vector3 BfromABEnd = B - orthogonalABEnd;

            if (i == 0)
            {
                if (draw_caps)
                {
                    Cap(A, B, thickness, cap_resolution);
                }
            }

            if (tile_texture)
            {
                float ABLen = AB.Length();
                float ABFloor = Mathf.Floor(ABLen);
                float ABFrac = ABLen - ABFloor;

                immediateMesh.SurfaceSetUV(new Vector2(ABFloor, 0));
                immediateMesh.SurfaceAddVertex(AtoABStart);
                immediateMesh.SurfaceSetUV(new Vector2(-ABFrac, 0));
                immediateMesh.SurfaceAddVertex(BtoABEnd);
                immediateMesh.SurfaceSetUV(new Vector2(ABFloor, 1));
                immediateMesh.SurfaceAddVertex(AfromABStart);
                immediateMesh.SurfaceSetUV(new Vector2(-ABFrac, 0));
                immediateMesh.SurfaceAddVertex(BtoABEnd);
                immediateMesh.SurfaceSetUV(new Vector2(-ABFrac, 1));
                immediateMesh.SurfaceAddVertex(BfromABEnd);
                immediateMesh.SurfaceSetUV(new Vector2(ABFloor, 1));
                immediateMesh.SurfaceAddVertex(AfromABStart);
            }
            else
            {
                immediateMesh.SurfaceSetUV(new Vector2(1, 0));
                immediateMesh.SurfaceAddVertex(AtoABStart);
                immediateMesh.SurfaceSetUV(new Vector2(0, 0));
                immediateMesh.SurfaceAddVertex(BtoABEnd);
                immediateMesh.SurfaceSetUV(new Vector2(1, 1));
                immediateMesh.SurfaceAddVertex(AfromABStart);
                immediateMesh.SurfaceSetUV(new Vector2(0, 0));
                immediateMesh.SurfaceAddVertex(BtoABEnd);
                immediateMesh.SurfaceSetUV(new Vector2(0, 1));
                immediateMesh.SurfaceAddVertex(BfromABEnd);
                immediateMesh.SurfaceSetUV(new Vector2(1, 1));
                immediateMesh.SurfaceAddVertex(AfromABStart);
            }

            if (i == points.Length - 2)
            {
                if (draw_caps)
                {
                    Cap(B, A, nextThickness, cap_resolution);
                }
            }
            else
            {
                if (draw_corners)
                {
                    Vector3 C = points[i + 2];
                    if (use_global_coords)
                    {
                        C = ToLocal(C);
                    }

                    Vector3 BC = C - B;
                    Vector3 orthogonalBCStart = (cameraOrigin - ((B + C) / 2)).Cross(BC).Normalized() * nextThickness;

                    float angleDot = AB.Dot(orthogonalBCStart);

                    if (angleDot > 0 && !Mathf.IsEqualApprox(angleDot, 1))
                    {
                        Corner(B, BtoABEnd, B + orthogonalBCStart, corner_resolution);
                    }
                    else if (angleDot < 0 && !Mathf.IsEqualApprox(angleDot, -1))
                    {
                        Corner(B, B - orthogonalBCStart, BfromABEnd, corner_resolution);
                    }
                }
            }

            progress += progressStep;
            thickness = Mathf.Lerp(start_thickness, end_thickness, progress);
            nextThickness = Mathf.Lerp(start_thickness, end_thickness, progress + progressStep);
        }

        immediateMesh.SurfaceEnd();
    }

    private void Cap(Vector3 center, Vector3 pivot, float thickness, int cap_resolution)
    {
        ImmediateMesh immediateMesh = (ImmediateMesh)Mesh;
        Vector3 orthogonal = (cameraOrigin - center).Cross(center - pivot).Normalized() * thickness;
        Vector3 axis = (center - cameraOrigin).Normalized();

        Vector3[] vertexArray = new Vector3[cap_resolution + 1];
        for (int i = 0; i < cap_resolution + 1; i++)
        {
            vertexArray[i] = new Vector3(0, 0, 0);
        }
        vertexArray[0] = center + orthogonal;
        vertexArray[cap_resolution] = center - orthogonal;

        for (int i = 1; i < cap_resolution; i++)
        {
            vertexArray[i] = center + (orthogonal.Rotated(axis, Mathf.Lerp(0.0f, Mathf.Pi, (float)i / cap_resolution)));
        }

        for (int i = 1; i < cap_resolution + 1; i++)
        {
            immediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)cap_resolution));
            immediateMesh.SurfaceAddVertex(vertexArray[i - 1]);
            immediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)cap_resolution));
            immediateMesh.SurfaceAddVertex(vertexArray[i]);
            immediateMesh.SurfaceSetUV(new Vector2(0.5f, 0.5f));
            immediateMesh.SurfaceAddVertex(center);
        }
    }

    private void Corner(Vector3 center, Vector3 start, Vector3 end, int cap_resolution)
    {
        ImmediateMesh immediateMesh = (ImmediateMesh)Mesh;
        Vector3[] vertexArray = new Vector3[cap_resolution + 1];
        for (int i = 0; i < cap_resolution + 1; i++)
        {
            vertexArray[i] = new Vector3(0, 0, 0);
        }
        vertexArray[0] = start;
        vertexArray[cap_resolution] = end;

        Vector3 axis = start.Cross(end).Normalized();
        Vector3 offset = start - center;
        float angle = offset.AngleTo(end - center);

        for (int i = 1; i < cap_resolution; i++)
        {
            vertexArray[i] = center + offset.Rotated(axis, Mathf.Lerp(0.0f, angle, (float)i / cap_resolution));
        }

        for (int i = 1; i < cap_resolution + 1; i++)
        {
            immediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)cap_resolution));
            immediateMesh.SurfaceAddVertex(vertexArray[i - 1]);
            immediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)cap_resolution));
            immediateMesh.SurfaceAddVertex(vertexArray[i]);
            immediateMesh.SurfaceSetUV(new Vector2(0.5f, 0.5f));
            immediateMesh.SurfaceAddVertex(center);
        }
    }
}