using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NonlinearPDESolver.Mesh
{
    public class FiniteElement
    {
        public Node Node1 { get; set; }
        public Node Node2 { get; set; }
        public Node Node3 { get; set; }
        public Node Node4 { get; set; }

        public int Count { get { return 4; } }

        public Node this[int index]
        {
            get 
            {
                switch (index)
                {
                    case 0: return Node1;
                    case 1: return Node2;
                    case 2: return Node3;
                    case 3: return Node4;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public override string ToString()
        {
            return "[ " + Node1.ToString() + "\t"+
                Node2.ToString() + "\t"+
                Node3.ToString() + "\t"+
                Node4.ToString() + " ]";
        }
    }
}
