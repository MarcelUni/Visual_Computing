import cv2
import numpy as np
import os
import socket
import copy

print('Starting application...')

#TODO: Vi skal teste gesture matching og se om alt det her er spildt arbejde

#TODO Dokumenter v1, og test, før vi går videre til v2 - evt test alles hænder 

# TODO Slet det der brect shit

#TODO - VÆR OBS - DER BLIVER LAVET NOGET BRECT SHIT, SOM IKKE SES NÅR PROGRAMMET KØRER UMIDDELBART

#TODO - Convexity Defects - hvis de skal inkorporeres skal det kun være antallet af dem

#TODO - En måde at gemme på i guess, så man ikke altid behøver tage nye billeder - NOK FUTURE WORK

#Communication with Unity ####################################################################
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
serverAddressPort = ("127.0.0.1", 5052)

# INITIALIZED VARIABLES #####################################################################

# Array to store filenames of the pictures taken
filenames = []

# Array to store contours of the gestures
contours_refs = []

# All gestures to be captured
gestures = ['Forward', 'Backward', 'ForwardSneak', 'BackwardSneak', 'Interact', 'Stop']

key = ''

######################## BUFFER RELATED #######################
# Buffer dict - used to count gesture matches
bufferDict = {}

# Adding all gestures to dict as keys with value 0
# bufferDict = {gesture: 0 for gesture in gestures}
bufferDict = {}
for gesture in gestures:
    bufferDict[gesture] = 0


# Adding no gesture to bufferDict
bufferDict['No gesture'] = 0

#TODO Find ud af hvordan vi finder 
# Value that a gesture needs to meet, in order to send
bufferThreshhold = 6

# Buffer size before it sends gesture to Unity
bufferTotalThreshold = 8
################################################################

# Initilizing gesture variables
currentGesture = 'Bla'

gesture_name = ''

# Initializing gesture index
gestureIndex = 0

# Threshold for the binary image processing 
# if the pixel value is below this, it will be turned to black, otherwise white
white_threshold = 167
match_threshold = 0.35

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

        defectsTotal, defects = getDefectsAmount(contoursGesture[0])

        print(f'Number of contours: {len(hierachy)}')
        print(f'Number of convexity defects: {defectsTotal}')

        #Checking if contours are present. If they are, store in contours array
        if len(contoursGesture) == 0:
            print(f"No contours found")
        contours_refs.append(contoursGesture)

        
        ### MAKING COPY WITH DRAWN DEFECTS ###
        newImage = drawDefects(img, contoursGesture[0], defects)
        cv2.imwrite('Defects_test.png', newImage)

    except Exception as e:
        print(f"Error processing the file {img}: {e}")        

def getBinaryVideo(frame):
    # Convert the frame to grayscale, for easier manipulation
    gray_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    _, binaryImg = cv2.threshold(gray_frame, white_threshold, 255, cv2.THRESH_BINARY)

    return binaryImg

# TODO Distance filtering
def drawDefects(frame, contours, defects):
    frame = cv2.cvtColor(frame, cv2.COLOR_GRAY2BGR)
    try:
        for i in range(defects.shape[0]):
            s,e,f,d = defects[i,0]
            start = tuple(contours[s][0])
            end = tuple(contours[e][0])
            far = tuple(contours[f][0])
            cv2.line(frame,start,end,[0,255,0],2)
            cv2.circle(frame,far,5,[0,0,255],-1)
    except:
        print('Error with Draw defects')
        close_application

    return frame

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
    # Processing er teknisk set opening 
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

def closingImage(img):
    #NOTE Marcel funktion:

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

def getDefectsAmount(contours):
    hull = cv2.convexHull(contours, returnPoints=False)
    defects = cv2.convexityDefects(contours,hull)

    print(defects)

    # As defects have locations, we are only interested in the amount
    if defects is None:
        print('No defects')
        defects_total = 0
    else:
        defects_total = defects.shape[0]

    return defects_total, defects

# QUALITY OF LIFE, SMALL FUNCTIONS ####################################################

def displayText(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 30), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

