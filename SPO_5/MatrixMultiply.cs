using System;
using System.Collections.Generic;

namespace SPO_5
{
    [Serializable]
    public class MatrixMultiply
    {
        private Matrix m1;
        private Matrix m2;
        private int size;


        public int Chef { get; set; }
        public bool IsAManager { get; set; }
        public List<int> WorkersList { get; set; }
        public int workerId { get; set; }

        public MatrixMultiply(int comSize, int size)
        {
            this.m1 = new Matrix(size * comSize, size * comSize);
            this.m2 = new Matrix(size * comSize, size * comSize);
            this.size = size;
            this.m1.Init(111);
            this.m2.Init(222);
        }

        public Matrix MultiplyForRank(int rank)
        {
            Matrix res = new Matrix(this.size, this.m2.Cols);
            for (int i = 0; i < this.size; i++)
            {
                for (int j = 0; j < this.m2.Cols; j++)
                {
                    for (int k = 0; k < this.m1.Cols; k++)
                    {
                        res[i, j] += this.m1[rank * this.size + i, k] * this.m2[k, j];
                    }
                }
            }
            return res;
        }
        public void Show()
        {
            Console.WriteLine("First Matrix");
            this.m1.Show();
            Console.WriteLine("Second Matrix");
            this.m2.Show();
        }
    }
}
