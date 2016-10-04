using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NonlinearPDESolver.Mesh
{
    public class Node
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Index { get; set; }

        public Node()
        {
            X = 0;
            Y = 0;
            Index = 0;
        }
        public Node(double x, double y)
        {
            X = x;
            Y = y;
            Index = 0;
        }

        public Node(double x, double y,  int index)
        {
            X = x;
            Y = y;
            Index = index;
        }

        public static double Length(Node a, Node b)
        {
            return Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }

        public static explicit operator Point(Node node)
        {
            return new Point((int)Math.Round(node.X), (int)Math.Round(node.Y));
        }
        public override string ToString()
        {
            return Index.ToString();//X + " ; " + Y;
        }
        public static Node Parse(string nodeStr)
        {
            string[] mas = nodeStr.Split(';');
            return new Node(double.Parse(mas[0].Trim()), double.Parse(mas[1].Trim()));
        }

    }
}
