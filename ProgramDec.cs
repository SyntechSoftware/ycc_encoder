// YCC_encoder//YCC_decoder V 1.3.1 build 128
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   24.11.2009
// modified 22.01.2010

using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace YCC_encoder
{
      class ProgramDec
    {

        static int exitCode = 0;
        
          // вывод помощи
          static void WriteHelp()
          {
              Console.WriteLine("YCC decoder V 1.3.1");
              Console.WriteLine("");
              Console.WriteLine("Usage:");
              Console.WriteLine("");
              Console.WriteLine("YCC_decoder Source [Flag(s)]");
              Console.WriteLine("");
              Console.WriteLine("Source - is a file, an image will decompress from which.");
              Console.WriteLine("Flags:");
              Console.WriteLine("-d(ebug) - debug mode: showing the additional information.");
              Console.WriteLine("-nocrc - ignore CRC-check errors.");
              Console.WriteLine("-p%PASSWORD% - if data was protected in file by %PASSWORD%.");

          }

        // основная функция чтения файла
        static int Main(string[] args)
        {
            // Если нехватает параметров - выводим "помощь"
            if (args.Length < 1)
            {
                WriteHelp();
                return 2;
            }


            bool debug = false;
            string pass = "";
            bool ch_crc = true;

            foreach (string s in args)
            {
                if (s.ToUpper() == "-NOCRC" || s.ToUpper() == "-NCRC") ch_crc = false; // Отключение контрольной суммы
                if (s.ToUpper() == "-DEBUG" || s.ToUpper() == "-D") debug = true; // Включение отладочной информации
                if (s.ToUpper() == "-H" || s.ToUpper() == "-?" || s.ToUpper() == "/?") { WriteHelp(); return 0; } 
                    // вывод "помощи"

                // указание пароля
                if (s.Substring(0, 2).ToUpper() == "-P")
                {
                    pass = s.Substring(2);
                }
            }

            // пробуем читать файл
            System.IO.FileStream fs;
            try
            {
                fs = new System.IO.FileStream(args[0], System.IO.FileMode.Open);
            }
            catch { return 2; }
            byte[] IBytes = new byte[4];

            fs.Read(IBytes, 0, 4);

            #region V2

            // для формата версии 2
            if (IBytes[3] == 2)
            {
                fs.Read(IBytes, 0, 4);
                int chanel = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);
                int width = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);
                int height = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);
                int widthR = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);

                YCrCb q;

                // для наличия отсутствия фона
                if (IBytes[0] == 1)
                    q = new YCrCb(chanel, width, height, widthR, new YCrCb.YCrCbPoint(IBytes[1], IBytes[2], IBytes[3]));
                else
                    q = new YCrCb(chanel, width, height, widthR, null);

                if (debug)
                {
                    Console.WriteLine("File version 2");
                    Console.WriteLine("W:" + widthR + " H:" + height);
                    if (IBytes[0] == 1)
                        Console.WriteLine("Base color present.");
                    else
                        Console.WriteLine("Without base color.");
                }

                int yy = 0;
                while (true)
                {
                    try
                    {
                        // читаем маркер
                        int y = fs.Read(IBytes, 0, 4);
                        if (y == 0) break;
                        int QL = BitConverter.ToInt32(IBytes, 0);
                        if (QL > 0)
                        {
                            // читаем фрагмент
                            byte[] Zcmpr = new byte[QL];
                            fs.Read(Zcmpr, 0, QL);
                            if (debug)
                            {
                                yy++;
                                Console.WriteLine();
                                Console.WriteLine("Ch#" + yy);
                                Console.WriteLine("Size in file " + QL + " bytes");
                            }
                            q.setZoneBytes(Zcmpr, debug);

                        }

                    }
                    catch { exitCode = 3; break; }
                }

                q.YCrCb2RGB();

                q.writeBMP(args[0] + ".BMP");
                return exitCode;
            }
            #endregion V2

            #region V3-4
            
            // для формата версии 3
            if (IBytes[3] == 3 || IBytes[3] == 4)
            {
                int VV = IBytes[3];

                fs.Read(IBytes, 0, 4);
                int chanel = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);
                int width = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);
                int height = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);
                int widthR = BitConverter.ToInt32(IBytes, 0);
                fs.Read(IBytes, 0, 4);

                YCrCb q;

                // с наличием или отсутствием фона
                if (IBytes[0] == 1)
                    q = new YCrCb(chanel, width, height, widthR, new YCrCb.YCrCbPoint(IBytes[1], IBytes[2], IBytes[3]));
                else
                    q = new YCrCb(chanel, width, height, widthR, null);

                if (debug)
                {
                    Console.WriteLine("File version " + VV);
                    if (VV == 4) Console.WriteLine("Crypted");

                    Console.WriteLine("W:" + widthR + " H:" + height);
                    if (IBytes[0] == 1)
                        Console.WriteLine("Base color present.");
                    else
                        Console.WriteLine("Without base color.");
                }

                _Rijndael crpt = new _Rijndael();
                crpt.Key = pass;

                int yy = 0;
                while (true)
                {
                    try
                    {
                        // чтение маркера фрагмента (длины)
                        int y = fs.Read(IBytes, 0, 4);
                        if (y == 0) break;
                        int QL = BitConverter.ToInt32(IBytes, 0);
                        if (QL > 0)
                        {
                            // чтение фрагмента
                            byte[] Zcmpr = new byte[QL];
                            fs.Read(Zcmpr, 0, QL);
                            if (QL > 16)
                            {
                                if (debug)
                                {
                                    yy++;
                                    Console.WriteLine();
                                    Console.WriteLine("Ch#" + yy);
                                    Console.WriteLine("Size in file " + QL + " bytes");
                                }

                                
                                if (VV == 4)
                                {
                                    // при включенном шифровании
                                    int yyy = q.setZoneBytesV3(crpt.Decrypt(Zcmpr), debug, VV, ch_crc);
                                    if (exitCode == 0 && yyy != 0) exitCode = yyy;
                                }
                                else
                                {
                                    // при отключенном шифровании
                                    int yyy = q.setZoneBytesV3(Zcmpr, debug, VV, ch_crc);
                                    if (exitCode == 0 && yyy != 0) exitCode = yyy;
                                }
                            }

                        }

                    }
                    catch { break; }
                }

                q.YCrCb2RGB();

                q.writeBMP(args[0] + ".BMP");
                return exitCode;
            }
            #endregion V3-4

            // сообщение об ошибке
            Console.WriteLine("The file contains the unsupported version");
            return 1;
        }
    }
    }

