namespace _3D_Cylinder_Shading;

public class Point3D
{
    public float X, Y, Z;
    public float Nx, Ny, Nz;

    public Point3D(float x, float y, float z, float nx = 0, float ny = 0, float nz = 0)
    {
        X = x; Y = y; Z = z;
        Nx = nx; Ny = ny; Nz = nz;
    }

    public Point3D Transform(Matrix4x4 matrix)
    {
        float[] vec = { X, Y, Z, 1 };
        float[] transformed = Matrix4x4.Multiply(matrix, vec);
        
        // Console.WriteLine($"Transforming point: ({X}, {Y}, {Z})");
        // Console.WriteLine($"Transformed to: ({transformed[0]}, {transformed[1]}, {transformed[2]})");
        
        // Apply perspective divide
        float w = transformed[3];
        if (Math.Abs(w) > 1e-6)
        {
            transformed[0] /= w;
            transformed[1] /= w;
            transformed[2] /= w;
        }
        
        return new Point3D(transformed[0], transformed[1], transformed[2], Nx, Ny, Nz);
    }

    public Point3D TransformNormal(Matrix4x4 matrix)
    {
        float[] vec = { Nx, Ny, Nz, 0 };
        float[] transformed = Matrix4x4.Multiply(matrix, vec);
        return new Point3D(X, Y, Z, transformed[0], transformed[1], transformed[2]);
    }
}
