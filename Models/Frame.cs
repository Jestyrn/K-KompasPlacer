using netDxf;
using netDxf.Blocks;
using netDxf.Entities;

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