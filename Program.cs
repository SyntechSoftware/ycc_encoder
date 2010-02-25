using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace YCC_encoder
{
      class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;
            if (args.Length < 2)
            {
                WriteHelp();
                return 2;
            }

            YCrCb c = new YCrCb(args[0]);
            if (!c.IsCorrect())
            {
                Console.WriteLine("File read error!");
                return 2;
            }

            c.RGB2YCrCb();

            bool debug = false;
            string pass = "";
            bool uPass = false;
            bool crc = false;

            foreach (string s in args)
            {
                if (s.ToUpper() == "-CRCNO") crc = true;
                if (s.ToUpper() == "-H" || s.ToUpper() == "-?" || s.ToUpper() == "/?") { WriteHelp(); return 0; }

                if (s.ToUpper() == "-DEBUG" || s.ToUpper() == "-D") debug = true;
                if (s.Substring(0, 2).ToUpper() == "-P")
                {
                    pass = s.Substring(2);
                    uPass = true;
                }
            }
            


            YCrCb tmp = c.GetCopy();
            if (debug)
            {
                tmp.convMono();
                tmp.writeBMP("Y.bmp");
                tmp = c.GetCopy();
                tmp.convCr();
                tmp.writeBMP("Cr.bmp");
                tmp = c.GetCopy();
                tmp.convCb();
                tmp.writeBMP("Cb.bmp");
            }

            //if (c.isMono())
            //    c.convMono();
            
            c.getBaseColor();

            c.makeBlock();

            if (debug)
            {
                tmp = c.GetCopy();
                tmp.drawBlock();
                tmp.convMono();
                tmp.writeBMP("Y_b.bmp");

                tmp = c.GetCopy();
                tmp.drawBlock();
                tmp.convCr();
                tmp.writeBMP("Cr_b.bmp");

                tmp = c.GetCopy();
                tmp.drawBlock();
                tmp.convCb();
                tmp.writeBMP("Cb_b.bmp");

                tmp = c.GetCopy();
                tmp.convMono();
                tmp.drawZone(0);
                tmp.YCrCb2RGB();
                tmp.writeBMP("Y_Z.bmp");

                tmp = c.GetCopy();
                tmp.convCr();
                tmp.drawZone(1);
                tmp.YCrCb2RGB();
                tmp.writeBMP("Cr_Z.bmp");

                tmp = c.GetCopy();
                tmp.convCb();
                tmp.drawZone(2);
                tmp.YCrCb2RGB();
                tmp.writeBMP("Cb_Z.bmp");
            }

            System.IO.FileStream fs = new System.IO.FileStream(args[1]+".YCC", System.IO.FileMode.Create);

            
            byte[] bInt ;
            bInt= BitConverter.GetBytes('Y'); fs.Write(bInt,0,1);
            bInt = BitConverter.GetBytes('C'); fs.Write(bInt,0,1);
            bInt = BitConverter.GetBytes('C'); fs.Write(bInt, 0, 1);
            if (uPass)
                bInt[0] = (byte)4;
                else
                bInt[0] = (byte)3;
            fs.Write(bInt, 0, 1);

            bInt = BitConverter.GetBytes(c.chanel); fs.Write(bInt, 0, 4);
            bInt = BitConverter.GetBytes(c.width); fs.Write(bInt, 0, 4);
            bInt = BitConverter.GetBytes(c.height); fs.Write(bInt, 0, 4);
            bInt = BitConverter.GetBytes(c.widthR); fs.Write(bInt, 0, 4);

            byte[] bB = new byte[4];
            
            if (c.baseColor != null)
            {
                bB[0] = (byte)1;
                bB[1] = c.baseColor.Y;
                bB[2] = c.baseColor.Cr;
                bB[3] = c.baseColor.Cb;
            }
            else
                bB[0] = (byte)0;

                fs.Write(bB,0,4);

            _Rijndael crpt = new _Rijndael();
            crpt.Key = pass;

            for (int i = 0; i < c.ZoneQTY; i++) 
            {
                byte[] bQQ = c.getZoneBytes(i,crc);
                byte[] b;
                if (bQQ.Length > 0)
                {
                    if (uPass)
                        b = crpt.Encrypt(bQQ);
                    else
                        b = bQQ;

                    try
                    {
                        int Q = b.Length;
                        byte[] bQ = BitConverter.GetBytes(b.Length);

                        fs.Write(bQ, 0, bQ.Length);
                        fs.Write(b, 0, b.Length);
                    }
                    catch /*(Exception ex)*/{
                        exitCode = 3;
                    }
                    finally
                    {
                    }
                }
            }
            fs.Close();
            return exitCode ;

        }
        static void WriteHelp() 
        {
            Console.WriteLine("YCC encoder V 1.3.1");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("");
            Console.WriteLine("YCC_encoder Source Destignation [Flag(s)]");
            Console.WriteLine("");
            Console.WriteLine("Source - is a file, an image will compress from which(recomend BMP24, but not necessarily).");
            Console.WriteLine("Destignation - the file(without extension) name in which will be written result.");
            Console.WriteLine("Flags:");
            Console.WriteLine("-d(ebug) - debug mode: showing the of additional information.");
            Console.WriteLine("-nocrc - not to calculate check sum.");
            Console.WriteLine("-p%PASSWORD% - protect data in file by %PASSWORD%.");

        }
    }
    }

