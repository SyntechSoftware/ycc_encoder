// YCC_encoder//YCC_decoder V 1.3.1 build 128
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   24.11.2009
// modified 29.01.2010

using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;

namespace YCC_encoder
{
    // Основной класс (объект) для работы с форматом YCC

    public partial class YCrCb
    {
        // размер
        int bytes;
        // каналов (1,3)
        public int chanel;
        // ширина
        public int width;
        // с выравниванием под 64 бита
        public int widthR;
        // высота
        public int height;

        public int ZoneQTY;

        // Объект содержащий фрагмент изображения
        public partial class Zone 
        {
            public byte[] body ;
            public int X;
            public int Y;
            public int H;
            public int W;
            public byte chanel;
            public bool comperss = false;
            public UInt16 crc;

            // сжатие массива байт
            private  byte[] inCompress(byte[] data)
            {
                MemoryStream output = new MemoryStream();
                GZipStream gzip = new GZipStream(output, CompressionMode.Compress, true);
                gzip.Write(data, 0, data.Length);
                gzip.Close();
                return output.ToArray();
            }

            // распаковка массива байт
            private byte[] inDecompress(byte[] data)
            {
                MemoryStream input = new MemoryStream();
                input.Write(data, 0, data.Length);
                input.Position = 0;
                GZipStream gzip = new GZipStream(input, CompressionMode.Decompress, true);
                MemoryStream output = new MemoryStream();
                byte[] buff = new byte[128];
                int read = -1;
                read = gzip.Read(buff, 0, buff.Length);
                while (read > 0)
                {
                    output.Write(buff, 0, read);
                    read = gzip.Read(buff, 0, buff.Length);
                }
                gzip.Close();
                return output.ToArray();
            }
            
            // функция-обертка сжатия
            public void Compress() 
            {
                body = inCompress(body);
            }

            // функция-обертка разжатия
            public void Decompress()
            {
                body = inDecompress(body);
            }
            
            // функция расчета контрольной суммы
            static UInt16 CRC(byte[] message)
            {
                UInt16 res = 0;
                byte q = 152;
                for (int i = 0; i < message.Length; i++)
                  {
                     res += (UInt16)(int)(q * message[i] * i);
                     q = message[i];
                  }
                if (res == 0) res = 1;
                return res;
            }

            // функция проверки контрольной суммы
            public bool check_crc()
            {
                if (crc == 0)
                    return true;
                else
                {
                    if (crc == CRC(body)) return true; else return false;
                }
            }

            // функция-обертка для рассчета контрольной суммы
            public void calc_crc() 
            {
                crc = CRC(body);
            }

        }

        // Объект описание пикселя
        public class YCrCbPoint
        {
            public byte Y = 0;
            public byte Cr = 128;
            public byte Cb = 128;
            
            // Простой конструктор (по-умолчанию)
            public YCrCbPoint(){}

            // Конструктор на описанию значения
            public YCrCbPoint(byte Y_, byte Cr_, byte Cb_)
            {
                Y = Y_;
                Cr = Cr_;
                Cb = Cb_;
            }

            // переопределенная  функция для представления класса как строки
            public override string ToString()
            {
                return "Y=" + Y.ToString() + ";Cr=" + Cr.ToString() + ";Cb=" + Cb.ToString() + ";";

            }

        }

        // Доступный снаружи описатель ошибки      
        public Exception ex;

        // каналы RGB
        byte[] R;
        byte[] G;
        byte[] B;

        // Каналі YCrCb
        byte[] Y;
        byte[] Cr;
        byte[] Cb;

        // массив наличия изборажения в пределах кусочков картинки
        double[,] GridFY;
        double[,] GridFCr;
        double[,] GridFCb;
        
        // массив границ областей
        int[, ,] GridF_m;

        // количество кусочков по высоте и ширине
        int GridX;
        int GridY;

        // цвет фона
        public YCrCbPoint baseColor;

