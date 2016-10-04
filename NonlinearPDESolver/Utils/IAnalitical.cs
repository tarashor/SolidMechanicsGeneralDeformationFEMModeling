using MatrixProj;

namespace NonlinearPDESolver.Utils
{
    public interface IAnalitical
    {
        Vector U(Vector alfa);
    }
}