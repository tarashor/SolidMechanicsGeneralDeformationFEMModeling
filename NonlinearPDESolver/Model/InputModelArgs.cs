using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NonlinearPDESolver.Model
{
    public class InputModelArgs
    {
        public Material Material { get; set; }
        public Rectangle Shape { get; set; }

        public BoundaryConditions Boundary { get; set; }
        //public Func<double, double> Load { get; set; }
        public double Load { get; set; }

        public InputModelArgs() 
        {
            Material = new Material();
            Shape = new Rectangle();
        }
    }
}
