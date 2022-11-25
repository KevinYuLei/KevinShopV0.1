using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper;

namespace WindowComponent.Window
{
    public enum WindowType
    {

    }

    internal interface IWindow
    {
        Point3d DatumPt { get; set; }
        double WindowWidth { get; set;}
        double WindowHeight { get; set; }
        double FrameDepth { get; set; }
        double FrameMargin { get; set; }
        double GlassDepth { get; set; }

        //Result Geometry
        DataTree<Brep> WindowFrames { get; set; }
        DataTree<Brep> Glass { get; set; }

        void CreateWindow();
    }
}
