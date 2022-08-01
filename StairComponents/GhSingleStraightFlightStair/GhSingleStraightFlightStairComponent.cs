using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using StairComponents.Stair;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace GhSingleStraightFlightStair
{
    public class GhSingleStraightFlightStairComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhSingleStraightFlightStairComponent()
          : base("SingleStraightFlightStair", "单跑直行楼梯",
              "Create a single straight flight stair",
              "KevinShop", "Stair")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("DatumPt", "基准点", "The datum point of the single straight flight stair", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddIntegerParameter("FloorCount", "层数", "The count of floor", GH_ParamAccess.item, 1);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddIntegerParameter("StepCount", "踏步数", "The count of steps", GH_ParamAccess.item, 12);
            pManager.AddNumberParameter("StepWidth", "踏步宽度", "The width of step", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("StepHeight", "踏步高度", "The height of step", GH_ParamAccess.item, 150);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("FlightLength", "梯段面宽", "The length of Flight", GH_ParamAccess.item, 1200);
            pManager.AddIntegerParameter("FlightType", "梯段类型", "The type of flight. 0 = Entirety, 1 = Separateness", GH_ParamAccess.item, 0);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("StepDepth", "踏步厚度", "The depth of step. When FlightType = 1, this parameter will be necessary and valid.", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("SideWidth", "边缘宽度", "The width of both sides of stairs. When FlightType = 1, this parameter will be necessary and valid.", GH_ParamAccess.item, 100);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("StairLandingWidth", "休息平台进深", "The width of stair landing. StairLandingWidth should be no shorter than the FlightLength.", GH_ParamAccess.item, 1200);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("StringerWidth", "梯梁厚度", "The width of stringer", GH_ParamAccess.item, 120);
            pManager.AddNumberParameter("StringerHeight", "梯梁高度", "The height of stringer", GH_ParamAccess.item, 300);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("HandrailHeight", "栏杆高度", "The height of handrail", GH_ParamAccess.item, 900);
            pManager.AddNumberParameter("HandrailMargin", "栏杆边距", "The margin of handrail", GH_ParamAccess.item, 50);
            pManager.AddIntegerParameter("HandrailType", "栏杆类型", "The type of handrail. 0 = Entirety, 1 = Separateness", GH_ParamAccess.item, 0);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("HandrailRadius", "栏杆半径/半边长", "The radius/half-length of handrail", GH_ParamAccess.item, 25);
            pManager.AddBooleanParameter("IsCircleHandrail", "是否圆管栏杆", "Whether to create circle handrails or not", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Flights", "梯段", "Flights", GH_ParamAccess.tree);
            pManager.AddBrepParameter("StairLandings", "休息平台", "StairLandings", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Stingers", "梯梁", "Stingers", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Handrails", "栏杆", "Handrails", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Create arguments
            Point3d datumPt = new Point3d();
            int floorCount = int.MinValue;

            int stepCount = int.MinValue;
            double stepWidth = double.NaN;
            double stepHeight = double.NaN;

            double flightLength = double.NaN;
            int flightTypeInt = int.MinValue;
            FlightType flightType = FlightType.Entirety;

            double stepDepth = double.NaN;
            double sideWidth = double.NaN;

            double stairLandingWidth = double.NaN;

            double stringerWidth = double.NaN;
            double stringerHeight = double.NaN;

            double handrailHeight = double.NaN;
            double handrailMargin = double.NaN;
            int handrailTypeInt = int.MinValue;
            HandrailType handrailType = HandrailType.Entirety;

            double handrailRadius = double.NaN;
            bool isCircleHandrail = true;

            //Initialize arguments
            DA.GetData("DatumPt", ref datumPt);
            DA.GetData("FloorCount", ref floorCount);

            DA.GetData("StepCount", ref stepCount);
            DA.GetData("StepWidth", ref stepWidth);
            DA.GetData("StepHeight", ref stepHeight);

            DA.GetData("FlightLength", ref flightLength);
            DA.GetData("FlightType", ref flightTypeInt);
            if (flightTypeInt > 1 || flightTypeInt < 0)
            {
                flightType = FlightType.Entirety;
            }
            else
            {
                flightType = (FlightType)flightTypeInt;
            }

            DA.GetData("StepDepth", ref stepDepth);
            DA.GetData("SideWidth", ref sideWidth);

            DA.GetData("StairLandingWidth", ref stairLandingWidth);

            DA.GetData("StringerWidth", ref stringerWidth);
            DA.GetData("StringerHeight", ref stringerHeight);

            DA.GetData("HandrailHeight", ref handrailHeight);
            DA.GetData("HandrailMargin", ref handrailMargin);
            DA.GetData("HandrailType", ref handrailTypeInt);
            if (handrailTypeInt > 1 || handrailTypeInt < 0)
            {
                handrailType = HandrailType.Entirety;
            }
            else
            {
                handrailType = (HandrailType)handrailTypeInt;
            }

            DA.GetData("HandrailRadius", ref handrailRadius);
            DA.GetData("IsCircleHandrail", ref isCircleHandrail);

            SingleLinearFlightStair singleLinearFlightStair = new SingleLinearFlightStair
                (
                datumPt, floorCount,
                stepCount, stepWidth, stepHeight, 
                flightLength, flightType,
                stepDepth, sideWidth,
                stairLandingWidth,
                 stringerWidth, stringerHeight,
                handrailHeight, handrailMargin,handrailType,
                handrailRadius, isCircleHandrail
                );
            singleLinearFlightStair.CreateStair();

            DA.SetDataTree(0, singleLinearFlightStair.Flights);
            DA.SetDataTree(1, singleLinearFlightStair.StairLandings);
            DA.SetDataTree(2, singleLinearFlightStair.Stringers);
            DA.SetDataTree(3, singleLinearFlightStair.Handrails);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8a0184b2-f83f-4cd0-8973-3617dc9d7928"); }
        }
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }
    }
}
