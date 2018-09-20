using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pTop
{
    public class MachineCode
    {
        ///   <summary>   
        ///   获取cpu序列号       
        ///   </summary>   
        ///   <returns> string </returns>  
        ///   

        public string Get_Code()
        {
            return GetCpuInfo() + "," + GetHDid(); // +"," + GetMoAddress();
        }
        public string GetCpuInfo()
        {
            try
            {
                string cpuInfo = " ";
                using (ManagementClass cimobject = new ManagementClass("Win32_Processor"))
                {
                    ManagementObjectCollection moc = cimobject.GetInstances();

                    foreach (ManagementObject mo in moc)
                    {
                        cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                        mo.Dispose();
                    }
                }
                return cpuInfo.ToString();
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
                return "";
            }
        }

        ///   <summary>   
        ///   获取硬盘ID       
        ///   </summary>   
        ///   <returns> string </returns>   
        public string GetHDid()
        {
            string HDid = " ";
            using (ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive"))
            {
                ManagementObjectCollection moc1 = cimobject1.GetInstances();
                int num = 0;
                foreach (ManagementObject mo in moc1)
                {
                    if (num == 0)
                    {
                        HDid = (string)mo.Properties["Model"].Value;
                        break;
                    }
                    mo.Dispose();
                    ++num;
                }
            }
            return HDid.ToString();
        }        

        ///   <summary>   
        ///   获取网卡硬件地址   
        ///   </summary>   
        ///   <returns> string </returns>   
        public string GetMoAddress()
        {
            string MoAddress = " ";
            using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection moc2 = mc.GetInstances();
                foreach (ManagementObject mo in moc2)
                {
                    if ((bool)mo["IPEnabled"] == true)
                        MoAddress = mo["MacAddress"].ToString();
                    mo.Dispose();
                }
            }
            return MoAddress.ToString();
        }
        public string MD5_PWD(string code)
        {
            string Encrypt_PWD = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(code, "MD5");
            return Encrypt_PWD;
        }
        public string MD5(string code)
        {
             
            //获取加密服务  
            System.Security.Cryptography.MD5CryptoServiceProvider md5CSP = new System.Security.Cryptography.MD5CryptoServiceProvider();  
         
            //获取要加密的字段，并转化为Byte[]数组  
            byte[] testEncrypt = System.Text.Encoding.Unicode.GetBytes(code);  
  
            //加密Byte[]数组  
            byte[] resultEncrypt = md5CSP.ComputeHash(testEncrypt);  
  
            //将加密后的数组转化为字段(普通加密)  
            string testResult = System.Text.Encoding.Unicode.GetString(resultEncrypt);  
  
            //作为密码方式加密   
            
            return testResult;
        }
    }  
}
