using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class FrameController
    {
        private List<FramePackage> framesPackages;
        private List<Insert> Result;
        private List<Detail> details;

        private double width;
        private double height;

        private int Pading;

        public FrameController(List<Detail> details, double width, double height, int pading = 0)
        {
            this.details = details;
            framesPackages = new List<FramePackage>();
            this.width = width;
            this.height = height;

            Pading = pading;
        }

        public List<Insert> TakeFilled()
        {
            Result = new List<Insert>();
            bool placed = false;

            foreach (var detail in details)
            {
                foreach (var package in framesPackages)
                {
                    Frame frame = package.Frame;

                    if (frame.Capacity >= detail.Area)
                    {
                        Vector2 newPos = FindBestPosition(frame, detail, out placed);

                        if (placed)
                        {
                            detail.MoveDetail(newPos.X, newPos.Y);

                            int index = framesPackages.IndexOf(package);
                            framesPackages[index].Details.Add(detail);

                            framesPackages[index].Frame.Capacity -= detail.Area;
                            framesPackages[index].Frame.FreeRects = UpdateFreeRects(framesPackages[index].Frame.FreeRects, detail.Bounds);
                            placed = true;

                            continue;
                        }
                    }
                    else
                    {
                        placed = false;
                    }
                }

                if (!placed)
                {
                    if (Frame.Bounds.CanInsert(detail.Bounds))
                    {
                        if (framesPackages.Count != 0)
                        {
                            int index = framesPackages.Count - 1;

                            var lastBox = framesPackages[index].Frame.BoundingBox;
                            detail.MoveDetail(lastBox.MaxX + Pading, 0);

                            framesPackages.Add(new FramePackage
                            {
                                Frame = new Frame(new Vector2(lastBox.MaxX + Pading, 0), width, height),
                                Details = new List<Detail> { detail }
                            });
                            framesPackages[index].Frame.Capacity -= detail.Area;
                            framesPackages[index].Frame.FreeRects = UpdateFreeRects(framesPackages[index].Frame.FreeRects, detail.Bounds);
                        }
                        else
                        {
                            detail.MoveDetail(0,0);

                            framesPackages.Add(new FramePackage
                            {
                                Frame = new Frame(new Vector2(0, 0), width, height),
                                Details = new List<Detail> { detail }
                            });
                            framesPackages[0].Frame.Capacity -= detail.Area;
                            framesPackages[0].Frame.FreeRects = UpdateFreeRects(framesPackages[0].Frame.FreeRects, detail.Bounds);
                        }

                        placed = true;
                    }
                    else
                    {
                        throw new Exception("Выбрана очень маленький размер\n" +
                            "Name: Width, Height\n" +
                            $"Frame: {Frame.Bounds.Width}, {Frame.Bounds.Height}\n" +
                            $"Detail: {detail.Bounds.Width}, {detail.Bounds.Height}\n");
                    }
                }
            }

            foreach (var package in framesPackages)
            {
                Result.Add(package.Frame.Insert);
                foreach (var detail in package.Details)
                    Result.Add(detail.Insert);
            }

            framesPackages.Clear();
            return Result;
        }

        private Vector2 FindBestPosition(Frame frame, Detail detail, out bool placed)
        {
            placed = false;
            double bestScore = double.MaxValue;
            BoundingBox bestBox = null;
            bool shouldRotate = false;

            foreach (var freePlace in frame.FreeRects)
            {
                if (freePlace.Width >= detail.Width && freePlace.Height >= detail.Height)
                {
                    double gapWidth = freePlace.Width - detail.Width;
                    double gapHeight = freePlace.Height - detail.Height;

                    double score = Math.Min(gapWidth, gapHeight);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBox = freePlace;
                        shouldRotate = false;
                    }
                }

                if (freePlace.Width >= detail.Height && freePlace.Height >= detail.Width)
                {
                    double gapWidth = freePlace.Width - detail.Height;
                    double gapHeight = freePlace.Height - detail.Width;

                    double score = Math.Min(gapWidth, gapHeight);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBox = freePlace;
                        shouldRotate = true;
                    }
                }
            }

            if (bestBox != null)
            {
                if (shouldRotate)
                    detail.RotateDetail(90);
                placed = true;

                return new Vector2(bestBox.MinX, bestBox.MinY);
            }

            return Vector2.Zero;
        }

        private List<BoundingBox> UpdateFreeRects(List<BoundingBox> freeRects, BoundingBox takenBox)
        {
            var result = new List<BoundingBox>();

            foreach (var bounds in freeRects)
            {
                if (takenBox.MinX == bounds.MinX && (takenBox.MaxY == bounds.MinY || takenBox.MinY == bounds.MinY))
                {
                    double scopeWidth = takenBox.Width / takenBox.Height;
                    double scopeHeight = takenBox.Height / takenBox.Width;

                    if (scopeWidth > 1.5)
                    {
                        result.Add(new BoundingBox(bounds.MinX, takenBox.MaxX, takenBox.MinY, bounds.MaxY));
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, bounds.MinY, bounds.MaxY));
                    }
                    else if (scopeHeight > 1.5)
                    {
                        result.Add(new BoundingBox(takenBox.MinX, bounds.MaxX, bounds.MinY, takenBox.MinY));
                        result.Add(new BoundingBox(bounds.MinX, bounds.MaxX, takenBox.MinY, bounds.MaxY));
                    }
                    else
                    {
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, bounds.MinY, takenBox.MinY));
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, takenBox.MinY, bounds.MaxY));
                        result.Add(new BoundingBox(bounds.MinX, takenBox.MaxX, takenBox.MinY, bounds.MaxY));
                    }
                }
                else
                {
                    result.Add(bounds);
                }
            }
            return result;
        }
    }

    public class Frame
    {
        public static int Id { get; private set; } = 0;
        public static BoundingBox Bounds { get; private set; }


        public BoundingBox BoundingBox { get; private set; }
        public Insert Insert { get; private set; }
        public List<EntityObject> Entities { get; private set; }
        public List<BoundingBox> FreeRects { get; set; }
        public double Capacity { get; set; }

        private Vector2 Axle;
        private double Width;
        private double Height;

        public Frame(Vector2 axle, double width, double height)
        {
            Axle = axle;
            Width = width;
            Height = height;

            CreateBounds();
            CreateDxf();
        }

        private void CreateDxf()
        {
            Block block = new Block($"Frame{Id}");
            block.Entities.Add(new Line(new Vector2(BoundingBox.MinX, BoundingBox.MinY), new Vector2(BoundingBox.MaxX, BoundingBox.MinY)));
            block.Entities.Add(new Line(new Vector2(BoundingBox.MaxX, BoundingBox.MinY), new Vector2(BoundingBox.MaxX, BoundingBox.MaxY)));
            block.Entities.Add(new Line(new Vector2(BoundingBox.MaxX, BoundingBox.MaxY), new Vector2(BoundingBox.MinX, BoundingBox.MaxY)));
            block.Entities.Add(new Line(new Vector2(BoundingBox.MinX, BoundingBox.MaxY), new Vector2(BoundingBox.MinX, BoundingBox.MinY)));

            Insert = new Insert(block);

            Id++;
        }

        private void CreateBounds()
        {
            BoundingBox = new BoundingBox(Axle, Width, Height);
            Bounds = BoundingBox;

            FreeRects = new List<BoundingBox> { BoundingBox };
            Capacity = BoundingBox.Area;
        }
    }

    public class FramePackage
    {
        public Frame Frame;
        public List<Detail> Details;

        public FramePackage()
        {
            Details = new List<Detail>();
        }
    }
}
