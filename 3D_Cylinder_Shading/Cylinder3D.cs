using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Cylinder_Shading;

public class Cylinder3D
{
    public List<Triangle3D> Triangles = new();

    public Cylinder3D(float radius, float height, int segments)
    {
        // Console.WriteLine($"Creating cylinder with radius={radius}, height={height}, segments={segments}");
        for (int i = 0; i < segments; i++)
        {
            float theta = (float)(i * 2 * Math.PI / segments);
            float nextTheta = (float)((i + 1) * 2 * Math.PI / segments);

            float nx = (float)Math.Cos(theta);
            float ny = (float)Math.Sin(theta);
            float nnextx = (float)Math.Cos(nextTheta);
            float nnexty = (float)Math.Sin(nextTheta);

            var top1 = new Point3D(radius * nx, radius * ny, height / 2, nx, ny, 0);
            var top2 = new Point3D(radius * nnextx, radius * nnexty, height / 2, nnextx, nnexty, 0);
            var bot1 = new Point3D(radius * nx, radius * ny, -height / 2, nx, ny, 0);
            var bot2 = new Point3D(radius * nnextx, radius * nnexty, -height / 2, nnextx, nnexty, 0);

            Triangles.Add(new Triangle3D(top1, bot1, top2));
            Triangles.Add(new Triangle3D(top2, bot1, bot2));
        }

        // Create top cap
        var topCenter = new Point3D(0, 0, height / 2, 0, 0, 1);
        for (int i = 0; i < segments; i++)
        {
            float theta = (float)(i * 2 * Math.PI / segments);
            float nextTheta = (float)((i + 1) * 2 * Math.PI / segments);

            float nx = (float)Math.Cos(theta);
            float ny = (float)Math.Sin(theta);
            float nnextx = (float)Math.Cos(nextTheta);
            float nnexty = (float)Math.Sin(nextTheta);

            var top1 = new Point3D(radius * nx, radius * ny, height / 2, 0, 0, 1);
            var top2 = new Point3D(radius * nnextx, radius * nnexty, height / 2, 0, 0, 1);

            Triangles.Add(new Triangle3D(topCenter, top1, top2));
        }

        // Create bottom cap
        var bottomCenter = new Point3D(0, 0, -height / 2, 0, 0, -1);
        for (int i = 0; i < segments; i++)
        {
            float theta = (float)(i * 2 * Math.PI / segments);
            float nextTheta = (float)((i + 1) * 2 * Math.PI / segments);

            float nx = (float)Math.Cos(theta);
            float ny = (float)Math.Sin(theta);
            float nnextx = (float)Math.Cos(nextTheta);
            float nnexty = (float)Math.Sin(nextTheta);

            var bot1 = new Point3D(radius * nx, radius * ny, -height / 2, 0, 0, -1);
            var bot2 = new Point3D(radius * nnextx, radius * nnexty, -height / 2, 0, 0, -1);

            Triangles.Add(new Triangle3D(bottomCenter, bot2, bot1));  // Note: reversed order for correct winding
        }

        Console.WriteLine($"Created {Triangles.Count} triangles");
    }

    public void Fill(IFillingProvider provider, Action<int, int, byte, byte, byte, byte> setPixel, Matrix4x4 view)
    {
        // Console.WriteLine($"Filling cylinder with {Triangles.Count} triangles");
        int visibleTriangles = 0;
        //int culledTriangles = 0;
        
        foreach (var tri in Triangles)
        {
            var normal = tri.ComputeFaceNormal();
            var rotatedNormal = Matrix4x4.Multiply(view, [normal[0], normal[1], normal[2], 0]);

            // Console.WriteLine($"Triangle normal before rotation: ({normal[0]}, {normal[1]}, {normal[2]})");
            // Console.WriteLine($"Triangle normal after rotation: ({rotatedNormal[0]}, {rotatedNormal[1]}, {rotatedNormal[2]})");

            // Only render if the face is visible (facing the camera)
            if (rotatedNormal[2] < 0)
            {
                visibleTriangles++;
                tri.Fill(provider, setPixel, view);
            }
            // else
            // {
            //     culledTriangles++;
            // }
        }
        // Console.WriteLine($"Rendered {visibleTriangles} visible triangles, culled {culledTriangles} triangles");
    }
}
