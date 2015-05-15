/*
 * This is the source code of ElectricalKit.
 * It is licensed under MIT License.
 * You should have received a copy of the license in this archive (see LICENSE).
 *
 * Copyright Alexander Gluschenko, 2014-2015.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ElectricalKit
{
    public class Scheme
    {
        public static Dictionary<int, Consumer[][]> Points = new Dictionary<int, Consumer[][]>();

        static Point[] SchemePoints = new Point[0];

        #region GraphPatterns
        public static GraphPatterns Patterns = new GraphPatterns
        {
            Resistor = new GraphPattern {
                Vectors = new float[] {
                    -0.5f, -0.2f, 0.5f, -0.2f,
                    -0.5f, 0.2f, 0.5f, 0.2f,
                    -0.5f, -0.2f, -0.5f, 0.2f,
                    0.5f, -0.2f, 0.5f, 0.2f,
                },
                Color = Color.Red
            },
            Capacitor = new GraphPattern {
                Vectors = new float[] {
                    -0.075f, -0.4f, -0.075f, 0.4f, 
                    0.075f, -0.4f, 0.075f, 0.4f, 
                    -0.5f, 0f, -0.075f, 0f,
                    0.5f, 0f, 0.075f, 0f,
                },
                Color = Color.Blue
            },
            Inductor = new GraphPattern {
                Vectors = new float[] {
                    -0.5f, 0, -0.45f, -0.1f,
                    -0.45f, -0.1f, -0.4f, -0.125f,
                    -0.4f, -0.125f, -0.35f, -0.125f,
                    -0.35f, -0.125f, -0.3f, -0.1f,
                    -0.3f, -0.1f, -0.25f, 0,
                    ///
                    -0.25f, 0, -0.2f, -0.1f,
                    -0.20f, -0.1f, -0.15f, -0.125f,
                    -0.15f, -0.125f, -0.1f, -0.125f,
                    -0.1f, -0.125f, -0.05f, -0.1f,
                    -0.05f, -0.1f, 0, 0,
                    ///
                    0f, 0, 0.05f, -0.1f,
                    0.05f, -0.1f, 0.1f, -0.125f,
                    0.1f, -0.125f, 0.15f, -0.125f,
                    0.15f, -0.125f, 0.2f, -0.1f,
                    0.2f, -0.1f, 0.25f, 0,
                    ///
                    0.25f, 0, 0.3f, -0.1f,
                    0.3f, -0.1f, 0.35f, -0.125f,
                    0.35f, -0.125f, 0.40f, -0.125f,
                    0.40f, -0.125f, 0.45f, -0.1f,
                    0.45f, -0.1f, 0.5f, 0,
                },
                Color = Color.Yellow
            },
            Source = new GraphPattern
            {
                Vectors = new float[] {
                    -0.075f, -0.4f, -0.075f, 0.4f, 
                    0.075f, -0.2f, 0.075f, 0.2f, 
                    -0.5f, 0f, -0.075f, 0f,
                    0.5f, 0f, 0.075f, 0f,
                },
                Color = Color.LightGreen
            },
            Wire = new GraphPattern
            {
                Vectors = new float[] {
                    -0.5f, 0f, 0.5f, 0f,
                },
                Color = Color.Orange
            },
        };
        #endregion

        public static Point RotatePoint(Point Input, float degAngle)
        {
            double Angle = degAngle * (Math.PI/180);//0.0174532925;
            int x = Input.X;
            int y = Input.Y;
            int rx = (int)Math.Round((Convert.ToDouble(x) * Math.Cos(Angle)) - (Convert.ToDouble(y) * Math.Sin(Angle)));
            int ry = (int)Math.Round((Convert.ToDouble(x) * Math.Sin(Angle)) + (Convert.ToDouble(y) * Math.Cos(Angle)));

            return new Point(rx, ry);
        }

        public static int GetAngle(Point First, Point Second) 
        {
            double X1 = First.X, Y1 = First.Y;
            double X2 = Second.X, Y2 = Second.Y;
            double Angle = Math.Atan2(Y2 - Y1, X2 - X1) * (180/Math.PI);
            Angle = (Angle < 0) ? Angle + 360 : Angle;

            return (int)Math.Round(Angle);
        }

        public static bool isRightAngle(int Angle) //Проверка прямоты угла
        {
            return (Angle == 0) || (Angle == 90) || (Angle == 180) || (Angle == 270) || (Angle == 360);
        }

        //Верстка, рисование
        public static void Parse(string Markup, Bitmap SchemePicture) 
        {
            if (Markup == "") return;
            ///

            Points.Clear();
            ///

            //Разбор верстки

            Markup = Markup.Replace("\r\n", "").Replace(" ", "");

            string[] ConPoints = Markup.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);//Точки ветвления и привязки к ним
            for (int Pnt = 0; Pnt < ConPoints.Length; Pnt++)
            {
                Consumer[][] ConsumerPoint = new Consumer[0][];

                string[] ParalConsumers = ConPoints[Pnt].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);//Параллельные потребители
                ConsumerPoint = new Consumer[ParalConsumers.Length][];
                for (int Par = 0; Par < ParalConsumers.Length; Par++)
                {
                    string[] PointConsumers = ParalConsumers[Par].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//Потребители линейной подцепи

                    Consumer[] ParArray = new Consumer[PointConsumers.Length];
                    for (int n = 0; n < PointConsumers.Length; n++)
                    {
                        ParArray[n] = new Consumer
                        {
                            Point = Pnt,
                            Name = PointConsumers[n],
                            Type = TypeByName(PointConsumers[n]),
                        };
                    }

                    ConsumerPoint[Par] = ParArray;
                }

                Points.Add(Pnt, ConsumerPoint);
            }

            //Исзначально планировались и трехфазные цепи, но теперь этот участок в комментах
            /*string[] Chains = Markup.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);//Цепь

            for (int i = 0; i < Chains.Length; i++ )
            {
                string[] SchemeArray = Chains[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);//Источник и потребители
                ///
                string SourceName = SchemeArray[0];

                string[] ConPoints = SchemeArray[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);//Точки ветвления и привязки к ним
                for (int Pnt = 0; Pnt < ConPoints.Length; Pnt++) 
                {
                    Consumer[][] ConsumerPoint = new Consumer[0][];

                    string[] PointData = ConPoints[Pnt].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);//Равенства точек

                    string[] ParalConsumers = PointData[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);//Параллельные потребители
                    ConsumerPoint = new Consumer[ParalConsumers.Length][];
                    for (int Par = 0; Par < ParalConsumers.Length; Par++ )
                    {
                        string[] PointConsumers = ParalConsumers[Par].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);//Потребители линейной подцепи

                        Consumer[] ParArray = new Consumer[PointConsumers.Length];
                        for (int n = 0; n < PointConsumers.Length; n++ )
                        {
                            ParArray[n] = new Consumer
                            {
                                Point = Pnt,
                                Name = PointConsumers[n],
                                Type = TypeByName(PointConsumers[n]),
                            };
                        }

                        ConsumerPoint[Par] = ParArray;
                    }

                    Points.Add(Pnt, ConsumerPoint);
                }
            }*/

            ///

            DrawScheme(SchemePicture);
        }


        //Геометрия
        public static void DrawScheme(Bitmap SchemePicture)
        {
            //Заполнение массива точек ветвления
            Scheme.Clear(SchemePicture);
            int TruePointsCount = Points.Count() + 1;//Коичество точек + одна, на которую замыкаются потребители с предпоследней точки ветвления
            SchemePoints = new Point[TruePointsCount];

            int Width = SchemePicture.Width;
            int Height = SchemePicture.Height;
            int Pairs = (int)Math.Ceiling((double)SchemePoints.Length / 2);
            int PairsInterval = 200;//Интервал между парами точек
            int SchemeLinesOffset = 80;//Отступы от горизонтальной середины

            int FirstWPos = 0;
            for (int i = 0; i < Pairs; i++) 
            {
                int TopPos = (PairsInterval / 2) + (PairsInterval * i) - ((PairsInterval * Pairs) / 2);
                int BottomPos = -TopPos;

                //

                if (i == 0) FirstWPos = TopPos;//Запомянаем сдвиг первой точки
                if (TruePointsCount == Pairs + i + 1)BottomPos = FirstWPos;//Присваеваем сдвиг к последней точке

                //

                if (TruePointsCount > i) SchemePoints[i] = new Point(Width / 2 + TopPos, Height / 2 - SchemeLinesOffset);
                if (TruePointsCount > Pairs + i) SchemePoints[Pairs + i] = new Point(Width / 2 + BottomPos, Height / 2 + SchemeLinesOffset);
            }

            /*for (int i = 0; i < SchemePoints.Length; i++) 
            {
                Scheme.DrawPoint(SchemePoints[i], Program.SchemePicture);
            }*/

            //

            for (int i = 0; i < SchemePoints.Length - 1; i++) //Рисование того, что между точками
            {
                Consumer[][] PointConsumers = Points[i];

                //MessageBox.Show(PointConsumers.Length.ToString(), "");

                for (int p = 0; p < PointConsumers.Length; p++)//Параллели между точками
                {
                    GraphPattern[] Patterns = new GraphPattern[PointConsumers[p].Length];
                    string[] Names = new string[PointConsumers[p].Length];
                    //
                    for (int l = 0; l < PointConsumers[p].Length; l++)//Перебор линейных потребителей
                    {
                        Patterns[l] = PatternByName(PointConsumers[p][l].Name);
                        Names[l] = PointConsumers[p][l].Name;
                    }

                    DrawConsumers(SchemePoints[i], SchemePoints[i + 1], p, PointConsumers.Length, Patterns, Names, SchemePicture);
                }
            }

            string VoltageSign = "";
            GraphPattern SourcePattern;
            if (Physics.isDirect) 
            { 
                VoltageSign = "+" + Physics.U + " В";
                SourcePattern = PatternByName("S");
            }
            else 
            {
                VoltageSign = "~" + Physics.U + " В";
                SourcePattern = PatternByName("L");
            }

            DrawConsumers(SchemePoints[0], SchemePoints[SchemePoints.Length - 1], 0, 0, new GraphPattern[] { SourcePattern }, new string[] { VoltageSign /*"A+"*/ }, SchemePicture);
        }

        public static GraphPattern PatternByName(string Name)
        {
            GraphPattern Ptrn = Patterns.Wire;
            ///
            if (Name.Contains("R")) Ptrn = Patterns.Resistor;
            if (Name.Contains("C")) Ptrn = Patterns.Capacitor;
            if (Name.Contains("L")) Ptrn = Patterns.Inductor;
            if (Name.Contains("S")) Ptrn = Patterns.Source;
            ///
            return Ptrn;
        }

        public static string TypeByName(string Name)
        {
            string Type = "W";
            ///
            if (Name.Contains("R")) Type = "R";
            if (Name.Contains("C")) Type = "C";
            if (Name.Contains("L")) Type = "L";
            if (Name.Contains("S")) Type = "S";
            ///
            return Type;
        }

        public static void SetConsumer(ref Consumer Con, double Param) //Изменяет параметр потребителя в зависимоти от типа (получатся по имени)
        {
            if (TypeByName(Con.Name) == "R") Con.R = Param;
            if (TypeByName(Con.Name) == "C") Con.C = Param;
            if (TypeByName(Con.Name) == "L") Con.L = Param;
        }

        public static string UnitByName(string Name) //Имя физических единиц от имени потребителя
        {
            string Type = TypeByName(Name);
            string Unit = "";
            ///
            if (Type == "R") Unit = "Ом";
            if (Type == "C") Unit = "μФ";
            if (Type == "L") Unit = "Гн";
            ///
            return Unit;
        }

        public static Consumer[] GetConsumersByType(Consumer[] Consumers, string Type) 
        {
            List<Consumer> OutContumers = new List<Consumer>();

            for (int i = 0; i < Consumers.Length; i++)
            {
                if(TypeByName(Consumers[i].Name) == Type)OutContumers.Add(Consumers[i]);
            }

            return OutContumers.ToArray();
        }

        //Рисование приметивов
        public static void DrawPattern(Point Center, float Rotation, GraphPattern Pattern, string Name, Bitmap SchemePicture, float Scale = 1)
        {
            Graphics SchemeGraph = Graphics.FromImage(SchemePicture);
            Scale *= 40;

            for (int i = 0; i < Pattern.Vectors.Length; i += 4)
            {
                Point FirstPatternPt = RotatePoint(new Point((int)(Pattern.Vectors[i + 0] * Scale), (int)(Pattern.Vectors[i + 1] * Scale)), Rotation);
                Point SecondPatternPt = RotatePoint(new Point((int)(Pattern.Vectors[i + 2] * Scale), (int)(Pattern.Vectors[i + 3] * Scale)), Rotation);

                Point First = new Point(Center.X + FirstPatternPt.X, Center.Y + FirstPatternPt.Y);
                Point Second = new Point(Center.X + SecondPatternPt.X, Center.Y + SecondPatternPt.Y);

                SchemeGraph.DrawLine(new Pen(Pattern.Color, 1), First, Second);
            }

            if (Name != "")
            {
                SolidBrush Brush = new SolidBrush(Pattern.Color);
                //
                Point PosPoint = new Point((int)(-0.45f * Scale), (int)(-0.45f * Scale));
                PosPoint = RotatePoint(PosPoint, Rotation);
                //

                SchemeGraph.DrawString(Name, new Font("SegoeUI", 10), Brush, PosPoint.X + Center.X - 10, PosPoint.Y + Center.Y - 10);
            }

            ///
            Program.SchemePictureBox.Image = SchemePicture;
        }

        public static void DrawConsumers(Point FirstPos, Point SecondPos, int Offset, int Parallels, GraphPattern[] Patterns, string[] Names, Bitmap SchemePicture) 
        {
            int Angle = GetAngle(FirstPos, SecondPos);
            Point Center = new Point((FirstPos.X + SecondPos.X) / 2, (FirstPos.Y + SecondPos.Y) / 2);

            int ConDist = 40;
            if (Parallels != 0) Parallels--;
            int OffsetCoord = (Offset * ConDist) - (int)(Parallels * ConDist) / 2;

            Point PosOffset = RotatePoint(new Point(0, OffsetCoord), Angle);
            Center = new Point(Center.X + PosOffset.X, Center.Y + PosOffset.Y);

            //

            Point[] LinearOffsets = new Point[Patterns.Length];
            
            int LinearNumber = Patterns.Length;
            for (int i = 0; i < Patterns.Length; i++)
            {
                int LinearX = ((ConDist + 5) * i) - (int)(LinearNumber * ConDist)/2 + (ConDist/2);
                if (LinearNumber != 1) LinearX -= 5;

                LinearOffsets[i] = new Point(LinearX, 0);
                Point LinearOffsetRot = RotatePoint(LinearOffsets[i], Angle);

                DrawPattern(new Point(Center.X + LinearOffsetRot.X, Center.Y + LinearOffsetRot.Y), Angle, Patterns[i], Names[i], SchemePicture);
            }

            //

            for (int i = 0; i < LinearOffsets.Length - 1; i++) 
            {
                Point RotatedFirst = RotatePoint(new Point(LinearOffsets[i].X + 20 + 1, LinearOffsets[i].Y), Angle);
                Point RotatedSecond = RotatePoint(new Point(LinearOffsets[i + 1].X - 20 - 1, LinearOffsets[i + 1].Y), Angle);

                DrawLine(new Point(Center.X + RotatedFirst.X, Center.Y + RotatedFirst.Y), new Point(Center.X + RotatedSecond.X, Center.Y + RotatedSecond.Y), SchemePicture);
            }

            //

            Point FirstConPoint = RotatePoint(new Point(-20, 0), Angle);
            Point SecondConPoint = RotatePoint(new Point(20, 0), Angle);

            Point FirstEndPoint = RotatePoint(new Point(LinearOffsets[0].X - 20, LinearOffsets[0].Y), Angle);
            Point SecondEndPoint = RotatePoint(new Point(LinearOffsets[LinearOffsets.Length - 1].X + 20, LinearOffsets[LinearOffsets.Length - 1].Y), Angle);

            //
            Point Direction = new Point(0, 0);

            if (FirstEndPoint.X < SecondEndPoint.X) Direction.X = 1;
            if (FirstEndPoint.X > SecondEndPoint.X) Direction.X = -1;
            if (FirstEndPoint.Y < SecondEndPoint.Y) Direction.Y = 1;
            if (FirstEndPoint.Y > SecondEndPoint.Y) Direction.Y = -1;
            //

            Point FirstPosEnd = new Point(Center.X + FirstEndPoint.X, Center.Y + FirstEndPoint.Y);
            Point SecondPosEnd = new Point(Center.X + SecondEndPoint.X, Center.Y + SecondEndPoint.Y);

            if (isRightAngle(Angle))
            {
                DrawPath(FirstPos, FirstPosEnd, Direction, SchemePicture);
                DrawPath(SecondPos, SecondPosEnd, new Point(Direction.X * -1, Direction.Y * -1), SchemePicture);
            }
            else 
            {
                DrawLine(FirstPos, FirstPosEnd, SchemePicture);
                DrawLine(SecondPos, SecondPosEnd, SchemePicture);
            }
        }



        public static void DrawPoint(Point Center, Bitmap SchemePicture, float Scale = 1)
        {
            Graphics SchemeGraph = Graphics.FromImage(SchemePicture);
            ///
            Scale *= 3.5f;

            SchemeGraph.FillEllipse(Brushes.White, Center.X - (int)Math.Round(Scale), Center.Y - (int)Math.Round(Scale), (int)Math.Round(Scale) * 2, (int)Math.Round(Scale) * 2);
            ///
            Program.SchemePictureBox.Image = SchemePicture;
        }

        public static void DrawLine(Point FirstPos, Point SecondPos, Bitmap SchemePicture) 
        {
            Graphics SchemeGraph = Graphics.FromImage(SchemePicture);
            ///
            SchemeGraph.DrawLine(new Pen(Color.White), FirstPos, SecondPos);
            ///
            Program.SchemePictureBox.Image = SchemePicture;
        }

        public static void DrawPath(Point FirstPos, Point SecondPos, Point Direction, Bitmap SchemePicture)
        {
            Graphics SchemeGraph = Graphics.FromImage(SchemePicture);
            ///
            Point PathPoint1 = new Point(FirstPos.X + (10 * Direction.X), FirstPos.Y + (10 * Direction.Y));
            Point PathPoint2 = new Point((PathPoint1.X * Physics.Positive(Direction.X)) + (SecondPos.X * Physics.Positive(Direction.Y)), (SecondPos.Y * Physics.Positive(Direction.X)) + (PathPoint1.Y * Physics.Positive(Direction.Y)));//new Point(PathPoint1.X, SecondPos.Y);
            //
            SchemeGraph.DrawLine(new Pen(Color.White), FirstPos, PathPoint1);
            SchemeGraph.DrawLine(new Pen(Color.White), PathPoint1, PathPoint2);
            SchemeGraph.DrawLine(new Pen(Color.White), PathPoint2, SecondPos);
            //
            if (FirstPos.X != SecondPos.X && FirstPos.Y != SecondPos.Y) DrawPoint(PathPoint1, Program.SchemePicture);//Если точки в прямой линии, то не рисует
            ///
            Program.SchemePictureBox.Image = SchemePicture;
        }

        public static void Clear(Bitmap SchemePicture)
        {
            Graphics SchemeGraph = Graphics.FromImage(SchemePicture);
            ///
            SchemeGraph.FillRectangle(Brushes.Black, 0, 0, SchemePicture.Width, SchemePicture.Height);
            ///
            Program.SchemePictureBox.Image = SchemePicture;
        }
    }

    //Структуры и прочая лабудень

    public struct Consumer
    {
        public int Point;//Индекс массива потребителей
        public string Name;
        public string Type;

        ///Physics
        //Ввод
        public double R;
        public double C;
        public double L;
        //Вывод
        public double I;
        public double U;
        public double P;
    }

    //Graph
    public struct GraphPattern {
        public float[] Vectors;
        public Color Color;
    }

    public struct GraphPatterns {
        public GraphPattern Resistor;
        public GraphPattern Capacitor;
        public GraphPattern Inductor;
        public GraphPattern Source;
        public GraphPattern Wire;
    }

}
