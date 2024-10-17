import cv2
import numpy as np
import os


# Array to store filenames of the pictures taken
filenames = []

# Function to manipulate the image (example implementation)
def getBinaryImage(frame, gestureName):

    # Save the image
    cv2.imwrite('captured_image.png', frame)
    print("Image saved as 'captured_image.png'")
    image = cv2.imread('captured_image.png')

    # Convert the image to grayscale, for easier manipulation
    grayImg = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    #_, image = cv2.threshold(grayImg, 127, 255, cv2.THRESH_BINARY)

    # Converting the image to binary - aka turning light pixels to white and dark pixels to black
    flattened_image = grayImg.flatten()
    for i in range(len(flattened_image)):
        if flattened_image[i] < 50:
            flattened_image[i] = 0
        else:
            flattened_image[i] = 255
    binaryImg = np.reshape(flattened_image, grayImg.shape)

    # Save the manipulated image
    binary_filename = f'{gestureName}_binary.png'
    cv2.imwrite(binary_filename, binaryImg)
    print(f"Manipulated image of {gestureName} saved")

    # Append binary filename to the list
    filenames.append(binary_filename)

    # Delete the original captured image
    try:
        os.remove('captured_image.png')
        print("Original image deleted.")
    except FileNotFoundError:
        print("Error: Original image file not found.")
    except Exception as e:
        print(f"Error deleting the image: {e}")
    return binaryImg

def getBinaryVideo(frame):
    # Convert the frame to grayscale, for easier manipulation
    gray_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    #_, image = cv2.threshold(grayImg, 127, 255, cv2.THRESH_BINARY)

    # Converting the image to binary - aka turning light pixels to white and dark pixels to black
    flattened_frame = gray_frame.flatten()
    for i in range(len(flattened_frame)):
        if flattened_frame[i] < 50:
            flattened_frame[i] = 0
        else:
            flattened_frame[i] = 255
    binaryImg = np.reshape(flattened_frame, gray_frame.shape)

    return binaryImg

def displayText(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 30), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

def close_application():
    cap.release()
    cv2.destroyAllWindows()

def full_close_application():
    cap.release()
    cv2.destroyAllWindows()
    # Delete all the files in the filenames array
    for filename in filenames:
        try:
            os.remove(filename)
            print(f"Deleted file: {filename}")
        except FileNotFoundError:
            print(f"Error: File {filename} not found.")
        except Exception as e:
            print(f"Error deleting the file {filename}: {e}")

# Open the camera
cap = cv2.VideoCapture(0)

# Take image for 1st gesture
while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    if not ret:
        break

    # Display the text on the frame
    frame = displayText(raw_frame.copy(), 'Press "s" to save the image, "q" to quit')

    cv2.imshow('Video Feed 1', frame)  # Display the frame

    # Wait for a key press (1 ms delay)
    key = cv2.waitKey(1) & 0xFF

    if key == ord('s'):  # Press 's' to save the image
        # Manipulate the image
        getBinaryImage(raw_frame, 'gesture1')

        cv2.destroyWindow('Video Feed 1')  # Close the video window
        break

    elif key == ord('q'):  # Press 'q' to quit
        # Release the camera and close windows
        close_application()
        break

# Take image for 2nd gesture
while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    if not ret:
        break

    # Display the text on the frame
    frame = displayText(raw_frame.copy(), 'Press "s" to save the image, "q" to quit')
    cv2.imshow('Video Feed 2', frame)  # Display the frame

    # Wait for a key press (1 ms delay)
    key = cv2.waitKey(1) & 0xFF

    if key == ord('s'):  # Press 's' to save the image
        # Manipulate the image
        getBinaryImage(raw_frame, 'gesture2')

        cv2.destroyWindow('Video Feed 2')  # Close the video window
        break

    elif key == ord('q'):  # Press 'q' to quit
        # Release the camera and close windows
        close_application()
        break

# Begin the main loop
while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    if not ret:
        break

    # Display the text on the frame
    frame = displayText(raw_frame.copy(), 'Press "q" to quit')
    
    # Converting the frame to binary
    binary_frame = getBinaryVideo(raw_frame)

    # Find the contours in the binary frame
    contours, hierarchy = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    # Find the contours of gesture1
    gesture1 = cv2.imread(filenames[0], cv2.IMREAD_GRAYSCALE)
    #_, gesture1 = cv2.threshold(gesture1, 127, 255, cv2.THRESH_BINARY) #denne linje er muligvis overflødig
    contours1, hierarchy = cv2.findContours(gesture1, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    # Draw the contours in red color
    #cv2.drawContours(frame, contours, -1, (0, 0, 255), 3)

    # See if the contours of the live feed match the contours of gesture1
    match = cv2.matchShapes(contours[0], contours1[0], 1, 0.0)

    #TODO: Sæt en match værdi op for alle gestures, og så et if-statement

    # If it has a 80% match, display in the bottom right corner
    if match < 0.2:
        frame = displayText(frame, 'MATCH!')

    # Display the frame
    cv2.imshow('Third Code Block', frame)
    cv2.imshow('Binary Frame', binary_frame)

    # Break the loop on 'q' key press
    if cv2.waitKey(1) & 0xFF == ord('q'):
        close_application()
        break