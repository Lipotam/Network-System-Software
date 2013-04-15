using System;

using MPI;

namespace SPO_5
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (new MPI.Environment(ref args))
            //{
            //    Intracommunicator comm = Communicator.world;
            //    if (comm.Rank == 0)
            //    {
            //        MatrixMultiply mp = new MatrixMultiply(comm.Size, 2);
            //        mp.Show();
            //        Request sendRequest = comm.ImmediateSend(mp, 1, 0);
            //        Console.WriteLine("Sending MatrixMyltiply");
            //        Matrix res = mp.MultiplyForRank(comm.Rank);
            //        sendRequest.Wait();
            //        ReceiveRequest recieveRequest = comm.ImmediateReceive<MatrixMultiply>(Communicator.anySource, 0);
            //        recieveRequest.Wait();
            //        Console.WriteLine("Recieve MatrixMultiply");
            //        sendRequest = comm.ImmediateSend(res, 1, 1);
            //        Console.WriteLine("Sending Matrix result");
            //        sendRequest.Wait();
            //        recieveRequest = comm.ImmediateReceive<Matrix>(Communicator.anySource, 1);
            //        recieveRequest.Wait();
            //        res = (Matrix)recieveRequest.GetValue();
            //        Console.WriteLine("Recieve Matrix result");
            //        res.Show();
            //    }
            //    else
            //    {
            //        ReceiveRequest receiveRequest = comm.ImmediateReceive<MatrixMultiply>(comm.Rank - 1, 0);
            //        receiveRequest.Wait();
            //        MatrixMultiply mp = (MatrixMultiply)receiveRequest.GetValue();
            //        Request sendRequest = comm.ImmediateSend(mp, (comm.Rank + 1) % comm.Size, 0);
            //        Matrix res = mp.MultiplyForRank(comm.Rank);
            //        sendRequest.Wait();
            //        receiveRequest = comm.ImmediateReceive<Matrix>(comm.Rank - 1, 1);
            //        receiveRequest.Wait();
            //        receiveRequest.GetValue();
            //        sendRequest = comm.ImmediateSend<Matrix>(((Matrix)receiveRequest.GetValue()).Append(res), (comm.Rank + 1) % comm.Size, 1);
            //        sendRequest.Wait();
            //    }
            //}
            using (new MPI.Environment(ref args))
            {
                const int matrixSize = 50;
                Intracommunicator comm = Communicator.world;
                if (comm.Rank == 0)
                {
                    DateTime startTime = DateTime.Now;
                    MatrixMultiply testMultiply = new MatrixMultiply(comm.Size, matrixSize);
                    Matrix result = testMultiply.MultiplyForRank(0);
                    for (int i = 1; i < comm.Size; i++)
                    {
                        result.Append(testMultiply.MultiplyForRank(i));
                    }

                    DateTime endTime = DateTime.Now;
                    Console.WriteLine("Test multiply" + (startTime - endTime).ToString());

                    startTime = DateTime.Now;
                    MatrixMultiply mp = new MatrixMultiply(comm.Size, matrixSize);
                    // mp.Show();
                    Request[] sendRequest = new Request[comm.Size];

                    for (int i = 1; i < comm.Size; i++)
                    {
                        sendRequest[i] = comm.ImmediateSend(mp, i, 0);
                    }
                    Console.WriteLine("Sending partly MatrixMyltiply");
                    Matrix res = mp.MultiplyForRank(comm.Rank);

                    for (int i = 1; i < comm.Size; i++)
                    {
                        sendRequest[i].Wait();
                    }

                    ReceiveRequest[] recieveRequest = new ReceiveRequest[comm.Size];

                    for (int i = 1; i < comm.Size; i++)
                    {
                        recieveRequest[i] = comm.ImmediateReceive<Matrix>(i, 1);
                    }
                    Console.WriteLine("Recieve partly MatrixMultiply");
                    for (int i = 1; i < comm.Size; i++)
                    {
                        recieveRequest[i].Wait();
                    }

                    for (int i = 1; i < comm.Size; i++)
                    {
                        res.Append((Matrix)recieveRequest[i].GetValue());
                    }
                    //Console.WriteLine("Result is");
                    // res.Show();

                    endTime = DateTime.Now;
                    Console.WriteLine("Parallel multiply" + (startTime - endTime).ToString());
                }
                else
                {
                    ReceiveRequest receiveRequest = comm.ImmediateReceive<MatrixMultiply>(0, 0);
                    receiveRequest.Wait();
                    MatrixMultiply mp = (MatrixMultiply)receiveRequest.GetValue();
                    Request sendRequest = comm.ImmediateSend(mp.MultiplyForRank(comm.Rank), 0, 1);
                    sendRequest.Wait();
                }
            }
        }
    }
}
