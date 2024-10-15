import cv2
import numpy as np
import os

# Initial white threshold value
white_threshold = 100
binary_filename = None

def getBinaryImage(frame, gestureName, white_threshold):
    # Save the image
    cv2.imwrite('captured_image.png', frame)
    print("Image saved as 'captured_image.png'")
    image = cv2.imread('captured_image.png')

    # Convert the image to grayscale, for easier manipulation
    grayImg = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Converting the image to binary - aka turning light pixels to white and dark pixels to black
    flattened_image = grayImg.flatten()
    for i in range(len(flattened_image)):
        if flattened_image[i] < white_threshold:
            flattened_image[i] = 0
        else:
            flattened_image[i] = 255
    binaryImg = np.reshape(flattened_image, grayImg.shape)

    # Save the manipulated image
    binary_filename = (f'{gestureName}_binary.png')
    cv2.imwrite(binary_filename, binaryImg)
    print(f"Manipulated image of {gestureName} saved as '{binary_filename}'")

    # Delete the original captured image
    try:
        os.remove('captured_image.png')
        print("Original image deleted.")
    except FileNotFoundError:
        print("Error: Original image file not found.")
    except Exception as e:
        print(f"Error deleting the image: {e}")
    return binary_filename

def on_trackbar(val): # Tager imod en integer værdi fra trackbaren
    global white_threshold
    white_threshold = val

cap = cv2.VideoCapture(0)

# Create a window and a trackbar
cv2.namedWindow('Binary Frame')

# Threshold er titlen, Binary Frame er vinduet, white_threshold er startværdien, 
# 255 er maxværdien, on_trackbar er funktionen der kaldes når trackbaren ændres
cv2.createTrackbar('Threshold', 'Binary Frame', white_threshold, 255, on_trackbar)

while True:
    ret, frame = cap.read()
    frame = cv2.flip(frame, 1)
    cv2.imshow('Binary Frame', frame)

    if cv2.waitKey(1) & 0xFF == ord('s'):
        if binary_filename:
            try:
                os.remove(binary_filename)
                print(f"Previous binary image '{binary_filename}' deleted.")
            except FileNotFoundError:
                print("Error: Previous binary image file not found.")
            except Exception as e:
                print(f"Error deleting the previous binary image: {e}")

        binary_filename = getBinaryImage(frame, 'test', white_threshold)
        binary = cv2.imread(binary_filename)
        cv2.imshow('Binary Image', binary)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        cap.release()
        cv2.destroyAllWindows()
        break

cap.release()
cv2.destroyAllWindows()