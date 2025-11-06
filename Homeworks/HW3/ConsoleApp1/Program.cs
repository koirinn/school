using System;

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


        if (coeffs.Length == 0)
            return "0";

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
                if (coeffs[i] > 0)
                {
                    sign = " + ";
                }
                else
                {
                    sign = " - ";
                }
            }
            else
            {
                if (coeffs[i] < 0)
                {
                    sign = " - ";
                }
                else
                {
                    sign = "";
                }
                firstSign = false;
            }

            string variablePart = "";
            if (i == 0)
            {
                variablePart = absCoeff.ToString();
            }
            else if (i == 1)
            {
                if (absCoeff == 1)
                {
                    variablePart = "x";
                }
                else
                {
                    variablePart = $"{absCoeff}x";
                }
            }
            else
            {
                if (absCoeff == 1)
                {
                    variablePart = $"x^{i}";
                }
                else
                {
                    variablePart = $"{absCoeff}x^{i}";
                }
            }

            result += sign + variablePart;
        }

        if (string.IsNullOrEmpty(result))
            return "0";

        return result;
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs = { 1.0, 0.0, 2.0 };
        Polynomial p = new Polynomial(coeffs); // 1 + 2x^2

        Console.WriteLine(p);
    }
}
