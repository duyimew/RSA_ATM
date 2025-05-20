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
        public Task22()
        {
            InitializeComponent();
        }

        private void btn_Random_Click(object sender, EventArgs e)
        {
            try
            {
                string decp = GeneratePrime(128).ToString();
                string decq = "";
                while (true)
                {
                    decq = GeneratePrime(128).ToString();
                    if (decq != decp) break; // đảm bảo p khác q
                }

                //Tính phi(n)
                BigInteger p = BigInteger.Parse(decp);
                BigInteger q = BigInteger.Parse(decq);
                BigInteger phi_n = (p - 1) * (q - 1);

                //Tính e
                BigInteger dece = 0;
                dece = GenerateE(dece, phi_n);
                textBox3.Text = dece.ToString();

                if (!radioButton1.Checked)
                {
                    // Nếu người dùng chọn "Hex", chuyển đổi từ Dec sang Hex
                    textBox1.Text = ConvertNumber(decp, "dec", "hex"); 
                    textBox2.Text = ConvertNumber(decq, "dec", "hex"); 
                    textBox3.Text = ConvertNumber(dece.ToString(), "dec", "hex"); 
                }
                else
                {
                    textBox1.Text = decp; 
                    textBox2.Text = decq; 
                    textBox3.Text = dece.ToString();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        public static BigInteger GeneratePrime(int bitLength)
        {
            BigInteger candidate = BigInteger.Zero;

            BigInteger startValue = (BigInteger.One << bitLength - 1);
            while (true)
            {
                Random random = new Random();
                Byte[] bytes = new Byte[(bitLength + 7) / 8];
                random.NextBytes(bytes);
                BigInteger bigInteger = new BigInteger(bytes);
                if (bigInteger.Sign == -1) bigInteger *= -1;
                if (bigInteger < startValue) bigInteger += startValue;
                candidate = bigInteger;
                if (IsPrime(candidate)) return candidate;
            }
        }
        
        public static BigInteger GenerateE(BigInteger E, BigInteger phi_n)
        {
            Random rnd = new Random();
            BigInteger minValue = BigInteger.Pow(2, 15);
            BigInteger maxValue = BigInteger.Pow(2, 31) - 1;
            //Chọn e
            while (true)
            {
                byte[] bytes = new byte[4];
                rnd.NextBytes(bytes);
                BigInteger candidate = new BigInteger(bytes);
                candidate = BigInteger.Abs(candidate);
                candidate = candidate % (maxValue - minValue + 1) + minValue;

                if (BigInteger.GreatestCommonDivisor(candidate, phi_n) == 1 && IsPrime(candidate))
                {
                    E = candidate;
                    return E;
                }
            }
        }

        private void btn_Encrypt_Click_1(object sender, EventArgs e)
        {
            try
            {
                // 1. Lấy giá trị p, q, e từ giao diện
                string sp = textBox1.Text;
                string sq = textBox2.Text;
                string se = textBox3.Text;
                BigInteger p = 0;
                BigInteger q = 0;
                BigInteger dece = 0;
                if (!radioButton1.Checked)
                {
                    p = BigInteger.Parse(ConvertNumber(sp, "hex", "dec"));
                    q = BigInteger.Parse(ConvertNumber(sq, "hex", "dec"));
                    dece = BigInteger.Parse(ConvertNumber(se, "hex", "dec"));
                }  // nếu người dùng nhập dưới dạng hex
                else
                {
                    p = BigInteger.Parse(sp);
                    q = BigInteger.Parse(sq);
                    dece = BigInteger.Parse(se);
                } // nếu là dec

                // 2. Tính n = p * q và phi(n)
                BigInteger n = p * q;
                BigInteger phi_n = (p - 1) * (q - 1);
                BigInteger d = ModInverse(dece, phi_n);

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
                messageBytes = getMessageByte(messageBytes, typeinput, message);
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
                    string hexp = ConvertNumber(p.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(ConvertNumber(hexp, "hex", "dec")) != p) hexp = ConvertNumber(p.ToString(), "dec", "hex");
                    string hexq = ConvertNumber(q.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(ConvertNumber(hexq, "hex", "dec")) != q) hexq = ConvertNumber(p.ToString(), "dec", "hex");
                    blockSize = (hexp.Length + hexq.Length) / 2 - 1;
                }
                // 6. Mã hóa từng khối
                StringBuilder fullHexBuilder = new StringBuilder();
                for (int i = 0; i < messageBytes.Length; i += blockSize)
                {
                    byte[] block = messageBytes.Skip(i).Take(blockSize).Reverse().ToArray(); // đảo byte

                    string hexString = ByteArrayToHex(block);
                    BigInteger bi = BigInteger.Parse(ConvertNumber(hexString, "hex", "dec"));
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

                    string hex = ByteArrayToHex(cipherBytes);

                    fullHexBuilder.Append(hex);

                }

                // 7. Chuyển dữ liệu mã hóa sang định dạng đầu ra và hiển thị
                string fullHex = fullHexBuilder.ToString();

                string output = ConvertHexTo(fullHex, typeoutput);
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
                string sp = textBox1.Text;
                string sq = textBox2.Text;
                string se = textBox3.Text;
                BigInteger p = 0;
                BigInteger q = 0;
                BigInteger eValue = 0;
                if (!radioButton1.Checked)
                {
                    p = BigInteger.Parse(ConvertNumber(sp, "hex", "dec"));
                    q = BigInteger.Parse(ConvertNumber(sq, "hex", "dec"));
                    eValue = BigInteger.Parse(ConvertNumber(se, "hex", "dec"));
                }
                else
                {
                    p = BigInteger.Parse(sp);
                    q = BigInteger.Parse(sq);
                    eValue = BigInteger.Parse(se);
                }
                BigInteger n = p * q;
                BigInteger phi_n = (p - 1) * (q - 1);
                BigInteger d = ModInverse(eValue, phi_n); // tính khóa bí mật d
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
                fullBytes = getMessageByte(fullBytes, typeinput, encryptedMessage);
                /*switch (typeinput)
                {
                    case "Base64":
                        string cleanedBinary2 = encryptedMessage.Replace(" ", "")
              .Replace("\r", "")
              .Replace("\n", "")
              .Replace("\t", "");
                        fullBytes = Convert.FromBase64String(cleanedBinary2);
                        break;

                    case "bin":
                        string cleanedBinary = encryptedMessage.Replace(" ", "")
                                                      .Replace("\r", "")
                                                      .Replace("\n", "")
                                                      .Replace("\t", "");

                        List<byte> binBytes = new List<byte>();
                        for (int i = 0; i < cleanedBinary.Length; i += 8)
                        {
                            string byteString = cleanedBinary.Substring(i, Math.Min(8, cleanedBinary.Length - i));
                            if (byteString.Length == 8)
                                binBytes.Add(Convert.ToByte(byteString, 2));
                        }
                        fullBytes = binBytes.ToArray();
                        break;


                    case "hex":
                        string cleanedBinary1 = encryptedMessage.Replace(" ", "")
                                  .Replace("\r", "")
                                  .Replace("\n", "")
                                  .Replace("\t", "");

                        List<byte> hexBytes = new List<byte>();
                        for (int i = 0; i < cleanedBinary1.Length; i += 2)
                        {
                            string hexByte = cleanedBinary1.Substring(i, Math.Min(2, cleanedBinary1.Length - i));
                            hexBytes.Add(Convert.ToByte(hexByte, 16));
                        }
                        fullBytes = hexBytes.ToArray();
                        break;

                    case "ASCII":
                        encryptedMessage = encryptedMessage.TrimEnd(' ', '\r', '\n', '\t');
                        fullBytes = System.Text.Encoding.ASCII.GetBytes(encryptedMessage);
                        break;

                    default:
                        MessageBox.Show("Vui lòng chọn loại đầu vào hợp lệ.");
                        break;
                }*/
                if (fullBytes == null) return;

                List<byte> finalDecryptedBytes = null;
                List<int> divisors = GetDivisors(fullBytes.Length);
                int partSize = 0;
                if (!string.IsNullOrWhiteSpace(textBox4.Text) && !string.IsNullOrWhiteSpace(textBox5.Text) && checkBox1.Checked)
                {
                    partSize = int.Parse(textBox4.Text);
                }
                else
                {
                    string hexp = ConvertNumber(p.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(ConvertNumber(hexp, "hex", "dec")) != p) hexp = ConvertNumber(p.ToString(), "dec", "hex");
                    string hexq = ConvertNumber(q.ToString(), "dec", "hex").TrimStart('0');
                    if (BigInteger.Parse(ConvertNumber(hexq, "hex", "dec")) != q) hexq = ConvertNumber(p.ToString(), "dec", "hex");
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
                    string hexString = ByteArrayToHex(part);
                    BigInteger bi = BigInteger.Parse(ConvertNumber(hexString, "hex", "dec"));
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
                string hex = ByteArrayToHex(finalDecryptedBytes.ToArray());
                string output = ConvertHexTo(hex, typeoutput);

                richTextBox2.Text = output;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        
        public static BigInteger CalculateD(BigInteger e, BigInteger p, BigInteger q)
        {
            BigInteger n = p * q;
            BigInteger phi_n = (p - 1) * (q - 1);
            return ModInverse(e, phi_n);
        }
        public static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                BigInteger q = a / m;
                BigInteger t = m;

                m = a % m;
                a = t;
                t = y;

                y = x - q * y;
                x = t;
            }

            if (x < 0)
                x += m0;

            return x;
        }
        private List<int> GetDivisors(int number)
        {
            List<int> divisors = new List<int>();
            for (int i = 2; i <= number / 2; i++)
            {
                if (number % i == 0)
                {
                    divisors.Add(i);
                }
            }
            divisors.Add(number);
            return divisors.OrderBy(x => x).ToList();
        }


        public string ByteArrayToHex(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray).Replace("-", "").ToLower();
        }

        public static string ConvertHexTo(string hex, string typeoutput)
        {
            try
            {
                byte[] bytes = Enumerable.Range(0, hex.Length / 2)
                                         .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                                         .ToArray();

                switch (typeoutput)
                {
                    case "Base64":
                        return Convert.ToBase64String(bytes);

                    case "bin":
                        return string.Join(" ", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                    case "ASCII":
                        return System.Text.Encoding.ASCII.GetString(bytes);

                    case "hex":
                        return string.Join(" ", bytes.Select(b => b.ToString("X2")));

                    default:
                        MessageBox.Show("Loại đầu ra không hợp lệ.");
                        return null;
                }
            }
            catch
            {
                MessageBox.Show("Hex không hợp lệ!");
                return null;
            }
        }
        private static BigInteger GenerateRandomBigInteger(BigInteger minValue, BigInteger maxValue)
        {
            Random rand = new Random();
            byte[] bytes = maxValue.ToByteArray();
            BigInteger randomValue;

            do
            {
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = (byte)rand.Next(0, 127);
                if (bytes.Length > 1) bytes[bytes.Length - 1] = 0;
                randomValue = new BigInteger(bytes);
                if (randomValue.Sign == -1)
                {
                    randomValue = -randomValue;
                }
            } while (randomValue < minValue || randomValue > maxValue);

            return randomValue;
        }


        public static Byte[] getMessageByte(Byte[] messageBytes, string inputType, string message)
        {
            switch (inputType)
            {
                case "Base64":
                    string cleanedBinary2 = message.Replace(" ", "")
          .Replace("\r", "")
          .Replace("\n", "")
          .Replace("\t", "");
                    messageBytes = Convert.FromBase64String(cleanedBinary2);
                    return messageBytes;

                case "bin":
                    string cleanedBinary = message.Replace(" ", "")
                                                  .Replace("\r", "")
                                                  .Replace("\n", "")
                                                  .Replace("\t", "");

                    List<byte> binBytes = new List<byte>();
                    for (int i = 0; i < cleanedBinary.Length; i += 8)
                    {
                        string byteString = cleanedBinary.Substring(i, Math.Min(8, cleanedBinary.Length - i));
                        if (byteString.Length == 8)
                            binBytes.Add(Convert.ToByte(byteString, 2));
                    }
                    messageBytes = binBytes.ToArray();
                    return messageBytes;



                case "hex":
                    string cleanedBinary1 = message.Replace(" ", "")
                              .Replace("\r", "")
                              .Replace("\n", "")
                              .Replace("\t", "");

                    List<byte> hexBytes = new List<byte>();
                    for (int i = 0; i < cleanedBinary1.Length; i += 2)
                    {
                        string hexByte = cleanedBinary1.Substring(i, Math.Min(2, cleanedBinary1.Length - i));
                        hexBytes.Add(Convert.ToByte(hexByte, 16));
                    }
                    messageBytes = hexBytes.ToArray();
                    return messageBytes;

                case "ASCII":
                    message = message.TrimEnd(' ', '\r', '\n', '\t');
                    messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
                    return messageBytes;

                default:
                    MessageBox.Show("Vui lòng chọn loại đầu vào hợp lệ.");
                    return null;
            }
        }

        public string ConvertNumber(string input, string fromBase, string toBase)
        {
            try
            {
                BigInteger decimalValue;

                switch (fromBase.ToLower())
                {
                    case "dec":
                        decimalValue = BigInteger.Parse(input.Trim());
                        break;
                    case "hex":
                        decimalValue = BigInteger.Parse("0" + input.Trim(), System.Globalization.NumberStyles.HexNumber);
                        break;
                    case "bin":
                        decimalValue = BigInteger.Parse(ConvertBinaryToDecimal(input.Trim()));
                        break;
                    default:
                        MessageBox.Show("Hệ cơ số đầu vào không hợp lệ!");
                        return null;
                }

                switch (toBase.ToLower())
                {
                    case "dec":
                        return decimalValue.ToString();
                    case "hex":
                        return decimalValue.ToString("X");
                    case "bin":
                        return ConvertDecimalToBinary(decimalValue);
                    default:
                        MessageBox.Show("Hệ cơ số đầu ra không hợp lệ!");
                        return null;
                }
            }
            catch
            {
                MessageBox.Show("Giá trị đầu vào không hợp lệ!");
                return null;
            }
        }

        private string ConvertBinaryToDecimal(string binary)
        {
            BigInteger result = 0;
            foreach (char c in binary)
            {
                if (c != '0' && c != '1')
                    throw new FormatException("Giá trị nhị phân không hợp lệ!");
                result = result * 2 + (c - '0');
            }
            return result.ToString();
        }

        private string ConvertDecimalToBinary(BigInteger dec)
        {
            if (dec == 0) return "0";
            string result = "";
            while (dec > 0)
            {
                result = (dec % 2) + result;
                dec /= 2;
            }
            return result;
        }
        public static bool IsPrime(BigInteger n, int k = 5)
        {
            if (n <= 1) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            if (n < 100)
            {
                for (BigInteger i = 2; i * i <= n; i++)
                {
                    if (n % i == 0)
                        return false;
                }
                return true;
            }
            BigInteger d = n - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            for (int i = 0; i < k; i++)
            {
                BigInteger a = GenerateRandomBigInteger(2, n - 2);
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                for (int j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1) break;
                }

                if (x != n - 1) return false;
            }
            return true;
        }
        public string DetectNumberType(string input)
        {
            input = input.Trim();

            if (System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d+$"))
            {
                return "dec";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(input, @"^[0-9a-fA-F]+$"))
            {
                return "hex";
            }

            return "unknown";
        }

    }
}
