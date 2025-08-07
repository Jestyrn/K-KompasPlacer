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

                    bool canInsert = frame.FreeRects.Any(free =>
                    (free.Width >= detail.Width && free.Height >= detail.Height) ||
                    (free.Width >= detail.Height && free.Height >= detail.Width));

                    // canInsert = frame.Capacity >= detail.Bounds.Area;

                    if (canInsert)
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
                            framesPackages[index+1].Frame.Capacity -= detail.Area;
                            framesPackages[index+1].Frame.FreeRects = UpdateFreeRects(framesPackages[index + 1].Frame.FreeRects, detail.Bounds);

                            framesPackages[index].Frame.FreeRects = RestoreFreeRects(framesPackages[index]);
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

        private List<BoundingBox> RestoreFreeRects(FramePackage framePackage)
        {
            List<Detail> details = framePackage.Details;
            List<BoundingBox> freeRects = framePackage.Frame.FreeRects;
            List<BoundingBox> result = new List<BoundingBox>();

            double maxX, maxY;
            maxX = int.MinValue;
            maxY = int.MaxValue;

            // 1. Найти максимально занятую площадь(по всем фигурам)
            // 2. Найти свободные части(высота детали, длина от детали до конца)
            // 3. Сортировать по Y
            // 4. Посмотреть на полученные фигуры
            // -- Есть пересечения?
            // --- Сверху - обрезать по X (левая + пв + пн)
            // ---- Обрезать по Y (левая + пв + пн)
            // ---- Обрезать по X (избавиться от лв)
            // 5. Объеденить все что можно (по Y) и избавиться от неактуальных

            return result;
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
                if (!bounds.Intersects(takenBox))
                {
                    result.Add(bounds);
                    continue;
                }

                double widthScope, heightScope;
                widthScope = Math.Abs(takenBox.Width / takenBox.Height);
                heightScope = Math.Abs(takenBox.Height / takenBox.Width);

                if (widthScope > 1.5)
                {
                    // Правая верняя часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        bounds.MinY,
                        takenBox.MaxY
                        ));

                    // Нижняя большая часть
                    result.Add(new BoundingBox(
                        bounds.MinX,
                        bounds.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));
                }
                else if(heightScope > 1.5)
                {
                    // Левая нижняя часть
                    result.Add(new BoundingBox(
                        bounds.MinX,
                        takenBox.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));

                    // Правая большая часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        bounds.MinY,
                        bounds.MaxY
                        ));
                }
                else
                {
                    // Правая верняя часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        bounds.MinY,
                        takenBox.MaxY
                        ));

                    // Левая нижняя часть
                    result.Add(new BoundingBox(
                        bounds.MinX,
                        takenBox.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));

                    // Правая нижняя часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));
                }
            }

            return result;//.Where(r => r.Width > 0.1 && r.Height > 0.1).ToList();
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
