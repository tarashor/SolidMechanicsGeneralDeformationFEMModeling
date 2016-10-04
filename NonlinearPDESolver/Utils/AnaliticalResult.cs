using System;
using MatrixProj;
using NonlinearPDESolver.Model;

namespace NonlinearPDESolver.Utils
{
    public class AnaliticalOneSideFixed : IAnalitical
    {
        InputModelArgs model;

        double l2; 
        double l3;
        double l4;
        double l5;
        double l6;
        double l7;

        double wK1;
        double wK2;

        double p;
        double Lambda;
        double D;

        public AnaliticalOneSideFixed(InputModelArgs model)
        {
            this.model = model;

            double alfa1 = model.Material.GetAlfa1();
            double h = model.Shape.H;
            double l = model.Shape.L;
            double E = model.Material.E[0];
            double v = model.Material.v[0, 0];

            D = E * h * h * h * (1 + alfa1) / (12 * (1 - v * v));
            Lambda = 14 * E * h / (30 * (1 + v));
            p = -model.Load;
            l2 = l * l;
            l3 = l * l * l;
            l4 = l * l * l * l;
            l5 = l * l * l * l * l;
            l6 = l * l * l * l * l * l;
            l7 = l * l * l * l * l * l * l;
            

            wK1 = p / (2 * Lambda);
            wK2 = p / (6 * D);
        }

        public Vector U(Vector alfa)
        {
            Vector res = new Vector(alfa.Length);

            double L = model.Shape.L / 2;

            double x = alfa[0] - L;
            double z = alfa[2];//- model.Shape.H / 2;

            
            double w = wK1 * ((x - L) * (x - L) - l2)
                - wK2 / 4 * ((x - L) * (x - L) * (x - L) * (x - L) - l4)
                - wK2 * (l3 * x + l3 * L);

            double u = p * p * (1 / (6 * Lambda * D)) * ((x - L) * (x - L) * (x - L) * (x - L) * (x - L) / 5 + l3 * (x - L) * (x - L) * 0.5 - 0.3 * l5) -
                        p * p / (2 * Lambda * Lambda) * ((x - L) * (x - L) * (x - L) / 3 + l3 / 3) -
                        p * p / (72 * D * D) * ((x - L) * (x - L) * (x - L) * (x - L) * (x - L) * (x - L) * (x - L) / 7 + (x - L) * (x - L) * (x - L) * (x - L) * l3 / 2 + l6 * x + l7 / 7);
                        //- p * p / (28 * D * D) * l7;


            double y = wK2 * ((x - L) * (x - L) * (x - L)+l3);

            res[0] = u + z * y;
            res[1] = 0;
            res[2] = w;

            return res;
        }
    }

    public class AnaliticalTwoSidesFixed : IAnalitical
    {
        private double lambda1_2;
        private double lambda1;
        private double k_2;
        private double lambda_2;

        private double N0;

        private double p;
        private double Lambda;
        private double D;
        private double B;

        private double alfa1;
        private double h;
        private double l;
        private double E;
        private double v;

        private double A;
        private double ch;
        private double sh;
        private double _b;

        public AnaliticalTwoSidesFixed(InputModelArgs model, double sigma)
        {
            alfa1 = model.Material.GetAlfa1();
            h = model.Shape.H;
            l = model.Shape.L;
            E = model.Material.E[0];
            v = model.Material.v[0, 0];

            D = E*h*h*h*(1 + alfa1)/(12*(1 - v*v));
            Lambda = 14*E*h/(30*(1 + v));
            B = E*h*(1 + alfa1)/(1 - v*v);
            N0 = sigma*h;

            lambda_2 = N0/D;
            k_2 = Lambda/(Lambda + N0);
            lambda1_2 = lambda_2*k_2;

            lambda1 = Math.Sqrt(lambda1_2);

            
            ch = Math.Cosh(lambda1 * l/2);
            sh = Math.Sinh(lambda1 * l/2);

            _b = l*(1/(Lambda + N0) - 1/(D*lambda_2))/(2*lambda1*sh);

            p = (-1)*CountLoad();

            
        }

        public double CountLoad()
        {
            double ldiv2 = l/2.0;
            double delta = Lambda*Lambda*(sh*ch - lambda1*ldiv2)/
                           (2*(Lambda + N0)*(Lambda + N0)*ldiv2*lambda1*sh*sh)
                           - 2*Lambda*(ldiv2*lambda1*ch - sh)/(lambda1_2*ldiv2*ldiv2*(Lambda + N0)*sh)
                           + 1.0/3.0;

            return Math.Sqrt((2*N0)/(B*delta))*(N0/ldiv2);
        }

        public Vector U(Vector alfa)
        {
            Vector res = new Vector(alfa.Length);

            double L = l/2;

            double x = alfa[0] - L;
            double z = alfa[2];


            double w = _b*p*(Math.Cosh(lambda1*x) - ch) + p*(x*x - l*l/4)/(2*D*lambda_2);
            double u = 0;
            double y = 0;

            res[0] = u + z * y;
            res[1] = 0;
            res[2] = w;

            return res;
        }
    }
}
