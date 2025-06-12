using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Cylinder_Shading;

public class PhongFillingProvider : IFillingProvider
{
    // Fixed light and camera positions
    public float[] LightPosition = { 0, 0, -10 };
    public float[] CameraPosition = { 0, 0, -10 };
    public float Ambient = 0.2f;
    public float Diffuse = 0.6f;
    public float Specular = 0.2f;
    public int Shininess = 32;

    public void GetColor(Point3D point, out byte r, out byte g, out byte b, out byte a)
    {
        // Use the transformed normal from the point
        float[] normal = Normalize(new float[] { point.Nx, point.Ny, point.Nz });
        float[] lightDir = Normalize(new float[] { LightPosition[0] - point.X, LightPosition[1] - point.Y, LightPosition[2] - point.Z });
        float[] viewDir = Normalize(new float[] { CameraPosition[0] - point.X, CameraPosition[1] - point.Y, CameraPosition[2] - point.Z });

        // Calculate diffuse lighting
        float dotNL = Math.Max(0, Dot(normal, lightDir));
        
        // Calculate specular lighting
        float[] reflectDir = Reflect(lightDir, normal);
        float dotRV = Math.Max(0, Dot(reflectDir, viewDir));
        float specular = (float)Math.Pow(dotRV, Shininess);

        // Combine lighting components
        float intensity = Ambient + Diffuse * dotNL + Specular * specular;
        intensity = Math.Clamp(intensity, 0, 1);

        // Convert to grayscale
        byte value = (byte)(intensity * 255);
        r = g = b = value;
        a = 255;
    }

    private float Dot(float[] a, float[] b) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];

    private float[] Normalize(float[] v)
    {
        float len = (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        if (len < 1e-6) return new float[] { 0, 0, 0 };
        return new float[] { v[0] / len, v[1] / len, v[2] / len };
    }

    private float[] Reflect(float[] L, float[] N)
    {
        float dot = Dot(N, L);
        return new float[]
        {
            2 * dot * N[0] - L[0],
            2 * dot * N[1] - L[1],
            2 * dot * N[2] - L[2]
        };
    }
}
