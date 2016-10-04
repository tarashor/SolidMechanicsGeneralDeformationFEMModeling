using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NonlinearPDESolver.Mesh;
using MatrixProj;

namespace NonlinearPDESolver.NumericalMethods
{
    public class Result
    {
        public List<FiniteElement> Elements { get; private set; }

        public Vector U { get; set; }

        public Result(List<FiniteElement> elements, Vector result)
        {
            Elements = elements;
            U = result;
        }

        public Result()
        {
            Elements = new List<FiniteElement>();
            U = new Vector(0); 
        }

        public Vector DU(double ksi, double eta, FiniteElement element)
        {
            Vector uElement = GetUByElement(element);
            return uElement * GetLocalDerivativeMatrix(element, ksi, eta);
        }

        public Vector GetUByIndex(int index)
        {
            Vector res = new Vector(2);
            if (U != null)
            {
                res[0] = U[2 * index];
                res[1] = U[2 * index + 1];
            }
            return res;
        }

        public static Vector GetUByIndex(int index, Vector U)
        {
            Vector res = new Vector(2);
            if (U != null)
            {
                res[0] = U[2 * index];
                res[1] = U[2 * index + 1];
            }
            return res;
        }

        private Vector GetUByElement(FiniteElement element)
        {
            Vector res = new Vector(8);
            if (U != null)
            {
                for (int i = 0; i < element.Count; i++)
                {
                    res[2 * i] = U[2 * element[i].Index];
                    res[2 * i + 1] = U[2 * element[i].Index + 1];
                }
            }
            return res;
        }

        private Matrix GetLocalDerivativeMatrix(FiniteElement element, double ksi, double eta)
        {
            Matrix LocalDerivativeMatrix = new Matrix(8, 4);

            Matrix gradNksieta = new Matrix(2, 4);

            gradNksieta[0, 0] = (eta - 1) * 0.25;
            gradNksieta[1, 0] = (ksi - 1) * 0.25;
            gradNksieta[0, 1] = (1 - eta) * 0.25;
            gradNksieta[1, 1] = (-ksi - 1) * 0.25;
            gradNksieta[0, 2] = (eta + 1) * 0.25;
            gradNksieta[1, 2] = (ksi + 1) * 0.25;
            gradNksieta[0, 3] = (-eta - 1) * 0.25;
            gradNksieta[1, 3] = (1 - ksi) * 0.25;

            Jacobian J = new Jacobian();
            J.Element = element;

            Matrix gradN = J.GetInverseJacobian(ksi, eta) * gradNksieta;

            LocalDerivativeMatrix[0, 0] = LocalDerivativeMatrix[1, 3] = gradN[0, 0];
            LocalDerivativeMatrix[2, 0] = LocalDerivativeMatrix[3, 3] = gradN[0, 1];
            LocalDerivativeMatrix[4, 0] = LocalDerivativeMatrix[5, 3] = gradN[0, 2];
            LocalDerivativeMatrix[6, 0] = LocalDerivativeMatrix[7, 3] = gradN[0, 3];

            LocalDerivativeMatrix[1, 1] = LocalDerivativeMatrix[0, 2] = gradN[1, 0];
            LocalDerivativeMatrix[3, 1] = LocalDerivativeMatrix[2, 2] = gradN[1, 1];
            LocalDerivativeMatrix[5, 1] = LocalDerivativeMatrix[4, 2] = gradN[1, 2];
            LocalDerivativeMatrix[7, 1] = LocalDerivativeMatrix[6, 2] = gradN[1, 3];

            return LocalDerivativeMatrix;
        }
    }
}
