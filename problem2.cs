using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp1
{

    class problem2
    {
        static public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        static public Image ByteArrayToImage(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            Image recImg = Image.FromStream(ms);
            return recImg;
        }

        static public byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        static public Image ImageToThumbnail(Image image)
        {
            byte[] image_byte = imageToByteArray(image);
            String image_hex = BitConverter.ToString(image_byte).Replace("-", "");

            String start = "FFD8FF";
            String end = "FFD9";
            int start_index = image_hex.LastIndexOf(start);
            int end_index = image_hex.IndexOf(end);

            String thumbnail_hex = image_hex.Substring(start_index, end_index - start_index + 4);

            byte[] thumbnail_byte = StringToByteArray(thumbnail_hex);
            Image thumbnail = ByteArrayToImage(thumbnail_byte);

            return thumbnail;
        }
        static void Main(string[] args)

        {
            Image image = Image.FromFile("C:\\Users\\HancomGMD\\Desktop\\1.jpg");
            Image thumbnail = ImageToThumbnail(image);
            thumbnail.Save("C:\\Users\\HancomGMD\\Desktop\\thumbnail.jpg");

        }
    }
}
