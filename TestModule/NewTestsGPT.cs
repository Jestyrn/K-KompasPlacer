using netDxf;
using netDxf.Blocks;
using netDxf.Entities;

namespace TestModule
{
    public class NewTestsGPT
    {
        private string Path { get; set; }

        public void TryToDo(string path)
        {
            Path = path;
        }
    }
}
