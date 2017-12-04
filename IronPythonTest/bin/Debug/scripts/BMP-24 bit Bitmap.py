from System.Collections.Generic import List
import System.Byte
import System.String

FILE_MAGIC = List[System.Byte]()
FILE_MAGIC.Add(0x42)
FILE_MAGIC.Add(0x4D)
FILE_MAGIC_LEN = 2
FILE_SIZE_OFFSET = 0x2

IMG_WIDTH_OFFSET = 0x12
IMG_HEIGHT_OFFSET = 0x16
IMG_BPP_OFFSET = 0x1C
IMG_PIXELS_LEN_OFFSET = 0x22
IMG_PIXELS_OFFSET = 0x36
IMG_UPSIDE_DOWN = True
IMG_PIXEL_FORMAT = "BGR"
IMG_PIXELS_HANDLER = True
BMP_HEADER_SIZE = 0x36
BMP_HEADER_SIZE_OFFSET = 0xA
BMP_HEADER = 0x28
BMP_HEADER_OFFSET = 0xE
BMP_PLANES = 0x01
BMP_PLANES_OFFSET = 0x1A
WRITE_ORDER = List[System.String]()
WRITE_ORDER.Add("FILE_MAGIC")
WRITE_ORDER.Add("FILE_SIZE")
WRITE_ORDER.Add("BMP_HEADER_SIZE")
WRITE_ORDER.Add("BMP_HEADER")
WRITE_ORDER.Add("IMG_WIDTH")
WRITE_ORDER.Add("IMG_HEIGHT")
WRITE_ORDER.Add("BMP_PLANES")
WRITE_ORDER.Add("IMG_BPP")
WRITE_ORDER.Add("IMG_PIXELS_LEN")
WRITE_ORDER.Add("IMG_PIXELS")

def ProcessPixels(pixels, W, H, BPP):
	result = List[System.Byte]()
	line = (W * BPP / 8)
	lineF = line
	if W%2 != 0:
		lineF = lineF + BPP / 8
	for i in range(0, H):
		for j in range(0, line):
			result.Add(pixels[i*lineF + j])
	return result
def SavePixels(pixels, W, H, BPP):
	if W%2 == 0:
		return List[System.Byte]()
	result = List[System.Byte]()
	line = (W * BPP / 8)
	for i in range(0, H):
		for j in range(0, line):
			result.Add(pixels[i*line + j])
		result.Add(0)
		result.Add(0)
		result.Add(0)
	return result