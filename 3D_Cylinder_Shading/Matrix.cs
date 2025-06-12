using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Cylinder_Shading;

public class Matrix4x4
{
    public float[,] M = new float[4, 4];

    public Matrix4x4() { }

    public static Matrix4x4 Identity()
    {
        var m = new Matrix4x4();
        for (int i = 0; i < 4; i++) m.M[i, i] = 1f;
        return m;
    }

    public static Matrix4x4 RotationX(float angleDeg)
    {
        float rad = angleDeg * (float)Math.PI / 180f;
        var m = Identity();
        m.M[1, 1] = (float)Math.Cos(rad);
        m.M[1, 2] = -(float)Math.Sin(rad);
        m.M[2, 1] = (float)Math.Sin(rad);
        m.M[2, 2] = (float)Math.Cos(rad);
        return m;
    }

    public static Matrix4x4 RotationY(float angleDeg)
    {
        float rad = angleDeg * (float)Math.PI / 180f;
        var m = Identity();
        m.M[0, 0] = (float)Math.Cos(rad);
        m.M[0, 2] = (float)Math.Sin(rad);
        m.M[2, 0] = -(float)Math.Sin(rad);
        m.M[2, 2] = (float)Math.Cos(rad);
        return m;
    }

    public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
    {
        var result = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
            {
                result.M[i, j] = 0;
                for (int k = 0; k < 4; k++)
                    result.M[i, j] += a.M[i, k] * b.M[k, j];
            }
        return result;
    }

    public static float[] Multiply(Matrix4x4 m, float[] v)
    {
        float[] result = new float[4];
        for (int i = 0; i < 4; i++)
        {
            result[i] = 0;
            for (int j = 0; j < 4; j++)
                result[i] += m.M[i, j] * v[j];
        }
        
        // Console.WriteLine($"Matrix multiplication:");
        // Console.WriteLine($"Input vector: [{v[0]}, {v[1]}, {v[2]}, {v[3]}]");
        // Console.WriteLine($"Result vector: [{result[0]}, {result[1]}, {result[2]}, {result[3]}]");
        
        return result;
    }
}
