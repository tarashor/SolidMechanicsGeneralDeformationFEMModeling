using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixProj;

namespace NonlinearPDESolver.Model
{
    public class Material
    {
        public const int DIMENSIONS = 3;

        public Vector E { get; set; }
        public Matrix v { get; set; }
        public Matrix G { get; set; }

        

        public Material()
        {
            E = new Vector(DIMENSIONS);
            v = new Matrix(DIMENSIONS, DIMENSIONS);
            G = new Matrix(DIMENSIONS, DIMENSIONS);
        }

        public double GetAlfa1()
        {
            double D = 1 - v[0, 1] * v[1, 0] - v[0, 2] * v[2, 0] - v[2, 1] * v[1, 2] - v[0, 2] * v[2, 1] * v[1, 0] - v[1, 2] * v[2, 0] * v[0, 1];
            return (v[2, 0] + v[1, 0] * v[2, 1]) * (v[2, 0] + v[1, 0] * v[2, 1]) * E[0] / (D * E[2]);
        }

        public double GetE1Modif()
        {
            double delta = 1 - v[0, 1] * v[1, 0];
            return E[0] / (delta * delta);
        }

        public double GetE0()
        {
            double D = 1 - v[0, 1] * v[1, 0] - v[0, 2] * v[2, 0] - v[2, 1] * v[1, 2] - v[0, 2] * v[2, 1] * v[1, 0] - v[1, 2] * v[2, 0] * v[0, 1];
            double delta = 1 - v[0, 1] * v[1, 0];
            return E[2] * delta * delta / D;
        }

        public double GetLambda1()
        {
            double delta = 1 - v[0, 1] * v[1, 0];
            return ((v[2, 1] + v[1, 0] * v[2, 1]) * E[0]) / (E[2] * delta * delta);
        }

        public double GetG13() 
        {
            return G[0, 2];
        }

        public void AutoFillG()
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                for (int j = 0; j < DIMENSIONS; j++)
                {
                    G[i, j] = E[i] / (2 * (1 + v[i, j]));
                }
            }
        }

        public void AutoFillV(double vCoef)
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                for (int j = 0; j < DIMENSIONS; j++)
                {
                    v[i, j] = vCoef;
                }
            }
        }

        public void AutoiFillE(double eCoef)
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                E[i] = eCoef;
            }
        }
    }
}
