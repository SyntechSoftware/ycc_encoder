// YCC_encoder V 1.3.1 build 128
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   24.11.2009
// modified 22.01.2010

using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;

namespace YCC_encoder
{
    public partial class YCrCb
    {
        Zone[] zones;

        public partial class Zone 
        {
            class C
            {
                public int from = -1;
                public int to = -1;

                public C(int _from, int _to)
                {
                    from = _from;
                    to = _to;
                }

            }

            public void Puzzle(int stepSize, int dep, int dep_c, double dep_l)
            {

                int GridX = W / stepSize + 1;
                int GridY = H / stepSize + 1;

                int ch_size = body.Length;

                for (int y = 0; y < GridY; y++)
                    for (int x = 0; x < GridX; x++)
                    {
                        int c = 0;
                        double Q = 0;

                        for (int ay = y * stepSize; ay < (y + 1) * stepSize; ay++)
                        {
                            for (int ax = x * stepSize; ax < (x + 1) * stepSize; ax++)
                            {
                                int adr = ay * W + ax;
                                if (adr < ch_size && ax < W && ay < H)
                                {
                                    c++;
                                    Q = Q + body[adr];
                                }
                            }
                        }
                        Q = Q / c;

                        bool qt = true;

                        for (int ay = y * stepSize; ay < (y + 1) * stepSize; ay++)
                            if (qt)
                            for (int ax = x * stepSize; ax < (x + 1) * stepSize; ax++)
                            {
                                int adr = ay * W + ax;
                                if (adr < ch_size && ax < W && ay < H)
                                {
                                    if (Math.Abs(Q - body[adr]) > dep)
                                    {
                                        qt = false; break;
                                    }
                                }
                            }
                        if (qt)
                        {
                            byte QQ = (byte)(int)Q;
                            for (int ay = y * stepSize; ay < (y + 1) * stepSize; ay++)
                            {
                                for (int ax = x * stepSize; ax < (x + 1) * stepSize; ax++)
                                {
                                    int adr = ay * W + ax;
                                    if (adr < ch_size && ax < W && ay < H)
                                        body[adr] = QQ;
                                }
                            }
                        }
                    }

                int[] QQRR = new int [256];
                for (int z = 0; z < body.Length; z++)
                    QQRR[body[z]]++;

                C[] Qd = new C[QQRR.Length];
                int Qd_stack = 0;
                int L = 256;

                QQRR[1] = 0;
                QQRR[2] = 0;
                QQRR[13] = 0;
                QQRR[14] = 0;

                Qd[Qd_stack++] = new C(1, 0);
                Qd[Qd_stack++] = new C(2, 0);

                Qd[Qd_stack++] = new C(13, 15);
                Qd[Qd_stack++] = new C(14, 15);


                for (int i = 2; i < L - 2; i++)
                {
                    if (QQRR[i] > QQRR[i - 1] && QQRR[i - 1] > 0)
                        if ((double)QQRR[i - 1] / (double)QQRR[i] < dep_l)
                        {
                            Qd[Qd_stack++] = new C(i - 1, i);
                            QQRR[i - 1] = 0;
                        }
                    if (QQRR[i] > QQRR[i + 1] && QQRR[i + 1] > 0)
                        if ((double)QQRR[i + 1] / (double)QQRR[i] < dep_l)
                        {
                            Qd[Qd_stack++] = new C(i + 1, i);
                            QQRR[i + 1] = 0;
                        }

                }

                for (int z = 0; z < body.Length; z++)
                {
                    for (int i = 0; i < Qd_stack; i++)
                        if (body[z] == Qd[i].from)
                        {
                            body[z] = (byte)Qd[i].to;
                            break;
                        }

                    body[z] = (byte)((body[z] / dep_c) * dep_c);

                }

            }
        }

        public Zone getZone(int id)
        {
            if (id < ZoneQTY)
                return zones[id];
            else
                return null;
        }

