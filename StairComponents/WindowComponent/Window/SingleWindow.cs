using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowComponent.Window
{
    public class SingleWindow : IWindow, IWindowPosition, IWindowRotation
    {
        //input fields
        //implement IWindow fields
        private Point3d datumPt=new Point3d();
        private double windowWidth = double.NaN;
        private double windowHeight = double.NaN;
        private double frameDepth = double.NaN;
        private double frameMargin = double.NaN;
        private double glassDepth = double.NaN;
        //implement IWindowPosition fields
        private double wallDepth = double.NaN;

        private bool isDepthFlip = false;
        private bool isWidthFlip = false;
        //implement IWindowRotation fields
        private double angle = 0.0;

        //output fields
        private DataTree<Brep> windowFrame = new DataTree<Brep>();
        private DataTree<Brep> glass = new DataTree<Brep>();

        //input properties
        //implement IWindow properties
        public Point3d DatumPt { get => datumPt; set => datumPt = value; }
        public double WindowWidth { get => Math.Abs(windowWidth); set => windowWidth = value; }
        public double WindowHeight { get => Math.Abs(windowHeight); set => windowHeight = value; }
        public double FrameDepth { get => Math.Abs(frameDepth); set => frameDepth = value; }
        public double FrameMargin { get => Math.Abs(frameMargin); set => frameMargin = value; }
        public double GlassDepth 
        {
            get => Math.Abs(glassDepth) < FrameDepth ? Math.Abs(glassDepth) : FrameDepth;
            set => glassDepth = value; 
        }
        //implement IWindowPosition properties
        public double WallDepth 
        {
            get => Math.Abs(wallDepth) > FrameDepth ? Math.Abs(wallDepth) : FrameDepth;
            set => wallDepth = value; 
        }

        public bool IsDepthFlip { get => isDepthFlip; set => isDepthFlip = value; }
        public bool IsWidthFlip { get => isWidthFlip; set => isWidthFlip = value; }

        //implement IWindowRotation properties
        public double Angle { get => angle % 360; set => angle = value; }

        //output properties
        //implement IWindow Properties
        public DataTree<Brep> WindowFrames { get => windowFrame; set => windowFrame = value; }
        public DataTree<Brep> Glass { get => glass; set => glass = value; }
        

        public SingleWindow
            (
            Point3d datumPt, double windowWidth, double windowHeight,
            double frameDepth, double frameMargin, double glassDepth,
            double wallDepth, bool isDepthFlip, bool isWidthFlip,
            double angle
            )
        {
            DatumPt = datumPt;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            FrameDepth = frameDepth;
            FrameMargin = frameMargin;
            GlassDepth = glassDepth;

            WallDepth = wallDepth;
            IsDepthFlip = isDepthFlip;
            IsWidthFlip = isWidthFlip;

            Angle = angle;
        }

        #region implement method for IWindow
        public virtual void CreateWindow()
        {
            CreateWindowFrame();
            CreateGlass();
            PositionWindow();
            RotateWindow();
        }

        //创建窗框
        protected virtual void CreateWindowFrame()
        {
            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            Plane framePlane=new Plane(DatumPt, Vector3d.XAxis, Vector3d.ZAxis);
            Rectangle3d frameRect = new Rectangle3d(framePlane, WindowWidth, WindowHeight);

            Curve frameCurve1 = frameRect.ToNurbsCurve();
            Curve frameCurve2=frameCurve1.Offset(framePlane, -frameMargin, tolerance, CurveOffsetCornerStyle.None)[0];
            
            Brep outterBrep = Surface.CreateExtrusion(frameCurve1, Vector3d.YAxis * FrameDepth).ToBrep().CapPlanarHoles(tolerance);
            Brep innerBrep = Surface.CreateExtrusion(frameCurve2, Vector3d.YAxis * frameDepth).ToBrep().CapPlanarHoles(tolerance);

            Brep windowFrame = Brep.CreateBooleanDifference(outterBrep, innerBrep, tolerance)[0];

            WindowFrames.Add(windowFrame);
        }

        //创建玻璃
        protected virtual void CreateGlass()
        {
            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            Plane glassPlane= new Plane(DatumPt,Vector3d.XAxis,Vector3d.ZAxis);
            Curve glassCurve = new Rectangle3d(glassPlane, WindowWidth - 2 * FrameMargin, WindowHeight - 2 * FrameMargin).ToNurbsCurve();
            glassCurve.Transform(Transform.Translation(FrameMargin, (FrameDepth-GlassDepth)/2, FrameMargin));

            Brep glass = Surface.CreateExtrusion(glassCurve, Vector3d.YAxis * GlassDepth).ToBrep().CapPlanarHoles(tolerance);

            Glass.Add(glass);
        }
        #endregion

        #region implement method for IWindowPosition
        public void PositionWindow()
        {
            for (int i = 0; i < WindowFrames.BranchCount; i++)
            {
                for (int j = 0; j < WindowFrames.Branch(i).Count; j++)
                {
                    WallDepthMove(WindowFrames.Branch(i)[j]);
                    WallDepthMove(Glass.Branch(i)[j]);
                }
            }
            if (IsDepthFlip)
            {
                for (int i = 0; i < WindowFrames.BranchCount; i++)
                {
                    for (int j = 0; j < WindowFrames.Branch(i).Count; j++)
                    {
                        DepthFlip(WindowFrames.Branch(i)[j]);
                        DepthFlip(Glass.Branch(i)[j]);
                    }
                }
            }
            if(IsWidthFlip)
            {
                for (int i = 0; i < WindowFrames.BranchCount; i++)
                {
                    for (int j = 0; j < WindowFrames.Branch(i).Count; j++)
                    {
                        WidthFlip(WindowFrames.Branch(i)[j]);
                        WidthFlip(Glass.Branch(i)[j]);
                    }
                }
            }
        }
        protected virtual void WallDepthMove(Brep windowBrep)
        {
            windowBrep.Transform(Transform.Translation(0,(WallDepth-FrameDepth)/2,0));
        }

        protected virtual void DepthFlip(Brep windowBrep)
        {
            Plane flipPlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.ZAxis);
            windowBrep.Transform(Transform.Mirror(flipPlane));
        }

        protected virtual void WidthFlip(Brep windowBrep)
        {
            Plane flipPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            windowBrep.Transform(Transform.Mirror(flipPlane));
        }
        #endregion

        #region implement method for IWindowPRotation
        public void RotateWindow()
        {
            for (int i = 0; i < WindowFrames.BranchCount; i++)
            {
                for (int j = 0; j < WindowFrames.Branch(i).Count; j++)
                {
                    double angleRadians = RhinoMath.ToRadians(Angle);
                    WindowFrames.Branch(i)[j].Transform(Transform.Rotation(angleRadians, Vector3d.ZAxis, DatumPt));
                    Glass.Branch(i)[j].Transform(Transform.Rotation(angleRadians, Vector3d.ZAxis, DatumPt));
                }
            }
        }
        #endregion
    }
}
