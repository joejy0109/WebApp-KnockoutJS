 public class SimpleEncryptor
    {
        static readonly string Keys = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static Random Rnd = new Random();

        private int _joinCount;

        public SimpleEncryptor(int joinCount)
        {
            _joinCount = joinCount;
        }

        public SimpleEncryptor() : this(4)
        {

        }

        static string Randomize(object value, int count)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(value);
            for (int i = 0; i < count; i++)
            {
                sb.Append(Keys[Rnd.Next(Keys.Length - 1)]);
            }
            return sb.ToString();
        }

        public string Encrypt(long val)
        {
            System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
            var ret = Randomize((val << 2).ToString("x"), _joinCount);
            Console.WriteLine("Encryption exetime: {0}", s.ElapsedMilliseconds);
            return ret;
        }

        public long Decrypt(string val)
        {
            System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
            var ret = Convert.ToInt64(val.Substring(0, val.Length - _joinCount), 16) >> 2;
            Console.WriteLine("Encryption exetime: {0}", s.ElapsedMilliseconds);
            return ret;
        }
    }
