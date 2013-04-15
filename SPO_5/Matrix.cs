using System;

namespace SPO_5
{
    [Serializable]
    public class Matrix
    {
        private double[,] _matrix;
        private int _cols;
        private int _rows;

        public Matrix(int rows, int cols)
        {
            this._rows = rows;
            this._cols = cols;
            this._matrix = new double[rows, cols];
        }
        public void Init(int seed)
        {
            Random rand = new Random(seed);
            for (int i = 0; i < this._rows; i++)
            {
                for (int j = 0; j < this._cols; j++)
                {
                    this._matrix[i, j] = rand.NextDouble();
                }
            }
        }
        public Matrix Append(Matrix m)
        {
            Matrix res = new Matrix(this._rows + m._rows, this._cols);
            for (int j = 0; j < res._cols; j++)
            {
                for (int i = 0; i < this._rows; i++)
                {
                    res._matrix[i, j] = this._matrix[i, j];
                }
                for (int i = 0; i < m._rows; i++)
                {
                    res._matrix[this._rows + i, j] = m._matrix[i, j];
                }
            }
            return res;
        }
        public void Show()
        {
            for (int i = 0; i < this.Rows; i++)
            {
                for (int j = 0; j < this.Cols; j++)
                {
                    Console.Write(this._matrix[i, j].ToString("#0.0") + " ");
                }
                Console.WriteLine();
            }
        }

        public double this[int i, int j]
        {
            get
            {
                return this._matrix[i, j];
            }
            set
            {
                this._matrix[i, j] = value;
            }
        }
        public int Cols
        {
            get
            {
                return this._cols;
            }
        }
        public int Rows
        {
            get
            {
                return this._rows;
            }
        }
    }
}