def displayTextBelow(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 65), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
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
    global cap, running, sock

    print("Closing application")

    running = False

    cap.release()
    cv2.destroyAllWindows()
    sock.close()
    print('Closing complete')

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

# KEY PRESS HANDLING ####################################################################
#def key_press_detection():
    global running, key_pressed
    while running:
        key = cv2.waitKey(1) & 0xFF  # Check for key press every 1 millisecond
        if key != 255:  # If a key is pressed
            key_pressed = key
            if key == ord('q'):
                close_application()

# STATES ###################################################################################

def state_capture_gestures(raw_frame, binary_frame):
    global gestures, gestureIndex, key

    if gestureIndex < len(gestures):
        gesture = gestures[gestureIndex]

        contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE) # Bruges til at croppe billedet

        # Checking for contours
        if len(contoursLive) == 0:
            frame = displayText(raw_frame.copy(), 'No contours found in live feed')
            cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
            return 'capture_gestures'
        
        # Displaying instructions
        frame = displayText(raw_frame.copy(), f'Capturing {gesture}. Press "s" to save,')

        # Cropper billedet til kun hånden
        # cropped_frame = cropToBrect(frame, contoursLive[0])
        
        #brect_binary_frame = drawBrect(binary_frame, contoursLive)
        binary_frame = getBinaryVideo(raw_frame.copy())

        # Displaying the feeds
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
        cv2.imshow('Binary Feed', binary_frame)  # Updates 'Binary Feed' window

        # Handle keyboard events
        if key == ord('s'):
            # Her skal frame måske tilbage til cropped_frame
            binaryImg = getBinaryImage(frame, gesture) # Uses the cropped image for processing
            process_gesture(binaryImg)
            gestureIndex += 1  # Move to the next gesture
            print(f'Current index:{gestureIndex}')

        #TODO Hvis billedet ikke har nogle contours, skal den bede bruger om at prøve igen

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
        return 'match_gestures'

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

    # Send gesture best matched gesture name to Unity
    if bufferTotal == bufferTotalThreshold:
        print('We made it thru!')

        if best_match_index != -1 and maxBufferValue >= bufferThreshhold:
            gesture_name = get_key_from_buffer(maxBufferValue)
            print(gesture_name)

            # Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

            # Send gesture_name to Unity via UDP
            sock.sendto(str.encode(gesture_name), serverAddressPort)
            print(f'Sending {gesture_name} to Unity')

            currentGesture = gesture_name
            print(f"currentGesture updated to: {currentGesture}")

        else:
            print('No certain gesture')

            currentGesture = 'No gesture'
            print(f"currentGesture updated to: {currentGesture}")

            # Send gesture_name to Unity via UDP
            sock.sendto(str.encode(gesture_name), serverAddressPort)
            print(f'Sending {gesture_name} to Unity')

            # Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

    # If there is no new gesture, keep the same gesture and send it
    elif currentGesture == currentGesture:
            # Send gesture_name to Unity via UDP
            sock.sendto(str.encode(gesture_name), serverAddressPort)
            print(f'Sending {gesture_name} to Unity')
    else:
        print('No currentGesture')
                
    frame = displayText(frame, f'Matched Gesture: {gesture_name}')

    # Displaying the feeds
    cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
    cv2.imshow('Binary Feed', binary_frame)  # Updates 'Binary Feed' window

    return 'match_gestures'  # Remain in the current state


# State dictionary
states = {
    'capture_gestures': state_capture_gestures,
    'match_gestures': state_match_gestures,
}

# MAIN #####################################################################################

# Open the camera
cap = cv2.VideoCapture(1)

running = True

# Initial state
current_state = 'capture_gestures'

while current_state and running:
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
    binary_frame = removeNoise(binary_frame) # Remove noise
    binary_frame = closingImage(binary_frame) # Closing image (should close potential holes in hands, like tattoos, and small shadows)

#todo Convexity Defects er nok ligegyldige da de har positioner - måske mere stabilitet kun kan være med mere træningsmateriale?

    #Define the key press
    key = cv2.waitKey(3) & 0xFF
    if key == ord('q'):
        close_application()
        
    # Execute the current state function
    current_state = states[current_state](frame, binary_frame)