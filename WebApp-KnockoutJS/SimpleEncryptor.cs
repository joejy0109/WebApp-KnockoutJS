public class SimpleEncryptor
{
    static readonly string Keys = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    static Random Rnd = new Random();

    private int _keyCount;

    public SimpleEncryptor(int keyCount)
    {
        _keyCount = keyCount;
    }

    static string GetRandomKeys(int count)
    {
        if (count == 0)
            throw new ArgumentException("count must be greather than 0");
        
        var sb = new StringBuilder(count);
        for (int i = 0; i < count; i++)
        {
            sb.Append(Keys[Rnd.Next(Keys.Length - 1)]);
        }
        return sb.ToString();
    }

    static readonly char flag = Encoding.UTF8.GetChars(Encoding.UTF8.GetPreamble())[0];

    public string Encrypt(long plainValue)
    {
        Stopwatch s = Stopwatch.StartNew();
                    
        var val = ((plainValue << 2).ToString("x")).ToString();
        // 대상 값 길이가 keyset 길이보다 크면 keyset크기를 동일하게 맞춘다.            
        var keyCnt = val.Length > _keyCount ? val.Length : _keyCount;
        var keys = GetRandomKeys(keyCnt);

        bool addedBOM = false;
        int chipherIdx = 0, wordIdx = 0;
        
        var totalLength = val.Length + keys.Length;

        var arr = new char[totalLength];
        for (int i = 0; i < totalLength; i++)
        {
            // 실제 값을 모두 채운 상태에서 빈 배열이 존재할 경우 임의의 바이트를 삽입한다.
            if (wordIdx >= val.Length && !addedBOM)
            {
                arr[i] = flag;
                addedBOM = true;
                continue;
            }
            // 대상 값 크기보다 keyset이 작은 경우 임의의 바이트를 삽입한 후 나머지 배열에
            // 랜덤 char를 채운다.
            if (addedBOM)
                arr[i] = Keys[Rnd.Next(Keys.Length - 1)];
            // 짝수(0자리 포함) 자리에 할당된 keyset만큼 채운다.
            else if ((i % 2 == 0 && chipherIdx < keys.Length))
                arr[i] = keys[chipherIdx++];
            // 홀수자리에 실제 값을 채운다.
            else if (wordIdx < val.Length)
                arr[i] = val[wordIdx++];
        }
        // 배열을 뒤집는다.
        Array.Reverse(arr);
        // 문자열로 치환.
        var ret = string.Join(string.Empty, arr);
        Console.WriteLine("Encryption exetime: {0}", s.ElapsedMilliseconds);
        return ret;
    }

    public long Decrypt(string chipherText)
    {
        Stopwatch s = Stopwatch.StartNew();

        var arr = new char[chipherText.Length];
        for (int i = 0; i < chipherText.Length; i++)
        {
            arr[i] = chipherText[i];
        }
        Array.Reverse(arr);
        var sb = new StringBuilder();
        for (int i = 0; i < arr.Length; i++)
        {
            // 임의의 바이트 발견시 char 할당 종료
            if (arr[i] == flag)
                break;
            if (i % 2 > 0) sb.Append(arr[i]);
        }
        var ret = Convert.ToInt64(sb.ToString(), 16) >> 2;
        Console.WriteLine("Decryption exetime: {0}", s.ElapsedMilliseconds);
        return ret;
    }
}