        public class Hystogram
        {
            public int[] ln = new int[256];
            byte[] ch_;
            public int baseColor = -1;
            public byte baseColorByte = 0;
            public Hystogram() { }
            public Hystogram(byte[] ch)
            {
                for (int i = 0; i < ch.Length; i++)
                    ln[ch[i]]++;

                int max_v = 0;
                int max_i = -1;
                int max2_v = 0;
                int max2_i = -1;

                for (int i = 0; i < 256; i++)
                    if (max_v < ln[i])
                    {
                        max_v = ln[i];
                        max_i = i;
                    }

                for (int i = 0; i < 256; i++)
                    if (max2_v < ln[i] && i != max_i)
                    {
                        max2_v = ln[i];
                        max2_i = i;
                    }

                if ((double)max2_v / (double)max_v < .33)
                {
                    baseColorByte = (byte)max_i;
                    baseColor = max_i;
                }
                ch_ = ch;
            }
        }


        bool isBase = false;

        public YCrCb(string filename)
        {
            bytes = 0;
            chanel = 0;
            if (readBMP(filename) == -1) { bytes = 0; chanel = 0; width = 0; height = 0; }
        }
      
        public int readBMP(string filename)
        {
            try
            {
                Bitmap bmpSrc;

                bmpSrc = (Bitmap)Bitmap.FromFile(filename);
                BitmapData bmpData = bmpSrc.LockBits(new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height),
                    ImageLockMode.ReadOnly, bmpSrc.PixelFormat);
                IntPtr ptr = bmpData.Scan0;

                bytes = bmpData.Stride * bmpSrc.Height;
                int maxWidth = bmpData.Stride * 8;

                byte[] bValues = new byte[bytes - 1];
                System.Runtime.InteropServices.Marshal.Copy(ptr, bValues, 0, bytes - 1);
                bmpSrc.UnlockBits(bmpData);

                chanel = bytes / 3;

                R = new byte[chanel];
                G = new byte[chanel];
                B = new byte[chanel];

                Y = new byte[chanel];
                Cr = new byte[chanel];
                Cb = new byte[chanel];

                widthR = bmpSrc.Width;
                if (bmpData.Stride > (widthR + widthR))
                    width = bmpData.Stride / 3;
                else
                    width = bmpData.Stride;

                height = bmpSrc.Height;

                for (int i = 0; i < (chanel - 1); i++)
                {
                    int a = i * 3;

                    R[i] = bValues[a];
                    G[i] = bValues[a + 1];
                    B[i] = bValues[a + 2];
                }
                RGB2YCrCb();
            }
            catch (Exception exx)
            {
                ex = exx;
                return -1;
            }
            return 0;
        }

        public void RGB2YCrCb()
        {

            for (int i = 0; i < chanel; i++)
            {


                double R_ = (int)R[i];
                double G_ = (int)G[i];
                double B_ = (int)B[i];

                int Y_ = (int)Math.Round((R_ * .299 + G_ * .587 + B_ * .114), 0);
                int Cb_ = (int)Math.Round((R_ * -.1687 + G_ * -.3313 + B_ * .500 + 128), 0);
                int Cr_ = (int)Math.Round((R_ * .500 + G_ * -.4187 + B_ * -.0813 + 128), 0);

                if (Y_ > 255) Y_ = 255;
                if (Y_ < 0) Y_ = 0;
                if (Cb_ > 255) Cb_ = 255;
                if (Cb_ < 0) Cb_ = 0;
                if (Cr_ > 255) Cr_ = 255;
                if (Cr_ < 0) Cr_ = 0;

                Y[i] = (byte)Y_;
                Cr[i] = (byte)Cr_;
                Cb[i] = (byte)Cb_;

                //            Y = R *  .299 + G *  .587 + B *  .114; 
                //U = R * -.169 + G * -.332 + B *  .500 + 128.; 
                //V = R *  .500 + G * -.419 + B * -.0813 + 128.; 

            }
        }

        public void convMono()
        {

            for (int i = 0; i < (chanel - 1); i++)
            {
                R[i] = Y[i];
                G[i] = Y[i];
                B[i] = Y[i];
                Cr[i] = 128;
                Cb[i] = 128;
            }
        }

