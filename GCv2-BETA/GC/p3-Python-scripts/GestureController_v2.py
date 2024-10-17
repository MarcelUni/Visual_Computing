import cv2
import numpy as np
import os
import socket


#TODO: Vi skal teste gesture matching og se om alt det her er spildt arbejde

#TODO Dokumenter v1, og test, før vi går videre til v2 - evt test alles hænder 

#Communication with Unity ####################################################################
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
serverAddressPort = ("127.0.0.1", 5052)

# INITIALIZED VARIABLES #####################################################################

# Array to store filenames of the pictures taken
filenames = []

# Array to store contours of the gestures
contours_refs = []

# All gestures to be captured
gestures = ['Forward', 'Backward', 'Stop',]


######################## BUFFER RELATED #######################
# Buffer dict - used to count gesture matches
bufferDict = {}

# Adding all gestures to dict as keys
for gesture in gestures:
    bufferDict[gesture] = 0
# Adding no gesture to bufferDict
bufferDict['No gesture'] = 0

# Value that a gesture needs to meet, in order to send
bufferThreshhold = 6

# Buffer size before it sends gesture to Unity
bufferTotalThreshold = 8
################################################################

# Initilizing gesture variables
currentGesture = 'Bla'
print(currentGesture)

gesture_name = ''

# Initializing gesture index
gestureIndex = 0

# Threshold for the binary image processing 
# if the pixel value is below this, it will be turned to black, otherwise white
white_threshold = 167
match_threshold = 0.4

# Create a folder to store the images
folder = "images"
if not os.path.exists(folder):
    os.makedirs(folder)

# FUNCTIONS ###########################################################################

# RELEVANT, DIRECT IMAGE MANIPULATION
def getBinaryImage(frame, gestureName):
    global folder

    # Save the image
    cv2.imwrite('captured_image.png', frame)
    print("Image saved as 'captured_image.png'")
    image = cv2.imread('captured_image.png')

    # Convert the image to grayscale, for easier manipulation
    grayImg = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Converting the image to binary - aka turning light pixels to white and dark pixels to black
    _, binaryImg = cv2.threshold(grayImg, white_threshold, 255, cv2.THRESH_BINARY)

    # Save the manipulated image
    binary_filename = os.path.join(folder, f'{gestureName}_binary.png')
    cv2.imwrite(binary_filename, binaryImg)
    print(f"Manipulated image of {gestureName} saved as '{binary_filename}'")

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

