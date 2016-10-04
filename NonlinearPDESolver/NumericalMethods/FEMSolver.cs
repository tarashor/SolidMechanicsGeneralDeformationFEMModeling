using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixProj;
using NonlinearPDESolver.Model;
using NonlinearPDESolver.Mesh;
using System.IO;

namespace NonlinearPDESolver.NumericalMethods
{
    public class FEMSolver
    {
        public InputModelArgs Model { get; private set; }
        public MeshGenerator Mesh { get; private set; }
        public Matrix ConstMatrix { get; private set; }
        
        public Matrix StiffnessMatrix { get; private set; }
        public Vector TotalVector { get; private set; }

        public double M1 { get; private set; }
        public double M2 { get; private set; }
        public double M3 { get; private set; }
        public double G13 { get; private set; }

        public Vector U { get; private set; }

        public Vector LinearU { get; private set; }
        
        FiniteElement elementCurrent { get; set; }

        public FEMSolver(InputModelArgs model)
        {
            Model = model;
            Mesh = new MeshGenerator();
        }

        Result previousRes;

        public Result SolvePDE(int Lelement, int Helement, double eps, out int iterations)
        {
            Mesh.GenerateMesh(Model.Shape, Lelement, Helement);
            GetConstantMatrix();
            previousRes = new Result(Mesh.Elements, null);
            U = new Vector(Mesh.Nodes.Count * 2);
            previousRes.U = U;

            iterations = 0;

            do
            {
                previousRes.U = U;
                GetStiffnessMatrix();
                GetTotalVector();
                AsumeBoundaryConditions();
                U = StiffnessMatrix.LUalgorithm(TotalVector);
                if (iterations == 0)
                {
                    LinearU = U;
                }
                iterations++;
            }
            while ((Vector.Norm(previousRes.U - U) > eps * Vector.Norm(U)) && (iterations < 20));

            previousRes.U = U;


            return previousRes;
        }

        private Matrix GetConstantMatrix()
        {
            ConstMatrix = new Matrix(4, 4);
            M1 = ConstMatrix[0, 0] = Model.Material.GetE1Modif() * (1 + Model.Material.GetAlfa1());
            M2 = ConstMatrix[1, 0] = ConstMatrix[0, 1] = Model.Material.GetLambda1() * Model.Material.GetE0();
            M3 = ConstMatrix[1, 1] = Model.Material.GetE0();
            G13 = ConstMatrix[2, 2] = ConstMatrix[2, 3] = ConstMatrix[3, 2] = ConstMatrix[3, 3] = Model.Material.GetG13();
            return ConstMatrix;
        }

        #region Boundary conditions
        private void AsumeBoundaryConditions()
        {
            if (Model.Boundary == BoundaryConditions.FixedLeftSide)
            {
                foreach (Node node in Mesh.LeftNodes)
                {
                    StiffnessMatrix[2 * node.Index, 2 * node.Index] *= 1000000000000;
                    StiffnessMatrix[2 * node.Index + 1, 2 * node.Index + 1] *= 1000000000000;
                }
            }

            if (Model.Boundary == BoundaryConditions.FixedRightSide)
            {
                foreach (Node node in Mesh.RightNodes)
                {
                    StiffnessMatrix[2 * node.Index, 2 * node.Index] *= 1000000000000;
                    StiffnessMatrix[2 * node.Index + 1, 2 * node.Index + 1] *= 1000000000000;
                }
            }
            if (Model.Boundary == BoundaryConditions.FixedTwoSides)
            {
                foreach (Node node in Mesh.LeftNodes)
                {
                    StiffnessMatrix[2 * node.Index, 2 * node.Index] *= 1000000000000;
                    StiffnessMatrix[2 * node.Index + 1, 2 * node.Index + 1] *= 1000000000000;
                }
                foreach (Node node in Mesh.RightNodes)
                {
                    StiffnessMatrix[2 * node.Index, 2 * node.Index] *= 1000000000000;
                    StiffnessMatrix[2 * node.Index + 1, 2 * node.Index + 1] *= 1000000000000;
                }
            }
        }
        #endregion

