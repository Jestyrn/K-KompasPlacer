using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static netDxf.Entities.HatchBoundaryPath;
using Arc = netDxf.Entities.Arc;
using Line = netDxf.Entities.Line;
using Vector3 = netDxf.Vector3;

namespace TesterModule
{
    public class NewTestsGPT
    {
        private string Path {  get; set; }

        public void TryToDo(string path)
        {
            Path = path;
        }
    }
}
