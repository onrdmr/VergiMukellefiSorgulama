from PIL import Image

# import matplotlib.pyplot as plt
import pytesseract



image = Image.open('./img.jpeg')
text = pytesseract.image_to_string(image)

print(text)

# plt.imshow(image)
# plt.show()