using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;

namespace StairComponents.Stair
{
    public abstract class Stair : IStair
    {
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
            CreateFlights();
            CreateStairLandings();
            CreateStringers();
            CreateHandrails();
        }

        protected abstract void CreateFlights();
        protected abstract void CreateStairLandings();
        protected abstract void CreateStringers();
        protected abstract void CreateHandrails();
    }

    internal interface ILinearStair:IStep,IFlight,ISeparatedStep,IStairLanding,IStringer,IHandrail,ISeparatedHandrail
    {

    }

    internal interface IArcStair: IStep, IFlight, ISeparatedStep, IStairLanding, IStringer, IHandrail,ISeparatedHandrail
    {
        double FlightAngle { get; set; }
        double ExternalRadius { get; set; }
        double InternalRadius { get; set; }
    }
}
