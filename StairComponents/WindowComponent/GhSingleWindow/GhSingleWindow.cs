using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using WindowComponent.Window;

namespace WindowComponent.GhSingleWindow
{
    public class GhSingleWindow : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhSingleWindow class.
        /// </summary>
        public GhSingleWindow()
          : base(
                "SingleWindow", 
                "单个窗户",
                "Create a single window",
                "KevinShop", 
                "Window")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("DatumPt","基准点","The datum point, which is close to the world origin point, of the single window", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddNumberParameter("WindowWidth", "窗户宽度", "The width of the single window", GH_ParamAccess.item, 600);
            pManager.AddNumberParameter("WindowHeight", "窗户高度", "The height of the single window", GH_ParamAccess.item, 1200);
            pManager.AddNumberParameter("FrameDepth", "窗框厚度", "The depth of the window frame", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("FrameMargin", "窗框边距", "The margin of the window frame", GH_ParamAccess.item, 60);
            pManager.AddNumberParameter("GlassDepth", "玻璃厚度", "The depth of glass, which must be less than FrameDepth", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //index = 0
            pManager.AddBrepParameter("WindowFrames", "窗框", "WindowFrames", GH_ParamAccess.tree);
            //index = 1
            pManager.AddBrepParameter("Glass", "玻璃", "Glass", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d datumPt = new Point3d();
            double windowWidth = double.NaN;
            double windowHeight = double.NaN;
            double frameDepth = double.NaN;
            double frameMargin = double.NaN;
            double glassDepth = double.NaN;

            DA.GetData("DatumPt", ref datumPt);
            DA.GetData("WindowWidth", ref windowWidth);
            DA.GetData("WindowHeight", ref windowHeight);
            DA.GetData("FrameDepth", ref frameDepth);
            DA.GetData("FrameMargin", ref frameMargin);
            DA.GetData("GlassDepth", ref glassDepth);

            SingleWindow singleWindow = new SingleWindow
                (
                datumPt, windowWidth, windowHeight,
                frameDepth, frameMargin, glassDepth);
            singleWindow.CreateWindow();

            DA.SetDataTree(0, singleWindow.WindowFrames);
            DA.SetDataTree(1, singleWindow.Glass);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("927A7605-DD14-428D-8AA0-15030181CB7C"); }
        }
    }
}