        #region Total Vector
        private Vector GetTotalVector()
        {
            TotalVector = new Vector(Mesh.Nodes.Count * 2);
            foreach (Segment segment in Mesh.Segments)
            {
                Vector localVector = GetLocalTotalVector(segment);
                for (int i = 0; i < segment.Count; i++)
                {
                    TotalVector[2 * segment[i].Index] += localVector[2 * i];
                    TotalVector[2 * segment[i].Index + 1] += localVector[2 * i + 1]; 
                }
            }
            
            foreach (FiniteElement element in Mesh.Elements)
            {
                Matrix NonlinearLocalTotalVector = GetNonlinearLocalTotalVector(element);

                for (int i = 0; i < element.Count; i++)
                {
                    TotalVector[2 * element[i].Index] -= NonlinearLocalTotalVector[2 * i, 0];
                    TotalVector[2 * element[i].Index + 1] -= NonlinearLocalTotalVector[2 * i + 1, 0];
                }
            }
            return TotalVector;
        }

        #region Local Vector
        private Vector GetLocalTotalVector(Segment segment)
        {
            Vector vector = new Vector(4);
            vector[1] = vector[3] = (Model.Load * segment.Length()) / 2.0;
            return vector;
        }

        #endregion

        private Matrix GetNonlinearLocalTotalVector(FiniteElement element)
        {
            elementCurrent = element;

            Matrix NonlinearLocalTotalVector = Integration.GaussianIntegrationMatrix(LocalVectorFunction);

            return NonlinearLocalTotalVector;
        }

        private Matrix LocalVectorFunction(double ksi, double eta)
        {
            Matrix derivativeMatrix = GetLocalDerivativeMatrix(elementCurrent, ksi, eta);

            Jacobian J = new Jacobian();
            J.Element = elementCurrent;
            Matrix VariableVectorOnElement = GetVariableVectorOnElement(elementCurrent, ksi, eta);

            return derivativeMatrix * VariableVectorOnElement * J.GetJacobianDeterminant(ksi, eta);
        }

        private Matrix GetVariableVectorOnElement(FiniteElement element, double ksi, double eta)
        {
            Matrix VariableVector = new Matrix(4, 1);
            Vector du = previousRes.DU(ksi, eta, element);
            double d1u1 = du[0];
            double d3u3 = du[1];
            double d3u1 = du[2];
            double d1u3 = du[3];


            /*VariableVector[0, 0] = 2 * M1 * d1u1 + 2 * M2 * d3u3 + 1.5 * M1 * d1u1 * d1u1 + 0.5 * M1 * d1u3 * d1u3 + (0.5 * M2 + G13) * d3u1 * d3u1 + 0.5 * M2 * d3u3 * d3u3 + M2 * d1u1 * d3u3 + G13 * d3u1 * d1u3;
            VariableVector[1, 0] = 2 * M2 * d1u1 + 2 * M3 * d3u3 + 1.5 * M3 * d3u3 * d3u3 + 0.5 * M2 * d1u1 * d1u1 + (0.5 * M2 + G13) * d1u3 * d1u3 + 0.5 * M3 * d3u1 * d3u1 + M2 * d3u3 * d1u1 + G13 * d1u3 * d3u1;
            VariableVector[2, 0] = G13 * (2 * d1u3 + 2 * d3u1 + d1u3 * d3u3 + d1u1 * d1u3) + (M2 + 2 * G13) * d3u1 * d1u1 + M3 * d3u1 * d3u3;
            VariableVector[3, 0] = G13 * (2 * d3u1 + 2 * d1u3 + d3u1 * d1u1 + d3u1 * d3u3) + (M2 + 2 * G13) * d3u3 * d1u3 + M1 * d1u3 * d1u1;
            */

            VariableVector[0, 0] = 0.5 * M1 * d1u1 * d1u1 + 0.5 * M1 * d1u3 * d1u3 + 0.5 * M2 * d3u1 * d3u1 + 0.5 * M2 * d3u3 * d3u3;
            VariableVector[1, 0] = 0.5 * M2 * d1u1 * d1u1 + 0.5 * M2 * d1u3 * d1u3 + 0.5 * M3 * d3u1 * d3u1 + 0.5 * M3 * d3u3 * d3u3;
            VariableVector[2, 0] = G13 * (d1u1 * d3u1 + d1u3 * d3u3);
            VariableVector[3, 0] = G13 * (d1u1 * d3u1 + d1u3 * d3u3);

            return VariableVector;
        }
        /*

        private Vector GetTotalVector()
        {
            TotalVector = new Vector(Mesh.Nodes.Count * 2);
            foreach (FiniteElement element in Mesh.BoundaryElements)
            {
                Vector localVector = GetLocalTotalVector(element);
                for (int i = 0; i < element.Count; i++)
                {
                    TotalVector[2 * element[i].Index] += localVector[2 * i];
                    TotalVector[2 * element[i].Index + 1] += localVector[2 * i + 1];
                }
            }
            using (StreamWriter sw = new StreamWriter(string.Format("TotalVector.txt")))
            {
                for (int i = 0; i < TotalVector.Length; i++)
                    sw.WriteLine(TotalVector[i]);
                sw.Close();
            }
            return TotalVector;
        }

        private Vector GetLocalTotalVector(FiniteElement element)
        {
            Vector vector = new Vector(8);
            vector[7] = vector[5] = Model.Load * (element[2].X - element[3].X) *(element[3].Y-element[0].Y) / 4;
            return vector;
        }
        */
        #endregion

