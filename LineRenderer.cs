using Godot;
using System;

public partial class LineRenderer : MeshInstance3D {
    [Export]
    public Vector3[] Points = [new Vector3(0, 0, 0), new Vector3(0, 5, 0)];

    [Export]
    public float StartThickness = 0.1f;

    [Export]
    public float EndThickness = 0.1f;

    [Export]
    public int CornerResolution = 5;

    [Export]
    public int CapResolution = 5;

    [Export]
    public bool DrawCaps = true;

    [Export]
    public bool DrawCorners = true;

    [Export]
    public bool UseGlobalCoords = true;

    [Export]
    public bool TileTexture = true;

    private Camera3D Camera;
    private Vector3 CameraOrigin;

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
        ImmediateMesh ImmediateMesh = (ImmediateMesh)Mesh;
        if (Points.Length < 2)
        {
            return;
        }

        Camera = GetViewport().GetCamera3D();
        if (Camera == null)
        {
            return;
        }
        CameraOrigin = ToLocal(Camera.GlobalTransform.Origin);

        float ProgressStep = 1.0f / Points.Length;
        float Progress = 0;
        float Thickness = Mathf.Lerp(StartThickness, EndThickness, Progress);
        float NextThickness = Mathf.Lerp(StartThickness, EndThickness, Progress + ProgressStep);