def process_gesture(img):
    try:
        #Reading image
        gesture = img
        if gesture is None:
            print(f"Error: File not found.")

        #Getting contour
        contoursGesture, hierachy = cv2.findContours(gesture, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        print(f'Number of contours: {len(hierachy)}')

        #Checking if contours are present. If they are, store in contours array
        if len(contoursGesture) == 0:
            print(f"No contours found")
        contours_refs.append(contoursGesture)

    except Exception as e:
        print(f"Error processing the file {img}: {e}")        

def getBinaryVideo(frame):
    # Convert the frame to grayscale, for easier manipulation
    gray_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    _, binaryImg = cv2.threshold(gray_frame, white_threshold, 255, cv2.THRESH_BINARY)

    return binaryImg

def findBestMatch(contours_refs, contours_live):
    
    best_match_value = float('inf')
    best_match_index = -1

    # Iterate through the contours array and compare with live contours
    for i, gesture_contours in enumerate(contours_refs):
        if len(gesture_contours) == 0:
            print(f"Warning: Empty contour encountered at index {i}. Skipping.")
            continue

        match_value = cv2.matchShapes(contours_live[0], gesture_contours[0], 1, 0.0)

        #print(f"Gesture: {gestures[i]}, Match Value: {match_value}")  # Debug print

        if match_value < best_match_value:
            best_match_value = match_value
            best_match_index = i
        
    return best_match_index, best_match_value

def removeNoise2(frame):

    erosion_iterations = 2
    dilate_iterations = 2

    kernel = np.array([[0,0,1,0,0],
                       [0,1,1,1,0],
                       [0,0,1,0,0]], dtype=np.uint8)

    # Erosion to remove bad pixels
    frame = cv2.erode(frame, kernel, iterations=erosion_iterations)

    # Dilate to get back to OG image
    frame = cv2.dilate(frame, kernel, iterations=dilate_iterations)

    return frame

#NOTE Marcel funktion:
def closingImage(img):

    # Closing image holes
    dilateIterations = 3
    erodeIterations = 3

    kernel = np.array([[0,0,1,0,0],
                       [0,1,1,1,0],
                       [1,1,1,1,1],
                       [0,1,1,1,0],
                       [0,0,1,0,0]],np.uint8)
    
    # Dilating
    img = cv2.dilate(img, kernel, iterations = dilateIterations)
    # Eroding
    img = cv2.erode(img, kernel, iterations = erodeIterations)

    return img

# QUALITY OF LIFE, SMALL FUNCTIONS ####################################################

def displayText(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 30), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

def displayMatchAccuracy(image, match):
    # Display the match accuracy on the frame in the bottom left corner
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, f'Match: {match}', (10, image.shape[0] - 10), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

# source: https://www.geeksforgeeks.org/python-get-key-from-value-in-dictionary/
def get_key_from_buffer(val):
  
    for key, value in bufferDict.items():
        if val == value:
            return key

    return "key doesn't exist"

def get_buffer_total():
    global bufferDict

    total = 0

    for key, value in bufferDict.items():
        if value == None:
            print(f'No value found for {key}, skipping')
            continue
        total += value

    return total

# BOUNDING RECTANGLE RELATED ############################################################
# TODO Skal lowkey slettes?

def cropToBrect(frame, contours): 
    x, y, width, height = cv2.boundingRect(contours)
    
    # Crop the frame to the dimensions of the brect
    cropped_frame = frame[y:y+height, x:x+width]

    return cropped_frame

def drawBrect(frame, contours):
    x, y, width, height = cv2.boundingRect(contours[0])

    # Draw the rectangle on the frame
    drawn_frame = cv2.rectangle(frame.copy(), (x, y), (x + width, y + height), (0, 255, 0), 2)

    return drawn_frame

# CLOSING THE APPLICATION

def close_application():
    global cap

    print("Closing application")
    cap.release()
    cv2.destroyAllWindows()

# Use this if we want to delete images afterwards
def full_close_application():
    global cap

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

# STATES ###################################################################################

def state_no_contours(raw_frame):
    frame = displayText(raw_frame.copy(), 'Error: No contours found in some of the images')
    cv2.imshow('Video Feed', frame)
    return 'no_contours'

def state_capture_gestures(raw_frame, binary_frame):
    global gestures, gestureIndex

    if gestureIndex < len(gestures):
        gesture = gestures[gestureIndex]
        frame = displayText(raw_frame.copy(), f'Capturing {gesture}. Press "s" to save,')

        contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        if len(contoursLive) > 0:
            cropped_frame = cropToBrect(frame, contoursLive[0])
            brect_binary_frame = drawBrect(binary_frame, contoursLive)

            # Displaying the feeds
            cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
            cv2.imshow('Binary Feed', brect_binary_frame)  # Updates 'Binary Feed' window
            #cv2.imshow('Cropped Feed', cropped_frame)  # Updates 'Cropped Feed' window

        # Handle keyboard events
        key = cv2.waitKey(2) & 0xFF
        if key == ord('s'):
            binaryImg = getBinaryImage(cropped_frame, gesture) # Uses the cropped image for processing
            process_gesture(binaryImg)
            gestureIndex += 1  # Move to the next gesture
            print(f'Current index:{gestureIndex}')

    if gestureIndex == len(gestures):
        return 'match_gestures'  # Move to the next state after all gestures are captured
    else:
        return 'capture_gestures'  # Stay in the current state if not all gestures are captured   
    

# State of matching the captured gesture with the live feed
def state_match_gestures(raw_frame, binary_frame):
    global contours_refs, match_threshold, bufferDict, currentGesture, bufferTotalThreshold, gesture_name

    # Display the frame with no text
    frame = displayText(raw_frame.copy(), '')

    # Find the contours in the binary frame
    contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    # Checking for contours
    if len(contoursLive) == 0:
        frame = displayText(frame, 'No contours found in live feed')
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
        return state_match_gestures

    best_match_index, best_match_value = findBestMatch(contours_refs, contoursLive)

    # Display the match accuracy on the frame
    displayMatchAccuracy(frame, round(best_match_value, 2))

    # TODO Skal buffer laves til en funktion?
######################## BUFFER ############################
    # Adding the best matched gesture to buffer, if it meets the threshold
    if best_match_value < match_threshold:
        gesture_name = gestures[best_match_index]
        bufferDict[gesture_name] += 1
    else:
        bufferDict['No gesture'] += 1   
    
    print(bufferDict)

    maxBufferValue = max(bufferDict.values())

    bufferTotal = get_buffer_total()

    # Print debug information
    #print(f"maxBufferValue: {maxBufferValue}")
    #print(f"bufferTotal: {bufferTotal}")
    #print(f"currentGesture: {currentGesture}")
    #print(f"best_match_index: {best_match_index}")


    # Send gesture best matched gesture name to Unity
    if bufferTotal == bufferTotalThreshold:
        print('We made it thru!')

        if best_match_index != -1 and maxBufferValue >= bufferThreshhold:
            gesture_name = get_key_from_buffer(maxBufferValue)
            print(gesture_name)

            #Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

            # Send gesture_name to Unity via UDP
            sock.sendto(str.encode(gesture_name), serverAddressPort)

            currentGesture = gesture_name

            print(f"currentGesture updated to: {currentGesture}")
        else:
            print('Something went wrong')
             #Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

    # If there is no new gesture, keep the same gesture and send it
    elif currentGesture == currentGesture:
            # Send gesture_name to Unity via UDP
            sock.sendto(str.encode(gesture_name), serverAddressPort)
    else:
        print('No currentGesture')
                
    frame = displayText(frame, f'Matched Gesture: {gesture_name}')

    # Displaying the feeds
    cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
    cv2.imshow('Binary Feed', binary_frame)  # Updates 'Binary Feed' window
    # cv2.imshow('Cropped Feed', cropped_frame)  # Updates 'Cropped Feed' window

    return 'match_gestures'  # Remain in the current state


# State dictionary
states = {
    'capture_gestures': state_capture_gestures,
    'match_gestures': state_match_gestures,
    'no_contours': state_no_contours
}

# MAIN #####################################################################################

# Open the camera
cap = cv2.VideoCapture(1)

# Initial state
current_state = 'capture_gestures'

while current_state:
    # Capture frame-by-frame
    ret, frame = cap.read()
    if not ret:
        print("Error: Failed to capture image")
        close_application()
        break

    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    # Cropping frame
    y, x, h, w = 0, 50, 550, 650
    frame = raw_frame[y:y+h, x:x+w]

    binary_frame = getBinaryVideo(raw_frame.copy()) # Getting binary frame
    binary_frame = removeNoise2(binary_frame) # Remove noisee
    binary_frame = closingImage(binary_frame) # Closing image (should remove noise, and close potential holes in hands, like tattoos)

    #Define the key press
    key = cv2.waitKey(2) & 0xFF
    if key == ord('q'):
        close_application()
        
    # Execute the current state function
    current_state = states[current_state](frame, binary_frame)

    
