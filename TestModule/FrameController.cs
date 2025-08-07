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

            foreach (var detail in details)
            {
                bool detailPlaced = false;

                // Пытаемся разместить в существующих фреймах
                foreach (var package in framesPackages)
                {
                    var frame = package.Frame;

                    bool canInsert = frame.FreeRects.Any(free =>
                        (free.Width >= detail.Width && free.Height >= detail.Height) ||
                        (free.Width >= detail.Height && free.Height >= detail.Width));

                    if (!canInsert)
                    {
                        frame.FreeRects = MergeAdjacentRects(RestoreFreeRects(package));
                        continue;
                    }

                    Vector2 newPos = FindBestPosition(frame, detail, out bool foundPosition);
                    if (!foundPosition) continue;

                    PlaceDetail(package, detail, newPos);
                    detailPlaced = true;
                    break;
                }

                // Если не нашли место — создаём новый фрейм
                if (!detailPlaced)
                {
                    if (!Frame.Bounds.CanInsert(detail.Bounds))
                        throw new Exception($"Выбран слишком маленький размер\n" +
                                            $"Frame: {Frame.Bounds.Width}, {Frame.Bounds.Height}\n" +
                                            $"Detail: {detail.Bounds.Width}, {detail.Bounds.Height}");

                    AddNewFrameWithDetail(detail);
                }
            }

            // Формируем результат
            foreach (var package in framesPackages)
            {
                Result.Add(package.Frame.Insert);
                Result.AddRange(package.Details.Select(d => d.Insert));
            }

            framesPackages.Clear();
            return Result;
        }

        private void PlaceDetail(FramePackage package, Detail detail, Vector2 position)
        {
            detail.MoveDetail(position.X, position.Y);
            package.Details.Add(detail);
            package.Frame.Capacity -= detail.Area;
            package.Frame.FreeRects = UpdateFreeRects(package.Frame.FreeRects, detail.Bounds);
        }

        private void AddNewFrameWithDetail(Detail detail)
        {
            Vector2 pos = framesPackages.Count == 0
                ? new Vector2(0, 0)
                : new Vector2(framesPackages.Last().Frame.BoundingBox.MaxX + Pading, 0);

            detail.MoveDetail(pos.X, pos.Y);

            var newPackage = new FramePackage
            {
                Frame = new Frame(pos, width, height),
                Details = new List<Detail> { detail }
            };

            newPackage.Frame.Capacity -= detail.Area;
            newPackage.Frame.FreeRects = UpdateFreeRects(newPackage.Frame.FreeRects, detail.Bounds);

            framesPackages.Add(newPackage);
        }

        private List<BoundingBox> RestoreFreeRects(FramePackage framePackage)
        {
            var result = new List<BoundingBox>();
            var boxes = framePackage.Frame.FreeRects;
            var main = framePackage.Frame.BoundingBox;

            double maxY = framePackage.Details.Min(d => d.Bounds.MaxY);
            double minY = 0;
            double maxX = framePackage.Details.Max(d => d.Bounds.MaxX);
            double minX = framePackage.Details.Min(d => d.Bounds.MinX);

            BoundingBox bigBox = new BoundingBox(minX, maxX, minY, maxY);

            result.Add(new BoundingBox(bigBox.MaxX, main.MaxX, main.MinY, main.MaxY));
            result.Add(new BoundingBox(main.MinX, main.MaxX, bigBox.MaxY, main.MaxY));

            return result;
        }

        private List<BoundingBox> MergeAdjacentRects(List<BoundingBox> rects)
        {
            bool merged;
            var result = new List<BoundingBox>(rects);

            do
            {
                merged = false;
                for (int i = 0; i < result.Count; i++)
                {
                    for (int j = i + 1; j < result.Count; j++)
                    {
                        if (TryMergeRects(result[i], result[j], out var mergedRect))
                        {
                            result.RemoveAt(j);
                            result.RemoveAt(i);
                            result.Add(mergedRect);
                            merged = true;
                            break;
                        }
                    }
                    if (merged) break;
                }
            } while (merged);

            return result;
        }

        private bool TryMergeRects(BoundingBox a, BoundingBox b, out BoundingBox merged)
        {
            merged = null;

            // Вертикальное объединение
            if (a.MinX == b.MinX && a.MaxX == b.MaxX)
            {
                if (a.MaxY == b.MinY) // a над b
                {
                    merged = new BoundingBox(a.MinX, a.MaxX, a.MinY, b.MaxY);
                    return true;
                }
                if (a.MinY == b.MaxY) // a под b
                {
                    merged = new BoundingBox(b.MinX, b.MaxX, b.MinY, a.MaxY);
                    return true;
                }
            }

            // Горизонтальное объединение
            if (a.MinY == b.MinY && a.MaxY == b.MaxY)
            {
                if (a.MaxX == b.MinX) // a слева от b
                {
                    merged = new BoundingBox(a.MinX, b.MaxX, a.MinY, a.MaxY);
                    return true;
                }
                if (a.MinX == b.MaxX) // a справа от b
                {
                    merged = new BoundingBox(b.MinX, a.MaxX, b.MinY, b.MaxY);
                    return true;
                }
            }

            return false;
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
