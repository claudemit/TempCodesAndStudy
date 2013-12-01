using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class class2 : GH_ScriptInstance
{
    #region Utility functions
    private void Print(string text) { /* Implementation hidden. */ }
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    private void Reflect(object obj) { /* Implementation hidden. */ }
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    private readonly RhinoDoc RhinoDocument;
    private readonly GH_Document GrasshopperDocument;
    private readonly IGH_Component Component;
    private readonly int Iteration;
    #endregion

    private void RunScript(Brep x, Curve y, int V, int U, ref object A)
    {
        Graphics g = Graphics.FromImage(bitmap1);
        g.FillRectangle(new SolidBrush(Color.White), 0, 0, u, v);
        System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(Color.Black);

        float step1 = u / U;
        float step2 = v / V;
        for (float i = 25; i < u - 25; i += step1)
        {
            for (float j = 25; j < v - 25; j += step2)
            {
                double Umin = x.Faces[0].Domain(0).Min;
                double Umax = x.Faces[0].Domain(0).Max;
                double Vmin = x.Faces[0].Domain(1).Min;
                double Vmax = x.Faces[0].Domain(1).Max;
                Point3d pos = x.Faces[0].PointAt(i / u * (Umax - Umin) + Umin, j / v * (Vmax - Vmin) + Vmin);
                double t; float R;
                if (y.ClosestPoint(pos, out t, 200))
                {
                    double dis = y.PointAt(t).DistanceTo(pos);
                    dis /= 200;
                    R = (float)(1 / dis * 2);
                    //  Print(R.ToString());
                    if (R > 40) R = 40;
                    ;
                }
                else { R = 20; }
                g.FillEllipse(myBrush, i - R, v - j - R, R * 2, R * 2);
            }
        }

        myBrush.Dispose();
        str = @"C:\maps\temp1.jpg";
        bitmap1.Save(str);
        Print(str);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        Mesh[] meshes = Mesh.CreateFromBrep(x, MeshingParameters.Smooth);
        Mesh M = new Mesh();
        foreach (Mesh partialMesh in meshes)
        {
            M.Append(partialMesh);
        }
        m_shapes.Add(M);


        Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial();
        mat.SetTransparencyTexture(str, true);
        mat.BackTransparency = 0;
        m_materials.Clear();
        m_materials.Add(mat);
    }

    // <Custom additional code> 
    string str = "";
    int u = 3000; int v = 3000;
    Bitmap bitmap1;
    private List<Mesh> m_shapes;
    private List<Rhino.Display.DisplayMaterial> m_materials;

    public override void BeforeRunScript()
    {
        bitmap1 = new Bitmap(u, v);
        m_shapes = new List<Mesh>();
        m_materials = new List<Rhino.Display.DisplayMaterial>();
    }
    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
        try
        {
            if (m_shapes == null) return;
            for (Int32 i = 0; i <= m_shapes.Count - 1; i++)
            {
                args.Display.DrawMeshShaded(m_shapes[i], m_materials[i]);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(@"An error occured during creating the image file.Please Check C:\maps" + "\r\n"
              + ex.ToString());
        }
    }

    private void InitializeStreamBitmap()
    {
        try
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream myStream = null;
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    bitmap1 = new Bitmap(myStream);
                    str = openFileDialog1.FileName;
                }
            }
        }
        catch (Exception)
        {
            MessageBox.Show("There was an error opening the image file.");
        }
    }
    // </Custom additional code> 
}