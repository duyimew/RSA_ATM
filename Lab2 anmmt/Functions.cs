using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Lab2_anmmt
{
    public class Functions
    {
        public BigInteger CalculatePhi_N(BigInteger p, BigInteger q)
        {
            BigInteger phi_n = (p - 1) * (q - 1);
            return phi_n;
        }

        public BigInteger GeneratePrime(int bitLength)
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

        public BigInteger GenerateE(BigInteger E, BigInteger phi_n)
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
        public BigInteger CalculateN(BigInteger p, BigInteger q)
        {
            BigInteger n = p * q;
            return n;
        }

        public  BigInteger CalculateD(BigInteger e, BigInteger p, BigInteger q)
        {
            BigInteger phi_n = CalculatePhi_N(p, q);
            return ModInverse(e, phi_n);
        }
        public  BigInteger ModInverse(BigInteger a, BigInteger m)
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
        public List<int> GetDivisors(int number)
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

        public  string ConvertHexTo(string hex, string typeoutput)
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
        private  BigInteger GenerateRandomBigInteger(BigInteger minValue, BigInteger maxValue)
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


        public  Byte[] getMessageByte(Byte[] messageBytes, string inputType, string message)
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
                    //message = message.TrimEnd(' ', '\r', '\n', '\t');
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
        public bool IsPrime(BigInteger n, int k = 5)
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
