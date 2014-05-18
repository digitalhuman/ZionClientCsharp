using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace ZionClient
{
    class Crypto
    {
        private string filename = "";

        public Crypto(string SSL_Filename)
        {
            this.filename = SSL_Filename;
        }

        public string Encrypt(string data)
        {
            try
            {
                byte[] encrypted = this.getPublicKey().Encrypt(ASCIIEncoding.UTF8.GetBytes(data), false);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception err)
            {
                return err.ToString();
            }
        }

        private RSACryptoServiceProvider getPublicKey()
        {
            if (File.Exists(filename) == false)
            {
                System.Windows.Forms.MessageBox.Show("SSL Certificate does not exists!");
            }
            else
            {
                X509Certificate2 cert = new X509Certificate2();
                cert.Import(filename);
                return (RSACryptoServiceProvider)cert.PublicKey.Key;
            }
            return new RSACryptoServiceProvider();
        }

        public void createDH()
        {
            try
            {
                CngKey key;

                if (CngKey.Exists("ZionClient", CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey))
                {
                    key = CngKey.Open("ZionClient", CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.MachineKey);
                }
                else
                {
                    key = CngKey.Create(CngAlgorithm.ECDiffieHellmanP521, "ZionClient", new CngKeyCreationParameters
                    {
                        ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                        KeyCreationOptions = CngKeyCreationOptions.MachineKey,
                        KeyUsage = CngKeyUsages.AllUsages,
                        Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                        UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None)
                    });
                }

                byte[] privateBytes = key.Export(CngKeyBlobFormat.EccPrivateBlob);
                byte[] publicBytes = key.Export(CngKeyBlobFormat.EccPublicBlob);

                //This:
                var privateTester1 =
                    new ECDiffieHellmanCng(CngKey.Import(privateBytes, CngKeyBlobFormat.EccPrivateBlob,
                        CngProvider.MicrosoftSoftwareKeyStorageProvider));

                //txtData.Text = privateTester1.ToXmlString(true);

                //Or that:
                var privateTester2 = new ECDiffieHellmanCng(key);
            }
            catch (Exception err)
            {
                //
            }
        }
    }
}
