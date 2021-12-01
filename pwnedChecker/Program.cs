using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Http;

namespace pwnedChecker
{
    class Program
    {
        static string hashPassSuffix;
        static string hashPassPrefix;
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Password to check:");
                var clearPass = string.Empty;
                ConsoleKey key;
                do
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && clearPass.Length > 0)
                    {
                        Console.Write("\b \b");
                        clearPass = new string(clearPass.Take(clearPass.Length-1).ToArray());
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        clearPass += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);
                Console.WriteLine(Environment.NewLine);
                //Console.SetCursorPosition(clearPass.Length, Console.CursorTop-1);
                // Console.Write(Console.CursorLeft+" || "+Console.CursorTop);
                createHash(clearPass);
                callAPI(hashPassPrefix);
                Console.WriteLine(Environment.NewLine);
            }
        }

        private static void callAPI(string hashPassPrefix)
        {
            string url= @"https://api.pwnedpasswords.com/range/"+hashPassPrefix;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage result = httpClient.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("Found {0} time(s)", new string((result.Content.ReadAsStringAsync().Result.Split('\n').ToList().FirstOrDefault(x => new string(x.TakeWhile(y => y != ':').ToArray()) == hashPassSuffix) ?? ":0").SkipWhile(x => x != ':').Skip(1).ToArray()));               
            }
        }

        private static void createHash(string password)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
   
                hashPassPrefix = new string(sb.ToString().Take(5).ToArray());
                hashPassSuffix = new string(sb.ToString().Skip(5).ToArray());
            }
        }
    }
}
