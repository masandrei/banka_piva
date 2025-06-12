using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace _3D_Cylinder_Shading;

public partial class Form1 : Form
{
    private Bitmap canvas;
    private Cylinder3D cylinder;
    private PhongFillingProvider lighting;
    private float angleX = 0f;
    private float angleY = 0f;
    private IFillingProvider fillingProvider;


    public Form1()
    {
        
        this.Text = "3D Cylinder Renderer";
        InitializeComponent();
        canvas = new Bitmap(800, 600);

        pictureBox1.Image = canvas;

        cylinder = new Cylinder3D(2, 4, 50);
        lighting = new PhongFillingProvider();
        var phongProvider = new PhongFillingProvider();
        phongProvider.CameraPosition = new float[]{ 0, 0, -10};
        // Position light at an angle to the cylinder
        phongProvider.LightPosition = new float[]{ 5, 5, -10};
        fillingProvider = phongProvider;
        KeyDown += OnKeyDown;
        Render();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        Console.WriteLine(e.KeyCode);
        switch (e.KeyCode)
        {
            case Keys.Left:
                angleY -= 5f;
                break;
            case Keys.Right:
                angleY += 5f;
                break;
            case Keys.Up:
                angleX -= 5f;
                break;
            case Keys.Down:
                angleX += 5f;
                break;
        }
        Render();
    }

    private unsafe void Render()
    {
        using Graphics g = Graphics.FromImage(canvas);
        g.Clear(Color.Black);

        // Create view matrix with a fixed offset to look at the cylinder
        Matrix4x4 rotationX = Matrix4x4.RotationX(angleX);
        Matrix4x4 rotationY = Matrix4x4.RotationY(angleY);
        Matrix4x4 view = rotationY * rotationX;
        
        Rectangle rect = new Rectangle(0, 0, canvas.Width, canvas.Height);
        BitmapData bmpData = canvas.LockBits(rect, ImageLockMode.ReadWrite, canvas.PixelFormat);
        cylinder.Fill(fillingProvider, (x, y, r, gr, b, a) =>
        {
            if (x >= 0 && x < canvas.Width && y >= 0 && y < canvas.Height)
            {
                byte* data = (byte*)bmpData.Scan0;
                var ptr = data + y * bmpData.Stride + x * 4;
                Color c = Color.FromArgb(a, r, gr, b);
                ptr[0] = c.B;
                ptr[1] = c.G;
                ptr[2] = c.R;
                ptr[3] = c.A;
            }
        }, view);
        canvas.UnlockBits(bmpData);
        pictureBox1.Invalidate();
    }

    private void DrawEdge(Graphics g, Point3D p1, Point3D p2, Pen pen)
    {
        Point screen1 = Project(p1);
        Point screen2 = Project(p2);
        g.DrawLine(pen, screen1, screen2);
    }

    private Point Project(Point3D point)
    {
        // Simple perspective projection
        float scale = 200f;  // Scale factor to make the cylinder visible
        float z = point.Z + 5f;  // Add some distance to prevent division by zero
        
        if (z <= 0.1f)  // Point is behind or too close to camera
        {
            Console.WriteLine($"Point behind camera: ({point.X}, {point.Y}, {point.Z})");
            return new Point(-1, -1);
        }

        // Project to screen space
        int x = (int)((point.X / z) * scale + canvas.Width / 2);
        int y = (int)((point.Y / z) * scale + canvas.Height / 2);
        
        Console.WriteLine($"Projecting point: ({point.X}, {point.Y}, {point.Z}) -> ({x}, {y})");
        
        return new Point(x, y);
    }
}
