using System;
using System.Globalization;
using System.Text;

namespace MatrixApp
{
    // ====================================================================
    // СТРУКТУРА ДЛЯ ЗБЕРЕЖЕННЯ МІНІМАЛЬНОГО ЕЛЕМЕНТА
    // ====================================================================
    public struct MatrixCoord
    {
        public double Value { get; }
        public int Row { get; }
        public int Col { get; }
        public int Depth { get; }

        public MatrixCoord(double value, int row, int col)
        {
            Value = value;
            Row = row;
            Col = col;
            Depth = -1;
        }

        public MatrixCoord(double value, int row, int col, int depth)
        {
            Value = value;
            Row = row;
            Col = col;
            Depth = depth;
        }

        public override string ToString()
        {
            return Depth == -1
                ? $"Значення: {Value:F2} (Координати: [{Row}, {Col}])"
                : $"Значення: {Value:F2} (Координати: [{Row}, {Col}, {Depth}])";
        }
    }

    // ====================================================================
    // БЕЗПЕЧНИЙ ДЛЯ ПОТОКІВ RANDOM
    // ====================================================================
    public static class GlobalRandom
    {
        private static readonly object _lock = new object();
        private static readonly Random _rnd = new Random();

        public static double NextDouble()
        {
            lock (_lock)
                return _rnd.NextDouble();
        }
    }

    // ====================================================================
    // АБСТРАКТНА МАТРИЦЯ
    // ====================================================================
    public abstract class MatrixBase
    {
        public abstract void SetFromArray(double[] values);
        public abstract void SetRandom();
        public abstract double FindMin();
        public abstract MatrixCoord FindMinWithCoord();
        public abstract string GetDimensions();
        public abstract double GetValue(int i, int j);
        public abstract double GetValue(int i, int j, int k);
    }

    // ====================================================================
    // 2D МАТРИЦЯ
    // ====================================================================
    public class Matrix2D : MatrixBase
    {
        private readonly double[,] data;
        public int Rows { get; }
        public int Cols { get; }

        public Matrix2D(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "Розміри мають бути додатні.");

            Rows = rows;
            Cols = cols;
            data = new double[Rows, Cols];
        }

        public override void SetFromArray(double[] values)
        {
            if (values.Length != Rows * Cols)
                throw new ArgumentException(nameof(values));

            int k = 0;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    data[i, j] = values[k++];
        }

        public override void SetRandom()
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    data[i, j] = GlobalRandom.NextDouble() * 100;
        }

        public override double FindMin() => FindMinWithCoord().Value;

        public override MatrixCoord FindMinWithCoord()
        {
            double min = double.MaxValue;
            int r = -1, c = -1;

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    if (data[i, j] < min)
                    {
                        min = data[i, j];
                        r = i;
                        c = j;
                    }

            return new MatrixCoord(min, r, c);
        }

        public override string GetDimensions() => $"{Rows}x{Cols}";

        public override double GetValue(int i, int j)
        {
            if (i < 0 || i >= Rows || j < 0 || j >= Cols)
                throw new ArgumentOutOfRangeException(nameof(i));

            return data[i, j];
        }

        public override double GetValue(int i, int j, int k)
        {
            throw new NotSupportedException("2D матриця не має третього індексу.");
        }
    }

    // ====================================================================
    // 3D МАТРИЦЯ
    // ====================================================================
    public class Matrix3D : MatrixBase
    {
        private readonly double[,,] data;
        public int Rows { get; }
        public int Cols { get; }
        public int Depth { get; }

        public Matrix3D(int rows, int cols, int depth)
        {
            if (rows <= 0 || cols <= 0 || depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows));

            Rows = rows;
            Cols = cols;
            Depth = depth;
            data = new double[Rows, Cols, Depth];
        }

        public override void SetFromArray(double[] values)
        {
            if (values.Length != Rows * Cols * Depth)
                throw new ArgumentException(nameof(values));

            int k = 0;
            for (int d = 0; d < Depth; d++)
                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Cols; j++)
                        data[i, j, d] = values[k++];
        }

        public override void SetRandom()
        {
            for (int d = 0; d < Depth; d++)
                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Cols; j++)
                        data[i, j, d] = GlobalRandom.NextDouble() * 100;
        }

        public override double FindMin() => FindMinWithCoord().Value;

        public override MatrixCoord FindMinWithCoord()
        {
            double min = double.MaxValue;
            int ir = -1, ic = -1, id = -1;

            for (int d = 0; d < Depth; d++)
                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Cols; j++)
                        if (data[i, j, d] < min)
                        {
                            min = data[i, j, d];
                            ir = i;
                            ic = j;
                            id = d;
                        }

            return new MatrixCoord(min, ir, ic, id);
        }

        public override string GetDimensions() => $"{Rows}x{Cols}x{Depth}";

        public override double GetValue(int i, int j)
        {
            throw new NotSupportedException("3D матриця потребує 3 індекси.");
        }

        public override double GetValue(int i, int j, int k)
        {
            if (i < 0 || i >= Rows || j < 0 || j >= Cols || k < 0 || k >= Depth)
                throw new ArgumentOutOfRangeException(nameof(i));

            return data[i, j, k];
        }
    }

    // ====================================================================
    // ОСНОВНА ПРОГРАМА (УВЕСЬ I/O ТУТ)
    // ====================================================================
    class Program
    {
        static void DisplayMatrix(MatrixBase m)
        {
            Console.WriteLine($"\nМатриця ({m.GetDimensions()}):");

            if (m is Matrix2D a)
            {
                for (int i = 0; i < a.Rows; i++)
                {
                    for (int j = 0; j < a.Cols; j++)
                        Console.Write($"{a.GetValue(i, j),8:F2} ");
                    Console.WriteLine();
                }
            }
            else if (m is Matrix3D b)
            {
                for (int d = 0; d < b.Depth; d++)
                {
                    Console.WriteLine($"\nСлой {d}:");
                    for (int i = 0; i < b.Rows; i++)
                    {
                        for (int j = 0; j < b.Cols; j++)
                            Console.Write($"{b.GetValue(i, j, d),8:F2} ");
                        Console.WriteLine();
                    }
                }
            }
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Matrix2D m2 = new Matrix2D(3, 3);
            m2.SetRandom();
            DisplayMatrix(m2);
            Console.WriteLine("Мінімум: " + m2.FindMinWithCoord());

            Matrix3D m3 = new Matrix3D(2, 2, 2);
            m3.SetRandom();
            DisplayMatrix(m3);
            Console.WriteLine("Мінімум: " + m3.FindMinWithCoord());
        }
    }
}
