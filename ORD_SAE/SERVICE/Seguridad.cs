using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ORD_SAE.Service
{
    public static class Seguridad
    {
        public static string key = "26f30b3f82358cf800590be9181bae35";
        public static string iv = "36431d924ea0a481";
        public static AesCryptoServiceProvider CrearEncriptor(string Key, string IV)
        {
            AesCryptoServiceProvider encriptor = new AesCryptoServiceProvider();
            byte[] keybytes = Encoding.UTF8.GetBytes(Key);
            byte[] ivbytes = Encoding.UTF8.GetBytes(IV);
            encriptor.Key = keybytes;
            encriptor.IV = ivbytes;
            return encriptor;
        }
        public static string EncriptarDatos(AesCryptoServiceProvider aesEnc, string Texto)
        {
            try
            {
                ICryptoTransform ct = aesEnc.CreateEncryptor();
                byte[] input = Encoding.UTF8.GetBytes(Texto);
                string TextoCifrado = Convert.ToBase64String(ct.TransformFinalBlock(input, 0, input.Length));
                return TextoCifrado;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static string DesencriptarDatos(AesCryptoServiceProvider aesEnc, string TextoCifrado)
        {
            try
            {
                byte[] b = Convert.FromBase64String(TextoCifrado);
                ICryptoTransform ct = aesEnc.CreateDecryptor();
                byte[] output = ct.TransformFinalBlock(b, 0, b.Length);
                return Encoding.UTF8.GetString(output);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static string DesencriptarDatos(this string Texto)
        {
            var encriptor = CrearEncriptor(key, iv);
            return DesencriptarDatos(encriptor, Texto);
        }
        public static string EncriptarDatos(this string Texto)
        {
            var encriptor = CrearEncriptor(key, iv);
            return EncriptarDatos(encriptor, Texto);
        }
        //public static string GetMd5Hash(string input)
        //{
        //    string hashValue = "";
        //    using (MD5CryptoServiceProvider md5Encriptor = new MD5CryptoServiceProvider())
        //    {
        //        hashValue = GetMd5Hash(md5Encriptor, input);
        //    }

        //    return hashValue;
        //}
        //private string GetMd5Hash(MD5 md5Hash, string input)
        //{
        //    byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        //    StringBuilder sBuilder = new StringBuilder();
        //    int i;
        //    for (i = 0; i <= data.Length - 1; i++)
        //    {
        //        sBuilder.Append(data[i].ToString("x2"));
        //    }
        //    return sBuilder.ToString();
        //}
    }
}
