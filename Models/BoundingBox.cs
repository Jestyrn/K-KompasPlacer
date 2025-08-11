using netDxf;

public class BoundingBox
{
    public double MinX { get; }
    public double MaxX { get; }
    public double MinY { get; }
    public double MaxY { get; }

    public double Width { get; }
    public double Height { get; }

    public double Area { get; }

    public BoundingBox(double minX, double maxX, double minY, double maxY)
    {
        MinX = Math.Min(minX, maxX);
        MaxX = Math.Max(minX, maxX);
        MinY = Math.Max(minY, maxY);
        MaxY = Math.Min(minY, maxY);

        Width = Math.Abs(MaxX - MinX);
        Height = Math.Abs(MaxY - MinY);

        Area = Width * Height;
    }

    public BoundingBox(Vector2 leftX_upY, double width, double height)
    {
        MinX = leftX_upY.X;
        MaxX = leftX_upY.X + width;
        MinY = leftX_upY.Y;
        MaxY = leftX_upY.Y - height;

        Width = Math.Abs(MaxX - MinX);
        Height = Math.Abs(MaxY - MinY);

        Area = Width * Height;
    }

    public bool Intersects(BoundingBox other)
    {
        // Прямоугольники НЕ пересекаются?
        return MinX < other.MaxX &&
               MaxX > other.MinX &&
               MinY > other.MaxY &&
               MaxY < other.MinY;
    }

    public bool CanInsert(BoundingBox whoInserted)
    {
        return Width >= whoInserted.Width &
                Height >= whoInserted.Height;
    }
}