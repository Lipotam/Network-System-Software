using System;
using System.Collections.Generic;
using MPI;
using SPO_5;

namespace Spo_6
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                if (args.Length != 1)
                {
                    return;
                }

                const int matrixSize = 50;
                Intracommunicator comm = Communicator.world;
                int groupCount = int.Parse(args[0]);

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

                    int workers = comm.Size - groupCount - 1;
                    Console.WriteLine("All process count: " + (comm.Size).ToString());
                    Console.WriteLine("Workers count: " + (workers + groupCount).ToString());
                    Console.WriteLine("Group count" + (groupCount).ToString());

                    int workerIndex = groupCount + 1;
                    int managersWithoutWorkers = groupCount;

                    Request[] sendRequest = new Request[groupCount];
                    for (int i = 0; i < groupCount; i++)
                    {
                        int workersIntheGroup = (int)Math.Floor((double)(workers / managersWithoutWorkers));
                        MatrixMultiply mp = new MatrixMultiply(workersIntheGroup + 1, matrixSize)
                            {
                                WorkersList = new List<int>(),
                                IsAManager = true
                            };

                        for (int j = 0; j < workersIntheGroup; j++)
                        {
                            mp.WorkersList.Add(workerIndex);
                            // Console.WriteLine("WorkerID " + workerIndex);
                            workerIndex++;
                            workers--;
                        }
                        managersWithoutWorkers--;
                        Console.WriteLine("Group " + i.ToString() + " has " + (workersIntheGroup + 1).ToString() + "members");
                        sendRequest[i] = comm.ImmediateSend(mp, i + 1, 0);
                    }

                    Console.WriteLine("Sending the job");

                    for (int i = 0; i < groupCount; i++)
                    {
                        sendRequest[i].Wait();
                    }

                    ReceiveRequest[] recieveRequest = new ReceiveRequest[groupCount];

                    for (int i = 0; i < groupCount; i++)
                    {
                        recieveRequest[i] = comm.ImmediateReceive<Matrix>(i + 1, 0);
                    }
                    Console.WriteLine("Recieve results");
                    for (int i = 0; i < groupCount; i++)
                    {
                        recieveRequest[i].Wait();
                    }
                    //for (int i = 0; i < groupCount; i++)
                    //    {
                    //    var result = recieveRequest[i].GetValue();
                    //    if(result == null)
                    //        {
                    //        Console.WriteLine("Null get");
                    //        }
                    //    Console.WriteLine("Group " + i + " has finished in " + ((TimeSpan)recieveRequest[i].GetValue()).ToString());
                    //}
                }
                else
                {
                    ReceiveRequest receiveRequest = comm.ImmediateReceive<MatrixMultiply>(Communicator.anySource, 0);
                    receiveRequest.Wait();
                    MatrixMultiply mp = (MatrixMultiply)receiveRequest.GetValue();
                    if (mp.IsAManager)
                    {
                        DateTime startTime = DateTime.Now;
                        mp.IsAManager = false;
                        mp.Chef = comm.Rank;

                        Request[] sendRequestToWorker = new Request[mp.WorkersList.Count];
                        int index = 0;

                        //Console.WriteLine("MAnager " + mp.WorkersList.Count);
                        foreach (int workerId in mp.WorkersList)
                        {
                            mp.workerId = index + 1;
                            sendRequestToWorker[index] = comm.ImmediateSend(mp, workerId, 0);
                            //  Console.WriteLine("Worker " + mp.WorkersList.Count + " " + index);
                            index++;
                        }

                        for (int i = 0; i < mp.WorkersList.Count; i++)
                        {
                            sendRequestToWorker[i].Wait();
                        }

                        var result = mp.MultiplyForRank(0);
                        ReceiveRequest[] receiveRequestFromWorkers = new ReceiveRequest[mp.WorkersList.Count];

                        for (int i = 0; i < mp.WorkersList.Count; i++)
                        {
                            receiveRequestFromWorkers[i] = comm.ImmediateReceive<Matrix>(mp.WorkersList.ToArray()[i], 0);
                        }
                        for (int i = 0; i < mp.WorkersList.Count; i++)
                        {
                            //  Console.WriteLine("Waiting for " + mp.WorkersList.ToArray()[i]);
                            receiveRequestFromWorkers[i].Wait();
                        }

                        for (int i = 0; i < mp.WorkersList.Count; i++)
                        {
                            result.Append((Matrix)receiveRequestFromWorkers[i].GetValue());
                        }

                        DateTime finishTime = DateTime.Now;
                        TimeSpan resultTime = (finishTime - startTime);
                        Request sendRequest = comm.ImmediateSend(resultTime, 0, 0);
                        sendRequest.Wait();
                        Console.WriteLine("Group " + comm.Rank + " has done in " + resultTime);
                    }
                    else
                    {
                        //Console.WriteLine("Worker " + comm.Rank + "manager" + mp.Chef);
                        mp.MultiplyForRank(mp.workerId);
                        Request sendRequest = comm.ImmediateSend(mp.MultiplyForRank(mp.workerId), mp.Chef, 0);
                        sendRequest.Wait();
                    }
                }
            }
        }
    }
}