        // простой конструктор - инициализация класса без содержимого
        public YCrCb()
        {
            bytes = 0;
            chanel = 0;
            width = 0;
            height = 0;
        }

        // конструктор - инициализация класса с созданием изображения с заданными характеристиками
        public YCrCb(int len, int width_, int height_, int widthR_, YCrCbPoint baseColor_)
        {
            baseColor = baseColor_;
            width = width_;
            widthR = widthR_;
            height = height_;
            GridX = width_ / 32 + 1;
            GridY = height_ / 32 + 1;
            
            bytes = len * 3;
            chanel = len;

            R = new byte[chanel];
            G = new byte[chanel];
            B = new byte[chanel];

            Y = new byte[chanel];
            Cr = new byte[chanel];
            Cb = new byte[chanel];

            for (int i = 0; i < (chanel - 1); i++)
            {
                if (baseColor != null)
                {
                    Y[i] = baseColor.Y;
                    Cr[i] = baseColor.Cr;
                    Cb[i] = baseColor.Cb;
                }
                else
                {
                    Y[i] = 0;
                    Cr[i] = 128;
                    Cb[i] = 128;
                }
            }

            GridFY = new double[GridX, GridY];
            GridFCr = new double[GridX, GridY];
            GridFCb = new double[GridX, GridY];
            GridF_m = new int[GridX, GridY, 3];
        }

        // создание класса с полным объемом данных используется для копирования
        public YCrCb(byte[] Y_, byte[] Cr_, byte[] Cb_, int len, int width_, int height_, int widthR_, int GridX_, int GridY_, double[,] GridFY_, double[,] GridFCr_, double[,] GridFCb_, YCrCbPoint baseColor_, int[, ,] GridF_m_)
        {

            baseColor = baseColor_;
            width = width_;
            widthR = widthR_;
            height = height_;
            GridX = GridX_;
            GridY = GridY_;

            bytes = len * 3;
            chanel = len;

            R = new byte[chanel];
            G = new byte[chanel];
            B = new byte[chanel];

            Y = new byte[chanel];
            Cr = new byte[chanel];
            Cb = new byte[chanel];

            for (int i = 0; i < (chanel - 1); i++)
            {
                Y[i] = Y_[i];
                Cr[i] = Cr_[i];
                Cb[i] = Cb_[i];
            }

            GridFY = new double[GridX, GridY];
            GridFCr = new double[GridX, GridY];
            GridFCb = new double[GridX, GridY];
            GridF_m = new int[GridX, GridY, 3];

            for (int x = 0; x < GridX; x++)
                for (int y = 0; y < GridY; y++)
                {
                    GridFY[x, y] = GridFY_[x, y];
                    GridFCr[x, y] = GridFCr_[x, y];
                    GridFCb[x, y] = GridFCb_[x, y];
                    GridF_m[x, y, 0] = GridF_m_[x, y, 0];
                    GridF_m[x, y, 1] = GridF_m_[x, y, 1];
                    GridF_m[x, y, 2] = GridF_m_[x, y, 2];
                }

        }

        // функция копирования (создания нового независимого экземпляра)
        public YCrCb GetCopy()
        {

            return new YCrCb(Y, Cr, Cb, chanel, width, height, widthR, GridX, GridY, GridFY, GridFCr, GridFCb, baseColor, GridF_m);
        }
        // функция проверки корректности объекта. В данной реализации очень примитивная
        public bool IsCorrect()
        {
            if (bytes > 0) return true; else return false;
        }

        // Обертка для метода writeBMP
        public int save(string filename)
        {
            return writeBMP(filename);
        }

