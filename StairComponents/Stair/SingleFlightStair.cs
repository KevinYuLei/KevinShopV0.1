using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace StairComponents.Stair
{

    public abstract class SingleFlightStair : Stair, ISeparatedStair
    {
        private double sideWidth;
        private double stepDepth;

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
        public double SideWidth
        {
            get
            {
                if (sideWidth > FlightWidth / 2)
                    return FlightWidth/10;
                else
                    return sideWidth;
            }
            set => sideWidth = value;
        }
    }

    public class SingleStraightFlightStair : SingleFlightStair
    {
        public SingleStraightFlightStair
            (
            Point3d datumPt, int floorCount,
            int stepCount, double stepWidth, double stepHeight, double flightWidth, FlightType flightType,
             double stepDepth,double sideWidth,
            double stairLandingDepth,
            double stringerHeight, double stringerDepth,
            double handrailHeight, double handrailRaius, int handrailCountPerFlight, bool isCircleHandrail
            )
        {
            DatumPt = datumPt;
            FloorCount = floorCount;

            StepCount = stepCount;
            StepWidth = stepWidth;
            StepHeight = stepHeight;
            FlightWidth = flightWidth;
            FlightType = flightType;

            StepDepth = stepDepth;
            SideWidth = sideWidth;

            StairLandingDepth = stairLandingDepth;

            StringerHeight = stringerHeight;
            StringerDepth = stringerDepth;
            StringerLength = FlightWidth;

            HandrailHeight = handrailHeight;
            HandrailRadius = handrailRaius;
            HandrailCountPerFlight = handrailCountPerFlight;
            IsCircleHandrail = isCircleHandrail;
        }

        protected override void CreateFlight()
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
        private void CreateEntireFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Brep flightBrep = Surface.CreateExtrusion(flightSideCurve, new Vector3d(FlightWidth, 0, 0)).ToBrep().CapPlanarHoles(0.1);
            Flights.Add(flightBrep);

        }
        private void CreateSeparateFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Brep leftFlightBrep = Surface.CreateExtrusion(flightSideCurve, new Vector3d(SideWidth, 0, 0)).ToBrep().CapPlanarHoles(0.1);
            Brep rightFlightBrep = leftFlightBrep.DuplicateBrep();
            rightFlightBrep.Transform(Transform.Translation(FlightWidth - SideWidth, 0, 0));
            Flights.Add(leftFlightBrep);
            Flights.Add(rightFlightBrep);

            CreateSeparatedSteps();
        }
        private Curve CreateFlightSideCurve()
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
        private void CreateSeparatedSteps()
        {
            for (int i = 0; i < StepCount; i++)
            {
                Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
                Curve stepBaseCurve = new Rectangle3d(basePlane, FlightWidth - 2 * SideWidth, StepWidth).ToNurbsCurve();
                Brep stepBrep = Surface.CreateExtrusion(stepBaseCurve, new Vector3d(0, 0, -StepDepth)).ToBrep().CapPlanarHoles(0.1);
                stepBrep.Transform(Transform.Translation(SideWidth, 0, StepHeight));
                stepBrep.Transform(Transform.Translation(0, StepWidth * i, StepHeight * i));
                Flights.Add(stepBrep);
            }
        }

        protected override void CreateStairLanding()
        {

        }

        protected override void CreateStringer()
        {

        }

        protected override void CreateHandrail()
        {

        }
    }
}