        ImmediateMesh.ClearSurfaces();
        ImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < Points.Length - 1; i++)
        {
            Vector3 A = Points[i];
            Vector3 B = Points[i + 1];

            if (UseGlobalCoords)
            {
                A = ToLocal(A);
                B = ToLocal(B);
            }

            Vector3 AB = B - A;
            Vector3 OrthogonalABStart = (CameraOrigin - ((A + B) / 2)).Cross(AB).Normalized() * Thickness;
            Vector3 OrthogonalABEnd = (CameraOrigin - ((A + B) / 2)).Cross(AB).Normalized() * NextThickness;

            Vector3 AtoABStart = A + OrthogonalABStart;
            Vector3 AfromABStart = A - OrthogonalABStart;
            Vector3 BtoABEnd = B + OrthogonalABEnd;
            Vector3 BfromABEnd = B - OrthogonalABEnd;

            if (i == 0)
            {
                if (DrawCaps)
                {
                    Cap(A, B, Thickness, CapResolution);
                }
            }

            if (TileTexture)
            {
                float ABLen = AB.Length();
                float ABFloor = Mathf.Floor(ABLen);
                float ABFrac = ABLen - ABFloor;

                ImmediateMesh.SurfaceSetUV(new Vector2(ABFloor, 0));
                ImmediateMesh.SurfaceAddVertex(AtoABStart);
                ImmediateMesh.SurfaceSetUV(new Vector2(-ABFrac, 0));
                ImmediateMesh.SurfaceAddVertex(BtoABEnd);
                ImmediateMesh.SurfaceSetUV(new Vector2(ABFloor, 1));
                ImmediateMesh.SurfaceAddVertex(AfromABStart);
                ImmediateMesh.SurfaceSetUV(new Vector2(-ABFrac, 0));
                ImmediateMesh.SurfaceAddVertex(BtoABEnd);
                ImmediateMesh.SurfaceSetUV(new Vector2(-ABFrac, 1));
                ImmediateMesh.SurfaceAddVertex(BfromABEnd);
                ImmediateMesh.SurfaceSetUV(new Vector2(ABFloor, 1));
                ImmediateMesh.SurfaceAddVertex(AfromABStart);
            }
            else
            {
                ImmediateMesh.SurfaceSetUV(new Vector2(1, 0));
                ImmediateMesh.SurfaceAddVertex(AtoABStart);
                ImmediateMesh.SurfaceSetUV(new Vector2(0, 0));
                ImmediateMesh.SurfaceAddVertex(BtoABEnd);
                ImmediateMesh.SurfaceSetUV(new Vector2(1, 1));
                ImmediateMesh.SurfaceAddVertex(AfromABStart);
                ImmediateMesh.SurfaceSetUV(new Vector2(0, 0));
                ImmediateMesh.SurfaceAddVertex(BtoABEnd);
                ImmediateMesh.SurfaceSetUV(new Vector2(0, 1));
                ImmediateMesh.SurfaceAddVertex(BfromABEnd);
                ImmediateMesh.SurfaceSetUV(new Vector2(1, 1));
                ImmediateMesh.SurfaceAddVertex(AfromABStart);
            }

            if (i == Points.Length - 2)
            {
                if (DrawCaps)
                {
                    Cap(B, A, NextThickness, CapResolution);
                }
            }
            else
            {
                if (DrawCorners)
                {
                    Vector3 C = Points[i + 2];
                    if (UseGlobalCoords)
                    {
                        C = ToLocal(C);
                    }

                    Vector3 BC = C - B;
                    Vector3 OrthogonalBCStart = (CameraOrigin - ((B + C) / 2)).Cross(BC).Normalized() * NextThickness;

                    float AngleDot = AB.Dot(OrthogonalBCStart);

                    if (AngleDot > 0 && !Mathf.IsEqualApprox(AngleDot, 1))
                    {
                        Corner(B, BtoABEnd, B + OrthogonalBCStart, CornerResolution);
                    }
                    else if (AngleDot < 0 && !Mathf.IsEqualApprox(AngleDot, -1))
                    {
                        Corner(B, B - OrthogonalBCStart, BfromABEnd, CornerResolution);
                    }
                }
            }

            Progress += ProgressStep;
            Thickness = Mathf.Lerp(StartThickness, EndThickness, Progress);
            NextThickness = Mathf.Lerp(StartThickness, EndThickness, Progress + ProgressStep);
        }

        ImmediateMesh.SurfaceEnd();
    }

    private void Cap(Vector3 center, Vector3 pivot, float thickness, int capResolution)
    {
        ImmediateMesh immediateMesh = (ImmediateMesh)Mesh;
        Vector3 Orthogonal = (CameraOrigin - center).Cross(center - pivot).Normalized() * thickness;
        Vector3 Axis = (center - CameraOrigin).Normalized();
        if(!Axis.IsNormalized()) return;

        Vector3[] VertexArray = new Vector3[capResolution + 1];
        for (int i = 0; i < capResolution + 1; i++)
        {
            VertexArray[i] = new Vector3(0, 0, 0);
        }
        VertexArray[0] = center + Orthogonal;
        VertexArray[capResolution] = center - Orthogonal;

        for (int i = 1; i < capResolution; i++)
        {
            VertexArray[i] = center + Orthogonal.Rotated(Axis, Mathf.Lerp(0.0f, Mathf.Pi, (float)i / capResolution));
        }

        for (int i = 1; i < capResolution + 1; i++)
        {
            immediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)capResolution));
            immediateMesh.SurfaceAddVertex(VertexArray[i - 1]);
            immediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)capResolution));
            immediateMesh.SurfaceAddVertex(VertexArray[i]);
            immediateMesh.SurfaceSetUV(new Vector2(0.5f, 0.5f));
            immediateMesh.SurfaceAddVertex(center);
        }
    }

    private void Corner(Vector3 center, Vector3 start, Vector3 end, int capResolution)
    {
        ImmediateMesh ImmediateMesh = (ImmediateMesh)Mesh;
        Vector3[] VertexArray = new Vector3[capResolution + 1];
        for (int i = 0; i < capResolution + 1; i++)
        {
            VertexArray[i] = new Vector3(0, 0, 0);
        }
        VertexArray[0] = start;
        VertexArray[capResolution] = end;

        Vector3 Axis = start.Cross(end).Normalized();
        Vector3 Offset = start - center;
        float angle = Offset.AngleTo(end - center);

        for (int i = 1; i < capResolution; i++)
        {
            VertexArray[i] = center + Offset.Rotated(Axis, Mathf.Lerp(0.0f, angle, (float)i / capResolution));
        }

        for (int i = 1; i < capResolution + 1; i++)
        {
            ImmediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)capResolution));
            ImmediateMesh.SurfaceAddVertex(VertexArray[i - 1]);
            ImmediateMesh.SurfaceSetUV(new Vector2(0, (i - 1) / (float)capResolution));
            ImmediateMesh.SurfaceAddVertex(VertexArray[i]);
            ImmediateMesh.SurfaceSetUV(new Vector2(0.5f, 0.5f));
            ImmediateMesh.SurfaceAddVertex(center);
        }
    }
}