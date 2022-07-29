using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StairComponents.Stair
{
    public interface IStair
    {
        void CreateStair();
    }

    public interface ISeparatedStair
    {
        double StepDepth { get; set; }
        double SideWidth { get; set; }
    }

    public interface IDoubleRunStair
    {
        double StairLandingWidth { get; set; }
        double StairShaftWidth { get; set; }
    }
}
