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
public class TeePipe : GH_ScriptInstance
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
            List<IndexPair> id; List<Vertice> vs;
            Vertice.CreateCollection(x, out id, out vs);
            for (int i = 0; i < vs.Count; i++)
            {
                for (int j = 0; j < y.Count; j++)
                {
                    if (vs[i].equalTo(y[j])) { vs[i].energe = 0.8; break; }
                }
            }
            for (int i = 0; i < 10; i++)
            {
                vs.ForEach(delegate(Vertice v) { v.transferEnerge(0.70, ref vs); });
            }

            for (int i = 0; i < vs.Count; i++)
            {
                vs[i].CrateEdges(vs);
                //Print(vs[i].edges.Count.ToString());
            }
            ////////////////

            Mesh mesh = new Mesh();
            for (int i = 0; i < id.Count; i++)
            {
                Polyline pl1 = new Polyline(); Polyline pl2 = new Polyline();
                if (vs[id[i].J].refer.Count == 3)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (vs[id[i].J].refer[j] == id[i].I)
                        {
                            pl1 = vs[id[i].J].edges[j]; break;
                        }
                    }
                }
                if (vs[id[i].I].refer.Count == 3)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (vs[id[i].I].refer[j] == id[i].J)
                        {
                            pl2 = vs[id[i].I].edges[j]; break;
                        }
                    }
                }
                //Print(pl1.Count.ToString());
                if (pl1.Count == 4 && pl2.Count == 0)
                {
                    Plane p = new Plane(vs[id[i].I].pos, vs[vs[id[i].I].refer[0]].pos - vs[id[i].I].pos);
                    pl2.AddRange(pl1);
                    pl2.Transform(Transform.PlanarProjection(p));

                }
                if (pl1.Count == 0 && pl2.Count == 4)
                {
                    Plane p = new Plane(vs[id[i].J].pos, vs[vs[id[i].J].refer[0]].pos - vs[id[i].J].pos);
                    pl1.AddRange(pl2);
                    pl1.Transform(Transform.PlanarProjection(p));

                }
                if (pl1.Count == 4 && pl2.Count == 4)
                {

                    Plane p1 = new Plane(pl1[0], pl1[1], pl1[2]);
                    Plane p2 = new Plane(pl2[0], pl2[1], pl2[2]);
                    if (Vector3d.VectorAngle(p1.Normal, p2.Normal) > Math.PI / 2) pl2.Reverse();
                    mesh.Append(mc.ClosedBridge(pl1, pl2));

                }
            }

            A = mesh;
        }
        catch (Exception ex) { Print(ex.ToString()); }
    }

    // <Custom additional code> 
    MeshCreation mc = new MeshCreation();
    class Vertice
    {
        public bool transferEnerge(double percentage, ref List<Vertice> vs)
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
        public List<Polyline> edges = new List<Polyline>();
        public void CrateEdges(List<Vertice> vs)
        {
            if (this.refer.Count == 3)
            {
                Point3d p1 = vs[this.refer[0]].pos; Vector3d v1 = p1 - this.pos; v1.Unitize();
                Point3d p2 = vs[this.refer[1]].pos; Vector3d v2 = p2 - this.pos; v2.Unitize();
                Point3d p3 = vs[this.refer[2]].pos; Vector3d v3 = p3 - this.pos; v3.Unitize();
                Plane p = new Plane(p1, p2, p3);
                Vector3d n = p.Normal; n.Unitize();
                Point3d N1 = this.pos + n * energe;
                Point3d N2 = this.pos - n * energe;
                Vector3d v0 = v1 + v2; v0.Unitize(); v0 *= this.energe;
                Point3d p12 = this.pos + v0;
                v0 = v2 + v3; v0.Unitize(); v0 *= this.energe;
                Point3d p23 = this.pos + v0;
                v0 = v3 + v1; v0.Unitize(); v0 *= this.energe;
                Point3d p31 = this.pos + v0;
                Polyline pl1 = new Polyline(); pl1.Add(N1); pl1.Add(p12); pl1.Add(N2); pl1.Add(p31);
                Polyline pl2 = new Polyline(); pl2.Add(N1); pl2.Add(p23); pl2.Add(N2); pl2.Add(p12);
                Polyline pl3 = new Polyline(); pl3.Add(N1); pl3.Add(p23); pl3.Add(N2); pl3.Add(p31);
                edges.Add(pl1); edges.Add(pl2); edges.Add(pl3);
            }
        }
        /////////////////////basic
        public Point3d pos;
        public bool dead = false;
        public List<int> refer = new List<int>();
        public double energe = 0;
        public Vertice(Point3d p)
        {
            pos = new Point3d(p);
        }
        public Vertice(Point3d p, int index)
        {
            pos = new Point3d(p);
            this.refer.Add(index);
        }
        public void Add(int i)
        {
            this.refer.Add(i);
        }
        public bool equalTo(Point3d pt)
        {
            if (pos.DistanceTo(pt) < 0.01) { return true; }
            return false;
        }
        public void cleanRefer()
        {
            if (this.refer.Count < 2) return;
            else if (this.refer.Count == 2)
            {
                if (this.refer[0] == this.refer[1]) this.refer.RemoveAt(0);
                return;
            }
            else
            {
                this.refer.Sort();
                List<int> newRefer = new List<int>();
                newRefer.Add(refer[0]);
                for (int i = 1; i < refer.Count - 1; i++)
                {
                    if (refer[i] != refer[i - 1] && refer[i] != refer[i + 1]) newRefer.Add(refer[i]);
                }
                if (refer[refer.Count - 2] != refer[refer.Count - 1]) newRefer.Add(refer[refer.Count - 1]);
                this.refer = newRefer;
            }
        }
        /// //////////////////static
        public static void CreateCollection(List<Line> x, out List<IndexPair> id, out  List<Vertice> vs)
        {
            id = new List<IndexPair>(); vs = new List<Vertice>();
            id.Add(new IndexPair(0, 1));
            vs.Add(new Vertice(x[0].From, 1));
            vs.Add(new Vertice(x[0].To, 0));
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
                if (sign1) { vs.Add(new Vertice(x[i].From)); a = vs.Count - 1; }
                if (sign2) { vs.Add(new Vertice(x[i].To)); b = vs.Count - 1; }
                vs[a].Add(b); vs[b].Add(a);
                id.Add(new IndexPair(a, b));
            }
            for (int j = 0; j < vs.Count; j++)
            {
                vs[j].cleanRefer();
            }
        }
        public static List<Point3d> DisplayPos(List<Vertice> vs)
        {
            List<Point3d> output = new List<Point3d>();
            vs.ForEach(delegate(Vertice v) { output.Add(v.pos); });
            return output;
        }
        public static List<string> DisplayEnerge(List<Vertice> vs)
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate(Vertice v) { output.Add(v.energe.ToString()); });
            return output;
        }
        public static List<string> DisplayLife(List<Vertice> vs)
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate(Vertice v) { output.Add(v.dead.ToString()); });
            return output;
        }
    }
    // </Custom additional code> 
}