        public void convCr()
        {

            for (int i = 0; i < (chanel - 1); i++)
            {
                R[i] = Cr[i];
                G[i] = Cr[i];
                B[i] = Cr[i];
                Y[i] = Cr[i];
                Cr[i] = 128;
                Cb[i] = 128;
            }
        }

        public void convCb()
        {

            for (int i = 0; i < (chanel - 1); i++)
            {
                R[i] = Cb[i];
                G[i] = Cb[i];
                B[i] = Cb[i];
                Y[i] = Cb[i];
                Cr[i] = 128;
                Cb[i] = 128;
            }
        }

        public void getBaseColor()
        {
            Hystogram YH = new Hystogram(Y);
            Hystogram CrH = new Hystogram(Cr);
            Hystogram CbH = new Hystogram(Cb);

            if (YH.baseColor != -1 && CbH.baseColor != -1 && CrH.baseColor != -1)
            {
                YCrCbPoint n = new YCrCbPoint();
                n.Y = YH.baseColorByte;
                n.Cr = CrH.baseColorByte;
                n.Cb = CbH.baseColorByte;
                baseColor = n;
                isBase = true;
            }
        }

        public bool isMono()
        {
            return isMono(0.2);
        }

        public bool isMono(double avg)
        {
            int center = 128;
            int c = 0;

            for (int i = 0; i < chanel; i++)
            {
                if ((Cr[i] - center) > 2) c++;
                if ((Cb[i] - center) > 2) c++;
                if ((Cr[i] - center) > 50) c = c + 10;
                if ((Cb[i] - center) > 50) c = c + 10;
            }

            double dep = (double)(c / 2) / (double)chanel;

            if (dep > avg) return false; else return true;

        }

        public void drawBlock()
        {
            int gridSize = 32;

            for (int y = 0; y < height / gridSize + 1; y++)
                for (int x = 0; x < width / gridSize + 1; x++)
                {

                    for (int ay = y * gridSize; ay < (y + 1) * gridSize; ay++)
                    {
                        for (int ax = x * gridSize; ax < (x + 1) * gridSize; ax++)
                        {
                            int adr = ay * width + ax;
                            if (adr < chanel && ax < widthR && ay < height)
                            {
                                Y[adr] = (byte)(int)(GridFY[x, y]);
                                Cr[adr] = (byte)(int)(GridFCr[x, y]);
                                Cb[adr] = (byte)(int)(GridFCb[x, y]);
                            }
                        }
                    }


                    int ay_ = y * gridSize + 4;
                    for (int ax = x * gridSize + 2; ax < (x) * gridSize + 7; ax++)
                    {
                        int adr = ay_ * width + ax;
                        if (adr < chanel && ax < widthR && ay_ < height)
                        {
                            if (GridF_m[x, y, 0] > 0)
                                Y[adr] = (byte)(255 - Y[adr]);
                            if (GridF_m[x, y, 1] > 0)
                                Cr[adr] = (byte)(255 - Cr[adr]);
                            if (GridF_m[x, y, 2] > 0)
                                Cb[adr] = (byte)(255 - Cb[adr]);
                        }
                    }
                    int ax_ = x * gridSize + 4;
                    for (int ay = y * gridSize + 2; ay < (y) * gridSize + 7; ay++)
                    {
                        int adr = ay * width + ax_;
                        if (adr < chanel && ax_ < widthR && ay < height)
                        {
                            if (GridF_m[x, y, 0] > 0)
                                Y[adr] = (byte)(255 - Y[adr]);
                            if (GridF_m[x, y, 1] > 0)
                                Cr[adr] = (byte)(255 - Cr[adr]);
                            if (GridF_m[x, y, 2] > 0)
                                Cb[adr] = (byte)(255 - Cb[adr]);
                        }
                    }


                }
        }

