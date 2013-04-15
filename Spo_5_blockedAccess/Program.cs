using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MPI;
using SPO_5;

namespace Spo_5_blockedAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
                {
                int matrixSize = 50;
                Intracommunicator comm = Communicator.world;
                if (comm.Rank == 0)
                {
                    DateTime startTime = DateTime.Now;
                    MatrixMultiply mp = new MatrixMultiply(comm.Size, matrixSize);
                    //mp.Show();
                    comm.Send<MatrixMultiply>(mp, 1, 0);
                    Console.WriteLine("Sending MatrixMyltiply");
                    Matrix res = mp.MultiplyForRank(comm.Rank);
                    comm.Receive<MatrixMultiply>(Communicator.anySource, 0);
                    Console.WriteLine("Recieve MatrixMultiply");
                    comm.Send<Matrix>(res, 1, 1);
                    Console.WriteLine("Sending Matrix result");
                    //res = comm.Receive<Matrix>(Communicator.anySource, 1);
                    Console.WriteLine("Recieve Matrix result");
                    //res.Show();
                    DateTime endTime = DateTime.Now;
                    Console.WriteLine("Test multiply" + (startTime - endTime).ToString());
                }
                else
                {
                    MatrixMultiply mp = comm.Receive<MatrixMultiply>(comm.Rank - 1, 0);
                    comm.Send<MatrixMultiply>(mp, (comm.Rank + 1) % comm.Size, 0);
                    Matrix res = mp.MultiplyForRank(comm.Rank);
                    Matrix m = comm.Receive<Matrix>(comm.Rank - 1, 1);
                    comm.Send<Matrix>(m.Append(res), (comm.Rank + 1) % comm.Size, 1);
                }
            }
        }
    }
}
