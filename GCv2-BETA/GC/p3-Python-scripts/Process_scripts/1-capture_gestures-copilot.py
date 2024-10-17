import cv2
import numpy as np
import os

# Array to store filenames of the pictures taken
filenames = []

# List of gestures to be captured
gestures = ['gesture1', 'gesture2']

# Function to manipulate the image (example implementation)
def getBinaryImage(frame, gestureName):
    # Save the image
    cv2.imwrite('captured_image.png', frame)
    print("Image saved as 'captured_image.png'")
    image = cv2.imread('captured_image.png')

    # Convert the image to grayscale, for easier manipulation
    grayImg = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

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

# State function to handle startup tasks
def state_startup():
    # Perform any necessary startup tasks here
    print("Performing startup tasks...")
    # Automatically transition to the next state
    return 'capture_gestures', 0  # Start capturing gestures from the first one

# State function to handle capturing gestures
def state_capture_gestures(raw_frame, gesture_index):
    gesture_name = gestures[gesture_index]
    frame = displayText(raw_frame.copy(), f'Capturing {gesture_name}. Press "s" to save, "q" to quit')
    cv2.imshow('Video Feed', frame)
    return 'capture_gestures', gesture_index  # Remain in the current state

# State function to handle matching gestures
def state_match_gesture(raw_frame):
    frame = displayText(raw_frame.copy(), 'Press "q" to quit')
    binary_frame = getBinaryVideo(raw_frame)
    contours, hierarchy = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    gesture1 = cv2.imread(filenames[0], cv2.IMREAD_GRAYSCALE)
    contours1, hierarchy = cv2.findContours(gesture1, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    match = cv2.matchShapes(contours[0], contours1[0], 1, 0.0)
    if match < 0.2:
        frame = displayText(frame, 'MATCH!')
    cv2.imshow('Video Feed', frame)
    cv2.imshow('Binary Frame', binary_frame)
    return 'match_gesture'  # Remain in the current state

# State dictionary
states = {
    'startup': state_startup,
    'capture_gestures': state_capture_gestures,
    'match_gesture': state_match_gesture
}

# Open the camera
cap = cv2.VideoCapture(0)

# Initial state
current_state = 'startup'
gesture_index = 0

while current_state:
    # Capture frame-by-frame
    ret, frame = cap.read()
    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    if not ret:
        break

    # Execute the current state function
    if current_state == 'capture_gestures':
        current_state, gesture_index = states[current_state](raw_frame, gesture_index)
    else:
        current_state, gesture_index = states[current_state]()

    # Handle key presses
    key = cv2.waitKey(1) & 0xFF
    if key == ord('s'):
        if current_state == 'capture_gestures':
            getBinaryImage(raw_frame, gestures[gesture_index])
            gesture_index += 1
            if gesture_index >= len(gestures):
                current_state = 'match_gesture'
    elif key == ord('q'):
        close_application()
        current_state = None