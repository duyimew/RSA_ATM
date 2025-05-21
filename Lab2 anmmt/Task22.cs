using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Lab2_anmmt
{
    public partial class Task22 : Form
    {
        Functions RSA = new Functions();
        public Task22()
        {
            InitializeComponent();
            
        }

        private void btn_Random_Click(object sender, EventArgs e)
        {
            try
            {
                string decp = RSA.GeneratePrime(128).ToString();
                string decq = "";
                while (true)
                {
                    decq = RSA.GeneratePrime(128).ToString();
                    if (decq != decp) break; // đảm bảo p khác q
                }

                BigInteger p = BigInteger.Parse(decp);
                BigInteger q = BigInteger.Parse(decq);

                //Tính phi(n)
                BigInteger phi_n = RSA.CalculatePhi_N(p, q);
                
                
                //Tính e
                BigInteger dece = 0;
                dece = RSA.GenerateE(dece, phi_n);
                tb_valueE.Text = dece.ToString();

                //Tính d
                BigInteger d = RSA.CalculateD(dece, p, q);
                

                //Tính n
                BigInteger n = RSA.CalculateN(p, q);
                

                if (!radioButton1.Checked)
                {
                    // Nếu người dùng chọn "Hex", chuyển đổi từ Dec sang Hex
                    tb_valueP.Text = RSA.ConvertNumber(decp, "dec", "hex"); 
                    tb_valueQ.Text = RSA.ConvertNumber(decq, "dec", "hex"); 
                    tb_valueE.Text = RSA.ConvertNumber(dece.ToString(), "dec", "hex");
                    tb_valuePhi_n.Text = RSA.ConvertNumber(phi_n.ToString(), "dec", "hex");
                    tb_valueD.Text = RSA.ConvertNumber(d.ToString(), "dec", "hex");
                    tb_valueN.Text = RSA.ConvertNumber(n.ToString(), "dec", "hex");
                }
                else
                {
                    tb_valueP.Text = decp; 
                    tb_valueQ.Text = decq; 
                    tb_valueE.Text = dece.ToString();
                    tb_valuePhi_n.Text = phi_n.ToString();
                    tb_valueD.Text = d.ToString();
                    tb_valueN.Text = n.ToString();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        

        private void btn_Encrypt_Click_1(object sender, EventArgs e)
        {
            try
            {
                // 1. Lấy giá trị p, q, e từ giao diện
                string sp = tb_valueP.Text;
                string sq = tb_valueQ.Text;
                string se = tb_valueE.Text;
                BigInteger p = 0;
                BigInteger q = 0;
                BigInteger dece = 0;
                
                if (!radioButton1.Checked)
                {
                    p = BigInteger.Parse(RSA.ConvertNumber(sp, "hex", "dec"));
                    q = BigInteger.Parse(RSA.ConvertNumber(sq, "hex", "dec"));
                    dece = BigInteger.Parse(RSA.ConvertNumber(se, "hex", "dec"));
                }  // nếu người dùng nhập dưới dạng hex
                else
                {
                    p = BigInteger.Parse(sp);
                    q = BigInteger.Parse(sq);
                    dece = BigInteger.Parse(se);
                } // nếu là dec

                if (!(RSA.IsPrime(p) && RSA.IsPrime(q)))
                {

                    return;
                }


                BigInteger n = RSA.CalculateN(p, q);
                BigInteger phi_n = RSA.CalculatePhi_N(p, q);
                BigInteger d = RSA.CalculateD(dece, p, q);
                tb_valuePhi_n.Text = phi_n.ToString();
                tb_valueN.Text = n.ToString();
                tb_valueD.Text = d.ToString();

                // 3. Xác định định dạng input và output từ radioButton
                string typeinput = "";
                if (radioButton3.Checked) typeinput = "Base64";
                else if (radioButton4.Checked) typeinput = "bin";
                else if (radioButton5.Checked) typeinput = "hex";
                else if (radioButton9.Checked) typeinput = "ASCII";
                string typeoutput = "";
                if (radioButton7.Checked) typeoutput = "Base64";
                else if (radioButton6.Checked) typeoutput = "bin";
                else if (radioButton8.Checked) typeoutput = "hex";
                else if (radioButton10.Checked) typeoutput = "ASCII";

                // 4. Chuyển thông điệp từ giao diện thành mảng byte[] dựa trên định dạng
                string message = richTextBox1.Text;
                byte[] messageBytes = null;
                messageBytes = RSA.getMessageByte(messageBytes, typeinput, message);
                if (messageBytes == null) return;

                

                List<string> encryptedMessage = new List<string>();
                // 5. Xác định kích thước khối mã hóa
                int blockSize = 0;
                if (!string.IsNullOrWhiteSpace(textBox4.Text) && !string.IsNullOrWhiteSpace(textBox5.Text) && checkBox1.Checked)
                {
                    blockSize = int.Parse(textBox4.Text); // dùng thủ công
                }
                else
                {
                    // tự tính blockSize dựa trên độ dài của p và q
                    string hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(RSA.ConvertNumber(hexp, "hex", "dec")) != p) hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex");
                    string hexq = RSA.ConvertNumber(q.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(RSA.ConvertNumber(hexq, "hex", "dec")) != q) hexq = RSA.ConvertNumber(p.ToString(), "dec", "hex");
                    blockSize = (hexp.Length + hexq.Length) / 2 - 1;
                }
                // 6. Mã hóa từng khối
                StringBuilder fullHexBuilder = new StringBuilder();
                for (int i = 0; i < messageBytes.Length; i += blockSize)
                {
                    byte[] block = messageBytes.Skip(i).Take(blockSize).Reverse().ToArray(); // đảo byte

                    string hexString = RSA.ByteArrayToHex(block);
                    BigInteger bi = BigInteger.Parse(RSA.ConvertNumber(hexString, "hex", "dec"));
                    if (bi >= n)
                    {
                        break; // khối quá lớn, bỏ qua
                    }

                    BigInteger c = BigInteger.ModPow(bi, dece, n); // mã hóa: c = m^e mod n
                    byte[] cipherBytes = c.ToByteArray();
                    while (cipherBytes.Length < (blockSize + 1) && (i + blockSize) < messageBytes.Length)
                    {
                        Array.Resize(ref cipherBytes, cipherBytes.Length + 1);
                    }
                    if ((i + blockSize) >= messageBytes.Length && cipherBytes[cipherBytes.Length - 1] == 0x00)
                    {
                        Array.Resize(ref cipherBytes, cipherBytes.Length - 1);
                    }

                    string hex = RSA.ByteArrayToHex(cipherBytes);

                    fullHexBuilder.Append(hex);

                }

                // 7. Chuyển dữ liệu mã hóa sang định dạng đầu ra và hiển thị
                string fullHex = fullHexBuilder.ToString();

                string output = RSA.ConvertHexTo(fullHex, typeoutput);
                encryptedMessage.Add(output);
                textBox4.Text = blockSize.ToString();
                textBox5.Text = (blockSize + 1).ToString();


                richTextBox2.Text = string.Join("", encryptedMessage.Select(c => c.ToString()));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        




        private void btn_Decrypt_Click_1(object sender, EventArgs e)
        {
            try
            {
                string sp = tb_valueP.Text;
                string sq = tb_valueQ.Text;
                string se = tb_valueE.Text;
                BigInteger p = 0;
                BigInteger q = 0;
                BigInteger eValue = 0;
                BigInteger n = BigInteger.Parse(tb_valueN.Text);
                BigInteger phi_n = BigInteger.Parse(tb_valuePhi_n.Text);
                BigInteger d = BigInteger.Parse(tb_valueD.Text);
                if (!radioButton1.Checked)
                {
                    p = BigInteger.Parse(RSA.ConvertNumber(sp, "hex", "dec"));
                    q = BigInteger.Parse(RSA.ConvertNumber(sq, "hex", "dec"));
                    eValue = BigInteger.Parse(RSA.ConvertNumber(se, "hex", "dec"));
                }
                else
                {
                    p = BigInteger.Parse(sp);
                    q = BigInteger.Parse(sq);
                    eValue = BigInteger.Parse(se);
                }
                string typeinput = "";
                if (radioButton3.Checked) typeinput = "Base64";
                else if (radioButton4.Checked) typeinput = "bin";
                else if (radioButton5.Checked) typeinput = "hex";
                else if (radioButton9.Checked) typeinput = "ASCII";
                string typeoutput = "";
                if (radioButton7.Checked) typeoutput = "Base64";
                else if (radioButton6.Checked) typeoutput = "bin";
                else if (radioButton8.Checked) typeoutput = "hex";
                else if (radioButton10.Checked) typeoutput = "ASCII";

                string encryptedMessage = richTextBox1.Text;

                byte[] fullBytes = null;
                fullBytes = RSA.getMessageByte(fullBytes, typeinput, encryptedMessage);
             
                if (fullBytes == null) return;

                List<byte> finalDecryptedBytes = null;
                List<int> divisors = RSA.GetDivisors(fullBytes.Length);
                int partSize = 0;
                if (!string.IsNullOrWhiteSpace(textBox4.Text) && !string.IsNullOrWhiteSpace(textBox5.Text) && checkBox1.Checked)
                {
                    partSize = int.Parse(textBox4.Text);
                }
                else
                {
                    string hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(RSA.ConvertNumber(hexp, "hex", "dec")) != p) hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex");
                    string hexq = RSA.ConvertNumber(q.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(RSA.ConvertNumber(hexq, "hex", "dec")) != q) hexq = RSA.ConvertNumber(p.ToString(), "dec", "hex");
                    partSize = (hexp.Length + hexq.Length) / 2;
                }
                List<byte[]> parts = new List<byte[]>();

                for (int i = 0; i < fullBytes.Length; i += partSize)
                {
                    byte[] part = fullBytes.Skip(i).Take(partSize).Reverse().ToArray();
                    parts.Add(part);
                }

                List<byte> decryptedBytes = new List<byte>();
                bool valid = true;

                foreach (byte[] part in parts)
                {
                    string hexString = RSA.ByteArrayToHex(part);
                    BigInteger bi = BigInteger.Parse(RSA.ConvertNumber(hexString, "hex", "dec"));
                    if (bi >= n)
                    {
                        valid = false;
                        break;
                    }
                    BigInteger m = BigInteger.ModPow(bi, d, n);


                    byte[] mBytes = m.ToByteArray();
                    decryptedBytes.AddRange(mBytes);
                }

                if (valid)
                {
                    finalDecryptedBytes = decryptedBytes;
                    textBox4.Text = partSize.ToString();
                    textBox5.Text = (partSize - 1).ToString();
                }


                if (finalDecryptedBytes == null)
                {
                    MessageBox.Show("Không tìm được cách chia hợp lệ.");
                    return;
                }
                string hex = RSA.ByteArrayToHex(finalDecryptedBytes.ToArray());
                string output = RSA.ConvertHexTo(hex, typeoutput);

                richTextBox2.Text = output;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }




        private void btn_CalculateValues_Click(object sender, EventArgs e)
        {

        }
    }
}
