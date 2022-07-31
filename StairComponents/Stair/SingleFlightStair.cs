using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper;


namespace StairComponents.Stair
{
    public class SingleLinearFlightStair : Stair, ILinearStair
    {
        #region Input Parameters Properties
        //DatumPt
        public Point3d DatumPt { get; set; }
        //Floor Count
        public int FloorCount { get; set; }

        //IStep Properties
        protected int stepCount;

        public int StepCount
        {
            get
            {
                if (stepCount <= 2)
                {
                    return 2;
                }
                else
                {
                    return stepCount;
                }
            }
            set => stepCount = value;
        }
        public double StepWidth { get; set; }
        public double StepHeight { get; set; }

        //IFlight Properties
        public double FlightLength { get; set; }
        public FlightType FlightType { get; set; }

        //ISeparatedStep Properties
        protected double stepDepth;
        protected double sideWidth;

        public double StepDepth
        {
            get
            {
                if (stepDepth > StepHeight)
                    return StepHeight;
                else
                    return stepDepth;
            }
            set => stepDepth = value;
        }
        public virtual double SideWidth
        {
            get
            {
                if (sideWidth > FlightLength / 2)
                    return FlightLength / 10;
                else
                    return sideWidth;
            }
            set => sideWidth = value;
        }

        //IStairLanding Properties
        protected double stairLandingLength;
        protected double stairLandingWidth;
        protected double stairLangingDepth;

        public virtual double StairLandingLength
        {
            get
            {
                if (stairLandingLength != FlightLength)
                    return FlightLength;
                else
                    return stairLandingLength;
            }
            set => stairLandingLength = value;
        }
        public virtual double StairLandingWidth
        {
            get
            {
                if(stairLandingWidth<FlightLength)
                {
                    return FlightLength;
                }
                else
                {
                    return stairLandingWidth;
                }
            }
            set => stairLandingWidth = value;
        }
        public virtual double StairLandingDepth
        {
            get
            {
                if (stairLangingDepth != StepHeight)
                    return StepHeight;
                else
                    return stairLangingDepth;
            }
            set => stairLangingDepth = value;
        }

        //IStringer Properties
        protected double stringerLength;

        public virtual double StringerLength
        {
            get
            {
                if (stringerLength != StairLandingLength)
                    return StairLandingLength;
                else
                    return stringerLength;
            }
            set => stringerLength = value;
        }
        public double StringerWidth { get; set; }
        public double StringerHeight { get; set; }

        //IHandrail Properties
        protected double handrailMargin;

        public double HandrailHeight { get; set; }
        public double HandrailMargin
        {
            get
            {
                if (handrailMargin>(StairLandingLength / 2 - HandrailRadius))
                {
                    return StairLandingLength / 2 - HandrailRadius;
                }
                else
                {
                    return handrailMargin;
                }
            }
            
            set => handrailMargin = value;
        }
        public HandrailType HandrailType { get; set; }

        //ISeparatedHandrail Properties
        public double HandrailRadius { get; set; }
        public int HandrailCountPerFlight { get; set; }
        public bool IsCircleHandrail { get; set; }

        //Result Properties
        //Number Results
        public double Height
        {
            get
            {
                return FloorCount * StepCount * StepHeight;
            }
        }
        //Geometry Results has been implemented in Stair Class
        #endregion

        public SingleLinearFlightStair
            (
            Point3d datumPt, int floorCount,
            int stepCount, double stepWidth, double stepHeight, 
            double flightLength, FlightType flightType,
             double stepDepth, double sideWidth,
            double stairLandingWidth,
            double stringerWidth, double stringerHeight,
            double handrailHeight, double handrailMargin,HandrailType handrailType,
            double handrailRaius, int handrailCountPerFlight, bool isCircleHandrail
            )
        {
            DatumPt = datumPt;
            FloorCount = floorCount;

            StepCount = stepCount;
            StepWidth = stepWidth;
            StepHeight = stepHeight;

            FlightLength = flightLength;
            FlightType = flightType;

            StepDepth = stepDepth;
            SideWidth = sideWidth;

            StairLandingLength = FlightLength;
            StairLandingWidth = stairLandingWidth;
            StairLandingDepth = StepHeight;

            StringerLength = FlightLength;
            StringerWidth = stringerWidth;
            StringerHeight = stringerHeight;

            HandrailHeight = handrailHeight;
            HandrailMargin = handrailMargin;
            HandrailType = handrailType;

            HandrailRadius = handrailRaius;
            HandrailCountPerFlight = handrailCountPerFlight;
            IsCircleHandrail = isCircleHandrail;
        }

