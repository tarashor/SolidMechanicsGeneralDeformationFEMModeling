using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixProj;

namespace NonlinearPDESolver.Utils
{
    public class MatrixEx
    {
        /*public static Matrix RemoveColumn(this Matrix matrix, int columnIndex)
        {
            Matrix res = new Matrix(matrix.CountRows, matrix.CountColumns - 1);
            for (int i = 0; i < matrix.CountRows; i++)
            {
                for (int j = 0; j < columnIndex; j++)
                {
                    res[i, j] = matrix[i, j];
                }

                for (int j = columnIndex + 1; j < matrix.CountColumns; j++)
                {
                    res[i, j - 1] = matrix[i, j];
                }
            }

            return res;
        }

        public static Matrix RemoveRow(this Matrix matrix, int rowIndex)
        {
            Matrix res = new Matrix(matrix.CountRows -1, matrix.CountColumns);
            for (int i = 0; i < matrix.CountRows; i++)
            {
                for (int j = 0; j < columnIndex; j++)
                {
                    res[i, j] = matrix[i, j];
                }

                for (int j = columnIndex + 1; j < matrix.CountColumns; j++)
                {
                    res[i, j - 1] = matrix[i, j];
                }
            }

            return res;
        }*/
    }
}
