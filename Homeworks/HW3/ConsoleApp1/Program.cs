using System;
using System.Reflection.PortableExecutable;

class Polynomial
{
    private int degree;
    private double[] coeffs;

    public Polynomial()
    {
        degree = 0;
        coeffs = new double[1] { 0.0 };
    }

    public Polynomial(double[] new_coeffs)
    {
        degree = new_coeffs.Length - 1;
        coeffs = (double[])new_coeffs.Clone();
    }

    public int Degree
    {
        get { return degree; }
    }

    public double[] Coeffs
    {
        get { return (double[])coeffs.Clone(); }
    }

    public override string ToString()
    {
        /*
        *Метод должен возвращать строковое представление многочлена.
        * 
        * Например, если коэффициенты: { 1.0, 0.0, 2.0 },
        * то многочлен имеет вид:
        *     P(x) = 1 + 2x^2
        * 
        * Правила форматирования:
        *  - Пропускать члены, у которых коэффициент равен 0.
        *  - Если коэффициент положительный и это не первый член — добавлять " + ".
        *  - Если отрицательный — добавлять " - " и брать модуль коэффициента.
        *  - Для x^1 писать просто "x", для x^0 — только число.
        * 
        * Пример вывода:
        *     "1 + 2x^2"
        */
        // return "";


        if (coeffs.Length == 0){
            return "0";
        }
        
        string result = "";
        bool firstSign = true;

        for (int i = 0; i < coeffs.Length; i++)
        {
            if (coeffs[i] == 0)
                continue;

            double absCoeff = Math.Abs(coeffs[i]);
            string sign = "";

            if (!firstSign)
            {

                sign = coeffs[i] > 0 ? " + " : " - ";
            }
            else
            {
                sign = coeffs[i] < 0 ? " - " : "";
                firstSign = false;
            }

            string variablePart = "";
            if (i == 0)
            {
                variablePart = absCoeff.ToString();
            }
            else if (i == 1)
            {
                variablePart = absCoeff == 1 ? "x" : $"{absCoeff}x";
            }
            else
            {
                variablePart = absCoeff == 1 ? $"x^{i}" : $"{absCoeff}x^{i}";
            }

            result += sign + variablePart;
        }

        if (string.IsNullOrEmpty(result))
            return "0";

        return result;
    }

    public static Polynomial operator +(Polynomial obj1, Polynomial obj2)
    {
        double[] coeffs1 = obj1.Coeffs;
        double[] coeffs2 = obj2.Coeffs;

        if (coeffs1.Length < coeffs2.Length)
        {
            double[] coeffs3 = (double[])coeffs2.Clone();
            for (int i = 0; i < coeffs2.Length; i++)
            {
                if (i < coeffs1.Length)
                {
                    coeffs3[i] += coeffs1[i];

                }
            }
            Polynomial obj3 = new Polynomial(coeffs3);
            return obj3;
        }
        else
        {
            double[] coeffs3 = (double[])coeffs1.Clone();
            for (int i = 0; i < coeffs1.Length; i++)
            {
                if (i < coeffs1.Length)
                {
                    coeffs3[i] += coeffs2[i];

                }
            }
            Polynomial obj3 = new Polynomial(coeffs3);
            return obj3;
        }

    }

    public static Polynomial operator * (Polynomial obj1, double k)
    {
        double[] coeffs1 = obj1.Coeffs;
        double[] coeffs2 = (double[])coeffs1.Clone();

        for(int i = 0; i < coeffs1.Length; i++)
        {
            coeffs2[i] *= k;
        }
        Polynomial obj2 = new Polynomial(coeffs2);
        return obj2;
    }

    public static Polynomial operator * (double k, Polynomial obj1)
    {
        return obj1 * k;
    }

    public static Polynomial operator * (Polynomial obj1, Polynomial obj2)
    {
        double[] coeffs1 = obj1.Coeffs;
        double[] coeffs2 = obj2.Coeffs;

        if(coeffs1.Length < coeffs2.Length)
        {
            double[] coeffs3 = (double[])coeffs2.Clone();
            for(int i = 0; i < coeffs1.Length; i++)
            {
                for(int j = 0; j < coeffs2.Length; j++)
                {
                    coeffs3[j] *= coeffs1[i];
                }
            }
            Polynomial obj3 = new Polynomial(coeffs3);
            return obj3;
        }
        else
        {
            double[] coeffs3 = (double[])coeffs1.Clone();
            for(int i = 0; i < coeffs2.Length; i++)
            {
                for(int j = 0; j < coeffs1.Length; j++)
                {
                    coeffs3[j] *= coeffs2[i];
                }
            }
            Polynomial obj3 = new Polynomial(coeffs3);
            return obj3;
        }
    }

    public double Evaluate(double x)
    {
        double res = 0;

        if(this.coeffs.Length == 0)
        {
            return 0;
        }

        for(int i = 0; i < this.coeffs.Length; i++)
        {
            if(i == 0)
            {
                res += this.coeffs[i];
            }
            if(i == 1)
            {
                if(this.coeffs[i] == 0)
                {
                    continue;
                }
                res += this.coeffs[i] * x;
            }
            if(i > 1)
            {
                if(this.coeffs[i] == 0)
                {
                    continue;
                }
                res += this.coeffs[i]* Math.Pow(x, i);
            }
        }
        return res;
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs1 = { 1.0, 0.0, 2.0 };
        double[] coeffs2 = { 1.0, 2.0, 3.0};
        Polynomial p1 = new Polynomial(coeffs1); // 1 + 2x^2
        Polynomial p2 = new Polynomial(coeffs2); // 1 + 2x + 3x^2 + 4x^3 + 5x^4

        Console.WriteLine(p1);
        Console.WriteLine(p2);

        Console.WriteLine(p1 + p2);
        Console.WriteLine(5 * p2);

        Console.WriteLine(p1.Evaluate(3));
        Console.WriteLine(p1 * p2);
    }
}
