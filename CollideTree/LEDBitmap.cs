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



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class bitmapLED : GH_ScriptInstance
{
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(Brep x, int y, int U, int u, int z, List<double> Colors, int stX, int stY, ref object A)
    {
        //  if(sign){sign = false;
        double Umin = x.Faces[0].Domain(0).Min;
        double Umax = x.Faces[0].Domain(0).Max;
        double Vmin = x.Faces[0].Domain(1).Min;
        double Vmax = x.Faces[0].Domain(1).Max;
        double t = (Vmax - Vmin) / (Umax - Umin) * u;
        v = (int)t;
        t = (Vmax - Vmin) / (Umax - Umin) * U;
        V = (int)t;
        bitmap1 = new Bitmap(u, v);
        m_shapes = new List<Mesh>();
        m_materials = new List<Rhino.Display.DisplayMaterial>();
        mappixel = new pixel[U, V];
        for (int i = 0; i < U; i++)
        {
            for (int j = 0; j < V; j++)
            {
                double posX = (double)i / (double)U * (double)u;
                double posY = (double)j / (double)V * (double)v;
                mappixel[i, j] = new pixel(posX, posY);
            }
        }
        // }

        Graphics g = Graphics.FromImage(bitmap1);
        g.FillRectangle(new SolidBrush(Color.Black), 0, 0, u, v);
        double iso = 0;
        Random Rnd = new Random();
        for (int j = 0; j < V; j++)
        {
            for (int i = 0; i < U; i++)
            {
                if (j <= y) { mappixel[i, j].color = Color.White; }
                else if (Rnd.NextDouble() > iso) { mappixel[i, j].color = Color.White; }
            }
            if (j > y) iso += 0.02;
        }
        //////////////////////////****************************************
        int count = 0;

        for (int j = stY + z; j > stY; j--)
        {
            for (int i = stX; i < stX + Colors.Count / z; i++)
            {

                if (count > Colors.Count - 1) break;
                if (Colors[count] != 1)
                {
                    mappixel[i, j].color = Color.Gray;
                }
                else
                {
                    //mappixel[i, j].color = Color.Green;
                }
                count++;
            }
        }
        ///////////////////////////////
        for (int i = 0; i < U; i++)
        {
            for (int j = 0; j < V; j++)
            {
                mappixel[i, j].Draw(ref g, (int)(u / 2 / U));
            }
        }
        g.Dispose();
        str = @"C:\maps\temp1.jpg";
        bitmap1.Save(str);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        Mesh[] meshes = Mesh.CreateFromBrep(x, MeshingParameters.Smooth);
        Mesh M = new Mesh();
        foreach (Mesh partialMesh in meshes)
        {
            M.Append(partialMesh);
        }
        m_shapes.Add(M);
        Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial();
        //  mat.SetTransparencyTexture(str, true);
        // mat.BackTransparency = 0;
        mat.Diffuse = Color.White;
        mat.SetBitmapTexture(str, true);

        m_materials.Clear();
        m_materials.Add(mat);


    }

    // <Custom additional code> 
    string str = "";
    int v, V;
    Bitmap bitmap1;
    pixel[,] mappixel;
    private List<Mesh> m_shapes;
    private List<Rhino.Display.DisplayMaterial> m_materials;
    // bool sign = true;

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


    class pixel
    {
        public Point2f position = new Point2f();
        public Color color = Color.Black;
        public pixel() { }
        public pixel(double x, double y)
        {
            position.X = (float)x;
            position.Y = (float)y;
        }
        public void Draw(ref Graphics g, int R)
        {
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(this.color);
            int i = (int)this.position.X; int j = (int)this.position.Y;
            g.FillEllipse(myBrush, i - R, j - R, R * 2, R * 2);
            myBrush.Dispose();
        }
    }
    // </Custom additional code> 
}