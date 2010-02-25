// YCC_encoder V 1.3.1 build 128
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   24.11.2009
// modified 18.12.2009

using System;
using System.Collections.Generic;
using System.Text;

namespace YCC_encoder
{
    class RaizeBlock
    {
        public int ZonesY = 0;
        public int ZonesCr = 0;
        public int ZonesCb = 0;

        static int[,] ZoneGridsY;
        static int[,] ZoneGridsCr;
        static int[,] ZoneGridsCb;


        public int _ZoneGrids(int F, int L, int O)
        {
            if (O == 0)
                return ZoneGridsY[F, L];
            else if (O == 1)
                return ZoneGridsCr[F, L];
            else 
                return ZoneGridsCb[F, L];
        }

        public RaizeBlock(int GridX, int GridY, int[, ,] GridF_m, int DEP, bool isBase)
        {

            for (int Q = 0; Q < DEP; Q++)
            {
                int cn = 1;

                int XC = 0;
                int YC = 0;
                int lvlC = 1;

                //writeX(GridX, GridY, GridF_m, Q, 1);
                while (cn != 0)
                {
                    cn = 0;

                    for (int y = 0; y < GridY; y++)
                        for (int x = 0; x < GridX; x++)
                            if (GridF_m[x, y, Q] == 1) cn = cn + GridF_m[x, y, Q];

                    if (cn > 0)
                    {
                        while (GridF_m[XC, YC, Q] != 1)
                        {
                            XC++;
                            if (XC >= GridX)
                            {
                                XC = 0;
                                YC++;
                            }
                        }

                        int l = 0;
                        lvlC++;
                        chV(GridX, GridY, XC, YC, lvlC, GridF_m, l, Q);
                    }
                    else
                        break;

                //    writeX(GridX, GridY, GridF_m, Q, 1);
                }
                //writeX(GridX, GridY, GridF_m, Q, 1);

                lvlC++;

                for (int i = 2; i <= lvlC; i++)
                    lvlC = FlexX(GridX, GridY, GridF_m, Q, lvlC, i);
            //    writeX(GridX, GridY, GridF_m, Q, 1);

                for (int i = 2; i <= lvlC; i++)
                    lvlC = FlexY(GridX, GridY, GridF_m, Q, lvlC, i);
            //   writeX(GridX, GridY, GridF_m, Q, 1);
                if (Q == 0)
                    ZoneGridsY = new int[lvlC + 1, 4];
                else if (Q == 1)
                    ZoneGridsCr = new int[lvlC + 1, 4];
                else
                    ZoneGridsCb = new int[lvlC + 1, 4];

                for (int i = 2; i <= lvlC; i++)
                   if (Q == 0)
                       make_zone(GridX, GridY, GridF_m, Q, i, ZoneGridsY);
                   else if (Q == 1)
                       make_zone(GridX, GridY, GridF_m, Q, i, ZoneGridsCr);
                   else
                        make_zone(GridX, GridY, GridF_m, Q, i, ZoneGridsCb);
               // writeX(GridX, GridY, GridF_m, Q, 1);

                if (Q == 0)
                    ZonesY = lvlC;
                if (Q == 1)
                    ZonesCr = lvlC;
                if (Q == 2)
                    ZonesCb = lvlC;
            }
        }

        static void make_zone(int GridX, int GridY, int[, ,] GridF_m, int Q, int F, int[,] ZoneGrids)
        {
            int Xmin = GridX;
            int Ymin = GridY;
            int Xmax = 0;
            int Ymax = 0;

            for (int y = 0; y < GridY; y++)
                for (int x = 0; x < GridX; x++)
                    if (GridF_m[x, y, Q] == F)
                    {
                        if (x > Xmax) Xmax = x;
                        if (y > Ymax) Ymax = y;
                        if (x < Xmin) Xmin = x;
                        if (y < Ymin) Ymin = y;
                    }
            for (int y = 0; y < GridY; y++)
                for (int x = 0; x < GridX; x++)
                    if (x >= Xmin && x <= Xmax)
                        if (y >= Ymin && y <= Ymax)
                            GridF_m[x, y, Q] = F;
            
            ZoneGrids[F, 0] = Xmin;
            ZoneGrids[F, 1] = Xmax;
            ZoneGrids[F, 2] = Ymin;
            ZoneGrids[F, 3] = Ymax;

        }

