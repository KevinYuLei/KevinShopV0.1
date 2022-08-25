using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace StairComponents.Stair
{
    public class SingleLinearFlightStair : Stair, ILinearStair
    {
        #region Input Parameters Properties
        //DatumPt
        public Point3d DatumPt { get; set; }

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

        public double ArmrestDepth { get => 30; }
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
        public bool IsCircleHandrail { get; set; }
        #endregion

        #region Result Parameters Properties
        //Number Results
        public double Height
        {
            get
            {
                return StepCount * StepHeight;
            }
        }
        //Geometry Results has been implemented in Stair Class
        #endregion

        public SingleLinearFlightStair
            (
            Point3d datumPt,
            int stepCount, double stepWidth, double stepHeight, 
            double flightLength, FlightType flightType,
             double stepDepth, double sideWidth,
            double stairLandingWidth,
            double stringerWidth, double stringerHeight,
            double handrailHeight, double handrailMargin,HandrailType handrailType,
            double handrailRaius, bool isCircleHandrail
            )
        {
            DatumPt = datumPt;

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
            IsCircleHandrail = isCircleHandrail;
        }

        //创建梯段
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
            else if (FlightType==FlightType.ObliqueEntirety)
            {
                CreateObliqueEntireFlight();
            }
            else if(FlightType==FlightType.ObliqueSeparateness)
            {
                CreateObliqueSeparateFlight();
            }
        }

        #region New method for creating flights
        //创建整体式梯段
        protected virtual void CreateEntireFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Brep flightBrep = Surface.CreateExtrusion(flightSideCurve, new Vector3d(FlightLength, 0, 0)).ToBrep().CapPlanarHoles(0.1);
            Flights.Add(flightBrep, new GH_Path(0));
        }
        //创建片状式梯段
        protected virtual void CreateSeparateFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Brep flightBrep1 = Surface.CreateExtrusion(flightSideCurve, new Vector3d(SideWidth, 0, 0)).ToBrep().CapPlanarHoles(0.1);
            Brep flightBrep2 = flightBrep1.DuplicateBrep();
            flightBrep2.Transform(Transform.Translation(FlightLength - SideWidth, 0, 0));

            List<Brep> separatedSteps = CreateSeparatedSteps();

            Flights.Add(flightBrep1,new GH_Path(0));
            Flights.Add(flightBrep2,new GH_Path(0));
            Flights.AddRange(separatedSteps, new GH_Path(1));
        }
        //创建斜边的整体式梯段
        protected virtual void CreateObliqueEntireFlight()
        {
            Curve flightSideCurve = CreateFlightSideCurve();
            Curve obliqueFlightSideCurve = CreateObliqueFlightSideCurve();
            Plane mirrorPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            mirrorPlane.Transform(Transform.Translation(FlightLength / 2, 0, 0));
            double lengthOfStep = FlightLength - SideWidth * 2;

            Brep obliqueBrep1 = Surface.CreateExtrusion(obliqueFlightSideCurve, new Vector3d(SideWidth, 0, 0)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Brep obliqueBrep2 = obliqueBrep1.DuplicateBrep();
            obliqueBrep2.Transform(Transform.Mirror(mirrorPlane));

            Brep stepFlightBrep = Surface.CreateExtrusion(flightSideCurve, new Vector3d(lengthOfStep, 0, 0)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            stepFlightBrep.Transform(Transform.Translation(SideWidth, 0, 0));

            Flights.Add(stepFlightBrep, new GH_Path(0));
            Flights.Add(obliqueBrep1, new GH_Path(0));
            Flights.Add(obliqueBrep2, new GH_Path(0));
        }
        //创建斜边的片状式梯段
        protected virtual void CreateObliqueSeparateFlight()
        {
            Curve obliqueFlightSideCurve=CreateObliqueFlightSideCurve();
            Plane mirrorPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            mirrorPlane.Transform(Transform.Translation(FlightLength / 2, 0, 0));

            Brep obliqueBrep1 = Surface.CreateExtrusion(obliqueFlightSideCurve, new Vector3d(SideWidth, 0, 0)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Brep obliqueBrep2 = obliqueBrep1.DuplicateBrep();
            obliqueBrep2.Transform(Transform.Mirror(mirrorPlane));

            List<Brep> separatedSteps = CreateSeparatedSteps();

            Flights.Add(obliqueBrep1, new GH_Path(0));
            Flights.Add(obliqueBrep2, new GH_Path(0));
            Flights.AddRange(separatedSteps, new GH_Path(1));
        }

        //创建梯段的辅助方法

        //创建阶梯式梯段侧边截面线
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

        //创建斜边式梯段侧边截面线
        protected virtual Curve CreateObliqueFlightSideCurve()
        {
            Curve obliqueFlightSideCurve;
            List<Vector3d> positions = new List<Vector3d>()
            {
                new Vector3d(0, 0, 0),
                new Vector3d(0, 0, StepHeight),
                new Vector3d(0,StepWidth*(StepCount-1), StepHeight*StepCount),
                new Vector3d(0, StepWidth*StepCount, StepHeight*StepCount),
                new Vector3d(0, StepWidth*StepCount, StepHeight*(StepCount-1)),
                new Vector3d(0,StepWidth,0),
                new Vector3d(0,0,0),
            };

            List<Point3d> pts = new List<Point3d>();
            for (int i = 0; i < positions.Count; i++)
            {
                Point3d pt = new Point3d(DatumPt);
                pt.Transform(Transform.Translation(positions[i]));
                pts.Add(pt);
            }
            obliqueFlightSideCurve = new Polyline(pts).ToPolylineCurve();
            return obliqueFlightSideCurve;
        }

        //创建片状式台阶
        //重构了方法的返回值
        protected virtual List<Brep> CreateSeparatedSteps()
        {
            //一般情况下，lengthOfStep = FlightLength - 2 * SideWidth
            List<Brep> separatedSteps= new List<Brep>();

            for (int i = 0; i < StepCount; i++)
            {
                Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
                Curve stepBaseCurve = new Rectangle3d(basePlane, FlightLength - 2 * SideWidth, StepWidth).ToNurbsCurve();
                Brep stepBrep = Surface.CreateExtrusion(stepBaseCurve, new Vector3d(0, 0, -StepDepth)).ToBrep().CapPlanarHoles(0.1);
                stepBrep.Transform(Transform.Translation(SideWidth, 0, StepHeight));
                stepBrep.Transform(Transform.Translation(0, StepWidth * i, StepHeight * i));
                separatedSteps.Add(stepBrep);
            }
            return separatedSteps;
        }
        #endregion

        //创建休息平台
        protected override void CreateStairLandings()
        {
            Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
            basePlane.Transform(Transform.Translation(0, StepCount * StepWidth, StepCount * StepHeight));
            Curve baseCurve = new Rectangle3d(basePlane, StairLandingLength, StairLandingWidth).ToNurbsCurve();
            Brep stairLanding = Surface.CreateExtrusion(baseCurve, new Vector3d(0, 0, -1*StairLandingDepth)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            StairLandings.Add(stairLanding,new GH_Path(0));
        }

        //创建梯梁
        protected override void CreateStringers()
        {
            Plane basePlane = new Plane(DatumPt, Vector3d.XAxis, Vector3d.YAxis);
            basePlane.Transform(Transform.Translation(0, StepCount * StepWidth, (StepCount - 1) * StepHeight));
            Curve baseCurve = new Rectangle3d(basePlane, StringerLength, StringerWidth).ToNurbsCurve();
            Brep stringer = Surface.CreateExtrusion(baseCurve, new Vector3d(0, 0, -1 * StringerHeight)).ToBrep().CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Stringers.Add(stringer,new GH_Path(0));
        }

        //创建栏杆
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
        #region New method for creating Handrails
        //创建整体式栏杆（栏板）
        protected virtual void CreateEntireHandrail()
        {
            //创建顶部扶手
            CreateArmrest();

            Curve bottomCurve = CreateStepPolyCurve();
            Curve topCurve = CreateHandrailTopCurve();
            bottomCurve = bottomCurve.Trim(CurveEnd.Both, StepWidth / 2);
            topCurve = topCurve.Trim(CurveEnd.Both, StepWidth / 2);

            if (FlightType == FlightType.ObliqueEntirety || FlightType == FlightType.ObliqueSeparateness)
            {
                Point3d pt1 = bottomCurve.PointAtStart;
                Point3d pt2 = bottomCurve.PointAtEnd;
                Point3d pt3 = bottomCurve.PointAtEnd;

                pt1.Transform(Transform.Translation(0, 0, 0.5*StepHeight));
                pt2.Transform(Transform.Translation(0, -1 * 0.5 * StepWidth, 0));
                bottomCurve = new Polyline(new List<Point3d> { pt1, pt2, pt3 }).ToNurbsCurve();
            }

            Point3d topPt1 = topCurve.PointAtStart;
            Point3d topPt2 = topCurve.PointAtEnd;
            Point3d bottomPt1 = bottomCurve.PointAtStart;
            Point3d bottomPt2 = bottomCurve.PointAtEnd;

            List<Curve> midSectionCurves = new List<Curve>
            {
                bottomCurve,
                new Line(topPt1,bottomPt1).ToNurbsCurve(),
                new Line(topPt2,bottomPt2).ToNurbsCurve(),
                topCurve };
            Curve sectionCurve1 = Curve.JoinCurves(midSectionCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
            Curve sectionCurve2 = sectionCurve1.DuplicateCurve();
            sectionCurve1.Transform(Transform.Translation(-1 * HandrailRadius, 0, 0));
            sectionCurve2.Transform(Transform.Translation(HandrailRadius, 0, 0));

            Brep handrail1=Brep.CreateFromLoft(new List<Curve> { sectionCurve1, sectionCurve2 }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0].CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //将栏杆侧边对齐梯段侧边，将此时边距(HandrailMargin)视为0
            handrail1.Transform(Transform.Translation(HandrailRadius, 0, 0));
            //根据边距确定栏杆最终位置
            handrail1.Transform(Transform.Translation(HandrailMargin, 0, 0));

            //通过镜像创建右侧栏杆
            Plane mirrorPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            mirrorPlane.Transform(Transform.Translation(StairLandingLength / 2, 0, 0));
            Brep handrail2 = handrail1.DuplicateBrep();
            handrail2.Transform(Transform.Mirror(mirrorPlane));
            //{0;0}中第一个数区分左右：若为0，则为左；若为1，则为右
            //{0;0}中第二个数区分种类：若为0，则为扶手、整体式栏板、水平构件；若为1，则为栏杆(垂直构件)
            Handrails.Add(handrail1,new GH_Path(0,0));
            Handrails.Add(handrail2,new GH_Path(1,0));
        }

        //创建杆件式栏杆
        protected virtual void CreateSeparatedHandrail()
        {
            //创建镜像平面，以便于后续实体通过镜像创建另一侧的实体
            Plane mirrorPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            mirrorPlane.Transform(Transform.Translation(StairLandingLength / 2, 0, 0));

            //创建各踏步沿宽度方向的线，以便于后续根据序号获得中点创建主杆
            List<Curve> stepWidthCurves = new List<Curve>();
            for (int i = 0; i < StepCount; i++)
            {
                Point3d pt1 = new Point3d(DatumPt);
                pt1.Transform(Transform.Translation(0, 0, StepHeight));
                pt1.Transform(Transform.Translation(0, StepWidth * i, StepHeight * i));
                Point3d pt2 = new Point3d(pt1);
                pt2.Transform(Transform.Translation(0, StepWidth, 0));
                Curve stepWidthCurve = new Line(pt1, pt2).ToNurbsCurve();
                stepWidthCurves.Add(stepWidthCurve);
            }
            //创建序号列表
            double mainPartInterval = 800;

            List<int> indices = new List<int>();
            int indexInterval = (int)Math.Round(mainPartInterval / StepWidth);
            indices.Add(0);
            for (int i = indexInterval; i < StepCount-1; i += indexInterval)
            {
                indices.Add(i);
            }
            indices.Add(StepCount - 1);

            //水平杆件

            //创建顶部扶手
            CreateArmrest();

            //创建底部、中部分 水平 隔栏杆曲线及其实体杆件
            double subPipeRadius = HandrailRadius / 2;
            double radiusOfHorizontalPipe = subPipeRadius + 10;
            double topInterval = 100;
            double bottomInterval = 50 + radiusOfHorizontalPipe;

            Curve handrailTopCurve = CreateHandrailTopCurve();
            Curve handrailMidCurve = handrailTopCurve.DuplicateCurve();
            Curve handrailBottomCurve = handrailTopCurve.DuplicateCurve();
            handrailMidCurve.Transform(Transform.Translation(0, 0, -1 * topInterval));
            handrailBottomCurve.Transform(Transform.Translation(0, 0, -1*(HandrailHeight - bottomInterval)));
            handrailMidCurve = handrailMidCurve.Trim(CurveEnd.Both, StepWidth / 2);
            handrailBottomCurve = handrailBottomCurve.Trim(CurveEnd.Both, StepWidth / 2);

            Brep handrailMidPipe1 = CreateCirclePipeOrNot(handrailMidCurve, radiusOfHorizontalPipe, IsCircleHandrail);
            Brep handrailBottomPipe1 = CreateCirclePipeOrNot(handrailBottomCurve, radiusOfHorizontalPipe, IsCircleHandrail);

            handrailMidPipe1.Transform(Transform.Translation(HandrailRadius, 0, 0));
            handrailMidPipe1.Transform(Transform.Translation(HandrailMargin, 0, 0));
            handrailBottomPipe1.Transform(Transform.Translation(HandrailRadius, 0, 0));
            handrailBottomPipe1.Transform(Transform.Translation(HandrailMargin, 0, 0));

            Brep handrailMidPipe2 = handrailMidPipe1.DuplicateBrep();
            Brep handrailBottomPipe2 = handrailBottomPipe1.DuplicateBrep();
            handrailMidPipe2.Transform(Transform.Mirror(mirrorPlane));
            handrailBottomPipe2.Transform(Transform.Mirror(mirrorPlane));

            //{0;0}中第一个数区分左右：若为0，则为左；若为1，则为右
            //{0;0}中第二个数区分种类：若为0，则为扶手、整体式栏板、水平构件；若为1，则为栏杆(垂直构件)
            Handrails.Add(handrailMidPipe1, new GH_Path(0,0));
            Handrails.Add(handrailBottomPipe1,new GH_Path(0,0));
            Handrails.Add(handrailMidPipe2, new GH_Path(1, 0));
            Handrails.Add(handrailBottomPipe2, new GH_Path(1, 0));

            //垂直杆件

            //创建主杆曲线、实体
            List<Curve> mainPipeCurves = new List<Curve>();
            List<Brep> mainPipes1 = new List<Brep>();
            List<Brep> mainPipes2 = new List<Brep>();
            for (int i = 0; i < indices.Count; i++)
            {
                Point3d startPt = (stepWidthCurves[indices[i]].PointAtStart + stepWidthCurves[indices[i]].PointAtEnd) / 2;
                Curve unitCurve = new Line(startPt, Vector3d.ZAxis).ToNurbsCurve();
                Curve mainPipeCurve=unitCurve.Extend(CurveEnd.End, CurveExtensionStyle.Line, new List<GeometryBase> { handrailTopCurve.Duplicate() });
                //延伸主杆曲线，在扶手倾斜段使得主杆不外漏一部分
                mainPipeCurve = mainPipeCurve.Extend(CurveEnd.End, ArmrestDepth / 2, CurveExtensionStyle.Line);
                mainPipeCurves.Add(mainPipeCurve);

                Brep mainPipe1 = CreateCirclePipeOrNot(mainPipeCurve, HandrailRadius, IsCircleHandrail);

                //消除半径影响，将栏杆默认紧贴楼梯梯段边缘
                mainPipe1.Transform(Transform.Translation(HandrailRadius, 0, 0));
                //完成上方内容后通过边距控制栏杆位置
                mainPipe1.Transform(Transform.Translation(HandrailMargin, 0, 0));

                
                Brep mainPipe2 = mainPipe1.DuplicateBrep();
                mainPipe2.Transform(Transform.Mirror(mirrorPlane));

                mainPipes1.Add(mainPipe1);
                mainPipes2.Add(mainPipe2);
            }
            Handrails.AddRange(mainPipes1, new GH_Path(0,1));
            Handrails.AddRange(mainPipes2, new GH_Path(1,1));

            //创建次杆曲线、实体
            List<Curve> subPipeCurves = new List<Curve>();
            List<Brep> subPipes1 = new List<Brep>();
            List<Brep> subPipes2 = new List<Brep>();
            
            for (int i = 0; i < mainPipeCurves.Count-1; i++)
            {
                double tOfPt;
                mainPipeCurves[i].ClosestPoint(mainPipeCurves[i+1].PointAtStart, out tOfPt);
                double distanceBetweenMainPipeCurves = mainPipeCurves[i+1].PointAtStart.DistanceTo(mainPipeCurves[i].PointAt(tOfPt));
                int countOfTweenCrvs = ((int)distanceBetweenMainPipeCurves / 110) + 1;

                Curve[] tweenCrvs = Curve.CreateTweenCurves(mainPipeCurves[i], mainPipeCurves[i+1], countOfTweenCrvs, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                for (int j = 0; j < tweenCrvs.Length; j++)
                {
                    double minOfCrvDomain = tweenCrvs[j].Domain.Min;
                    double maxnOfCrvDomain = tweenCrvs[j].Domain.Max;
                    double t0 = (minOfCrvDomain + maxnOfCrvDomain) * (3.0 / 7);
                    double t1 = (minOfCrvDomain + maxnOfCrvDomain) * (4.0 / 7);
                    Point3d pt0 = tweenCrvs[j].PointAt(t0);
                    Point3d pt1 = tweenCrvs[j].PointAt(t1);
                    Curve subPipeCrv = new Line(pt0, pt1).ToNurbsCurve();

                    subPipeCrv = subPipeCrv.Extend(CurveEnd.Start, CurveExtensionStyle.Line, new List<GeometryBase> { handrailBottomCurve});
                    subPipeCrv = subPipeCrv.Extend(CurveEnd.End, CurveExtensionStyle.Line, new List<GeometryBase> { handrailMidCurve });

                    subPipeCurves.Add(subPipeCrv);
                }
            }
            for (int k = 0; k < subPipeCurves.Count; k++)
            {
                Brep subPipe1 = CreateCirclePipeOrNot(subPipeCurves[k], subPipeRadius, IsCircleHandrail);

                subPipe1.Transform(Transform.Translation(HandrailRadius, 0, 0));
                subPipe1.Transform(Transform.Translation(HandrailMargin, 0, 0));

                Brep subPipe2 = subPipe1.DuplicateBrep();
                subPipe2.Transform(Transform.Mirror(mirrorPlane));

                subPipes1.Add(subPipe1);
                subPipes2.Add(subPipe2);
            }
            //{0;0}中第一个数区分左右：若为0，则为左；若为1，则为右
            //{0;0}中第二个数区分种类：若为0，则为扶手、整体式栏板；若为1，则为栏杆
            Handrails.AddRange(subPipes1, new GH_Path(0,1));
            Handrails.AddRange(subPipes2, new GH_Path(1,1));
        }

        //创建扶手
        protected virtual void CreateArmrest()
        {
            double extendLength = 50;
            double offsetLength = 30;

            //创建扶手底部两条线，并对曲线进行了延长处理
            Curve handrailTopCurve = CreateHandrailTopCurve();
            handrailTopCurve = handrailTopCurve.Extend(CurveEnd.Both, extendLength, CurveExtensionStyle.Smooth);
            Curve armrestCurve1 = handrailTopCurve.DuplicateCurve();
            Curve armrestCurve2 = handrailTopCurve.DuplicateCurve();
            armrestCurve1.Transform(Transform.Translation(-1 * (HandrailRadius + offsetLength), 0, 0));
            armrestCurve2.Transform(Transform.Translation(HandrailRadius + offsetLength, 0, 0));

            //创建扶手顶部两条线
            Curve armrestCurve3 = armrestCurve2.DuplicateCurve();
            Curve armrestCurve4 = armrestCurve1.DuplicateCurve();
            armrestCurve3.Transform(Transform.Translation(0, 0, ArmrestDepth));
            armrestCurve4.Transform(Transform.Translation(0, 0, ArmrestDepth));

            Brep armrest1 = Brep.CreateFromLoft(new List<Curve> { armrestCurve1, armrestCurve2, armrestCurve3, armrestCurve4 }, Point3d.Unset, Point3d.Unset, LoftType.Straight, true)[0].CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //消除HandrailRadius影响
            armrest1.Transform(Transform.Translation(HandrailRadius, 0, 0));
            //根据边距确定扶手最终位置
            armrest1.Transform(Transform.Translation(HandrailMargin, 0, 0));

            //通过镜像创建右侧扶手
            Plane mirrorPlane = new Plane(DatumPt, Vector3d.YAxis, Vector3d.ZAxis);
            mirrorPlane.Transform(Transform.Translation(StairLandingLength / 2, 0, 0));
            Brep armrest2 = armrest1.DuplicateBrep();
            armrest2.Transform(Transform.Mirror(mirrorPlane));
            //{0;0}中第一个数区分左右：若为0，则为左；若为1，则为右
            //{0;0}中第二个数区分种类：若为0，则为扶手、整体式栏板、水平构件；若为1，则为栏杆(垂直构件)
            Handrails.Add(armrest1,new GH_Path(0,0));
            Handrails.Add(armrest2,new GH_Path(1,0));
        }

        //辅助方法
        //创建底部沿踏步的折线曲线
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

        //创建顶部有长倾斜段的折线曲线
        protected virtual Curve CreateHandrailTopCurve()
        {
            Curve flightSidePolyCurve = CreateStepPolyCurve();
            Point3d topPt1 = new Point3d(flightSidePolyCurve.PointAtStart);
            Point3d topPt2 = new Point3d(flightSidePolyCurve.PointAtStart);
            Point3d topPt3 = new Point3d(flightSidePolyCurve.PointAtEnd);
            Point3d topPt4 = new Point3d(flightSidePolyCurve.PointAtEnd);
            topPt1.Transform(Transform.Translation(0, 0, HandrailHeight+StepHeight));
            topPt2.Transform(Transform.Translation(0, StepWidth, HandrailHeight+StepHeight));
            topPt3.Transform(Transform.Translation(0, -1 * StepWidth, HandrailHeight));
            topPt4.Transform(Transform.Translation(0, 0, HandrailHeight));

            Curve topCurve = new Polyline(new List<Point3d> { topPt1, topPt2, topPt3, topPt4 }).ToNurbsCurve();
            return topCurve;
        }

        protected virtual Brep CreateCirclePipeOrNot(Curve rail,double radius, bool isCircle)
        {
            Brep pipe;

            if(isCircle==true)
            {
                pipe= Brep.CreatePipe(rail, radius, true, PipeCapMode.Flat, true, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians)[0];
            }
            else
            {
                Plane basePlane = new Plane(rail.PointAtStart, rail.TangentAtStart);
                Curve baseRect = new Rectangle3d(basePlane, new Interval(-radius, radius), new Interval(-radius, radius)).ToNurbsCurve();
                pipe = Brep.CreateFromSweep(rail, baseRect, false, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
            }

            return pipe;
        }
        #endregion
    }
}
