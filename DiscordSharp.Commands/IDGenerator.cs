using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordSharp.Commands
{
    public class IDGenerator
    {
        private static Random AlphaGenerator = new Random(DateTime.Now.Millisecond);
        private static Random NumericalGenerator = new Random((int)(DateTime.Now.Millisecond * 98734));

        private static String[] Alphabet = new string[] { "a", "b", "c", "d", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        internal IDGenerator() { }

        public static string GenerateRandomCode()
        {
            List<string> code = new List<string>();
            for (int i = 0; i < 25; i++)
            {
                int letterOrNum = NumericalGenerator.Next(0, 9);
                if (IsOdd(letterOrNum)) //alpha
                {
                    string letter = Alphabet[AlphaGenerator.Next(0, Alphabet.Length - 1)];
                    if (code.Count > 0)
                        while (letter == code[i - 1])
                            letter = Alphabet[AlphaGenerator.Next(0, Alphabet.Length - 1)];
                    code.Add(letter);
                }
                else //num
                {
                    int num = NumericalGenerator.Next(0, 9);
                    if (code.Count > 0)
                        while (num.ToString() == code[i - 1])
                            num = NumericalGenerator.Next(0, 9);
                    code.Add(num.ToString());
                }
            }


            return ArrayToString(code.ToArray<string>());
        }

        private static string ArrayToString(String[] array)
        {
            string code = "";
            for (int i = 0; i < array.Length - 1; i++)
            {
                code += array[i];
            }
            return code;
        }

        private static bool IsOdd(int num)
        {
            return num % 2 != 0;
        }
    }
}