        // Запись изображения в формат BMP
        public int writeBMP(string filename)
        {
            if (bytes == 0) return -2;
            try
            {
                byte[] rgbValues = new byte[bytes];
                for (int i = 0; i < (chanel - 1); i++)
                {
                    int a = i * 3;
                    rgbValues[a] = R[i];
                    rgbValues[a + 1] = G[i];
                    rgbValues[a + 2] = B[i];

                }

                Bitmap destBmp = new Bitmap(widthR, height, PixelFormat.Format24bppRgb);
                BitmapData bmpData2 = destBmp.LockBits(new Rectangle(0, 0, destBmp.Width, destBmp.Height),
                        ImageLockMode.ReadOnly, destBmp.PixelFormat);
                IntPtr ptr2 = bmpData2.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr2, bytes - 1);
                destBmp.UnlockBits(bmpData2);
                destBmp.Save(filename, ImageFormat.Bmp);
            }
            catch (Exception exx)
            {
                ex = exx; return -1;
            }
            return 0;
        }

        // Конвертация из YCrCb в RGB
        public void YCrCb2RGB()
        {


            for (int i = 0; i < chanel; i++)
            {

                double Y_ = Y[i];
                double Cr_ = Cr[i];
                double Cb_ = Cb[i];

                int R_ = (int)Math.Round(Y_ + (1.402 * (Cr_ - 128)), 0);
                int G_ = (int)Math.Round(Y_ - (0.3455 * (Cb_ - 128) + (0.7169 * (Cr_ - 128))), 0);
                int B_ = (int)Math.Round(Y_ + (1.772 * (Cb_ - 128)), 0);

                if (R_ > 255) R_ = 255;
                if (R_ < 0) R_ = 0;
                if (G_ > 255) G_ = 255;
                if (G_ < 0) G_ = 0;
                if (B_ > 255) B_ = 255;
                if (B_ < 0) B_ = 0;

                R[i] = (byte)R_;
                G[i] = (byte)G_;
                B[i] = (byte)B_;
            }
        }

