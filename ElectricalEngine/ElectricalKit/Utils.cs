using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricalKit
{
    public class Utils
    {
        public static string ExampleScheme(int id)
        {
            string[] Markups = new string[] {
                "R1;\r\n" + 
                "R2, R3 | R4;\r\n" +  
                "R5;", 

                "R1, L1, C1;\r\n" + 
                "R2, L2, C2 | R3, L3, C3;\r\n" +  
                "R4;",

                "R1, C1;\r\n" +
                "R2 | R3, L1;\r\n" +
                "R4;\r\n" +
                "R5, R6, R7 | L2, C2;\r\n" +
                "R10;\r\n" +
                "R11 | R12, R13, R14 | L3, L4, L5 | R15, L6, C3, L7, C4;\r\n",
            };

            return Markups[id];
        }

        public static string RandomScheme() 
        {
            Random Rnd = new Random();

            string Scheme = "";
            //
            int PointsNumber = Random(Rnd, 3, 7);

            for (int p = 0; p < PointsNumber; p++ )
            {
                string Point = "";
                //
                int LinesNum = Random(Rnd, 1, 4);

                for (int l = 0; l < LinesNum; l++) 
                {
                    string Line = "";
                    //
                    int ConsNum = Random(Rnd, 1, 3);

                    for (int c = 0; c < ConsNum; c++)
                    {
                        string Con = RandomConsumer(Rnd);
                        //
                        if (c < ConsNum - 1) Con += ", ";
                        //
                        Line += Con;
                    }

                    if (l < LinesNum - 1) Line += " | ";

                    //
                    Point += Line;
                }

                //
                Scheme += Point + ";\r\n";
            }

            //
            RCount = LCount = CCount = 0;
            //

            return Scheme;
        }

        protected static int RCount, LCount, CCount;
        protected static string RandomConsumer(Random Rnd) 
        {
            int RndId = Random(Rnd, 0, 2);

            string Type = "";
            int Number = 0;

            if (RndId == 0) 
            {
                Type = "R";
                RCount++;
                Number = RCount;
            }

            if (RndId == 1)
            {
                Type = "C";
                CCount++;
                Number = CCount;
            }

            if (RndId == 2)
            {
                Type = "L";
                LCount++;
                Number = LCount;
            }

            return Type + Number;
        }

        protected static int Random(Random rnd, int a, int b) 
        {
            int c = rnd.Next(a, b + 1);

            return c;
        }
    }
}
