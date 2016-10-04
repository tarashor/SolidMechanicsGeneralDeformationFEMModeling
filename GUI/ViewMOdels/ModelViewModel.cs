using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using MatrixProj;
using NonlinearPDESolver.Mesh;
using NonlinearPDESolver.Model;
using System.Collections.ObjectModel;
using NonlinearPDESolver.NumericalMethods;
using NonlinearPDESolver.Utils;

namespace GUI.ViewMOdels
{
    public class ModelViewModel:ViewModelBase
    {
        public InputModelArgs Model { get; private set; }

        public IAnalitical analitical { get; private set; }
        public ModelViewModel()
        {
            Model = new InputModelArgs();
            Results = new ObservableCollection<ResultItem>();
            SetDefaultValues();
        }

        public const string HeightPropertyName = "Height";
        public const string LengthPropertyName = "Length";
        public const string EPropertyName = "E";
        public const string vPropertyName = "v";
        public const string LNPropertyName = "LN";
        public const string HNPropertyName = "HN";
        public const string LoadPropertyName = "Load";
        public const string AccuracyPropertyName = "Accuracy";
        public const string TwoSideBoundaryFixedPropertyName = "TwoSideBoundaryFixed";

        private double _accurancy = 0;
        private int _hn = 0;
        private int _ln = 0;
        private bool _fixTwoSide = false;

        private void SetDefaultValues()
        {
            Height = 0.1;
            Length = 1;
            E = 210000;
            v = 0.27;
            LN = 20;
            HN = 4;
            Load = 50;
            Accuracy = 0.001;
            TwoSideBoundaryFixed = false;
            IsSolved = false;
            DrawResult = new RoutedCommand("DrawResult", typeof (ModelViewModel));
        }

        public void Solve()
        {
            IsSolved = false;
            Model.Material.AutoFillG();
            if (Model.Boundary == BoundaryConditions.FixedLeftSide)
                analitical = new AnaliticalOneSideFixed(Model);
            else
            {
                double sigma = Model.Load;
                
                analitical = new AnaliticalTwoSidesFixed(Model, sigma);
                Model.Load = (-1)*((AnaliticalTwoSidesFixed) analitical).CountLoad();
            }

            FEMSolver solver = new FEMSolver(Model);

            int iter;

            Result result = solver.SolvePDE(LN, HN, Accuracy, out iter);

            Results.Clear();
            foreach (Node node in solver.Mesh.MiddleNodes)
            {
                Vector alfa = new Vector(3);
                alfa[0] = node.X;
                alfa[2] = node.Y;
                ResultItem resultItem = new ResultItem();
                resultItem.Alfa1 = node.X;
                resultItem.U1Numeric = result.U[2*node.Index];
                resultItem.U1Analitical = analitical.U(alfa)[0];

                resultItem.U3Numeric = result.U[2*node.Index + 1];
                resultItem.U3Analitical = analitical.U(alfa)[2];

                Results.Add(resultItem);
            }

            
            NonlinearResult = getNonlinearDrawREsult(result, solver);
            LinearResult = getlinearDrawREsult(solver);
            AxesData = getAxesData(Model);

            IsSolved = true;
        }

        #region Properties

        public ICommand DrawResult
        {
            get; private set; 
        }

        public ObservableCollection<ResultItem> Results { get; private set; }

        public bool TwoSideBoundaryFixed
        {
            get { return _fixTwoSide; }
            set
            {
                _fixTwoSide = value;
                if (value)
                    Model.Boundary = BoundaryConditions.FixedTwoSides;
                else
                    Model.Boundary = BoundaryConditions.FixedLeftSide;
                OnPropertyChanged(TwoSideBoundaryFixedPropertyName);
            }
        }

        public double Length
        {
            get { return Model.Shape.L; }
            set
            {
                Model.Shape.L = value;
                OnPropertyChanged(LengthPropertyName);
            }
        }

        public double Height
        {
            get { return Model.Shape.H; }
            set
            {
                Model.Shape.H = value;
                OnPropertyChanged(HeightPropertyName);
            }
        }
        public double E
        {
            get { return Model.Material.E[0]; }
            set
            {
                Model.Material.AutoiFillE(value);
                OnPropertyChanged(EPropertyName);
            }
        }
        public double v
        {
            get { return Model.Material.v[0,0]; }
            set
            {
                Model.Material.AutoFillV(value);
                OnPropertyChanged(vPropertyName);
            }
        }
        public double Load
        {
            get { return Model.Load; }
            set
            {
                Model.Load = value;
                OnPropertyChanged(LoadPropertyName);
            }
        }

