using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StairComponents.Stair
{
    //多跑直行楼梯
    internal class PluralLinearFlightStair : SingleLinearFlightStair
    {
        #region New Input Properties
        protected int floorCount;
        public int FloorCount
        {
            get
            {
                if(floorCount<2)
                { 
                    return 2;
                }
                else
                {
                    return floorCount;
                }
            }
            set => floorCount = value;
        }
        #endregion

        #region Override Output Properties
        public override double Height
        {
            get
            {
                return StepCount * StepHeight * FloorCount;
            }
        }
        #endregion

        #region New Output Properties
        public virtual double FloorHeight { get => StepCount * StepHeight; }
        #endregion

        //构造函数
        public PluralLinearFlightStair
            (
            Point3d datumPt, 
            int stepCount, double stepWidth, double stepHeight,
            double flightLength, FlightType flightType,
            double stepDepth, double sideWidth,
            double stairLandingWidth,
            double stringerWidth, double stringerHeight,
            double handrailHeight, double handrailMargin, HandrailType handrailType,
            double handrailRaius, bool isCircleHandrail, 
            //New Input Properties
            int floorCount) : base
            (
                datumPt,
                stepCount, stepWidth, stepHeight,
                flightLength, flightType,
                stepDepth, sideWidth,
                stairLandingWidth,
                stringerWidth, stringerHeight,
                handrailHeight, handrailMargin, handrailType,
                handrailRaius, isCircleHandrail
                )
        {
            FloorCount = floorCount;
        }

        #region New methods for duplicate Result Breps according to FloorCount
        //这些方法仅适用于 多跑直行楼梯

        //重载1:复制 单个物件
        protected virtual void DuplicateResultBrepByFloorCount(Brep brepToDuplicate, DataTree<Brep> resultBrepsTree)
        {
            for (int i = 0; i < FloorCount-1; i++)
            {
                Brep nextBrep = brepToDuplicate.DuplicateBrep();

                double yDistance = (StepCount * StepWidth + stairLandingWidth) * (i + 1);
                double zDistance = StepCount * StepHeight * (i + 1);

                nextBrep.Transform(Transform.Translation(0, yDistance, zDistance));

                resultBrepsTree.Add(nextBrep, new GH_Path(i + 1));
            }
        }

        //重载2:复制 单个物件,数据结构含物件类型
        protected virtual void DuplicateResultBrepByFloorCount(Brep brepToDuplicate, DataTree<Brep> resultBrepsTree, int pathForResultBrep)
        {
            for (int i = 0; i < FloorCount - 1; i++)
            {
                Brep nextBrep = brepToDuplicate.DuplicateBrep();

                double yDistance = (StepCount * StepWidth + stairLandingWidth) * (i + 1);
                double zDistance = StepCount * StepHeight * (i + 1);

                nextBrep.Transform(Transform.Translation(0, yDistance, zDistance));

                resultBrepsTree.Add(nextBrep, new GH_Path(i + 1, pathForResultBrep));
            }
        }

        //重载3:复制 一个列表的物件
        protected virtual void DuplicateResultBrepByFloorCount(List<Brep> brepsToDuplicate, DataTree<Brep> resultBrepsTree)
        {
            for (int i = 0; i < FloorCount-1; i++)
            {
                double yDistance = (StepCount * StepWidth + stairLandingWidth) * (i + 1);
                double zDistance = StepCount * StepHeight * (i + 1);
                List<Brep> nextBreps = new List<Brep>();
                for (int j = 0; j < brepsToDuplicate.Count; j++)
                {
                    Brep nextBrep = brepsToDuplicate[j].DuplicateBrep();
                    nextBrep.Transform(Transform.Translation(0, yDistance, zDistance));
                    nextBreps.Add(nextBrep);
                }
                resultBrepsTree.AddRange(nextBreps, new GH_Path(i + 1));
            }
        }

        //重载4:复制 一个列表的物件,数据结构含物件类型
        protected virtual void DuplicateResultBrepByFloorCount(List<Brep> brepsToDuplicate, DataTree<Brep> resultBrepsTree, int pathForResultBrep)
        {
            for (int i = 0; i < FloorCount - 1; i++)
            {
                double yDistance = (StepCount * StepWidth + stairLandingWidth) * (i + 1);
                double zDistance = StepCount * StepHeight * (i + 1);
                List<Brep> nextBreps = new List<Brep>();
                for (int j = 0; j < brepsToDuplicate.Count; j++)
                {
                    Brep nextBrep = brepsToDuplicate[j].DuplicateBrep();
                    nextBrep.Transform(Transform.Translation(0, yDistance, zDistance));
                    nextBreps.Add(nextBrep);
                }
                resultBrepsTree.AddRange(nextBreps, new GH_Path(i + 1, pathForResultBrep));
            }
        }
        #endregion

        #region Override methods for creating different types of Flights
        //重写 创建整体式梯段
        protected override void CreateEntireFlight()
        {
            base.CreateEntireFlight();

            //将生成的 首层的 整体式梯段 复制至其他层
            DuplicateResultBrepByFloorCount(Flights.Branch(0)[0], Flights);
        }

        //重写 创建片状式梯段
        protected override void CreateSeparateFlight()
        {
            base.CreateSeparateFlight();

            //将生成的 首层的 片状式梯段 复制至其他层
            DuplicateResultBrepByFloorCount(Flights.Branch(0, 0)[0], Flights, 0);
            DuplicateResultBrepByFloorCount(Flights.Branch(0, 0)[1], Flights, 0);
            DuplicateResultBrepByFloorCount(Flights.Branch(0, 1), Flights, 1);
        }

        //重写 创建斜边的整体式梯段
        protected override void CreateObliqueEntireFlight()
        {
            base.CreateObliqueEntireFlight();

            //将生成的 首层的 斜边的整体式梯段 复制至其他层
            DuplicateResultBrepByFloorCount(Flights.Branch(0)[0], Flights);
            DuplicateResultBrepByFloorCount(Flights.Branch(0)[1], Flights);
            DuplicateResultBrepByFloorCount(Flights.Branch(0)[2], Flights);
        }

        //重写 创建斜边的片状式梯段
        protected override void CreateObliqueSeparateFlight()
        {
            base.CreateObliqueSeparateFlight();

            //将生成的 首层的 斜边的片状式梯段 复制至其他层
            DuplicateResultBrepByFloorCount(Flights.Branch(0, 0)[0], Flights, 0);
            DuplicateResultBrepByFloorCount(Flights.Branch(0, 0)[1], Flights, 0);
            DuplicateResultBrepByFloorCount(Flights.Branch(0, 1), Flights, 1);
        }
        #endregion

        #region Override method for creating StairLandings
        //重写 创建休息平台
        protected override void CreateStairLandings()
        {
            base.CreateStairLandings();
            //补中间所有休息平台的三角连接体
            Point3d pt1 = DatumPt;
            Point3d pt2 = DatumPt;
            Point3d pt3 = DatumPt;
            pt2.Transform(Transform.Translation(0, StepWidth, 0));
            pt3.Transform(Transform.Translation(0, 0, -StepHeight));
            Curve hatchingOfTriangularBrep = new Polyline(new List<Point3d> { pt1, pt2, pt3, pt1 }).ToNurbsCurve();
            Brep triangularBrep = Surface.CreateExtrusion(hatchingOfTriangularBrep, new Vector3d(FlightLength, 0, 0)).ToBrep().CapPlanarHoles(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            triangularBrep.Transform(Transform.Translation(0, StepCount*StepWidth+StairLandingWidth, StepCount*StepHeight));
            StairLandings.Add(triangularBrep, new GH_Path(0));

            //将生成的 首层的 休息平台 复制至其他层
            DuplicateResultBrepByFloorCount(StairLandings.Branch(0)[0], StairLandings);
            DuplicateResultBrepByFloorCount(StairLandings.Branch(0)[1], StairLandings);
            //最顶层不需要补三角连接体
            StairLandings.Branch(FloorCount - 1).RemoveAt(1);
        }
        #endregion

        #region Override method for creating Stringers
        //重写 创建梯梁
        protected override void CreateStringers()
        {
            base.CreateStringers();

            //将生成的 首层的 梯梁 复制至其他层
            DuplicateResultBrepByFloorCount(Stringers.Branch(0)[0], Stringers);
        }
        #endregion

        #region Override methods for creating different types of Handrails

        #endregion
    }
}