        #region StiffnessMatrix

        private Matrix GetStiffnessMatrix()
        {
            StiffnessMatrix = new Matrix(Mesh.Nodes.Count * 2, Mesh.Nodes.Count * 2);
            foreach (FiniteElement element in Mesh.Elements)
            {
                Matrix localStiffnessMatrix = GetLocalStiffnessMatrix(element);
                
                for (int i = 0; i < element.Count; i++)
                {
                    for (int j = 0; j < element.Count; j++)
                    {
                        StiffnessMatrix[2 * element[i].Index, 2 * element[j].Index] += localStiffnessMatrix[2 * i, 2 * j];
                        StiffnessMatrix[2 * element[i].Index + 1, 2 * element[j].Index] += localStiffnessMatrix[2 * i + 1, 2 * j];
                        StiffnessMatrix[2 * element[i].Index, 2 * element[j].Index + 1] += localStiffnessMatrix[2 * i , 2 * j + 1];
                        StiffnessMatrix[2 * element[i].Index + 1, 2 * element[j].Index + 1] += localStiffnessMatrix[2 * i + 1, 2 * j + 1];
                    }    
                }
            }

            return StiffnessMatrix;
        }



        #region Local Matrix

        private Matrix GetLocalStiffnessMatrix(FiniteElement element)
        {
            elementCurrent = element;
            
            Matrix localStiffnessMatrix = Integration.GaussianIntegrationMatrix(LocalStiffnessMatrixFunction);
            
            return localStiffnessMatrix;
        }

        private Matrix LocalStiffnessMatrixFunction(double ksi, double eta)
        {
            Matrix derivativeMatrix = GetLocalDerivativeMatrix(elementCurrent, ksi, eta);

            Jacobian J = new Jacobian();
            J.Element = elementCurrent;
            //Matrix VariableMatrixOnElement = GetVariableMatrixOnElement(elementCurrent, ksi, eta);

            return derivativeMatrix * ConstMatrix * Matrix.Transpose(derivativeMatrix) * J.GetJacobianDeterminant(ksi, eta);
        }
        /*
        private Matrix GetVariableMatrixOnElement(FiniteElement element, double ksi, double eta)
        {
            Matrix VariableMatrix = new Matrix(4, 4);
            Vector du = previousRes.DU(ksi, eta, element);
            double d1u1 = du[0];
            double d3u3 = du[1];
            double d3u1 = du[2];
            double d1u3 = du[3];

            VariableMatrix[0, 0] = 1.5 * M1 * d1u1 + 0.5 * M2 * d3u3;
            VariableMatrix[0, 1] = M2 * d3u3 + M2 * d1u1;
            VariableMatrix[0, 2] = M2 * d3u1 + G13 * (2 * d3u1 + d1u3);
            VariableMatrix[0, 3] = M1 * d1u3 + G13 * d1u1;

            VariableMatrix[1, 1] = 1.5 * M3 * d3u3 + 0.5 * M2 * d1u1;
            VariableMatrix[1, 2] = M3 * d3u1 + G13 * d1u3;
            VariableMatrix[1, 3] = M2 * d1u3 + G13 * (2 * d1u3 + d3u1);

            VariableMatrix[2, 2] = 0.5 * M3 * d3u3 + (0.5 * M2 + G13)*d1u1;
            VariableMatrix[2, 3] = G13 * (d3u3 + d1u1);

            VariableMatrix[3, 3] = 0.5 * M1 * d1u1 + (0.5 * M2 + G13) * d3u3;

            return VariableMatrix;
        }*/

        private Matrix GetLocalDerivativeMatrix(FiniteElement element, double ksi, double eta)
        {
            Matrix LocalDerivativeMatrix = new Matrix(8, 4);

            Matrix gradNksieta = new Matrix(2,4);

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
        #endregion

        #endregion
    }
}
