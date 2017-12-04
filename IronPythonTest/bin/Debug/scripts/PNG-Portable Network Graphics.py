from System.Collections.Generic import List
import System.Byte
import System.String

FILE_MAGIC = List[System.Byte]()
FILE_MAGIC.Add(0x89)
FILE_MAGIC.Add(0x50)
FILE_MAGIC.Add(0x4E)
FILE_MAGIC.Add(0x47)
FILE_MAGIC.Add(0x0D)
FILE_MAGIC.Add(0x0A)
FILE_MAGIC.Add(0x1A)
FILE_MAGIC.Add(0x0A)
FILE_MAGIC_LEN = 8
FILE_BIG_ENDIAN = True

IMG_WIDTH_OFFSET = 0x13
IMG_WIDTH_TYPE = "BYTE"
IMG_HEIGHT_OFFSET = 0x17
IMG_HEIGHT_TYPE = "BYTE"
IMG_BPP = 32
IMG_PIXELS_OFFSET = 0x29
IMG_PIXELS_LEN = 0x1D77
IMG_PIXEL_FORMAT = "ARGB"
IMG_DEFLATE = True
IMG_PIXELS_HANDLER = True
WRITE_ORDER = List[System.String]()
WRITE_ORDER.Add("FILE_MAGIC")
WRITE_ORDER.Add("IMG_WIDTH")
WRITE_ORDER.Add("IMG_HEIGHT")
WRITE_ORDER.Add("IMG_PIXELS")

def ProcessPixels(pixels, W, H, BPP):
	result = List[System.Byte]()
	line = (H * BPP / 8) + 1
	for i in range(0, (H*W*BPP/8) + H):
		if i%line != 0:
			result.Add(pixels[i])
	return result