using Grasshopper.Kernel;
using Rhino.Geometry;
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
            pManager.AddPointParameter("DatumPt", "DatumPt", "The datum point of the single straight flight stair", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddIntegerParameter("FloorCount", "FloorCount", "The count of floor", GH_ParamAccess.item, 1);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddIntegerParameter("StepCount", "StepCount", "The count of steps", GH_ParamAccess.item, 12);
            pManager.AddNumberParameter("StepWidth", "StepWidth", "The width of step", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("StepHeight", "StepHeight", "The height of step", GH_ParamAccess.item, 150);
            pManager.AddNumberParameter("FlightWidth", "FlightWidth", "The width of Flight", GH_ParamAccess.item, 1200);
            pManager.AddIntegerParameter("FlightType", "FlightType", "The type of flight. 0 = Entirety, 1 = Separateness", GH_ParamAccess.item, 0);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("StepDepth", "StepDepth", "The depth of step. When FlightType = 1, this parameter is necessary.", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("SideWidth", "SideWidth", "The width of both sides of stairs", GH_ParamAccess.item, 100);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("StairLandingDepth", "StairLandingDepth", "The depth of stair landing", GH_ParamAccess.item, 1200);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("StringerHeight", "StringerHeight", "The height of stringer", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("StringerDepth", "StringerDepth", "The depth of stringer", GH_ParamAccess.item, 120);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("HandrailHeight", "HandrailHeight", "The height of handrail", GH_ParamAccess.item, 900);
            pManager.AddNumberParameter("HandrailRadius", "HandrailRadius", "The radius of handrail", GH_ParamAccess.item, 50);
            pManager.AddIntegerParameter("HandrailCountPerFlight", "HandrailCountPerFlight", "The count of handrail per flight", GH_ParamAccess.item, 10);
            pManager.AddBooleanParameter("IsCircleHandrail", "IsCircleHandrail", "Whether to create circle handrails or not", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Flights", "Flights", "Flights", GH_ParamAccess.tree);
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
            double flightWidth = double.NaN;
            int flightTypeInt = int.MinValue;
            FlightType flightType = FlightType.Entirety;

            double stepDepth = double.NaN;
            double sideWidth = double.NaN;

            double stairLandingDepth = double.NaN;

            double stringerHeight = double.NaN;
            double stringerDepth = double.NaN;

            double handrailHeight = double.NaN;
            double handrailRadius = double.NaN;
            int handrailCountPerFlight = int.MinValue;
            bool isCircleHandrail = true;

            //Initialize arguments
            DA.GetData("DatumPt", ref datumPt);
            DA.GetData("FloorCount", ref floorCount);

            DA.GetData("StepCount", ref stepCount);
            DA.GetData("StepWidth", ref stepWidth);
            DA.GetData("StepHeight", ref stepHeight);
            DA.GetData("FlightWidth", ref flightWidth);
            DA.GetData("FlightType", ref flightTypeInt);
            if (((double)flightTypeInt) > 1 || ((double)flightTypeInt) < 0)
            {
                flightType = FlightType.Entirety;
            }
            else
            {
                flightType = (FlightType)flightTypeInt;
            }

            DA.GetData("StepDepth", ref stepDepth);
            DA.GetData("SideWidth", ref sideWidth);

            DA.GetData("StairLandingDepth", ref stairLandingDepth);

            DA.GetData("StringerHeight", ref stringerHeight);
            DA.GetData("StringerDepth", ref stringerDepth);

            DA.GetData("HandrailHeight", ref handrailHeight);
            DA.GetData("HandrailRadius", ref handrailRadius);
            DA.GetData("HandrailCountPerFlight", ref handrailCountPerFlight);
            DA.GetData("IsCircleHandrail", ref isCircleHandrail);

            Stair stair = new SingleStraightFlightStair
                (
                datumPt, floorCount,
                stepCount, stepWidth, stepHeight, flightWidth, flightType,
                stepDepth, sideWidth,
                stairLandingDepth,
                stringerHeight, stringerDepth,
                handrailHeight, handrailRadius, handrailCountPerFlight, isCircleHandrail
                );
            stair.CreateStair();

            DA.SetDataTree(0, stair.Flights);
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