        // перегруженная функция отрисовки области из массива байт
        public bool setZone(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream storageStream = new MemoryStream(data))
            using (GZipStream gzipStream = new GZipStream(storageStream, CompressionMode.Decompress))
            {
                return setZone((Zone)bf.Deserialize(gzipStream));
            }
        }

        // перегруженная функция отрисовки области из описания области
        public bool setZone(Zone z)
        {
            if (z == null) return false;
            int Xmin = z.X;
            int Ymin = z.Y;
            int Xmax = z.X + z.W;
            int Ymax = z.Y + z.H;
            int W = z.W;
            int H = z.H;
            int cn = z.chanel;

            for (int x = Xmin; x <= Xmax; x++)
                for (int y = Ymin; y <= Ymax; y++)
                {
                    int adrI = y * width + x;
                    int adrO = (y - Ymin) * W + (x - Xmin);
                    if (adrI < chanel && x < widthR && y < height && (y - Ymin) < H && (x - Xmin) < W)
                    {
                        if (cn == 0)
                            Y[adrI] = z.body[adrO];
                        if (cn == 1)
                            Cr[adrI] = z.body[adrO];
                        if (cn == 2)
                            Cb[adrI] = z.body[adrO];
                    }
                }

            return true;
        }

        // Конвертация
        private byte[] Int16ToBytes(int V)
        {
            byte[] R = new byte[2];

            int w1 = V / 256;
            int w2 = V - w1 * 256;

            R[0] = (byte)w1;
            R[1] = (byte)w2;
            return R;
        }

        // Конвертация
        static byte[] Int16ToBytesReal(UInt16 V)
        {
            byte[] R = new byte[2];

            UInt16 w1 = (UInt16)(V / 256);
            UInt16 w2 = (UInt16)(V - w1 * 256);

            R[0] = (byte)w1;
            R[1] = (byte)w2;
            return R;
        }

        // Конвертация
        static UInt16 BytesToInt16Real(byte[] R)
        {
            if (R.Length < 2) return 0;

            return (UInt16)(R[0] * 256 + R[1]);
        }

        // Конвертация
        private int BytesToInt16(byte[] R)
        {
            if (R.Length < 2) return 0;

            return R[0] * 256 + R[1];
        }

        // Конвертация
        private byte C2V(int v1, bool v2)
        {
            int qq;

            if (v2) qq = 0; else qq = 1;

            return (byte)(int)(v1 * 2 + qq);

        }

        // Конвертация
        private bool V2Cb(byte Q)
        {
            int qq = Q % 2;
            if (qq == 0) return true; else return false;
        }

        // Конвертация
        private int V2Ci(byte Q)
        {
            int qq = Q % 2;
            return (Q - qq) / 2;
        }


        // извлечение области из массива байт и её отрисовка
        public bool setZoneBytes(byte[] rez, bool debug)
        {
            Zone z = new Zone();
            byte[] bb = new byte[2];
            bb[0] = rez[0]; bb[1] = rez[1];
            z.X = BytesToInt16(bb);

            bb[0] = rez[2]; bb[1] = rez[3];
            z.Y = BytesToInt16(bb);

            bb[0] = rez[4]; bb[1] = rez[5];
            z.W = BytesToInt16(bb);

            bb[0] = rez[6]; bb[1] = rez[7];
            z.H = BytesToInt16(bb);

            byte q = rez[8];
            z.comperss = V2Cb(q);
            z.chanel = (byte)V2Ci(q);
            z.body = new byte[rez.Length - 9];
            for (int i = 0; i < z.body.Length; i++)
                z.body[i] = rez[i + 9];

            if (debug)
            {

                Console.WriteLine("X:" + z.X + " Y:" + z.Y + " W:" + z.W + " H:" + z.H);
                string QQ = "Type: ";
                if (z.chanel == 0) QQ = QQ + "Y;";
                if (z.chanel == 1) QQ = QQ + "Cr;";
                if (z.chanel == 2) QQ = QQ + "Cb;";
                if (z.comperss) QQ = QQ + " Compressed";
                Console.WriteLine(QQ);
            }

            z.Decompress();

            if (debug)
            {
                Console.WriteLine("Real ch. size: " + z.body.Length);
            }

            return setZone(z);
        }


        // извлечение области из массива байт и её отрисовка для версии формата 3
        public int setZoneBytesV3(byte[] rez, bool debug, int VV, bool ch_crc)
        {
            if (rez == null)
                rez = new byte[0];

            if (rez.Length == 0 ) 
            {
                if (VV == 4) 
                {
                        Console.WriteLine("Corrupt stream on encoded file. Wrong passowrd?");
                }
                return 4;
            }

            Zone z = new Zone();
            byte[] bb = new byte[2];
            bb[0] = rez[0]; bb[1] = rez[1];
            z.X = BytesToInt16(bb);

            bb[0] = rez[2]; bb[1] = rez[3];
            z.Y = BytesToInt16(bb);

            bb[0] = rez[4]; bb[1] = rez[5];
            z.W = BytesToInt16(bb);

            bb[0] = rez[6]; bb[1] = rez[7];
            z.H = BytesToInt16(bb);

            bb[0] = rez[8]; bb[1] = rez[9];
            z.crc = BytesToInt16Real(bb);

            byte q = rez[10];
            z.comperss = V2Cb(q);
            z.chanel = (byte)V2Ci(q);
            z.body = new byte[rez.Length - 11];
            for (int i = 0; i < z.body.Length; i++)
                z.body[i] = rez[i + 11];

                if (z.check_crc() != true)
                {
                    Console.WriteLine("CRC error");
                    if (ch_crc)
                        return 5;
                }

            if (debug)
            {

                Console.WriteLine("X:" + z.X + " Y:" + z.Y + " W:" + z.W + " H:" + z.H);
                string QQ = "Type: ";
                if (z.chanel == 0) QQ = QQ + "Y;";
                if (z.chanel == 1) QQ = QQ + "Cr;";
                if (z.chanel == 2) QQ = QQ + "Cb;";
                if (z.comperss) QQ = QQ + " Compressed";
                Console.WriteLine(QQ);
            }

            z.Decompress();

            if (debug)
            {
                Console.WriteLine("Real ch. size: " + z.body.Length);
            }

            if (setZone(z)) return 0; else return 3;

        }

    }

}
