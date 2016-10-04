using System;
using System.Collections.Generic;
using NonlinearPDESolver.Mesh;
using MatrixProj;

namespace NonlinearPDESolver.NumericalMethods
{
    public class Jacobian
    {
        public FiniteElement Element { get; set; }
        public Matrix GetJacobian(double ksi, double eta)
        {
            Matrix res = new Matrix(2, 2);
            double x1ksi = 0.25 * ((1 - eta) * (Element.Node2.X - Element.Node1.X) + (1 + eta) * (Element.Node3.X - Element.Node4.X));
            double x3ksi = 0.25 * ((1 - eta) * (Element.Node2.Y - Element.Node1.Y) + (1 + eta) * (Element.Node3.Y - Element.Node4.Y));
            double x1eta = 0.25 * ((1 - ksi) * (Element.Node4.X - Element.Node1.X) + (1 + ksi) * (Element.Node3.X - Element.Node2.X));
            double x3eta = 0.25 * ((1 - ksi) * (Element.Node4.Y - Element.Node1.Y) + (1 + ksi) * (Element.Node3.Y - Element.Node2.Y));
            res[0, 0] = x1ksi;
            res[1, 0] = x1eta;
            res[0, 1] = x3ksi;
            res[1, 1] = x3eta;
            return res;
        }

        public double GetJacobianDeterminant(double ksi, double eta)
        {
            return GetJacobian(ksi, eta).Determinant();
        }

        
        public Matrix GetInverseJacobian(double ksi, double eta)
        {
            Matrix J = GetJacobian(ksi, eta);
            Matrix res = new Matrix(2, 2);
            double det = J.Determinant();
            res[0, 0] = J[1, 1] / det;
            res[1, 1] = J[0, 0] / det;
            res[0, 1] = -J[0, 1] / det;
            res[1, 0] = -J[1, 0] / det;
            return res;
        }
    }
}
