using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NonlinearPDESolver.Model;

namespace NonlinearPDESolver.Mesh
{
    public class MeshGenerator
    {
        public List<Node> Nodes { get; private set; }
        public List<Node> LeftNodes { get; private set; }
        public List<Node> RightNodes { get; private set; }
        public List<Node> TopNodes { get; private set; }
        public List<Node> BottomNodes { get; private set; }
        public List<Node> MiddleNodes { get; private set; }
        public List<FiniteElement> Elements { get; private set; }
        public List<Segment> Segments { get; private set; }
        public List<FiniteElement> BoundaryElements { get; private set; }

        private Rectangle _shape;

        public MeshGenerator() 
        {
            Nodes = new List<Node>();
            Elements = new List<FiniteElement>();
            Segments = new List<Segment>();
            LeftNodes = new List<Node>();
            RightNodes = new List<Node>();
            MiddleNodes = new List<Node>();
            TopNodes = new List<Node>();
            BottomNodes = new List<Node>();
            BoundaryElements = new List<FiniteElement>();
        }

        public void GenerateMesh(Rectangle shape, int LElements, int HElements)
        {
            _shape = shape;

            GenerateMeshNodes(LElements, HElements);
            GenerateFiniteElements(LElements, HElements);

            GenerateBoundarySegments(LElements, HElements);

            GenerateBoundaryElements(LElements, HElements);
        }

        private void GenerateBoundaryElements(int LElements, int HElements)
        {
            BoundaryElements.Clear();

            for (int i = 0; i < LElements; i++)
            {
                BoundaryElements.Add(Elements[i]);
            }
        }

        private void GenerateBoundarySegments(int LElements, int HElements)
        {
            Segments.Clear();

            for (int i = 0; i < LElements; i++)
            {
                Segment segment = new Segment(Nodes[i], Nodes[i + 1]);
                Segments.Add(segment);
            }
        }

        private void GenerateFiniteElements(int LElements, int HElements)
        {
            Elements.Clear();

            int LNodes = LElements + 1;
            int finiteElementsCount = Nodes.Count - LNodes;

            for (int i = 0; i < finiteElementsCount; i++)
            {
                if ((i + 1) % LNodes != 0)
                {
                    FiniteElement element = new FiniteElement();
                    element.Node4 = Nodes[i];
                    element.Node3 = Nodes[i+1];
                    element.Node1 = Nodes[i + LNodes];
                    element.Node2 = Nodes[i + LNodes + 1];

                    Elements.Add(element);
                }
 
            }

        }

        private void GenerateMeshNodes(int LElements, int HElements)
        {
            Nodes.Clear();
            LeftNodes.Clear();
            RightNodes.Clear();

            int indexCur = 0;
            double xCur = 0;
            double yCur = _shape.H / 2;

            if (HElements % 2 == 1) HElements++;

            int HNodes = HElements + 1;
            int LNodes = LElements + 1;

            double xStep = _shape.L / Convert.ToDouble(LElements);
            double yStep = _shape.H / Convert.ToDouble(HElements);

            int HNodesdiv2 = HNodes / 2;

            for (int i = 0; i < HNodesdiv2; i++)
            {
                if (i == 0)
                {
                    TopNodes.Clear();
                }
                for (int j = 0; j < LNodes; j++)
                {
                    Node node = new Node();
                    node.Index = indexCur;
                    node.X = xCur;
                    node.Y = yCur;
                    Nodes.Add(node);
                    if (j == 0)
                        LeftNodes.Add(node);
                    if (j == (LNodes - 1))
                        RightNodes.Add(node);

                    indexCur++;
                    xCur += xStep;
                    if (i == 0)
                    {
                        TopNodes.Add(node);
                    }
                }
                yCur -= yStep;
                xCur = 0;
            }

            xCur = 0;
            yCur = 0;
            MiddleNodes.Clear();
            for (int j = 0; j < LNodes; j++)
            {
                Node node = new Node();
                node.Index = indexCur;
                node.X = xCur;
                node.Y = yCur;
                Nodes.Add(node);
                MiddleNodes.Add(node);
                if (j == 0)
                    LeftNodes.Add(node);
                if (j == (LNodes - 1))
                    RightNodes.Add(node);
                indexCur++;
                xCur += xStep;
            }

            xCur = 0;
            yCur -= yStep;
            for (int i = 0; i < HNodesdiv2; i++)
            {
                if (i == (HNodesdiv2 - 1))
                {
                    BottomNodes.Clear();
                }
                for (int j = 0; j < LNodes; j++)
                {
                    Node node = new Node();
                    node.Index = indexCur;
                    node.X = xCur;
                    node.Y = yCur;
                    Nodes.Add(node);
                    if (j == 0)
                        LeftNodes.Add(node);
                    if (j == (LNodes - 1))
                        RightNodes.Add(node);
                    
                    if (i == (HNodesdiv2 - 1))
                    {
                        BottomNodes.Add(node);
                    }
                    indexCur++;
                    xCur += xStep;
                }
                yCur -= yStep;
                xCur = 0;
            }
        }

        public void PrintNodes() 
        {
            foreach (Node node in Nodes)
            {
                Console.WriteLine(node.ToString());
            }
        }

        public void PrintElements()
        {
            foreach (FiniteElement element in Elements)
            {
                Console.WriteLine(element.ToString());
            }
        }
    }
}
