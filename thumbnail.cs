using System;
using System.IO;

namespace Image
{
    class ThumbnailFinder
    {
        public static byte[] Find(byte[] from)
        {
            var startIndex = searchIndex(from, new byte[] { 0xff, 0xd8 }, 2);
            var endIndex   = searchIndex(from, new byte[] { 0xff, 0xd9 }, 1);

            if (startIndex == -1 || endIndex == -1)
                return null;

            if (startIndex > endIndex)
                return null;

            var length = endIndex - startIndex + 1;
            var result = new byte[length];
            Array.Copy(from, startIndex, result, 0, length);

            return result;
        }

        private static int searchIndex(byte[] org, byte[] pattern, int nth)
        {
            var count = 0;

            for (var i = 0; i < org.Length; i++)
            {
                if (i + pattern.Length > org.Length)
                    return -1;

                if (org[i] == pattern[0] && org[i+1] == pattern[1])
                    count++;

                if (count == nth)
                    return i;
            }

            return -1;
        }
    }

    class MainClass
    {
        static void Main(string[] args)
        {
            var jpgImage = File.ReadAllBytes(@"C:\Users\HancomGMD\Desktop\1.jpg");

            var thumbnail = ThumbnailFinder.Find(from: jpgImage);
            if (thumbnail != null)
                File.WriteAllBytes(path: @"C:\Users\HancomGMD\Desktop\1(thumbnail).jpg",thumbnail);
        }
    }
}