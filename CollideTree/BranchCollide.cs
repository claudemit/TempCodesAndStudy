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
public class Branch : GH_ScriptInstance
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

  private void RunScript(int x, double y, Brep z, ref object A)
  {
    List<branch> bls = new List<branch>();
    //  List<branch> Temp = new List<branch>();
    bls.Add(new branch(Plane.WorldXY, length, Math.PI / y));
    for (int i = 0; i < x; i++)
    {
      bls.AddRange(branch.grow(bls, z));
    }
    A = branch.DisplayBranches(bls);
  }

  // <Custom additional code> 
  static double length = 10;
  Random rnd = new Random();
  /// <summary>
  /// ///////////////////////////////////////////////
  /// </summary>
  class branch
  {
    public static List<Polyline> DisplayBranches(List<branch> bls)
    {
      List<Polyline> output = new List<Polyline>();
      bls.ForEach(delegate(branch br)
        {
          output.Add(br.display());
        });
      return output;
    }
    public static List<branch> grow(List<branch> bls, Brep b)
    {
      Random rnd = new Random();
      List<branch> output = new List<branch>();
      for (int i = 0; i < bls.Count; i++)
      {
        branch b3 = bls[i];
        if (b3.life)
        {
          branch b1 = new branch(b3.p2, b3.length, b3.deg);
          branch b2 = new branch(b3.p3, b3.length, b3.deg);
          double seed = rnd.NextDouble();
          if(seed > 0.3333 && seed < 0.666){b1.rotateBranch();}
          if(seed > 0.666){b1.rotateBranch();}
          seed = rnd.NextDouble();
          if(seed > 0.3333 && seed < 0.666){b2.rotateBranch();}
          if(seed > 0.666){b2.rotateBranch();}
          output.Add(new branch(b1));
          output.Add(new branch(b2));
          b3.life = false;
        }
      }

      for (int i = 0; i < output.Count; i++)
      {
        for (int j = 0; j < output.Count; j++)
        { if(i != j && output[j].life ){
            if(output[i].collide(output[j])){
              output[i].life = false; break;
            }
          }
        }

        for (int j = 0; j < bls.Count; j++){
          if(output[i].collide(bls[j])){
            output[i].life = false; break;
          }
        }
      }




      List<branch> output2 = new List<branch>();
      for (int i = 0; i < output.Count; i++)
      {
        if (output[i].life) output2.Add(output[i]);
      }
      return output2;
    }

    /// //////////////////////////////////////////////////////////////////////
    public Plane p1 = Plane.WorldXY;
    public Plane p2 = Plane.WorldXY;
    public Plane p3 = Plane.WorldXY;

    public double R;
    double length; double deg;
    public bool life = true;
    public branch(branch other){
      this.p1 = other.p1; this.p2 = other.p2; this.p3 = other.p3;
      this.R = other.R;this.length = other.length;this.deg = other.deg;
    }
    public branch(Plane p, double _length, double _deg)
    {
      p2.Transform(Transform.Translation(new Vector3d(0, 0, _length)));
      p3.Transform(Transform.Translation(new Vector3d(0, 0, _length)));
      p2.Transform(Transform.Rotation(_deg, Vector3d.YAxis, Point3d.Origin));
      p3.Transform(Transform.Rotation(-_deg, Vector3d.YAxis, Point3d.Origin));
      this.length = _length; this.deg = _deg;
      double d1 = p2.Origin.DistanceTo(p1.Origin);
      double d2 = p2.Origin.DistanceTo(p3.Origin);
      if (d2 < d1) d1 = d2;
      R = d1 / 2;
      this.p2.Transform(Transform.PlaneToPlane(this.p1, p));
      this.p3.Transform(Transform.PlaneToPlane(this.p1, p));
      this.p1.Transform(Transform.PlaneToPlane(this.p1, p));

    }
    public Polyline display()
    {
      Polyline pl = new Polyline();
      pl.Add(p2.Origin);
      pl.Add(p1.Origin);
      pl.Add(p3.Origin);
      return pl;
    }

    public void rotateBranch()
    {
      this.p1.Rotate(Math.PI / 3, p1.Normal, p1.Origin);
      this.p2.Rotate(Math.PI / 3, p1.Normal, p1.Origin);
      this.p3.Rotate(Math.PI / 3, p1.Normal, p1.Origin);
    }


    public bool collide(branch b1)
    {
      if (this.p2.Origin.DistanceTo(b1.p2.Origin) < this.R * 2 ) return true;
      if (this.p2.Origin.DistanceTo(b1.p3.Origin) < this.R * 2 ) return true;
      if (this.p3.Origin.DistanceTo(b1.p2.Origin) < this.R * 2 ) return true;
      if (this.p3.Origin.DistanceTo(b1.p3.Origin) < this.R * 2 ) return true;
      return false;
    }
  
}
}