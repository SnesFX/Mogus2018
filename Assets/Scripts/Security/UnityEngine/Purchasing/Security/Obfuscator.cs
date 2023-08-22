using System;
using System.Linq;

namespace UnityEngine.Purchasing.Security
{
    public static class Obfuscator
    {
        public class InvalidOrderArray : Exception
        {
        }

        public static byte[] DeObfuscate(byte[] data, int[] order, int key)
        {
            byte[] array = new byte[data.Length];
            int num = data.Length / 20 + 1;
            bool flag = data.Length % 20 != 0;
            Array.Copy(data, array, data.Length);
            for (int num2 = order.Length - 1; num2 >= 0; num2--)
            {
                int num3 = order[num2];
                int num4 = ((flag && num3 == num - 1) ? (data.Length % 20) : 20);
                byte[] sourceArray = array.Skip(num2 * 20).Take(num4).ToArray();
                Array.Copy(array, num3 * 20, array, num2 * 20, num4);
                Array.Copy(sourceArray, 0, array, num3 * 20, num4);
            }
            return array.Select((byte x) => (byte)(x ^ key)).ToArray();
        }

        public static byte[] Obfuscate(byte[] data, int[] order, out int rkey)
        {
            System.Random random = new System.Random();
            int key = random.Next(2, 255);
            byte[] array = new byte[data.Length];
            int num = data.Length / 20 + 1;
            if (order == null || order.Length < num)
            {
                throw new InvalidOrderArray();
            }
            Array.Copy(data, array, data.Length);
            for (int i = 0; i < num - 1; i++)
            {
                int num2 = (order[i] = random.Next(i, num - 1));
                int num3 = 20;
                byte[] sourceArray = array.Skip(i * 20).Take(num3).ToArray();
                Array.Copy(array, num2 * 20, array, i * 20, num3);
                Array.Copy(sourceArray, 0, array, num2 * 20, num3);
            }
            order[num - 1] = num - 1;
            rkey = key;
            return array.Select((byte x) => (byte)(x ^ key)).ToArray();
        }
    }
}
