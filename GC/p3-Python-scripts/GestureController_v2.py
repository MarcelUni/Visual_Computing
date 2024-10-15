import cv2
import numpy as np
import os
import socket


#TODO: Vi skal teste gesture matching og se om alt det her er spildt arbejde

#TODO v2 - Crop billedet til hænderne, og kun analyser hænderne for bedre data

#TODO Lav setup på bordet

#TODO Dokumenter v1, og test, før vi går videre til v2 - evt test alles hænder 

#TODO VILLE v1 VIRKER BEDRE VED AT FJERNE ARMENE ALENE?

#Communication with Unity ####################################################################
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
serverAddressPort = ("127.0.0.1", 5052)

# INITIALIZED VARIABLES #####################################################################

# Array to store filenames of the pictures taken
filenames = []

# Array to store contours of the gestures
contours_refs = []

# All gestures to be captured
gestures = ['Forward', 'Backward']

# Buffer dict - used to count gesture matches
bufferDict = {}

# Adding all gestures to dict as keys
for gesture in gestures:
    bufferDict[gesture] = 0
# Adding no gesture to bufferDict
bufferDict['No gesture'] = 0

bufferThreshhold = 8

currentGesture = ''

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

# FUNCTIONS #################################################################################

# FUNCTIONS UNDER TESTING - PROBABLY FROM CO-PILOT

# RELEVANT, DIRECT MANIPULATION
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

def removeNoise(frame):

    # Source: https://github.com/Gustav-skole/DDU/blob/master/AI/basic.py

    connectivity = 8

    output = cv2.connectedComponentsWithStats(frame, connectivity, cv2.CV_32S)

    num_stats = output[0]
    labels = output[1]
    stats = output[2]

    new_image = frame.copy()

    for label in range(num_stats):
        if stats[label,cv2.CC_STAT_AREA] == 1:
            new_image[labels == label] = 0

    return new_image

def removeNoise2(frame):
    dilate_iterations = 2
    erosion_iterations = 2

    kernel = np.array([0,0,1,0,0],
                       [0,1,1,1,0],
                       [0,0,1,0,0],)

    #3x3 disk dilate
    frame = cv2.dilate(frame, kernel, iterations=dilate_iterations)

    #Erosion to get back OG image
    frame = cv2.erode(frame, kernel, iterations=erosion_iterations)

    return frame

#TODO Lav close shape function så sådan noget som Bonks tatoveringer forsvinder
def closeShape(frame):
    dilate_iterations = 5
    erosion_iterations = 5

    kernel = np.array([[0,0,1,0,0],
                       [0,1,1,1,0],
                       [1,1,1,1,1],
                       [0,1,1,1,0],
                       [0,0,1,0,0]])

    #3x3 disk dilate
    frame = cv2.dilate(frame, kernel, iterations=dilate_iterations)

    #Erosion to get back OG image
    frame = cv2.erode(frame, kernel, iterations=erosion_iterations)

    return frame

# QUALITY OF LIFE, SMALL FUNCTIONS

def displayText(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 30), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

def displayMatchAccuracy(image, match):
    # Display the match accuracy on the frame in the bottom left corner
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, f'Match: {match}', (10, image.shape[0] - 10), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

def get_key_from_buffer(val):
  
    for key, value in bufferDict.items():
        if val == value:
            return key

    return "key doesn't exist"

# BOUNDING RECTANGLE RELATED
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

def state_capture_gestures(raw_frame):
    global gestures, gestureIndex

    if gestureIndex < len(gestures):
        gesture = gestures[gestureIndex]
        frame = displayText(raw_frame.copy(), f'Capturing {gesture}. Press "s" to save,')
        binary_frame = getBinaryVideo(raw_frame.copy())

        contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        if len(contoursLive) > 0:
            cropped_frame = cropToBrect(frame, contoursLive[0])
            brect_binary_frame = drawBrect(binary_frame, contoursLive)

            # Displaying the feeds
            cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
            cv2.imshow('Binary Feed', brect_binary_frame)  # Updates 'Binary Feed' window
            #cv2.imshow('Cropped Feed', cropped_frame)  # Updates 'Cropped Feed' window

        # Handle keyboard events
        key = cv2.waitKey(1) & 0xFF
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
def state_match_gestures(raw_frame):
    global contours_refs, match_threshold, bufferDict, currentGesture

    # Display the frame with no text
    frame = displayText(raw_frame.copy(), '')
    binary_frame = getBinaryVideo(raw_frame)  # Convert the frame to binary

    # Find the contours in the binary frame
    contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    if len(contoursLive) > 0:
        cropped_frame = cropToBrect(frame, contoursLive[0])
        brect_binary_frame = drawBrect(binary_frame, contoursLive)

        best_match_index, best_match_value = findBestMatch(contours_refs, contoursLive)

        # Display the match accuracy on the frame
        displayMatchAccuracy(frame, round(best_match_value, 2))

        #TODO Skal buffer være her?
######################## BUFFER ############################
        # Adding the best matched gesture to buffer
        if best_match_value < match_threshold:
            gesture_name = gestures[best_match_index]
            bufferDict[gesture_name] += 1
        else:
            bufferDict['No gesture'] += 1   
        
        #print(bufferDict)

        maxBufferValue = max(bufferDict.values())

        # TODO Skal altid sende en gesture baseret på buffer, også no gesture

        # Send gesture best matched gesture name to Unity
        if best_match_index != -1 and maxBufferValue == bufferThreshhold:
            gesture_name = get_key_from_buffer(maxBufferValue)
            frame = displayText(frame, f'Matched Gesture: {gesture_name}')
            print(gesture_name)

            #Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

            # Send gesture_name to Unity via UDP
            sock.sendto(str.encode(gesture_name), serverAddressPort)

            currentGesture = gesture_name

            #NOTE Tror ikke den her giver mening?
        elif currentGesture == '':
             # Send gesture_name to Unity via UDP
            sock.sendto(str.encode('No gesture'), serverAddressPort)
        else:
            print('No currentGesture')


        # Displaying the feeds
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
        cv2.imshow('Binary Feed', brect_binary_frame)  # Updates 'Binary Feed' window
        # cv2.imshow('Cropped Feed', cropped_frame)  # Updates 'Cropped Feed' window

    else:
        frame = displayText(frame, 'No contours found in live feed')
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window

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
    y = 0
    x = 50
    h = 480
    w = 500
    frame = raw_frame[y:y+h, x:x+w]

    # Removing potential noise
    frame = removeNoise2(frame)

    # Closing - hopefully fixing Bonk tattoos
    frame = closeShape(frame)

    #Define the key press
    key = cv2.waitKey(1) & 0xFF
    if key == ord('q'):
        close_application()
        
    # Execute the current state function
    current_state = states[current_state](frame)

    
