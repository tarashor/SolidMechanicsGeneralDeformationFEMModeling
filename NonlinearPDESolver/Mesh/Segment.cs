using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NonlinearPDESolver.Mesh
{
    public class Segment
    {
        #region - Properties -

        public Node A
        {
            get;
            set;
        }

        public Node B
        {
            get;
            set;
        }

        public int Count
        {
            get { return 2; }
        }

        public Node this[int index]
        {
            get 
            {
                switch (index)
                {
                    case 0: return A;
                    case 1: return B;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        public Segment()
        {
            A = new Node();
            B = new Node();
        }

        public Segment(Node a, Node b)
        {
            this.A = a;
            this.B = b;
        }

        public double Length()
        {
            return Node.Length(A, B);
        }

    }

}
