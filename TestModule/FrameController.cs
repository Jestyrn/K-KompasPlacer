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
                // Проходимся по фреймам
                foreach (var package in framesPackages)
                {
                    Frame frame = package.Frame;

                    // Площдь свободного места больше площади детали
                    if (frame.Capacity >= detail.Area)
                    {
                        Vector2 newPos = FindBestPosition(frame, detail, out placed);
                        if (frame.BoundingBox.Contains(detail.Bounds))
                        {
                            if (placed)
                            {
                                detail.MoveDetail(newPos.X, newPos.Y);

                                int index = framesPackages.IndexOf(package);
                                framesPackages[index].Details.Add(detail);

                                framesPackages[index].Frame.Capacity -= detail.Area;
                                framesPackages[index].Frame.FreeRects = UpdateFreeRects(framesPackages[index].Frame.FreeRects, detail.Bounds);

                                continue;
                            }
                        }
                        else
                        {
                            placed = false;
                        }
                    }
                    else
                    {
                        placed = false;
                    }
                }

                // Пытаемся вставить
                if (!placed)
                {
                    if (framesPackages.Count != 0)
                    {
                        int index = (framesPackages.Count - 1) <= 0 ? 0 : framesPackages.Count - 1;

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
                }
            }

            foreach (var package in framesPackages)
            {
                Result.Add(package.Frame.Insert);
                foreach (var detail in package.Details)
                    Result.Add(detail.Insert);
            }

            return Result;
        }

        private Vector2 FindBestPosition(Frame frame, Detail detail, out bool placed)
        {
            placed = false;
            double bestScore = int.MaxValue;
            BoundingBox bestBox = null;

            foreach (var freePlace in frame.FreeRects)
            {
                if (freePlace.Area > detail.Area)
                {
                    if ((freePlace.Width >= detail.Width) & (freePlace.Height >= detail.Height))
                    {
                        double score = Math.Min((freePlace.Width - detail.Width), (freePlace.Height - detail.Height));
                        if (score < bestScore)
                        {
                            bestBox = freePlace;
                            bestScore = score;
                        }
                    }
                    else if ((freePlace.Width >= detail.Height) & (freePlace.Height >= detail.Width))
                    {
                        detail.RotateDetail(90);

                        double score = Math.Min((freePlace.Width - detail.Width), (freePlace.Height - detail.Height));
                        if (score < bestScore)
                        {
                            bestBox = freePlace;
                            bestScore = score;
                        }
                    }
                    continue;
                }
            }
            if (bestBox == null) placed = false;
            else placed = true;

            return bestBox != null ? new Vector2(bestBox.MinX, bestBox.MinY) : new Vector2();
        }

        private List<BoundingBox> UpdateFreeRects(List<BoundingBox> freeRects, BoundingBox takenBox)
        {
            var result = new List<BoundingBox>();

            foreach (var bounds in freeRects)
            {
                if (bounds.Intersects(takenBox))
                {
                    result.Add(bounds);
                }
                else
                {
                    Vector2 cutPoint = new Vector2(takenBox.MaxX, takenBox.MinY);
                    double newWidth, newHeight;

                    newWidth = bounds.Width - takenBox.Width;
                    newHeight = bounds.Height - takenBox.Height;

                    if ((newHeight / newWidth) >= 2)
                    {
                        // Высота овер большая
                        // По горизонтали
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, bounds.MinY, takenBox.MinY));
                        result.Add(new BoundingBox(bounds.MinX, bounds.MaxX, takenBox.MinY, bounds.MaxY));
                    }
                    else if ((newWidth / newHeight) >= 2)
                    {
                        // Ширина овер большая
                        // По вертикали
                        // л н огрызок
                        // п огромный
                        result.Add(new BoundingBox(bounds.MinX, takenBox.MaxX, takenBox.MinY, bounds.MaxY));
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, bounds.MinY, bounds.MaxY));
                    }
                    else
                    {
                        // Нормик
                        // Оба
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, bounds.MinY, takenBox.MinY));
                        result.Add(new BoundingBox(takenBox.MaxX, bounds.MaxX, takenBox.MinY, bounds.MaxY));
                        result.Add(new BoundingBox(bounds.MinX, takenBox.MaxX, takenBox.MinY, bounds.MaxY));
                    }
                }
            }
            return result;
        }
    }

    public class Frame
    {
        public static int Id { get; private set; } = 0;

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
