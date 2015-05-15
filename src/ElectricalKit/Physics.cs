/*
 * This is the source code of ElectricalKit.
 * It is licensed under MIT License.
 * You should have received a copy of the license in this archive (see LICENSE).
 *
 * Copyright Alexander Gluschenko, 2014-2015.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalKit
{
    public class Physics
    {
        //Ввод
        public static bool isDirect = true;//false - переменный
        public static double U = 220;//Вольт
        public static double F = 50;//Герц

        //Вывод
        public static double I = 0;//Ампер
        public static double R = 0;//Ом
        public static double XL = 0;//Ом (индуктивное сопротивление)
        public static double XC = 0;//Ом (ёмкостное сопротивление)
        public static double Z = 0;//Ом
        public static double P = 0;//Ватт
        public static double Q = 0;//ВАР
        public static double S = 0;//ВА
        public static double CosFi = 0;
        public static double SinFi = 0;

        ///

        public static void CalculateAll() 
        {
            //Цепь
            if(!isDirect)
            {
                XC = CalcXC(F, CommonX(Scheme.Points, "C"));
                XL = CalcXL(F, CommonX(Scheme.Points, "L"));
                Z = CalcZ(R, XC, XL);
            }

            R = CommonR(Scheme.Points);

            if (isDirect) I = CalcI(R, U);
            if (!isDirect) I = CalcI(Z, U);

            if (!isDirect)
            {
                CosFi = CalcCosFi(R, Z);
                SinFi = CalcSinFi(CosFi);
                P = CalcP(I, U, CosFi);
                Q = CalcP(I, U, SinFi);
                S = CalcP(I, U);
            }
            else 
            {
                P = CalcP(I, U);
            }

            //Потребители
            for (int i = 0; i < Scheme.Points.Count(); i++) //Точки
            {
                Consumer[][] SchemePoint = Scheme.Points[i];

                //Падение напряжения на точке (используется при наличи параллельности)
                double ParalR = ParallelR(SchemePoint);
                double ParalU = CalcU(ParalR, I);

                //

                for (int line = 0; line < SchemePoint.Length; line++) //Подцепи
                {
                    double ParalI = CalcI(LinearR(SchemePoint[line]), ParalU);//Сумма линейных токов равна общему току (Кирхгоф)

                    for (int j = 0; j < SchemePoint[line].Length; j++) //Потребители
                    {
                        Consumer Con = SchemePoint[line][j];
                        //

                        Con.I = ParalI;
                        Con.U = CalcU(Con.R, Con.I);
                        Con.P = CalcP(Con.I, Con.U);

                        //
                        Scheme.Points[i][line][j] = Con;
                    }
                }
            }
        }

        ///

        //Напряжение
        public static double CalcU(double R, double I) 
        {
            return R * I;
        }

        //Сила тока

        public static double CalcI(double R, double U) 
        {
            return U / R;
        }

        //Мощность
        public static double CalcP(double I, double U) 
        {
            return I * U; //P = UI
        }

        public static double CalcP(double I, double U, double CosFi)
        {
            return I * U * CosFi; //P = UI * CosFi
        }

        public static double CalcP2(double I, double R)
        {
            return (I * I) * R; //P = I^2 * R
        }

        public static double CalcP3(double U, double R)
        {
            return (U * U) / R; //P = U^2 / R
        }

        public static double CalcCosFi(double R, double Z) 
        {
            if (Z == 0) return 1;
            return R / Z;
        }

        public static double CalcSinFi(double CosFi) 
        {
            return Math.Sqrt(1 - Math.Pow(CosFi, 2));
        }

        //Спротивление

        public static double CalcR(double U, double I) 
        {
            return U / I;
        }

        public static double CommonR(Dictionary<int, Consumer[][]> Points) 
        {
            double CR = 0;

            for (int i = 0; i < Points.Count(); i++ )
            {
                CR += ParallelR(Points[i]);
            }

            return CR;
        }

        public static double ParallelR(Consumer[][] SchemePoint)// 1 / (1/R1 + 1/R2 + 1/Rn ...)
        {
            double Sum = 0;

            for (int i = 0; i < SchemePoint.Length; i++ )//Параллельные линии (подцепи)
            {
                double LR = LinearR(SchemePoint[i]);

                Sum += 1 / LR;

                if (SchemePoint.Length == 1) return LR;//Если нет параллельных подцепей
            }

            if (Sum == 0) return 0;
            return 1 / Sum;
        }

        public static double LinearR(Consumer[] Consumers)
        {
            Consumer[] Resistors = Scheme.GetConsumersByType(Consumers, "R");

            double LR = 0;
            for (int i = 0; i < Resistors.Length; i++) 
            {
                LR += Resistors[i].R;
            }

            return LR;
        }

        //Дальше дубликация кода, но она совершенно легальна и одобрена церковью

        //Расчет емкостных и индуктивных сопротивлений (по строению мало отсличяется от методов выше)
        public static double CommonX(Dictionary<int, Consumer[][]> Points, string ConType) //Type = "C" или "L"
        {
            double CX = 0;

            for (int i = 0; i < Points.Count(); i++)
            {
                CX += ParallelX(Points[i], ConType);
            }

            return CX;
        }

        public static double ParallelX(Consumer[][] SchemePoint, string ConType)// 1 / (1/X1 + 1/X2 + 1/Xn ...)
        {
            double Sum = 0;

            for (int i = 0; i < SchemePoint.Length; i++)//Параллельные линии (подцепи)
            {
                double LX = LinearX(SchemePoint[i], ConType);

                Sum += 1 / LX;

                if (SchemePoint.Length == 1) return LX;//Если нет параллельных подцепей
            }

            if (Sum == 0) return 0;
            return 1 / Sum;
        }

        public static double LinearX(Consumer[] Consumers, string ConType)
        {
            Consumer[] Resistors = Scheme.GetConsumersByType(Consumers, ConType);

            double LX = 0;
            for (int i = 0; i < Resistors.Length; i++)
            {
                double X = 0;
                if (ConType == "C") X = Resistors[i].C / 1000000;//Деление для исбавления от микрофарад
                if (ConType == "L") X = Resistors[i].L;

                LX += X;
            }

            return LX;
        }

        //

        public static double CalcXL(double F, double L) 
        {
            return 2 * Math.PI * F * L;
        }

        public static double CalcXC(double F, double C)
        {
            if (C == 0) return 0;
            return 1 / (2 * Math.PI * F * C);
        }

        public static double CalcZ(double R, double XC, double XL) 
        {
            return Math.Sqrt(Math.Pow(R, 2) + Math.Pow((XL - XC), 2));
        }

        //

        public static int Positive(int number) 
        {
            if (number < 0) return number *= -1;
            return number;
        }

        public static double Positive(double number) 
        {
            if (number < 0) return number *= -1;
            return number;
        }

        public static double Round(double number, double symbols = 8) //Не для полного округления! Полное: Math.Round
        {
            double n = 10; //Система счисления
            return Math.Round(number * (symbols * n)) / (symbols * n);
        }
    }
}
