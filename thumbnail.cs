using System;
using System.IO;

namespace Media
{
    class ImageClass
    {
        public void SaveThumbnail(byte[] from, string to)
        {
            var imageString = BitConverter.ToString(from).Replace("-", "");

            var startIndex = SearchIndex(imageString, "FFD8FF", 2);
            var endIndex   = SearchIndex(imageString, "FFD9", 1);

            var thumbnailString = imageString.Substring(startIndex, endIndex - startIndex + 4);

            File.WriteAllBytes(to, StringToByteArray(thumbnailString));
        }

        private int SearchIndex (string org, string find, int ordinal)
        {
            var index = 0;
            var count = 0;

            for (var i = 0; i < org.Length; i += 2)
            {
                if (i + find.Length > org.Length)
                {
                    break;
                }

                if (org.Substring(i, find.Length) == find)
                {
                    count++;
                }

                if (count == ordinal)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private byte[] StringToByteArray(string imageString)
        {
            var len = imageString.Length;
            var bytes = new byte[len / 2];

            for (var i = 0; i < len; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(imageString.Substring(i, 2), 16);
            }

            return bytes;
        }
    }

    class MainClass
    {
        static void Main(string[] args)
        {
            ImageClass image = new ImageClass();

            var jpgImage = File.ReadAllBytes(@"C:\Users\HancomGMD\Desktop\1.jpg");
       
            image.SaveThumbnail(jpgImage, @"C:\Users\HancomGMD\Desktop\1(thumbnail).jpg");
        }
    }
}