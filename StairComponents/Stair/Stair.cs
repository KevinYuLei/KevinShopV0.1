using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper;

namespace StairComponents.Stair
{
    public abstract class Stair : IStair
    {
        //DatumPt
        public Point3d DatumPt { get; set; }
        //Floor Count
        public int FloorCount { get; set; }

        //Flight Properties
        private int stepCount;

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
        public double FlightWidth { get; set; }
        public FlightType FlightType { get; set; }


        //Stair Landing Properties
        public double StairLandingDepth { get; set; }

        //Stringer Properties
        public double StringerHeight { get; set; }
        public double StringerDepth { get; set; }
        public double StringerLength { get; set; }

        //Handrail Properties
        public double HandrailHeight { get; set; }
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

        //Geometry Results
        private DataTree<Brep> flights = new DataTree<Brep>();
        private DataTree<Brep> stairLandings = new DataTree<Brep>();
        private DataTree<Brep> stringers = new DataTree<Brep>();
        private DataTree<Brep> handrails = new DataTree<Brep>();
        public DataTree<Brep> Flights { get => flights; set => flights = value; }

        public DataTree<Brep> StairLandings { get => stairLandings; set => stairLandings = value; }
        public DataTree<Brep> Stringers { get => stringers; set => stringers = value; }
        public DataTree<Brep> Handrails { get => handrails; set => handrails = value; }

        //Interface Method
        public void CreateStair()
        {
            CreateFlight();
            CreateStairLanding();
            CreateStringer();
            CreateHandrail();
        }

        protected abstract void CreateFlight();
        protected abstract void CreateStairLanding();
        protected abstract void CreateStringer();
        protected abstract void CreateHandrail();
    }

    public enum FlightType
    {
        Entirety,
        Separateness
    }

}
