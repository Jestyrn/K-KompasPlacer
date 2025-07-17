using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesterModule
{
    public class DetailsProcessor
    {
        public static List<Detail> GetAllDetails(string path)
        {
            DxfDocument dxf = DxfDocument.Load(path);
            List<Block> blocks = (List<Block>)dxf.Blocks.ToList().Where(x => !x.Name.Contains("*")).ToList();
            List<EntityObject> ents = new List<EntityObject>();
            
            List<Detail> details = new List<Detail>();

            foreach (var block in blocks)
            {
                foreach (var entity in block.Entities.Where(x => x.Type == EntityType.Line | x.Type == EntityType.Arc | x.Type == EntityType.Circle))
                {
                    ents.Add(entity);
                }

                if (ents.Count != 0)
                {
                    details.Add(new Detail(ents));
                    ents.Clear();
                }
            }

            return details;
        }
    }

    public class Detail
    {
        public List<EntityObject> Entities { get; private set; } = new List<EntityObject>();
        public int Quantity { get; set; }
        public Vector3 Origin { get; private set; }
        public double Angle { get; private set; }
        public double Width { get; private set; } = 0;
        public double Height { get; private set; } = 0;

        private List<Vector3> _points = new List<Vector3>();

        public Detail(List<EntityObject> objects)
        {
            Entities.AddRange(objects);

            GetAllPOints();
            FindCenter();
            GetAngle();

            double maxX, maxY, minX, minY;
            minX = minY = double.MaxValue;
            maxX = maxY = double.MinValue;

            foreach (var point in _points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);

                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            Width = maxX - minX;
            Height = maxY - minY;
        }

        private void GetAngle()
        {
            if (_points.Count < 2) return;

            double xx = 0, xy = 0;
            foreach (var p in _points)
            {
                xx += (p.X - Origin.X) * (p.X - Origin.X);
                xy += (p.X - Origin.X) * (p.Y - Origin.Y);
            }

            Angle = 0.5 * Math.Atan2(2 * xy, xx) * (180 / Math.PI);
        }

        private void FindCenter()
        {
            if (Entities.Count == 0)
            {
                Origin = Vector3.Zero;
                return;
            }

            double sumX, sumY;
            sumX = sumY = 0;

            int cnt = 0;

            foreach (var point in _points)
            {
                sumX += point.X;
                sumY += point.Y;

                cnt++;
            }

            Origin = new Vector3(sumX / (double)cnt, sumY / (double)cnt, 0);
        }

        private void GetAllPOints()
        {
            foreach (var item in Entities)
            {
                switch (item.Type)
                {
                    case EntityType.Arc:
                        _points.Add(((Arc)item).Center);
                        break;
                    case EntityType.Circle:
                        _points.Add(((Circle)item).Center);
                        break;
                    case EntityType.Insert:
                        _points.Add(((Insert)item).Position);
                        break;
                    case EntityType.Line:
                        _points.Add(((Line)item).StartPoint);
                        _points.Add(((Line)item).EndPoint);
                        break;
                }
            }
        }

        public List<EntityObject> RotateDetail(double angle)
        {
            double radians = angle * Math.PI / 180;

            foreach (var entity in Entities)
            {
                switch (entity)
                {
                    case Line line:
                        line.StartPoint = RotatePoint(line.StartPoint, radians);
                        line.EndPoint = RotatePoint(line.EndPoint, radians);
                        break;

                    case Arc arc:
                        arc.Center = RotatePoint(arc.Center, radians);
                        arc.StartAngle += angle;
                        arc.EndAngle += angle;
                        break;

                    case Circle circle:
                        circle.Center = RotatePoint(circle.Center, radians);
                        break;
                }
            }
            Angle = (Angle + angle) % 360;

            return Entities;
        }

        private Vector3 RotatePoint(Vector3 point, double angleRadians)
        {
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            double x = point.X - Origin.X;
            double y = point.Y - Origin.Y;

            return new Vector3(
                x * cos - y * sin + Origin.X,
                x * sin + y * cos + Origin.Y,
                0
            );
        }
    }
}
