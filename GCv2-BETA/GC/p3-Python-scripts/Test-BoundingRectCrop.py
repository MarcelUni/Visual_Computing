import cv2 as cv
import numpy as np

def cropImage(img, contours):
    # Check if the image is loaded correctly
    if img is None:
        print("Error: Image not loaded. Check the file path.")
        return None
    
    cnt = contours[0]  # Select the first contour

    # Get the bounding rectangle
    x, y, w, h = cv.boundingRect(cnt)

    # Crop based on the bounding rectangle
    cropped_img = img[y:y + h, x:x + w]

    return cropped_img

# Load the image
test_img = cv.imread('test_binary.png', cv.IMREAD_GRAYSCALE)

# Check if the image is loaded correctly
if test_img is None:
    print("Error: Image not loaded. Check the file path.")
else:
    # Find contours
    contours, _ = cv.findContours(test_img, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE)

    # Check if any contours are found
    if len(contours) == 0:
        print("No contours found.")
    else:
        print(f"Number of contours found: {len(contours)}")
        cnt = contours[0]  # Select the first contour

        # Get the bounding rectangle
        x, y, w, h = cv.boundingRect(cnt)
        print(f"Bounding rectangle: x={x}, y={y}, w={w}, h={h}")

        # Convert the grayscale image to BGR for color drawing
        test_img_bgr = cv.cvtColor(test_img, cv.COLOR_GRAY2BGR)

        # Crop based on the bounding rectangle
        cropped_img = test_img_bgr[y:y + h, x:x + w]

        # Draw the rectangle on the image in green
        cv.rectangle(test_img_bgr, (x, y), (x + w, y + h), (0, 255, 0), 2)

        # Display the image
        cv.imshow('BoundingRectCrop', cropped_img)
        cv.waitKey(0)
        cv.destroyAllWindows()