using _3D_Cylinder_Shading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Cylinder_Shading;

public interface IFillingProvider
{
    void GetColor(Point3D point, out byte r, out byte g, out byte b, out byte a);
}
