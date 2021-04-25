using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SEED_Encryption_Algoithm
{
    public partial class MainWindow : Window
    {
        static UInt32[] roundKeys;

        public MainWindow()
        {
            InitializeComponent();
            roundKeys = null;
        }

        private void generateKeyClick(object sender, RoutedEventArgs e)
        {
            string key = KeyBox.Text;
            if(key.Length!=16)
            {
                bool? result = new CustomMessageBox("Длина ключа должна равняться 128 битам!", MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
            else
            {
                byte[] keyBytes = Encoding.GetEncoding(1251).GetBytes(key);
                UInt32[] keyParts = SEED.getParts(keyBytes);
                SEED.keySchedule(ref roundKeys, keyParts[0], keyParts[1], keyParts[2], keyParts[3]);
                List<Subkey> subkeys = new List<Subkey>();
                for (int i = 0; i < 16; i++)
                {
                    subkeys.Add(new Subkey() { RoundNumber = i + 1, K0 = roundKeys[i], K1 = roundKeys[i + 1] });
                }
                KeyGrid.ItemsSource = subkeys;
            }
        }

        public int amount = 0;
        public string decrypted = null;

        private void encryptClick(object sender, RoutedEventArgs e)
        {
            bool? err = null;
            EncryptedBlock.Clear();
            BlockToDecrypt.Clear();
            DecryptedBlock.Clear();
            string text = BlockToEncrypt.Text;
            if (String.IsNullOrEmpty(text))
            {
                err = new CustomMessageBox("Блок для зашифрования не может быть пустым!", MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
            else
            {
                if (text.Length % 16 != 0)
                {
                    string x = " ";
                    for (int i = text.Length % 16; i < 16; i++)
                    {
                        text += x;
                    }
                }
                if(roundKeys == null)
                {
                    err = new CustomMessageBox("Вы не сгенериовали подключи!", MessageType.Error, MessageButtons.Ok).ShowDialog();
                }
                else if(text.Length>16)
                {
                    err = new CustomMessageBox("Длина блока превышает 128 бит!", MessageType.Error, MessageButtons.Ok).ShowDialog();
                }
                else
                {
                    byte[] block = Encoding.GetEncoding(1251).GetBytes(text);
                    List<RoundValue> roundList = null;
                    UInt32[] blockParts = SEED.getParts(block);
                    UInt32[] encrypted = SEED.seedEncryption(ref roundList, roundKeys, blockParts[0], blockParts[1], blockParts[2], blockParts[3]);
                    byte[] res0 = new byte[4];
                    byte[] res1 = new byte[4];
                    byte[] res2 = new byte[4];
                    byte[] res3 = new byte[4];
                    res0 = BitConverter.GetBytes(encrypted[2]);
                    res1 = BitConverter.GetBytes(encrypted[3]);
                    res2 = BitConverter.GetBytes(encrypted[0]);
                    res3 = BitConverter.GetBytes(encrypted[1]);
                    Array.Reverse(res0);
                    Array.Reverse(res1);
                    Array.Reverse(res2);
                    Array.Reverse(res3);
                    byte[] res = SEED.assembleParts(res0, res1, res2, res3);
                    string result = Encoding.GetEncoding(1251).GetString(res);
                    EncryptedBlock.Text += result;
                    BlockToDecrypt.Text += result;
                    EncryptGrid.ItemsSource = roundList;
                }
            }
        }

        private void decryptClick(object sender, RoutedEventArgs e)
        {
            bool? err = null;
            DecryptedBlock.Clear();
            string text = BlockToDecrypt.Text;
            if (String.IsNullOrEmpty(text))
            {
                err = new CustomMessageBox("Блок для расшифрования не может быть пустым!", MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
            else
            {
                if (roundKeys == null)
                {
                    err = new CustomMessageBox("Вы не сгенериовали подключи!", MessageType.Error, MessageButtons.Ok).ShowDialog();
                }
                else
                {
                    byte[] block = Encoding.GetEncoding(1251).GetBytes(text);
                    List<RoundValue> roundList = null;
                    UInt32[] blockParts = SEED.getParts(block);
                    UInt32[] encrypted = SEED.seedDecryption(ref roundList, roundKeys, blockParts[0], blockParts[1], blockParts[2], blockParts[3]);
                    byte[] res0 = new byte[4];
                    byte[] res1 = new byte[4];
                    byte[] res2 = new byte[4];
                    byte[] res3 = new byte[4];
                    res0 = BitConverter.GetBytes(encrypted[2]);
                    res1 = BitConverter.GetBytes(encrypted[3]);
                    res2 = BitConverter.GetBytes(encrypted[0]);
                    res3 = BitConverter.GetBytes(encrypted[1]);
                    Array.Reverse(res0);
                    Array.Reverse(res1);
                    Array.Reverse(res2);
                    Array.Reverse(res3);
                    byte[] res = SEED.assembleParts(res0, res1, res2, res3);
                    string result = Encoding.GetEncoding(1251).GetString(res);
                    DecryptedBlock.Text += result;
                    DecryptGrid.ItemsSource = roundList;
                }
            }
        }

        private void generateGClick(object sender, RoutedEventArgs e)
        {
            List<GFunctionValue> list = new List<GFunctionValue>();
            try
            {
                string text = GBox.Text;
                UInt32 x = Convert.ToUInt32(text);
                GFunctionValue valuesToShow = null;
                UInt32 result = SEED.ShowGFunction(ref valuesToShow, x);
                list.Add(valuesToShow);
                GResult.Clear();
                GResult.Text = result.ToString();
                GGrid.ItemsSource = list;
            }
            catch(Exception ex)
            {
                bool? result = new CustomMessageBox(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private void encryptTextClick(object sender, RoutedEventArgs e)
        {
            EncryptedText.Clear();
            TextToDecrypt.Clear();
            DecryptedText.Clear();
            string text = TextToEncrypt.Text;
            if (String.IsNullOrEmpty(text))
            {
                bool? result = new CustomMessageBox("Текст для зашифрования не может быть пустым", MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
            else
            {
                if (text.Length % 16 != 0)
                {
                    string x = " ";
                    int num = 0;
                    for (int i = text.Length % 16; i < 16; i++)
                    {
                        text += x;
                        num++;
                    }
                    amount = num;
                }
                int blocksAmount = text.Length / 16;
                string[] blocks = new string[blocksAmount];
                for (int i = 0; i < text.Length / 16; i++)
                {
                    blocks[i] = text.Substring(i * 16, 16);
                }
                foreach (string s in blocks)
                {
                    List<RoundValue> roundList = null;
                    byte[] block = Encoding.GetEncoding(1251).GetBytes(s);
                    UInt32[] blockParts = SEED.getParts(block);
                    UInt32[] encrypted = SEED.seedEncryption(ref roundList,roundKeys, blockParts[0], blockParts[1], blockParts[2], blockParts[3]);
                    byte[] res0 = new byte[4];
                    byte[] res1 = new byte[4];
                    byte[] res2 = new byte[4];
                    byte[] res3 = new byte[4];
                    res0 = BitConverter.GetBytes(encrypted[2]);
                    res1 = BitConverter.GetBytes(encrypted[3]);
                    res2 = BitConverter.GetBytes(encrypted[0]);
                    res3 = BitConverter.GetBytes(encrypted[1]);
                    Array.Reverse(res0);
                    Array.Reverse(res1);
                    Array.Reverse(res2);
                    Array.Reverse(res3);
                    byte[] res = SEED.assembleParts(res0, res1, res2, res3);
                    string result = Encoding.GetEncoding(1251).GetString(res);
                    EncryptedText.Text += result;
                    TextToDecrypt.Text += result;
                }
            }
        }

        private void decryptTextClick(object sender, RoutedEventArgs e)
        {
            DecryptedText.Clear();
            string text = TextToDecrypt.Text;
            int blocksAmount = text.Length / 16;
            string[] blocks = new string[blocksAmount];
            for (int i = 0; i < text.Length / 16; i++)
            {
                blocks[i] = text.Substring(i * 16, 16);
            }
            foreach (string s in blocks)
            {
                List<RoundValue> roundList = null;
                byte[] block = Encoding.GetEncoding(1251).GetBytes(s);
                UInt32[] blockParts = SEED.getParts(block);
                UInt32[] encrypted = SEED.seedDecryption(ref roundList, roundKeys, blockParts[0], blockParts[1], blockParts[2], blockParts[3]);
                byte[] res0 = new byte[4];
                byte[] res1 = new byte[4];
                byte[] res2 = new byte[4];
                byte[] res3 = new byte[4];
                res0 = BitConverter.GetBytes(encrypted[2]);
                res1 = BitConverter.GetBytes(encrypted[3]);
                res2 = BitConverter.GetBytes(encrypted[0]);
                res3 = BitConverter.GetBytes(encrypted[1]);
                Array.Reverse(res0);
                Array.Reverse(res1);
                Array.Reverse(res2);
                Array.Reverse(res3);
                byte[] res = SEED.assembleParts(res0, res1, res2, res3);
                string result = Encoding.GetEncoding(1251).GetString(res);
                DecryptedText.Text += result;
            }
        }

        private void generateKClick(object sender, RoutedEventArgs e)
        {
            string key = BoxKey.Text;
            if (key.Length != 16)
            {
                bool? result = new CustomMessageBox("Длина ключа должна равняться 128 битам!", MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
            else
            {
                byte[] keyBytes = Encoding.GetEncoding(1251).GetBytes(key);
                UInt32[] keyParts = SEED.getParts(keyBytes);
                SEED.keySchedule(ref roundKeys, keyParts[0], keyParts[1], keyParts[2], keyParts[3]);
                List<Subkey> subkeys = new List<Subkey>();
                for (int i = 0; i < 16; i++)
                {
                    subkeys.Add(new Subkey() { RoundNumber = i + 1, K0 = roundKeys[i], K1 = roundKeys[i + 1] });
                }
                KeyGrid.ItemsSource = subkeys;
            }
        }
    }
}
