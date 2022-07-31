using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper;

namespace StairComponents.Stair
{
    public enum FlightType
    {
        Entirety,
        Separateness
    }

    public enum HandrailType
    {
        Entirety,
        Separateness
    }

    internal interface IStair
    {
        DataTree<Brep> Flights { get; set; }
        DataTree<Brep> StairLandings { get; set; }
        DataTree<Brep> Stringers { get; set; }
        DataTree<Brep> Handrails { get; set; }
        void CreateStair();
    }

    internal interface IStep
    {
        int StepCount { get; set; }
        double StepWidth { get; set; }
        double StepHeight { get; set; }
    }

    internal interface IFlight
    {
        double FlightLength { get; set; }
        FlightType FlightType { get; set; }
    }

    internal interface ISeparatedStep
    {
        double StepDepth { get; set; }
        double SideWidth { get; set; }
    }

    internal interface IStairLanding
    {
        double StairLandingLength { get; set; }
        double StairLandingWidth { get; set; }
        double StairLandingDepth { get; set; }
    }

    internal interface IDoubleParallelFlights
    {
        double StairShaftWidth { get; set; }
    }

    internal interface IStringer
    {
        double StringerLength { get; set; }
        double StringerWidth { get; set; }
        double StringerHeight { get; set; }
    }

    internal interface IHandrail
    {
        double HandrailHeight { get; set; }
        double HandrailMargin { get; set; }
        HandrailType HandrailType { get; set; }
    }

    internal interface ISeparatedHandrail
    {
        double HandrailRadius { get; set; }
        int HandrailCountPerFlight { get; set; }
        bool IsCircleHandrail { get; set; }
    }
}