        public void drawZone(int ch_number)
        {
            int gridSize = 32;
            byte[] CrC = new byte[10240];
            byte[] CbC = new byte[10240];

            System.Random r = new System.Random();

            CrC[0] = 128;
            CbC[0] = 128;

            for (int y = 0; y < height / gridSize + 1; y++)
                for (int x = 0; x < width / gridSize + 1; x++)
                {

                    for (int ay = y * gridSize; ay < (y + 1) * gridSize; ay++)
                    {
                        for (int ax = x * gridSize; ax < (x + 1) * gridSize; ax++)
                        {
                            int adr = ay * width + ax;
                            if (adr < chanel && ax < widthR && ay < height)
                            {
                                if (GridF_m[x, y, ch_number] > 1)
                                {
                                    if (CrC[GridF_m[x, y, ch_number]] == 0)
                                    {
                                        CrC[GridF_m[x, y, ch_number]] = (byte)(int)(r.Next(24)*10-120);
                                        CbC[GridF_m[x, y, ch_number]] = (byte)(int)(r.Next(24)*10-120);
                                    }

                                    Cr[adr] = CrC[GridF_m[x, y, ch_number]];
                                    Cb[adr] = CbC[GridF_m[x, y, ch_number]];
                                }
                            }
                        }
                    }
                }
        }

        public void makeBlock()
        {
            int gridSize = 32;
            GridX = width / gridSize + 1;
            GridY = height / gridSize + 1;

            GridFY = new double[GridX, GridY];
            GridFCr = new double[GridX, GridY];
            GridFCb = new double[GridX, GridY];
            GridF_m = new int[GridX, GridY, 3];

            for (int y = 0; y < GridY; y++)
                for (int x = 0; x < GridX; x++)
                {
                    int c = 0;
                    for (int ay = y * gridSize; ay < (y + 1) * gridSize; ay++)
                    {
                        for (int ax = x * gridSize; ax < (x + 1) * gridSize; ax++)
                        {
                            int adr = ay * width + ax;
                            if (adr < chanel && ax < widthR && ay < height)
                            {
                                c++;
                                GridFY[x, y] = GridFY[x, y] + Y[adr];
                                GridFCb[x, y] = GridFCb[x, y] + Cb[adr];
                                GridFCr[x, y] = GridFCr[x, y] + Cr[adr];
                            }
                        }
                    }

                    GridFY[x, y] = GridFY[x, y] / c;
                    GridFCr[x, y] = GridFCr[x, y] / c;
                    GridFCb[x, y] = GridFCb[x, y] / c;
                }

            if (baseColor != null)
            {
                for (int y = 0; y < GridY; y++)
                    for (int x = 0; x < GridX; x++)
                    {
                        if (GridFY[x, y] != baseColor.Y) GridF_m[x, y, 0]++;
                        if (GridFCr[x, y] != baseColor.Cr) GridF_m[x, y, 1]++;
                        if (GridFCb[x, y] != baseColor.Cb) GridF_m[x, y, 2]++;
                    }
            }
            else 
            {
                //double QY = 0;
                //double QCr = 0;
                //double QCb = 0;
                //int c = 0;
                //for (int y = 0; y < GridY; y++)
                //    for (int x = 0; x < GridX; x++)
                //    {
                //        QY=QY+GridFY[x,y];
                //        QCr=QCr+GridFCr[x,y];
                //        QCb=QCb+GridFCb[x,y];
                //        c++;
                //    }
                //QY = QY / c;
                //QCr = QCr / c;
                //QCb = QCb / c;

                for (int y = 0; y < GridY; y++)
                    for (int x = 0; x < GridX; x++)
                    {
                      //  if (Math.Abs(QY - GridFY[x, y]) > 10) GridF_m[x, y, 0]++;
                        GridF_m[x, y, 0]++;
                        //if (Math.Abs(QCr - GridFCr[x, y]) > 10) GridF_m[x, y, 1]++;
                        GridF_m[x, y, 1]++;
                        //if (Math.Abs(QCb - GridFCb[x, y]) > 10) GridF_m[x, y, 2]++;
                        GridF_m[x, y, 2]++;
                    }
            }


            RaizeBlock r = new RaizeBlock(GridX, GridY, GridF_m, 3,isBase);
           
            ZoneQTY = r.ZonesCb + r.ZonesCr + r.ZonesY-6;
            zones = new Zone[ZoneQTY];
            int z = 0;

            for (int i = 2; i < r.ZonesY; i++)
                zones[z++] = getZone(i, (r._ZoneGrids(i, 0,0)) * gridSize, (r._ZoneGrids(i, 1,0)+1) * gridSize, r._ZoneGrids(i, 2,0) * gridSize, (r._ZoneGrids(i, 3,0)+1)* gridSize, 0);
            for (int i = 2; i < r.ZonesCr; i++)
                zones[z++] = getZone(i, (r._ZoneGrids(i, 0,1)) * gridSize, (r._ZoneGrids(i, 1,1)+1) * gridSize, r._ZoneGrids(i, 2,1) * gridSize, (r._ZoneGrids(i, 3,1)+1) * gridSize, 1);
            for (int i = 2; i < r.ZonesCb; i++)
                zones[z++] = getZone(i, (r._ZoneGrids(i, 0,2)) * gridSize, (r._ZoneGrids(i, 1,2)+1) * gridSize, r._ZoneGrids(i, 2,2) * gridSize, (r._ZoneGrids(i, 3,2)+1) * gridSize, 2);
        }