        public int LN
        {
            get { return _ln; }
            set
            {
                _ln = value;
                OnPropertyChanged(LNPropertyName);
            }
        }

        public int HN
        {
            get { return _hn; }
            set
            {
                _hn = value;
                OnPropertyChanged(HNPropertyName);
            }
        }

        public double Accuracy
        {
            get { return _accurancy; }
            set
            {
                _accurancy = value;
                OnPropertyChanged(AccuracyPropertyName);
            }
        }

        #endregion

        public bool IsSolved { get; set; }

        private string getAxesData(InputModelArgs args)
        {
            NumberFormatInfo numberFormatInfo =  new CultureInfo( "en-US", false ).NumberFormat;
            numberFormatInfo.NumberDecimalSeparator = ".";
            string resData = "M 0,0";
            resData += "L " + (args.Shape.L*zoomValue).ToString("0.00000", numberFormatInfo) + ",0";
            /*resData += "M " + (args.Shape.L*zoomValue).ToString("0.00000", numberFormatInfo) + ","+
                      ((-1)*args.Shape.H*zoomValue).ToString("0.00000", numberFormatInfo);
            resData += "L " + (args.Shape.L * zoomValue).ToString("0.00000", numberFormatInfo) + "," + (args.Shape.L * zoomValue).ToString("0.00000", numberFormatInfo);*/
            return resData;
        }

        private string getNonlinearDrawREsult(Result result, FEMSolver solver)
        {
            string resData = "M ";
            List<Node> leftNodes = solver.Mesh.LeftNodes;
            leftNodes.Reverse();
            resData += nodeResToSTring(leftNodes[0], result.GetUByIndex(leftNodes[0].Index));
            for (int i = 1; i < leftNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(leftNodes[i], result.GetUByIndex(leftNodes[i].Index));
            }
            List<Node> topNodes = solver.Mesh.TopNodes;
            for (int i = 1; i < topNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(topNodes[i], result.GetUByIndex(topNodes[i].Index));
            }
            List<Node> rightNodes = solver.Mesh.RightNodes;
            for (int i = 1; i < rightNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(rightNodes[i], result.GetUByIndex(rightNodes[i].Index));
            } 
            List<Node> bottomNodes = solver.Mesh.BottomNodes;
            bottomNodes.Reverse();
            for (int i = 1; i < bottomNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(bottomNodes[i], result.GetUByIndex(bottomNodes[i].Index));
            }
            resData += "Z";

            return resData;
        }

        private string getlinearDrawREsult(FEMSolver solver)
        {
            string resData = "M ";
            List<Node> leftNodes = solver.Mesh.LeftNodes;
            //leftNodes.Reverse();
            resData += nodeResToSTring(leftNodes[0], Result.GetUByIndex(leftNodes[0].Index, solver.LinearU));
            for (int i = 1; i < leftNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(leftNodes[i], Result.GetUByIndex(leftNodes[i].Index, solver.LinearU));
            }
            List<Node> topNodes = solver.Mesh.TopNodes;
            for (int i = 1; i < topNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(topNodes[i], Result.GetUByIndex(topNodes[i].Index, solver.LinearU));
            }
            List<Node> rightNodes = solver.Mesh.RightNodes;
            for (int i = 1; i < rightNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(rightNodes[i], Result.GetUByIndex(rightNodes[i].Index, solver.LinearU));
            }
            List<Node> bottomNodes = solver.Mesh.BottomNodes;
            //bottomNodes.Reverse();
            for (int i = 1; i < bottomNodes.Count; i++)
            {
                resData += " L " + nodeResToSTring(bottomNodes[i], Result.GetUByIndex(bottomNodes[i].Index, solver.LinearU));
            }
            resData += "Z";

            return resData;
        }

        private string nodeResToSTring(Node node, Vector U)
        {
            NumberFormatInfo numberFormatInfo =  new CultureInfo( "en-US", false ).NumberFormat;
            numberFormatInfo.NumberDecimalSeparator = ".";
            return (string.Format("{0},{1}", ((node.X + U[0]) * zoomValue).ToString("0.00000", numberFormatInfo), ((node.Y + U[1]) * (-1) * zoomValue).ToString("0.00000", numberFormatInfo)));
        }

        private double zoomValue = 1000;
        public string NonlinearResult { get; set; }
        public string LinearResult { get; set; }
        public string AxesData { get; set; }
    }
}
