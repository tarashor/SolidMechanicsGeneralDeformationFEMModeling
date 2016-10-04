using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NonlinearPDESolver.Mesh;
using NonlinearPDESolver.Model;
using NonlinearPDESolver.NumericalMethods;
using System.IO;
using NonlinearPDESolver.Utils;
using MatrixProj;

namespace NonlinearPDESolver
{
    class Program
    {
        static void Main(string[] args)
        {

            InputModelArgs model = new InputModelArgs();
            model.Shape.L = 1;
            model.Shape.H = 0.1;
            model.Material.AutoiFillE(40000);
            model.Material.AutoFillV(0.3);
            model.Material.AutoFillG();
            model.Boundary = BoundaryConditions.FixedLeftSide;
            model.Load = 2752.4039476340849;
            FEMSolver solver = new FEMSolver(model);

            int iter;

            Result result = solver.SolvePDE(30, 6, 0.001, out iter);

            AnaliticalOneSideFixed analitical = new AnaliticalOneSideFixed(model);

            using (StreamWriter sw = new StreamWriter("ResultU1.txt")) 
            {
                foreach (Node node in solver.Mesh.MiddleNodes)
                {
                    Vector alfa = new Vector(3);
                    alfa[0] = node.X;
                    alfa[2] = node.Y;
                    sw.WriteLine(node.X.ToString() + " " + solver.U[2 * node.Index].ToString("0.00000") + "\t" + analitical.U(alfa)[0].ToString("0.00000"));
                }
                sw.Close();
            }

            

            using (StreamWriter sw = new StreamWriter("ResultU3.txt"))
            {
                foreach (Node node in solver.Mesh.MiddleNodes)
                {
                    Vector alfa = new Vector(3);
                    alfa[0] = node.X;
                    alfa[2] = node.Y;
                    sw.WriteLine(node.X.ToString() + "\t" + result.U[2 * node.Index + 1].ToString("0.00000") + "\t" + analitical.U(alfa)[2].ToString("0.00000"));
                }
                sw.Close();
            }
            Console.WriteLine("Ітерацій: " + iter.ToString());
            Console.WriteLine("Files Filed");
            Console.Read();
        }

        public static double p(double x1)
        {
            return 100;
        }

        public static double f(double x1, double x2)
        {
            return (x1 * x1) * (1 + x2);
        }
    }
}
