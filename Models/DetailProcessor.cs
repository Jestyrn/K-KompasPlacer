using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Tables;
using Vector3 = netDxf.Vector3;

namespace TestModule
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
}
