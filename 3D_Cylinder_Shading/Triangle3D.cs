using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Cylinder_Shading;

public class Triangle3D
{
    public Point3D P1, P2, P3;

    public Triangle3D(Point3D p1, Point3D p2, Point3D p3)
    {
        P1 = p1; P2 = p2; P3 = p3;
    }

    public float[] ComputeFaceNormal()
    {
        float[] u = { P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z };
        float[] v = { P3.X - P1.X, P3.Y - P1.Y, P3.Z - P1.Z };

        float nx = u[1] * v[2] - u[2] * v[1];
        float ny = u[2] * v[0] - u[0] * v[2];
        float nz = u[0] * v[1] - u[1] * v[0];
        float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);

        return len > 1e-6 ? new float[] { nx / len, ny / len, nz / len } : new float[] { 0, 0, 0 };
    }

    private class AETEntry
    {
        public float x;
        public float yMax;
        public float invSlope;

        public AETEntry(float x, float yMax, float invSlope)
        {
            this.x = x;
            this.yMax = yMax;
            this.invSlope = invSlope;
        }
    }

    public void Fill(IFillingProvider provider, Action<int, int, byte, byte, byte, byte> setPixel, Matrix4x4 view)
    {
        var tp1 = P1.Transform(view);
        var tp2 = P2.Transform(view);
        var tp3 = P3.Transform(view);

        // Project points to screen space
        var p1 = Project(tp1);
        var p2 = Project(tp2);
        var p3 = Project(tp3);

        var tn1 = P1.TransformNormal(view);
        var tn2 = P2.TransformNormal(view);
        var tn3 = P3.TransformNormal(view);

        // Sort vertices by Y coordinate
        var points = new List<Point> { p1, p2, p3 }.OrderBy(p => p.Y).ToList();
        var v0 = points[0];
        var v1 = points[1];
        var v2 = points[2];

        // Build edge table
        var edgeTable = new Dictionary<int, List<AETEntry>>();
        int yMin = int.MaxValue;
        int yMax = int.MinValue;

        void AddEdge(Point p1, Point p2)
        {
            if (p1.Y == p2.Y) return;

            var lower = p1.Y < p2.Y ? p1 : p2;
            var upper = p1.Y < p2.Y ? p2 : p1;

            int yStart = (int)Math.Ceiling((double)lower.Y);
            int yEnd = (int)Math.Ceiling((double)upper.Y);

            yMin = Math.Min(yMin, yStart);
            yMax = Math.Max(yMax, yEnd);

            float invSlope = (upper.X - lower.X) / (float)(upper.Y - lower.Y);
            float xStart = lower.X + (yStart - lower.Y) * invSlope;

            if (!edgeTable.ContainsKey(yStart))
                edgeTable[yStart] = new List<AETEntry>();

            edgeTable[yStart].Add(new AETEntry(xStart, yEnd, invSlope));
        }

        AddEdge(v0, v1);
        AddEdge(v1, v2);
        AddEdge(v0, v2);

        // Fill using AET
        var AET = new List<AETEntry>();

        for (int y = yMin; y < yMax; y++)
        {
            if (edgeTable.ContainsKey(y))
                AET.AddRange(edgeTable[y]);

            AET.RemoveAll(e => e.yMax <= y);
            AET.Sort((a, b) => a.x.CompareTo(b.x));

            for (int i = 0; i < AET.Count - 1; i += 2)
            {
                int xStart = (int)Math.Round(AET[i].x);
                int xEnd = (int)Math.Round(AET[i + 1].x);

                for (int x = xStart; x <= xEnd; x++)
                {
                    // Interpolate Z and normal for shading
                    float t = (x - xStart) / (float)(xEnd - xStart);
                    float z = tp1.Z + (tp2.Z - tp1.Z) * t;
                    
                    // Use transformed normals for shading
                    float nx = tn1.Nx + (tn2.Nx - tn1.Nx) * t;
                    float ny = tn1.Ny + (tn2.Ny - tn1.Ny) * t;
                    float nz = tn1.Nz + (tn2.Nz - tn1.Nz) * t;

                    var point = new Point3D(x, y, z, nx, ny, nz);
                    provider.GetColor(point, out byte r, out byte g, out byte b, out byte a);
                    setPixel(x, y, r, g, b, a);
                }
            }

            foreach (var edge in AET)
                edge.x += edge.invSlope;
        }
    }

    private Point Project(Point3D point)
    {
        float scale = 200f;
        float z = point.Z + 5f;
        
        if (z <= 0.1f)
            return new Point(-1, -1);

        int x = (int)((point.X / z) * scale + 400);
        int y = (int)((point.Y / z) * scale + 300);
        
        return new Point(x, y);
    }
}
