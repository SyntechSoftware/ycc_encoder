// YCC_GUI V 1.0 build 14
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   15.01.2010
// modified 29.01.2010

using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        string YCC_dec = "";
        string YCC_enc = "";

        public Form1()
        {
            InitializeComponent();
        }

        // При завантаженні форми
        private void Form1_Load(object sender, EventArgs e)
        {

            YCC_enc = Application.StartupPath + "\\YCC_encoder.exe";
            YCC_dec = Application.StartupPath + "\\YCC_decoder.exe";

            if (System.IO.File.Exists(YCC_enc))
            {
                groupBox1.Enabled = true;
                label3.Text = "Ожидаю действий пользователя...";
            }
            else
                label3.Text = "Не могу найти модуль-кодировшик!";

            if (System.IO.File.Exists(YCC_dec))
            {
                groupBox2.Enabled = true;
                label9.Text = "Ожидаю действий пользователя...";
            }
            else
                label9.Text = "Не могу найти модуль чтения!";
        }

        // кнопка выбора файла источния для кодирования
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                textBox1.Text = openFileDialog1.FileName;
                String Q = textBox1.Text;
                if (Q.LastIndexOf(".") > 1)
                    textBox2.Text = Q.Substring(0,Q.LastIndexOf("."));

            }

        }

        // кнопка выбора файла источника для чтения
        private void button4_Click(object sender, EventArgs e)
        {
            textBox5.Text = "";
            if (openFileDialog2.ShowDialog() != DialogResult.Cancel)
            {
                textBox5.Text = openFileDialog2.FileName;
            }
        }

        // проверка существует ли файл, который ввели в поле файла источника кодирования
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(textBox1.Text))
                textBox1.BackColor = System.Drawing.Color.Honeydew;
            else
                textBox1.BackColor = System.Drawing.Color.Pink;
        }


        // проверка существует ли файл, который ввели в поле файла источника чтения
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(textBox5.Text))
                textBox5.BackColor = System.Drawing.Color.Honeydew;
            else
                textBox5.BackColor = System.Drawing.Color.Pink;

        }

        // проверка ввели ли в поле файла получателя кодирования что-то
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
                textBox2.BackColor = System.Drawing.Color.Honeydew;
            else
                textBox2.BackColor = System.Drawing.Color.Pink;


        }

        // проверка пароля введенного в первое поле
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
                if (textBox3.Text == textBox4.Text)
                {
                    textBox3.BackColor = System.Drawing.Color.LightGreen;
                    textBox4.BackColor = System.Drawing.Color.LightGreen;
                }
                else
                {
                    textBox3.BackColor = System.Drawing.Color.Pink;
                    textBox4.BackColor = System.Drawing.Color.Pink;
                }
                if (textBox3.Text.Length == 0)
                    textBox3.BackColor = System.Drawing.Color.White;
                if (textBox4.Text.Length == 0)
                    textBox4.BackColor = System.Drawing.Color.White;
        }

        // проверка пароля введенного во второе поле
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            textBox3_TextChanged(sender, e);
        }

        // нажали записать
        private void button1_Click(object sender, EventArgs e)
        {

            if (System.IO.File.Exists(textBox1.Text))
            {
                try
                {
                    string cmdl = "\"" + textBox1.Text + "\" \"" + textBox2.Text + "\"";
                    label3.Text = "Кодирую файл...";
                    System.Windows.Forms.Application.DoEvents();

                    if (textBox4.Text.Length > 0 && textBox4.Text == textBox3.Text)
                        cmdl = cmdl + " -p" + textBox3.Text;

                    if (checkBox1.Checked)
                        cmdl = cmdl + " -nocrc";

                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = YCC_enc;
                    prc.StartInfo.Arguments = cmdl;
                    prc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    prc.Start();
                    prc.WaitForExit(30000);

                    label3.Text = GetCode(prc.ExitCode);

                }
                catch
                { }
            }

        }

        // текстовое сообщение по полю возврата при запуске консольного кодировщика
        string GetCode(int code) 
        {
            switch (code)
            {
                case 0: return "OK! Ожидаю дальнейших действий пользователя.."; 
                case 2: return "Ошибки при работе с файлами!"; 
                case 1: return "Некорректный формат файла!"; 
                case 3: return "Внутренняя ошибка ПО!"; 
                case 4: return "Пароль не верен!"; 
                case 5: return "Ошибка CRC!"; 
                default: return "Неизвестный код возврата!";
            }

        }

        // нажали прочитать
        private void button2_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(textBox5.Text))
            {
                try
                {
                    string cmdl = "\"" + textBox5.Text + "\"";
                    label9.Text = "Запускаю распаковщик...";
                    System.Windows.Forms.Application.DoEvents();

                    if (textBox7.Text.Length > 0)
                        cmdl = cmdl + " -p" + textBox7.Text;

                    if (checkBox2.Checked)
                        cmdl = cmdl + " -nocrc";

                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = YCC_dec;
                    prc.StartInfo.Arguments = cmdl;
                    prc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    prc.Start();
                    prc.WaitForExit(30000);
                    
                    label9.Text = GetCode(prc.ExitCode);

                    if (prc.ExitCode == 0){
                        System.Diagnostics.Process prc2 = new System.Diagnostics.Process();
                        prc2.StartInfo.FileName = "cmd";
                        
                        int DD = textBox5.Text.LastIndexOf("\\");
                        string path_ = textBox5.Text.Substring(0, DD);
                        string file_ = textBox5.Text.Substring(DD+1);
                        prc2.StartInfo.Arguments = "/C start /D\"" + path_ + "\" " + file_ + ".bmp";
                        prc2.Start();
                    }
                        
                }
                catch
                { }
            }
        }
    }
}
