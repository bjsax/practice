from io import BytesIO
from PIL import Image

def get_thumbnail(filename):
    with open(filename, 'rb') as f:
        image_content = f.read()
    image_hex = image_content.hex()
    start = 'ffd8ff'
    end = 'ffd9'
    start_index = image_hex.rfind(start)
    end_index = image_hex.find(end)

    thumbnail_hex = image_hex[start_index:end_index+4]
    thumbnail_content = bytes.fromhex(thumbnail_hex)
    thumbnail = Image.open(BytesIO(thumbnail_content))
    thumbnail.show()

def main() :
    filename = 'C:\\Users\\HancomGMD\\Desktop\\1.jpg'
    get_thumbnail(filename)

main()
