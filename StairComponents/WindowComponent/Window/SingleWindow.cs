using Grasshopper;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowComponent.Window
{
    public class SingleWindow : IWindow
    {
        //input fields
        private Point3d datumPt=new Point3d();
        private double windowWidth = double.NaN;
        private double windowHeight = double.NaN;
        private double frameDepth = double.NaN;
        private double frameMargin = double.NaN;
        private double glassDepth = double.NaN;
        //output fields
        private DataTree<Brep> windowFrame = new DataTree<Brep>();
        private DataTree<Brep> glass = new DataTree<Brep>();

        //input properties
        public Point3d DatumPt { get => datumPt; set => datumPt = value; }
        public double WindowWidth { get => windowWidth; set => windowWidth = value; }
        public double WindowHeight { get => windowHeight; set => windowHeight = value; }
        public double FrameDepth { get => frameDepth; set => frameDepth=value; }
        public double FrameMargin { get => frameMargin; set => frameMargin = value; }
        public double GlassDepth 
        {
            get => glassDepth < frameDepth ? glassDepth : frameDepth;
            set => glassDepth = value; 
        }
        //output properties
        public DataTree<Brep> WindowFrames { get => windowFrame; set => windowFrame = value; }
        public DataTree<Brep> Glass { get => glass; set => glass = value; }


        public SingleWindow
            (
            Point3d datumPt, double windowWidth, double windowHeight,
            double frameDepth, double frameMargin, double glassDepth
            )
        {
            DatumPt = datumPt;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            FrameDepth = frameDepth;
            FrameMargin = frameMargin;
            GlassDepth = glassDepth;
        }

        public virtual void CreateWindow()
        {
            CreateWindowFrame();
            CreateGlass();
        }

        protected virtual void CreateWindowFrame()
        {

        }

        protected virtual void CreateGlass()
        {

        }
    }
}
