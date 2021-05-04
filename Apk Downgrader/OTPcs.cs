using System;
using System.Collections;
using System.IO;

namespace OTP
{
    public class Decrypter
    {
        public byte[] DecryptOTP(Byte[] input, Byte[] key)
        {
            byte[] output = new byte[input.Length];
            BitArray i = new BitArray(input);
            BitArray k = new BitArray(key);
            i.Xor(k);
            i.CopyTo(output, 0);
            return output;
        }

        public void DecryptOTPFile(String file, String keyFile, String outputDirectory, bool useLowMem = false, bool outputToConsole = true, int batches = 1000000)
        {
            if (useLowMem)
            {
                File.Delete(outputDirectory + Path.GetFileNameWithoutExtension(keyFile));
                FileStream ifile = new FileStream(file, FileMode.Open, FileAccess.Read);
                FileStream kfile = new FileStream(keyFile, FileMode.Open, FileAccess.Read);
                FileStream ofile = new FileStream(outputDirectory, FileMode.Append);
                int code = 0;
                for (int i = 1; (long)i * (long)batches < ifile.Length + (long)batches; i++)
                {
                    int adjusted = (long)i * (long)batches < ifile.Length ? batches : (int)(ifile.Length % batches);
                    byte[] tmp11 = new byte[adjusted];
                    byte[] tmp12 = new byte[adjusted];
                    for (int ii = 0; ii < adjusted; ii++)
                    {
                        tmp11[ii] = (byte)ifile.ReadByte();
                        tmp12[ii] = (byte)kfile.ReadByte();
                        if(tmp11[ii] == 0x00 && tmp11[ii] != tmp12[ii] || tmp12[ii] == 0x00 && tmp11[ii] != tmp12[ii])
                        {
                            code++;
                        }
                    }

                    //DecryptOTP
                    byte[] output1 = new byte[tmp11.Length];
                    BitArray inp = new BitArray(tmp11);
                    BitArray k = new BitArray(tmp12);
                    inp.Xor(k);
                    inp.CopyTo(output1, 0);

                    ofile.Write(output1, 0, output1.Length);
                    ofile.Flush();
                    if (outputToConsole) Console.WriteLine("\r" + (i * (batches / 1000000)) + " MB /" + (ifile.Length / 1000000) + " MB (" + ((double)i * (double)batches / ifile.Length * 100) + " %)");
                }
                ofile.Flush();
                ifile.Close();
                kfile.Close();
                ofile.Close();
                Console.WriteLine("Contains " + code + " bytes of actual data one file contains scattered around the file");
            }
            else
            {
                if (outputToConsole) Console.WriteLine("Started Decryption, please wait.");
                byte[] fileContents = File.ReadAllBytes(file);
                byte[] keyFileContents = File.ReadAllBytes(keyFile);

                //DecryptOPT
                byte[] output = new byte[fileContents.Length];
                BitArray i = new BitArray(fileContents);
                BitArray k = new BitArray(keyFileContents);
                i.Xor(k);
                i.CopyTo(output, 0);

                File.WriteAllBytes(outputDirectory, output);
            }
        }
    }
}