using pTop.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace pTop
{
    /// <summary>
    /// License_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class License_Dialog : Window
    {
        public License_Dialog()
        {
            InitializeComponent();
            update_countrys();
            MachineCode mc = new MachineCode();
            string machine_code = mc.Get_Code();
            machine_code = mc.MD5_PWD(machine_code);
            this.code_txt.Text = machine_code;
        }
        private void update_countrys()
        {
            List<string> countrys = new List<string>();
            string country_ini = "countrys.ini";
            StreamReader sr = new StreamReader(country_ini,Encoding.Default);
            while (!sr.EndOfStream)
                countrys.Add(sr.ReadLine());

            for (int i = 0; i < countrys.Count; ++i)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = countrys[i];
                this.country_txt.Items.Add(item);
            }
            this.country_txt.Text = "China";
        }
        private void send1_btn_clk(object sender, RoutedEventArgs e)
        {
            try
            {
                string information = get_information("<br />");
                if (information == "")
                    return;

                string sEmailMSG = "mailto:" + "ptop@ict.ac.cn" + "?subject=pFind Registrition&body=" + information;
                System.Diagnostics.Process.Start(sEmailMSG);
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
            /*
            try
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add("pfinder@ict.ac.cn");
                message.Subject = "Config Information";
                message.From = new System.Net.Mail.MailAddress("From@online.microsoft.com");
                message.Body = information;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("");
                smtp.Send(message);
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.ToString());
            }
             */
        }

        private void send2_btn_clk(object sender, RoutedEventArgs e)
        {
            try
            {
                string information = get_information("\r\n");
                if (information == "")
                    return;
                Clipboard.SetData(DataFormats.Text, information);
            }
            catch (Exception ex)
            {

            }
            MessageBox.Show(Message_Help.COPY_OK);
        }

        private string get_information(string line_flag)
        {
            string name = name_txt.Text;
            string company = company_txt.Text;
            string country = country_txt.Text;
            string email = email_txt.Text;
            string code = code_txt.Text;

            if (name == "" || company == "" || country == "" || email == "" || code == "")
            {
                MessageBox.Show(Message_Help.ALL_NOT_NULL);
                return "";
            }
            if (!email.Contains('@'))
            {
                MessageBox.Show(Message_Help.EMAIL_WRONG);
                return "";
            }

            string information = "User Name: " + name + line_flag + "Institute/Company Name: " + company
                + line_flag + "Country: " + country + line_flag + "Email address: " + email + line_flag
                + "Activation Code: " + code;
            return information;
        }
    }
}
