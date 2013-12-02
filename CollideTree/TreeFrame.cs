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


using MeshClassLibrary;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class TreeFrame : GH_ScriptInstance
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
    private void RunScript(List<Line> x, List<Point3d> y, ref object A, ref object B, ref object C)
    {
        try
        {
            List<IndexPair> id; List<Vertice1> vs;
            Vertice1.CreateCollection(x, out id, out vs);
            for (int i = 0; i < vs.Count; i++)
            {
                for (int j = 0; j < y.Count; j++)
                {
                    if (vs[i].equalTo(y[j])) { vs[i].energe = 0.8; break; }
                }
            }
            for (int i = 0; i < 10; i++)
            {
                vs.ForEach(delegate(Vertice1 v) { v.transferEnerge(0.70, ref vs); });
            }


            for (int i = 0; i < vs.Count; i++)
            {
                vs[i].CrateEdges(vs,5);
                //Print(vs[i].edges.Count.ToString());
            }
            ////////////////

            Mesh mesh = new Mesh();
       

            A = mesh;
        }
        catch (Exception ex) { Print(ex.ToString()); }
    }

    // <Custom additional code> 
    MeshCreation mc = new MeshCreation();
    public class Vertice1 : BasicVertice
    {
        /// static
        public Vertice1(Point3d p) : base(p) { }
        public static List<Point3d> DisplayPos(List<Vertice1> vs)
        {
            List<Point3d> output = new List<Point3d>();
            vs.ForEach(delegate(Vertice1 v) { output.Add(v.pos); });
            return output;
        }
        public static List<string> DisplayEnerge(List<Vertice1> vs)
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate(Vertice1 v) { output.Add(v.energe.ToString()); });
            return output;
        }
        public static List<string> DisplayLife(List<Vertice1> vs)
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate(Vertice1 v) { output.Add(v.dead.ToString()); });
            return output;
        }
        public static void CreateCollection(List<Line> x, out List<IndexPair> id, out  List<Vertice1> vs)
        {
            id = new List<IndexPair>(); vs = new List<Vertice1>();
            id.Add(new IndexPair(0, 1));
            vs.Add(new Vertice1(x[0].From, 1));
            vs.Add(new Vertice1(x[0].To, 0));
            for (int i = 1; i < x.Count; i++)
            {
                bool sign1 = true;
                bool sign2 = true;
                int a = 0, b = 0;
                for (int j = 0; j < vs.Count; j++)
                {
                    if (vs[j].equalTo(x[i].From)) { sign1 = false; a = j; }
                    if (vs[j].equalTo(x[i].To)) { sign2 = false; b = j; }
                    if (!sign1 && !sign2) { break; }
                }
                if (sign1) { vs.Add(new Vertice1(x[i].From)); a = vs.Count - 1; }
                if (sign2) { vs.Add(new Vertice1(x[i].To)); b = vs.Count - 1; }
                vs[a].Add(b); vs[b].Add(a);
                id.Add(new IndexPair(a, b));
            }
        }
        /// ////////////////
        public Vertice1(Point3d p, int index) : base(p, index) { }
        public List<Polyline> edges = new List<Polyline>();
        public bool transferEnerge(double percentage, ref List<Vertice1> vs)
        {
            bool sign = false;
            if (!this.dead && this.energe != 0)
            {
                this.dead = true;
                for (int i = 0; i < this.refer.Count; i++)
                {
                    if (vs[this.refer[i]].energe == 0)
                    {
                        vs[this.refer[i]].energe = this.energe * percentage;
                        sign = true;
                    }
                }
            }
            return sign;
        }
        public void CrateEdges(List<Vertice1> vs,double t)
        {
            if (this.refer.Count == 3)
            {
                Point3d p1 = vs[this.refer[0]].pos; Vector3d v1 = p1 - this.pos; v1.Unitize();
                Point3d p2 = vs[this.refer[1]].pos; Vector3d v2 = p2 - this.pos; v2.Unitize();
                Point3d p3 = vs[this.refer[2]].pos; Vector3d v3 = p3 - this.pos; v3.Unitize();
                p1 = this.pos + v1 * t; p2 = this.pos + v1 * t; p3 = this.pos + v1 * t;
                Polyline pl1 = new Polyline(); pl1.Add(p1); pl1.Add(this.pos); pl1.Add(p3); pl1.Add(p1);
                Polyline pl2 = new Polyline(); pl2.Add(p3); pl2.Add(this.pos); pl2.Add(p2); pl2.Add(p3);
                Polyline pl3 = new Polyline(); pl3.Add(p2); pl3.Add(this.pos); pl3.Add(p1); pl3.Add(p2);
                edges.Add(pl1); edges.Add(pl2); edges.Add(pl3);
            }
        }
    }
    // </Custom additional code> 
}