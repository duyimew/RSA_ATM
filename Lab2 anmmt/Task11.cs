using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace Lab2_anmmt
{
    public partial class Task11 : Form
    {

        public Task11()
        {
            InitializeComponent();
        }

        private void bt_Random_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = GeneratePrime(8).ToString();
                textBox2.Text = GeneratePrime(16).ToString();
                textBox3.Text = GeneratePrime(64).ToString();
            }
            catch (Exception ex)
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
        private static BigInteger GenerateRandomBigInteger(BigInteger minValue, BigInteger maxValue)
        {
            Random rand = new Random();
            byte[] bytes = maxValue.ToByteArray();
            BigInteger randomValue;

            do
            {
                for(int i= 0;i<bytes.Length;i++)
                bytes[i]= (byte)rand.Next(0,127);
                if(bytes.Length>1) bytes[bytes.Length - 1] =0; 

                randomValue = new BigInteger(bytes);
     
                if (randomValue.Sign == -1)
                {
                    randomValue = -randomValue;
                }
            } while (randomValue < minValue || randomValue > maxValue);

            return randomValue;
        }
        public static void CheckUnder289(BigInteger number)
        {
            BigInteger bigInteger = BigInteger.Pow(2, 89) - 1;
            if (number >= bigInteger)
            {
                MessageBox.Show("Số quá lớn");
                return;
            }

            if (IsPrime(number))
            {
                MessageBox.Show("Đây là số nguyên tố");
                return;
            }
            MessageBox.Show("Đây không phải số nguyên tố");
        }

        public static BigInteger GCD(BigInteger a, BigInteger b)
        {
            try
            {
                while (b != 0)
                {
                    BigInteger temp = b;
                    b = a % b;
                    a = temp;
                }
                return a;
            }
            catch
            {
                MessageBox.Show("Không tìm được");
                return BigInteger.Zero;
            }
        }

        public static BigInteger ModularExponentiation(BigInteger a, BigInteger x, BigInteger p)
        {
            BigInteger result = 1;
            a = a % p;

            while (x > 0)
            {
                if ((x % 2) == 1)
                {
                    result = (result * a) % p;
                }
                x = x >> 1;  // x = x / 2
                a = (a * a) % p;
            }
            return result;
        }

        public static int[] MersennePrimes = new int[]
       {
2,
3,
5,
7,
13,
17,
19,
31,
61,
89
           
       };
        public static BigInteger[] GetLargestPrimesUnderMersenne()
        {
            BigInteger[] largestPrimes = new BigInteger[10];
            for (int i = 0; i < MersennePrimes.Length; i++)
            {
                BigInteger bigInteger = BigInteger.Pow(2, MersennePrimes[i])-1;
                largestPrimes[i] = FindLargestPrimeUnder(bigInteger);
            }
            return largestPrimes;
        }

        private static BigInteger FindLargestPrimeUnder(BigInteger number)
        {
            BigInteger candidate = number - 1;
            while (candidate > 2)
            {
                if (IsPrime(candidate))
                {
                    return candidate;
                }
                candidate--;
            }
            return 2;
        }

        private void bt_find1_Click(object sender, EventArgs e)
        {
            try
            {
                BigInteger[] bigIntegers = GetLargestPrimesUnderMersenne();
                foreach (BigInteger bigInteg in bigIntegers)
                {
                    richTextBox1.Text += bigInteg.ToString() + "\n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void bt_check_Click(object sender, EventArgs e)
        {
            try
            {
                BigInteger bigInteger = BigInteger.Parse(textBox4.Text);
                CheckUnder289(bigInteger);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void bt_find2_Click(object sender, EventArgs e)
        {
            try
            {
                BigInteger bigIntegerA = BigInteger.Parse(textBox5.Text);
                BigInteger bigIntegerB = BigInteger.Parse(textBox6.Text);
                textBox7.Text = GCD(bigIntegerA, bigIntegerB).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void bt_find3_Click(object sender, EventArgs e)
        {
            try
            {
                BigInteger bigIntegerA = BigInteger.Parse(textBox10.Text);
                BigInteger bigIntegerX = BigInteger.Parse(textBox9.Text);
                BigInteger bigIntegerP = BigInteger.Parse(textBox11.Text);
                textBox8.Text = ModularExponentiation(bigIntegerA, bigIntegerX, bigIntegerP).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