        static void writeX(int GridX, int GridY, int[, ,] GridF_m, int Q, int V)
        {
            
            int[] tmpX = new int[GridY];
            int[] tmpY = new int[GridX];

            for (int y = 0; y < GridY; y++)
            {
                tmpX[y] = 0;
                for (int x = 0; x < GridX; x++)
                    if (GridF_m[x, y, Q] == V) { tmpX[y]++; }
            }

            for (int x = 0; x < GridX; x++)
            {
                tmpY[x] = 0;
                for (int y = 0; y < GridY; y++)
                    if (GridF_m[x, y, Q] == V) { tmpY[x]++; }
            }
            Console.Clear();
            Console.WriteLine("Ch#" + Q);
            for (int x = 0; x < GridX; x++)
            {
                for (int y = 0; y < GridY; y++)
                    Console.Write(GridF_m[x, y, Q]);

                Console.WriteLine("  " + tmpY[x]);
            }

            Console.WriteLine();
            for (int y = 0; y < GridY; y++)
                Console.Write(tmpX[y]);
            System.Threading.Thread.Sleep(1000);
        }

        static int cmpPRC(int l1, int l2)
        {

            int l1w = l1;
            int l2w = l2;

            if (l1w > l2w)
            {
                int tmp = l1w;
                l1w = l2w;
                l2w = tmp;
            }

            int chng = l2w - l1w; //Отклонение

            if (l2w == 0)
                return 0; // Если большее = 0 тогда прирост = 0
            else
                return (int)((double)chng / (double)l2w * 100); // Делим отклонение на большее

        }

        static int FlexX(int GridX, int GridY, int[, ,] GridF_m, int Q, int lvlC, int F)
        {

            int[] tmpY = new int[GridX];

            for (int x = 0; x < GridX; x++)
            {
                tmpY[x] = 0;
                for (int y = 0; y < GridY; y++)
                    if (GridF_m[x, y, Q] == F) { tmpY[x]++; }
            }

            int l = tmpY[0];
            int b = F;
            int nb = b;
            for (int x = 0; x < GridX; x++)
            {
                if (cmpPRC(l, tmpY[x]) > 30 && cmpPRC(l, tmpY[x]) < 100  && Math.Abs(l - tmpY[x]) > 1)
                    nb = lvlC++;
                if (b != nb)
                    for (int y = 0; y < GridY; y++)
                        if (GridF_m[x, y, Q] == F) { GridF_m[x, y, Q] = nb; }
                l = tmpY[x];
            }
            return lvlC;
        }

        static int FlexY(int GridX, int GridY, int[, ,] GridF_m, int Q, int lvlC, int F)
        {

            int[] tmpX = new int[GridY];

            for (int y = 0; y < GridY; y++)
            {
                tmpX[y] = 0;
                for (int x = 0; x < GridX; x++)
                    if (GridF_m[x, y, Q] == F) { tmpX[y]++; }
            }

            int l = tmpX[0];
            int b = F;
            int nb = b;
            for (int y = 0; y < GridY; y++)
            {
                if (cmpPRC(l, tmpX[y]) > 30 && cmpPRC(l, tmpX[y]) < 100 && Math.Abs(l - tmpX[y]) > 1)
                    nb = lvlC++;
                if (b != nb)
                    for (int x = 0; x < GridX; x++)
                        if (GridF_m[x, y, Q] == F) { GridF_m[x, y, Q] = nb; }
                l = tmpX[y];
            }
            return lvlC;
        }

        static void chV(int GridX, int GridY, int XC, int YC, int lvlC, int[, ,] GridF_m, int l, int Q)
        {

            if (l > 1000) return;

            if (YC > -1 && YC < GridY)
                if (XC > -1 && XC < GridX)
                    if (GridF_m[XC, YC, Q] == 1)
                    {
                        GridF_m[XC, YC, Q] = lvlC;
                        chV(GridX, GridY, XC, YC + 1, lvlC, GridF_m, l + 1, Q);
                        chV(GridX, GridY, XC, YC - 1, lvlC, GridF_m, l + 1, Q);

                        chV(GridX, GridY, XC + 1, YC + 1, lvlC, GridF_m, l + 1, Q);
                        chV(GridX, GridY, XC + 1, YC - 1, lvlC, GridF_m, l + 1, Q);

                        chV(GridX, GridY, XC - 1, YC + 1, lvlC, GridF_m, l + 1, Q);
                        chV(GridX, GridY, XC - 1, YC - 1, lvlC, GridF_m, l + 1, Q);

                        chV(GridX, GridY, XC + 1, YC, lvlC, GridF_m, l + 1, Q);
                        chV(GridX, GridY, XC - 1, YC, lvlC, GridF_m, l + 1, Q);

                    }
        }

    }

}
