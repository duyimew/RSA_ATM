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
    public partial class RSAPlayfair : Form
    {
        RSA_Functions RSA = new RSA_Functions();
        
        public char[,] playfairMatrix;
        public RSAPlayfair()
        {
            InitializeComponent();
            
        }

        private void btn_Random_Click(object sender, EventArgs e)
        {
            try
            {
                if (btn_Random.Text == "Random")
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

                    convertTypeForRandom(decp, decq, d.ToString(), n.ToString(), dece.ToString(), phi_n.ToString());
                }
                else if(btn_Random.Text == "Tính D")
                {
                    BigInteger n = BigInteger.Parse(tb_valueP.Text);
                    BigInteger dece = BigInteger.Parse(tb_valueE.Text);
                    var result = IsProductOfTwoPrimes(n, out BigInteger p, out BigInteger q);
                    if (!result)
                    {
                        MessageBox.Show("Nhập lại N. N phải là tích của đúng hai số nguyên tố");
                        return;
                    }
                    BigInteger d = RSA.CalculateD(dece, p, q);
                    tb_valueQ.Text = d.ToString();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private bool IsProductOfTwoPrimes(BigInteger n, out BigInteger prime1, out BigInteger prime2)
        {
            prime1 = prime2 = 0;
            for (BigInteger i = 2; i * i <= n; i++)
            {
                if (n % i == 0)
                {
                    BigInteger j = n / i;
                    if (RSA.IsPrime(i) && RSA.IsPrime(j))
                    {
                        prime1 = i;
                        prime2 = j;
                        return true;
                    }
                }
            }

            return false;
        }
        public void convertTypeForRandom(string decp, string decq, string d, string n, string dece, string phi_n)
        {
            if (!radioButton1.Checked)
            {
                // Nếu người dùng chọn "Hex", chuyển đổi từ Dec sang Hex
                tb_valueP.Text = RSA.ConvertNumber(decp, "dec", "hex");
                tb_valueQ.Text = RSA.ConvertNumber(decq, "dec", "hex");
                tb_valueE.Text = RSA.ConvertNumber(dece, "dec", "hex");
                tb_valuePhi_n.Text = RSA.ConvertNumber(phi_n, "dec", "hex");
                tb_valueD.Text = RSA.ConvertNumber(d, "dec", "hex");
                tb_valueN.Text = RSA.ConvertNumber(n, "dec", "hex");
            }
            else
            {
                tb_valueP.Text = decp;
                tb_valueQ.Text = decq;
                tb_valueE.Text = dece;
                tb_valuePhi_n.Text = phi_n;
                tb_valueD.Text = d;
                tb_valueN.Text = n;
            }
        }

        public void convertTypeForEnOrDe( string d, string n, string phi_n)
        {
            if (!radioButton1.Checked)
            {
                // Nếu người dùng chọn "Hex", chuyển đổi từ Dec sang Hex
                tb_valuePhi_n.Text = RSA.ConvertNumber(phi_n, "dec", "hex");
                tb_valueD.Text = RSA.ConvertNumber(d, "dec", "hex");
                tb_valueN.Text = RSA.ConvertNumber(n, "dec", "hex");
            }
            else
            {
                tb_valuePhi_n.Text = phi_n;
                tb_valueN.Text = n;
                tb_valueD.Text = d;
            }
        }

        private void btn_Encrypt_Click_1(object sender, EventArgs e)
        {
            try
            {
                BigInteger p = 0;
                BigInteger q = 0;
                BigInteger dece = 0;
                BigInteger n = 0;
                BigInteger phi_n = 0;
                BigInteger d = 0;
                if (radioButton12.Checked)
                {
                    // 1. Lấy giá trị p, q, e từ giao diện
                    string sp = tb_valueP.Text;
                    string sq = tb_valueQ.Text;
                    string se = tb_valueE.Text;
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
                        MessageBox.Show("Chỉ được sử dụng các số nguyên tố cho p và q.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    n = RSA.CalculateN(p, q);
                    phi_n = RSA.CalculatePhi_N(p, q);
                    d = RSA.CalculateD(dece, p, q);
                    convertTypeForEnOrDe(d.ToString(), n.ToString(), phi_n.ToString());
                }
                else
                {
                    n = BigInteger.Parse(tb_valueP.Text);
                    d = BigInteger.Parse(tb_valueQ.Text);
                    dece = BigInteger.Parse(tb_valueE.Text);
                }
                // 3. Xác định định dạng input và output từ radioButton
                string typeinput = getInputType();

                string typeoutput = getOutputType();
                

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
                    if (radioButton12.Checked)
                    {
                        // tự tính blockSize dựa trên độ dài của p và q
                        string hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex").TrimStart('0');
                        if (BigInteger.Parse(RSA.ConvertNumber(hexp, "hex", "dec")) != p) hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex");
                        if (hexp.Length % 2 != 0)
                            hexp = "0" + hexp;
                        string hexq = RSA.ConvertNumber(q.ToString(), "dec", "hex").TrimStart('0');
                        if (BigInteger.Parse(RSA.ConvertNumber(hexq, "hex", "dec")) != q) hexq = RSA.ConvertNumber(q.ToString(), "dec", "hex");
                        if (hexq.Length % 2 != 0)
                            hexq = "0" + hexq;
                        blockSize = (hexp.Length + hexq.Length) / 2 - 1;
                    }
                    else
                    {
                        string hexn = RSA.ConvertNumber(n.ToString(), "dec", "hex").TrimStart('0');
                        if (BigInteger.Parse(RSA.ConvertNumber(hexn, "hex", "dec")) != n) hexn = RSA.ConvertNumber(n.ToString(), "dec", "hex");
                        if (hexn.Length % 2 != 0)
                            hexn = "0" + hexn;
                        blockSize = (hexn.Length) / 2 - 1;
                        if (blockSize == 0)
                        {
                            MessageBox.Show("BlockSize = 0 - RSA cannot work");
                            return;
                        }
                    }
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
                BigInteger p = 0;
                BigInteger q = 0;
                BigInteger eValue = 0;
                BigInteger n = 0;
                BigInteger phi_n = 0;
                BigInteger d = 0;
                if (radioButton12.Checked)
                {
                    string sp = tb_valueP.Text;
                    string sq = tb_valueQ.Text;
                    string se = tb_valueE.Text;

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

                    if (!(RSA.IsPrime(p) && RSA.IsPrime(q)))
                    {
                        MessageBox.Show("Chỉ được sử dụng các số nguyên tố cho p và q.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    n = RSA.CalculateN(p, q);
                    phi_n = RSA.CalculatePhi_N(p, q);
                    d = RSA.CalculateD(eValue, p, q);
                    convertTypeForEnOrDe(d.ToString(), n.ToString(), phi_n.ToString());
                }
                else
                {
                    n = BigInteger.Parse(tb_valueP.Text);
                    d = BigInteger.Parse(tb_valueQ.Text);
                    eValue = BigInteger.Parse(tb_valueE.Text);
                }
                string typeinput = getInputType();
                
                string typeoutput = getOutputType();

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
                    if (radioButton12.Checked)
                    {
                        string hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex").TrimStart('0');
                        if (BigInteger.Parse(RSA.ConvertNumber(hexp, "hex", "dec")) != p) hexp = RSA.ConvertNumber(p.ToString(), "dec", "hex");
                        if (hexp.Length % 2 != 0)
                            hexp = "0" + hexp;
                        string hexq = RSA.ConvertNumber(q.ToString(), "dec", "hex").TrimStart('0');
                        if (BigInteger.Parse(RSA.ConvertNumber(hexq, "hex", "dec")) != q) hexq = RSA.ConvertNumber(q.ToString(), "dec", "hex");
                        if (hexq.Length % 2 != 0)
                            hexq = "0" + hexq;
                        partSize = (hexp.Length + hexq.Length) / 2;
                    }
                    else 
                    {
                        string hexn = RSA.ConvertNumber(n.ToString(), "dec", "hex").TrimStart('0');
                        if (BigInteger.Parse(RSA.ConvertNumber(hexn, "hex", "dec")) != n) hexn = RSA.ConvertNumber(n.ToString(), "dec", "hex");
                        if (hexn.Length % 2 != 0)
                            hexn = "0" + hexn;
                        partSize = (hexn.Length) / 2;
                        if (partSize - 1 == 0)
                        {
                            MessageBox.Show("BlockSize = 0 - RSA cannot work");
                            return;
                        }
                    }
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

        public string getInputType()
        {
            if (radioButton3.Checked) return "Base64";
            else if (radioButton4.Checked) return "bin";
            else if (radioButton5.Checked) return "hex";
            else if (radioButton9.Checked) return "ASCII";
            return null;
        }

        public string getOutputType()
        {
            if (radioButton7.Checked) return "Base64";
            else if (radioButton6.Checked) return "bin";
            else if (radioButton8.Checked) return "hex";
            else if (radioButton10.Checked) return "ASCII";
            return null;
        }

        //*******************************************************************************************************************************
        //PLAYFAIR CODE SECTION 
        //*******************************************************************************************************************************
        
        private void DisplayMatrix()
        {
            bool useExtended = false;
            if (rb_6x6.Checked) useExtended = true;
            else useExtended = false;
            int size = useExtended ?6:5;
            tbl_Matrix.RowCount = size;
            tbl_Matrix.ColumnCount = size;
            tbl_Matrix.Controls.Clear();

            string key = textBox1.Text;
            playfairMatrix = CreatePlayfairMatrix(key, useExtended);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Label lbl = new Label();
                    lbl.Text = playfairMatrix[i, j].ToString();
                    lbl.Dock = DockStyle.Fill;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BorderStyle = BorderStyle.FixedSingle;
                    tbl_Matrix.Controls.Add(lbl, j, i);
                }
            }
        }

        public char[,] CreatePlayfairMatrix(string key, bool useExtended = false)
        {
            key = key.ToUpper().Replace("J", "I");
            //string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string alphabet = useExtended
            ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" // 6x6
            : "ABCDEFGHIKLMNOPQRSTUVWXYZ";           // 5x5 (J bị loại bỏ)
            string uniqueKey = "";

            foreach (char c in key)
            {
                if (!uniqueKey.Contains(c) && alphabet.Contains(c)) //Kiểm tra kí tự có tồn tại trong chuỗi uniqueKey chưa 
                    uniqueKey += c;                                    // và xem nó phải là chữ cái trong bảng chữ cái 25 kí tự hay không
            }

            foreach (char c in alphabet)
            {
                if (!uniqueKey.Contains(c)) // Sau khi đã thêm chuỗi key vào đằng trước rồi thì những kí tự còn lại sẽ được đề vào sau
                    uniqueKey += c;
            }
            int size = useExtended ? 6 : 5;
            char[,] matrix = new char[size, size];
            int index = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = uniqueKey[index++]; //Tạo 1 ma trận 5x5 có các tự của key và các chữ cái còn lại không nằm trong key
                }
            }
            return matrix;
        }


        private void Encrypt_click(object sender, EventArgs e)
        {
            
            string plaintext = rtb_PlayfairInput.Text;            
            string pairsText = GeneratePairs(plaintext);
            rtb_PlayfairPairs.Text = pairsText;

            rtb_PlayfairOutput.Text = EnorDePairs(pairsText, true);

        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            string cipertext = rtb_PlayfairInput.Text;
            rtb_PlayfairOutput.Text = EnorDePairs(cipertext, false);
        }

        private string EnorDePairs(string pairsText, bool EnorDe)
        {
            string[] pairs = pairsText.Split(' ');
            string result = "";
            bool useExtended = false;
            if (rb_6x6.Checked) useExtended = true;
            else useExtended = false;
            foreach (var pair in pairs)
            {
                result += EnorDePlayfair(pair[0], pair[1], EnorDe, useExtended) + " ";
            }

            return result.Trim();
        }

        private string EnorDePlayfair(char a, char b, bool EnorDe, bool useExtended = false)
        {
            int rowA = 0, colA = 0, rowB = 0, colB = 0;
            int size = useExtended ? 6 : 5;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (playfairMatrix[i, j] == a) // Xét trong cặp kí tự, tìm kiếm kí tự đầu tiên trong ma trận
                    {
                        rowA = i;
                        colA = j;
                    }
                    if (playfairMatrix[i, j] == b) // Tìm trong kí tự thứ 2 trong cặp kí tự
                    {
                        rowB = i;
                        colB = j;
                    }
                }
            }
            // Chia ra 3 trường hợp của Playfair: Cùng hàng; cùng cột; khác hàng khác cột
            if (rowA == rowB) //Trường hợp cùng hàng
            {
                if (EnorDe)
                {
                    colA = (colA + 1) % size;
                    colB = (colB + 1) % size;
                }
                else
                {
                    colA = (size + colA - 1) % size;
                    colB = (size + colB - 1) % size;
                }
            }
            else if (colA == colB) //Trường hợp cùng cột 
            {
                if (EnorDe)
                {
                    rowA = (rowA + 1) % size;
                    rowB = (rowB + 1) % size;
                }
                else
                {
                    rowA = (size + rowA - 1) % size;
                    rowB = (size + rowB - 1) % size;
                }
            }
            else // Trường hợp khác hàng khác cột
            {
                int temp = colA;
                colA = colB;
                colB = temp;
            }

            return playfairMatrix[rowA, colA].ToString() + playfairMatrix[rowB, colB].ToString();
        }

        private string GeneratePairs(string text)
        {
            
            text = text.ToUpper().Replace("J", "I"); // 5x5 cổ điển: thay J bằng I
            
            StringBuilder formattedText = new StringBuilder();

            // Loại bỏ ký tự không phải chữ cái
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                    formattedText.Append(c);
            }

            List<string> pairs = new List<string>();
            int i = 0;

            // Chia thành từng cặp ký tự
            while (i < formattedText.Length)
            {
                char first = formattedText[i];
                char second = (i + 1 < formattedText.Length) ? formattedText[i + 1] : 'X'; // Nếu lẻ thì thêm X

                if (first == second)
                {
                    // Nếu hai ký tự giống nhau, chèn 'X' vào giữa và đẩy ký tự thứ hai sang cặp sau
                    pairs.Add($"{first}X");
                    i++; // Giữ nguyên vị trí ký tự thứ hai để xử lý lại trong vòng lặp
                }
                else
                {
                    pairs.Add($"{first}{second}");
                    i += 2;
                }
            }

            return string.Join(" ", pairs); // Ghép lại thành chuỗi kết quả
        }

        private void Task22_Load(object sender, EventArgs e)
        {
            DisplayMatrix();
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            DisplayMatrix();  //Mỗi lần nhập một kí tự mới vào ô textbox để dành nhập key thì sẽ cập nhật lại ma trận
        }

        private void rb_6x6_CheckedChanged(object sender, EventArgs e)
        {
            DisplayMatrix();
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
                label9.Visible = radioButton12.Checked;
                label10.Visible = radioButton12.Checked;
                label11.Visible = radioButton12.Checked;
                tb_valueN.Visible = radioButton12.Checked;
                tb_valueD.Visible = radioButton12.Checked;
                tb_valuePhi_n.Visible = radioButton12.Checked;
                label1.Text = radioButton12.Checked ? "p":"n";
                label2.Text = radioButton12.Checked ? "q":"d";
                btn_Random.Text = radioButton12.Checked ? "Random" : "Tính D";
                groupBox3.Visible = radioButton12.Checked;
        }



    }
}
