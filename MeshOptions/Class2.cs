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
public class Script_Instance : GH_ScriptInstance
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

    private void RunScript(Brep x, bool y, int v, int u, ref object A)
    {
        Mesh[] meshes = Mesh.CreateFromBrep(x, MeshingParameters.Smooth);
        Mesh M = new Mesh();
        foreach (Mesh partialMesh in meshes)
        {
            M.Append(partialMesh);
        }
        m_shapes.Add(M);
        InitializeStreamBitmap(u, v);
        Print(str);
        Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial();
        mat.SetTransparencyTexture(str, true);
        mat.BackTransparency = 0;
        m_materials.Clear();
        m_materials.Add(mat);
    }

    // <Custom additional code> 
    bool sign2 = true;
    string str = "";
    int u = 3000; int v = 3000;
    Bitmap bitmap1 = new Bitmap(3000, 3000, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    private List<Mesh> m_shapes;
    private List<Rhino.Display.DisplayMaterial> m_materials;

    public override void BeforeRunScript()
    {
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
            MessageBox.Show(ex.ToString());
        }
    }
    private void InitializeStreamBitmap(int x, int y)
    {
        Graphics g = Graphics.FromImage(bitmap1);
        g.FillRectangle(new SolidBrush(Color.White), 0, 0, u, v);
        //Pen blackPen = new Pen(Color.Black, 3);
        System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(Color.Black);
        int step1 = (int)(u / x);
        int step2 = (int)(v / y);
        for (int i = 25; i < u - 25; i += step1)
        {
            for (int j = 25; j < v - 25; j += step2)
            {
                //g.DrawEllipse(blackPen, 0 + i, 0 + j, 50, 50);
                g.FillEllipse(myBrush, 0 + i, 0 + j, 50, 50);
            }
        }
        // blackPen.Dispose();
        myBrush.Dispose();
        if (sign2) { str = @"C:\Users\Administrator\Desktop\temp1.png"; sign2 = !sign2; }
        else { str = @"C:\Users\Administrator\Desktop\temp2.png"; sign2 = !sign2; }
        //str = @"C:\Users\Administrator\Desktop\temp1.jpg";
        bitmap1.Save(str);
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