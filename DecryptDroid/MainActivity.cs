using Android.App;
using Android.OS;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace DecryptDroid
{
    [Activity(Label = "DecryptDroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            // SetContentView (Resource.Layout.Main);
            string decryppath = "//";
            DecryptDirectory(decryppath);
        }

        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] pswrdBytes)
        {
            byte[] dcrBtes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] sltBites = new byte[] { 3, 4, 5, 6, 7, 8, 9, 10 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged AES = new AesManaged())
                {

                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(pswrdBytes, sltBites, 10000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.ECB;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    dcrBtes = ms.ToArray();

                }
            }

            return dcrBtes;
        }


        public void DecryptFile(string file, string password)
        {

            byte[] bytesToBeDecrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            File.WriteAllBytes(file, bytesDecrypted);
            string extension = System.IO.Path.GetExtension(file);
            string result = file.Substring(0, file.Length - extension.Length);
            System.IO.File.Move(file, result);

        }



        public void DecryptDirectory(string location)
        {
            string password = "password";

            string[] files = Directory.GetFiles(location);
            string[] childDirectories = Directory.GetDirectories(location);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string extension = Path.GetExtension(files[i]);
                    if (extension == ".locked")
                    {
                        DecryptFile(files[i], password);
                    }
                }
                catch (SystemException)
                {
                    continue;
                }

            }
            for (int i = 0; i < childDirectories.Length; i++)
            {
                try
                {
                    DecryptDirectory(childDirectories[i]);
                }
                catch (SystemException)
                {
                    continue;
                }
            }
          
        }
    }
}

