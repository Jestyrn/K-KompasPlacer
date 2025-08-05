using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class FrameController
    {
        private List<FramePackage> frames;
        private List<Insert> Result;
        private List<Detail> details;

        private double width;
        private double height;

        private int Pading;

        public FrameController(List<Detail> details, double width, double height, int pading = 0)
        {
            this.details = details;
            frames = new List<FramePackage>();
            this.width = width;
            this.height = height;

            Pading = pading;
        }

        public List<Insert> Execute()
        {
            Result = new List<Insert>();

            foreach (var detail in details)
            {
                bool placed = false;

                // Пробуем вставить в существующие фреймы
                foreach (var framePkg in frames)
                {
                    var calc = new CalculatePosition(framePkg.Frame.FreeRects, detail);
                    var fit = calc.FindBestFit();

                    if (fit.Success)
                    {
                        if (fit.WillRotate)
                            detail.RotateDetail(90);

                        detail.MoveDetail(fit.Position.X, fit.Position.Y);
                        framePkg.Details.Add(detail);

                        framePkg.Frame.FreeRects = UpdateFreeRects(framePkg.Frame.FreeRects, fit.TakenBox);
                        placed = true;
                        break;
                    }
                }

                // Если не влезло — создаем новый фрейм
                if (!placed)
                {
                    var newFrame = new Framer(new Vector2(0, 0), width, height);
                    var newPkg = new FramePackage
                    {
                        Frame = newFrame,
                        Details = new List<Detail>()
                    };

                    var calc = new CalculatePosition(newFrame.FreeRects, detail);
                    var fit = calc.FindBestFit();

                    if (!fit.Success)
                        throw new Exception("Деталь не помещается даже в пустую рамку.");

                    if (fit.WillRotate)
                        detail.RotateDetail(90);

                    detail.MoveDetail(fit.Position.X, fit.Position.Y);
                    newPkg.Details.Add(detail);
                    newFrame.FreeRects = UpdateFreeRects(newFrame.FreeRects, fit.TakenBox);

                    frames.Add(newPkg);
                }
            }

            // Сборка финального результата
            foreach (var pkg in frames)
            {
                Result.Add(pkg.Frame.Insert);
                foreach (var detail in pkg.Details)
                    Result.Add(detail.Insert);
            }

            return Result;
        }

        private List<BoundingBox> UpdateFreeRects(List<BoundingBox> freeRects, BoundingBox taken)
        {
            var updated = new List<BoundingBox>();

            foreach (var rect in freeRects)
            {
                if (!rect.Intersects(taken))
                {
                    updated.Add(rect);
                    continue;
                }

                updated.AddRange(rect.Split(taken));
            }

            return updated;
        }
    }

    public class Framer
    {
        public static int Id { get; private set; } = 0;

        public BoundingBox BoundingBox { get; private set; }
        public Insert Insert { get; private set; }
        public List<EntityObject> Entities { get; private set; }
        public List<BoundingBox> FreeRects { get; set; }
        public double Capacity { get; private set; }

        private Vector2 Axle;
        private double Width;
        private double Height;

        public Framer(Vector2 axle, double width, double height)
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

    public class CalculatePosition
    {
        private List<BoundingBox> FreePlaces;
        private Detail Detail;

        public CalculatePosition(List<BoundingBox> freePlaces, Detail detail)
        {
            FreePlaces = freePlaces.OrderByDescending(x => x.Area).ToList();
            Detail = detail;
        }

        public bool TryFound(out bool willRotate, out Vector2 position)
        {
            willRotate = false;

            if (CheckDetails(out position))
            {
                return true;
            }
            else if (CheckDetails(out position, true))
            {
                willRotate = true;
                return true;
            }

            return false;
        }

        private bool CheckDetails(out Vector2 newPos, bool rotate = false)
        {
            if (rotate) Detail.RotateDetail(90);
            newPos = new Vector2(Detail.Bounds.MinX, Detail.Bounds.MinY);

            double bestScore = int.MaxValue;
            BoundingBox bestBox = null;

            foreach (var place in FreePlaces)
            {
                if (place.Width >= Detail.Width & place.Height >= Detail.Height)
                {
                    double leftoverHoriz = place.Width - Detail.Width;
                    double leftoverVert = place.Height - Detail.Height;
                    double score = Math.Min(leftoverHoriz, leftoverVert);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBox = place;
                    }
                }
            }

            if (bestBox == null) return false;
            var takenPlace = new BoundingBox(new Vector2(bestBox.MinX, bestBox.MinY), Detail.Width, Detail.Height);

            // Записать
            // Порезать
            // Убрать остатки

            return true;
        }

        public PlacementResult FindBestFit()
        {
            var result = new PlacementResult();

            if (CheckDetails(out Vector2 pos))
            {
                result.Success = true;
                result.Position = pos;
                result.WillRotate = false;
                result.TakenBox = new BoundingBox(pos, Detail.Width, Detail.Height);
                return result;
            }
            else if (CheckDetails(out pos, true))
            {
                result.Success = true;
                result.Position = pos;
                result.WillRotate = true;
                result.TakenBox = new BoundingBox(pos, Detail.Width, Detail.Height);
                return result;
            }

            result.Success = false;
            return result;
        }
    }

    public class PlacementResult
    {
        public bool Success { get; set; }
        public Vector2 Position { get; set; }
        public bool WillRotate { get; set; }
        public BoundingBox TakenBox { get; set; }
    }

    public class FramePackage
    {
        public Framer Frame;
        public List<Detail> Details;
    }
}