        protected override void CreateFlights()
        {
            if (FlightType == FlightType.Entirety)
            {
                CreateEntireFlight();
            }
            else if (FlightType == FlightType.Separateness)
            {
                CreateSeparateFlight();
            }
        }

        #region New method ,for create flights,added from this class
        protected virtual void CreateEntireFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Brep flightBrep = Surface.CreateExtrusion(flightSideCurve, new Vector3d(FlightLength, 0, 0)).ToBrep().CapPlanarHoles(0.1);
            Flights.Add(flightBrep);

        }
        protected virtual void CreateSeparateFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Brep leftFlightBrep = Surface.CreateExtrusion(flightSideCurve, new Vector3d(SideWidth, 0, 0)).ToBrep().CapPlanarHoles(0.1);
            Brep rightFlightBrep = leftFlightBrep.DuplicateBrep();
            rightFlightBrep.Transform(Transform.Translation(FlightLength - SideWidth, 0, 0));
            Flights.Add(leftFlightBrep);
            Flights.Add(rightFlightBrep);

            CreateSeparatedSteps();
        }
        protected virtual Curve CreateFlightSideCurve()
        {
            List<Point3d> stepPts = new List<Point3d>();
            stepPts.Add(DatumPt);
            for (int i = 0; i < StepCount; i++)
            {
                Point3d pt1 = new Point3d(DatumPt);
                Point3d pt2 = new Point3d(DatumPt);
                pt1.Transform(Transform.Translation(0, StepWidth * i, StepHeight * (i + 1)));
                pt2.Transform(Transform.Translation(0, StepWidth * (i + 1), StepHeight * (i + 1)));
                stepPts.Add(pt1);
                stepPts.Add(pt2);
            }
            Point3d lastButTwoPt = new Point3d(DatumPt);
            Point3d lastPt = new Point3d(DatumPt);
            lastButTwoPt.Transform(Transform.Translation(0, StepWidth * StepCount, StepHeight * (StepCount - 1)));
            lastPt.Transform(Transform.Translation(0, StepWidth, 0));
            stepPts.Add(lastButTwoPt);
            stepPts.Add(lastPt);
            stepPts.Add(DatumPt);
            Curve flightSideCurve = new Polyline(stepPts).ToNurbsCurve();
            return flightSideCurve;
        }
        protected virtual void CreateSeparatedSteps()
        {
            for (int i = 0; i < StepCount; i++)
            {
                Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
                Curve stepBaseCurve = new Rectangle3d(basePlane, FlightLength - 2 * SideWidth, StepWidth).ToNurbsCurve();
                Brep stepBrep = Surface.CreateExtrusion(stepBaseCurve, new Vector3d(0, 0, -StepDepth)).ToBrep().CapPlanarHoles(0.1);
                stepBrep.Transform(Transform.Translation(SideWidth, 0, StepHeight));
                stepBrep.Transform(Transform.Translation(0, StepWidth * i, StepHeight * i));
                Flights.Add(stepBrep);
            }
        }
        #endregion

        protected override void CreateStairLandings()
        {
            Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
            basePlane.Transform(Transform.Translation(0, StepCount * StepWidth, StepCount * StepHeight));
            Curve baseCurve = new Rectangle3d(basePlane, StairLandingLength, StairLandingWidth).ToNurbsCurve();
            Brep stairLanding = Surface.CreateExtrusion(baseCurve, new Vector3d(0, 0, -1*StairLandingDepth)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            StairLandings.Add(stairLanding);
        }

        protected override void CreateStringers()
        {
            Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
            basePlane.Transform(Transform.Translation(0, StepCount * StepWidth, (StepCount - 1) * StepHeight));
            Curve baseCurve = new Rectangle3d(basePlane, StringerLength, StringerWidth).ToNurbsCurve();
            Brep stringer = Surface.CreateExtrusion(baseCurve, new Vector3d(0, 0, -1 * StringerHeight)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Stringers.Add(stringer);
        }

        protected override void CreateHandrails()
        {
            if(HandrailType==HandrailType.Entirety)
            {
                CreateEntireHandrail();
            }
            else if(HandrailType==HandrailType.Separateness)
            {
                CreateSeparatedHandrail();
            }
        }

        protected virtual void CreateEntireHandrail()
        {
            Curve flightSidePolyCurve = CreateStepPolyCurve();
            Point3d topPt1 = new Point3d(flightSidePolyCurve.PointAtStart);
            Point3d topPt2 = new Point3d(flightSidePolyCurve.PointAtStart);
            Point3d topPt3 = new Point3d(flightSidePolyCurve.PointAtEnd);
            Point3d topPt4 = new Point3d(flightSidePolyCurve.PointAtEnd);
            topPt1.Transform(Transform.Translation(0, 0, HandrailHeight));
            topPt2.Transform(Transform.Translation(0, StepWidth, HandrailHeight));
            topPt3.Transform(Transform.Translation(0, -1*StepWidth, HandrailHeight));
            topPt4.Transform(Transform.Translation(0, 0, HandrailHeight));

            Curve topCurve = new Polyline(new List<Point3d> { topPt1, topPt2, topPt3, topPt4 }).ToNurbsCurve();
            List<Curve> midSectionCurves = new List<Curve> { flightSidePolyCurve, new Line(flightSidePolyCurve.PointAtStart, topPt1).ToNurbsCurve(), new Line(flightSidePolyCurve.PointAtEnd, topPt4).ToNurbsCurve(), topCurve };
            Curve sectionCurve1 = Curve.JoinCurves(midSectionCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
            Curve sectionCurve2 = sectionCurve1.DuplicateCurve();
            sectionCurve1.Transform(Transform.Translation(-1 * HandrailRadius, 0, 0));
            sectionCurve2.Transform(Transform.Translation(HandrailRadius, 0, 0));

            Brep handrail1=Brep.CreateFromLoft(new List<Curve> { sectionCurve1, sectionCurve2 }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0].CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //将栏杆侧边对齐梯段侧边，将此时边距(HandrailMargin)视为0
            handrail1.Transform(Transform.Translation(HandrailRadius, 0, 0));

            handrail1.Transform(Transform.Translation(HandrailMargin, 0, 0));

            Plane mirrorPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            mirrorPlane.Transform(Transform.Translation(StairLandingLength / 2, 0, 0));
            Brep handrail2 = handrail1.DuplicateBrep();
            handrail2.Transform(Transform.Mirror(mirrorPlane));
            Handrails.Add(handrail1);
            Handrails.Add(handrail2);
        }
        protected virtual void CreateSeparatedHandrail()
        {

        }
        protected virtual Curve CreateStepPolyCurve()
        {
            List<Point3d> stepPts = new List<Point3d>();
            for (int i = 0; i < StepCount; i++)
            {
                Point3d pt1 = new Point3d(DatumPt);
                Point3d pt2 = new Point3d(DatumPt);
                pt1.Transform(Transform.Translation(0, StepWidth * i, StepHeight * (i + 1)));
                pt2.Transform(Transform.Translation(0, StepWidth * (i + 1), StepHeight * (i + 1)));
                stepPts.Add(pt1);
                stepPts.Add(pt2);
            }
            Curve flightSidePolyCurve = new Polyline(stepPts).ToNurbsCurve();
            return flightSidePolyCurve;
        }
    }
}