        public byte[] getZoneBytes(int id, bool NoCRC)
        {

            Zone z = getZone(id);
            if (z == null) return new byte[0];

            if (z.W < 0 || z.H < 0)
                return new byte[0];
            z.Compress();

            int Xmin = z.X;
            int Ymin = z.Y;

            int Xmax = z.X + z.W;
            int Ymax = z.Y + z.H;

            int W = z.W;
            int H = z.H;

            int cn = z.chanel;

            byte[] rez = new byte[z.body.Length + 11];

            byte[] bX = Int16ToBytes(z.X);
            byte[] bY = Int16ToBytes(z.Y);
            byte[] bW = Int16ToBytes(z.W);
            byte[] bH = Int16ToBytes(z.H);
            if (!NoCRC ) z.calc_crc();

            byte[] bCRC = Int16ToBytesReal(z.crc);
            byte chh = C2V(z.chanel, z.comperss);

            rez[0] = bX[0];
            rez[1] = bX[1];

            rez[2] = bY[0];
            rez[3] = bY[1];

            rez[4] = bW[0];
            rez[5] = bW[1];

            rez[6] = bH[0];
            rez[7] = bH[1];
            rez[8] = bCRC[0];
            rez[9] = bCRC[1];
            rez[10] = chh;

            for (int i = 0; i < z.body.Length; i++)
                rez[i + 11] = z.body[i];

            return rez;
        }


        Zone getZone(int num, int Xmin, int Xmax, int Ymin, int Ymax, byte cn) 
        {
            Zone f = new Zone();
            f.X = Xmin;
            f.Y = Ymin;

            if (Ymax > height) Ymax = height;
            if (Xmax > widthR) Xmax = widthR;


            f.W = Xmax - Xmin;
            f.H = Ymax - Ymin;


            if (f.W == 0 || f.H == 0) return null;
            f.chanel = cn;

            f.body = new byte[(f.W+1) * (f.H+1)];
            for (int x = Xmin; x <= Xmax ; x++)
                for (int y = Ymin; y <= Ymax; y++)
                {
                    int adrI = y * width + x;
                    int adrO = (y - Ymin) * f.W + (x - Xmin);
                    if (adrI < chanel && x < widthR && y < height && (y - Ymin) < f.H && (x - Xmin) < f.W)
                    {
                        if (cn == 0)
                            f.body[adrO] = Y[adrI];
                        if (cn == 1)
                            f.body[adrO] = Cr[adrI];
                        if (cn == 2)
                            f.body[adrO] = Cb[adrI];
                    }
                }
            if (cn != 0)
                f.Puzzle(5, 7,3,.85);
            else
                f.Puzzle(3, 4,2,.6);
            return f;
            
        }
    }

}
