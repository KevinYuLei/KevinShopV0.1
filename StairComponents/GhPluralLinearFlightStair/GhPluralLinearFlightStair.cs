using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using StairComponents.Stair;

namespace StairComponents.GhPluralLinearFlightStair
{
    public class GhPluralLinearFlightStair : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhPluralLinearFlightStair class.
        /// </summary>
        public GhPluralLinearFlightStair()
          : base(
                "PluralLinearFlightStair", 
                "多跑直行楼梯",
                "Create a plural linear flights stair",
                "KevinShop", 
                "Stair")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("DatumPt", "基准点", "The datum point of the single straight flight stair", GH_ParamAccess.item, Point3d.Origin);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddIntegerParameter("StepCount", "踏步数", "The count of steps", GH_ParamAccess.item, 12);
            pManager.AddNumberParameter("StepWidth", "踏步宽度", "The width of step", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("StepHeight", "踏步高度", "The height of step", GH_ParamAccess.item, 150);

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddNumberParameter("FlightLength", "梯段面宽", "The length of Flight", GH_ParamAccess.item, 1200);
            pManager.AddIntegerParameter("FlightType", "梯段类型", "The type of flight. 0 = Entirety, 1 = Separateness, 2 = ObliqueEntirety, 3 = ObliqueSeparateness, ", GH_ParamAccess.item, 0);

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

            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item, "Split row");

            pManager.AddIntegerParameter("FloorCount", "层数/跑数", "The count of floors of the stair, and this count should be not less than 2", GH_ParamAccess.item, 2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //index = 0
            pManager.AddBrepParameter("Flights", "梯段", "Flights", GH_ParamAccess.tree);
            //index = 1
            pManager.AddBrepParameter("StairLandings", "休息平台", "StairLandings", GH_ParamAccess.tree);
            //index = 2
            pManager.AddBrepParameter("Stingers", "梯梁", "Stingers", GH_ParamAccess.tree);
            //index = 3
            pManager.AddBrepParameter("Handrails", "栏杆", "Handrails", GH_ParamAccess.tree);
            //index = 4
            pManager.AddTextParameter("-----------", "-----------", "Split row", GH_ParamAccess.item);
            //index = 5
            pManager.AddNumberParameter("TotalHeight", "总高", "Total hight", GH_ParamAccess.item);
            //index = 6
            pManager.AddNumberParameter("FloorHeight", "层高", "Floor height", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Create arguments
            Point3d datumPt = new Point3d();

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
            //new input
            int floorCount = int.MinValue;

            //Initialize arguments
            DA.GetData("DatumPt", ref datumPt);

            DA.GetData("StepCount", ref stepCount);
            DA.GetData("StepWidth", ref stepWidth);
            DA.GetData("StepHeight", ref stepHeight);

            DA.GetData("FlightLength", ref flightLength);
            DA.GetData("FlightType", ref flightTypeInt);
            //将int类型转换为 梯段 对应的枚举类型
            int countOfFlightType = 4;
            flightTypeInt = flightTypeInt % countOfFlightType;
            flightType = (FlightType)flightTypeInt;

            DA.GetData("StepDepth", ref stepDepth);
            DA.GetData("SideWidth", ref sideWidth);

            DA.GetData("StairLandingWidth", ref stairLandingWidth);

            DA.GetData("StringerWidth", ref stringerWidth);
            DA.GetData("StringerHeight", ref stringerHeight);

            DA.GetData("HandrailHeight", ref handrailHeight);
            DA.GetData("HandrailMargin", ref handrailMargin);
            DA.GetData("HandrailType", ref handrailTypeInt);
            //将int类型转换为 栏杆 对应的枚举类型
            int countOfHandrailType = 2;
            handrailTypeInt = handrailTypeInt % countOfHandrailType;
            handrailType = (HandrailType)handrailTypeInt;

            DA.GetData("HandrailRadius", ref handrailRadius);
            DA.GetData("IsCircleHandrail", ref isCircleHandrail);

            DA.GetData("FloorCount", ref floorCount);

            PluralLinearFlightStair pluralLinearFlightStair = new PluralLinearFlightStair
                (
                datumPt,
                stepCount, stepWidth, stepHeight,
                flightLength, flightType,
                stepDepth, sideWidth,
                stairLandingWidth,
                stringerWidth, stringerHeight,
                handrailHeight, handrailMargin, handrailType,
                handrailRadius, isCircleHandrail,
                floorCount
                );
            pluralLinearFlightStair.CreateStair();

            DA.SetDataTree(0, pluralLinearFlightStair.Flights);
            DA.SetDataTree(1, pluralLinearFlightStair.StairLandings);
            DA.SetDataTree(2, pluralLinearFlightStair.Stringers);
            DA.SetDataTree(3, pluralLinearFlightStair.Handrails);
            DA.SetData("TotalHeight", pluralLinearFlightStair.Height);
            DA.SetData("FloorHeight", pluralLinearFlightStair.FloorHeight);
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
            get { return new Guid("6BB38F1B-8FA0-4F5D-A7C6-6744D0D5DEBF"); }
        }

    }
}