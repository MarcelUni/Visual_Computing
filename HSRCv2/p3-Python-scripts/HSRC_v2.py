import cv2          # Henter open cv library
import numpy as np  # Henter Numpy
import os           # Henter styresystemet på computeren
import socket       # Til at sende og modtage pakker over netværk


print('Starting application...')


# HAND SIGN RECOGNITION CONTROLLER - HSRC 

# FUTURE WORK #####
# https://stackoverflow.com/questions/75267154/how-do-i-add-an-image-overlay-to-my-live-video-using-cv2 - Kan evt bruges til hvis der skal være instrukser til brugeren for hvordan de skal placere deres hånd for billederne
# 
# En måde at gemme på i guess, så man ikke altid behøver tage nye billeder 
# 
# Trackbars, so relevante variabler kan skiftes ved brug

# Iterable variable for defects test images.
# DELETE FOR FINAL VERSION
i = 0

#Communication with Unity ####################################################################
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # Creating UDP socket - For sending data without checking if data is recieved
serverAddressPort = ("127.0.0.1", 5052) # Sætter IP adressen som er lokaladressen, derefter sætter porten

# INITIALIZED VARIABLES #####################################################################

# List to store filenames of the pictures taken
filenames = []

# List to store contours of the hand_signs
contours_refs = []

# List to store convexity defects for contours
defects_hand_signs = []

# All hand_signs to be captured
hand_signs = ['Forward', 'Backward', 'ForwardSneak', 'BackwardSneak', 'Interact', 'Stop']
#hand_signs = ['Forward', 'Backward'] # Test bunch

previous_hand_sign = "No hand_sign"

key = ''

################# BUFFER RELATED #######################
# Buffer dict - used to count hand_sign matches
# Adding all hand_signs to dict as keys with value 0
bufferDict = {hand_sign: 0 for hand_sign in hand_signs} # List comprehension - https://www.w3schools.com/python/python_lists_comprehension.asp

# Adding no hand_sign to bufferDict
bufferDict['No hand_sign'] = 0

# Buffer size before it sends hand_sign to Unity
bufferTotalThreshold = 4

# Value that a handsign needs to meet, in order to send 
bufferThreshhold = round(bufferTotalThreshold*0.75, 0) # 75% of bufferTotalThreshold
print(f'Buffer threshold: {bufferThreshhold}')



################################################################

# Initilizing hand_sign variables
currenthand_sign = 'Bla'

hand_sign_name = ''

hand_signIndex = 0

# Threshold for the binary image processing 
white_threshold = 167

# Threshold determining how accurate a match should be to return the hand_sign
match_threshold = 0.42

# Create a folder to store the images
folder = "images"
if not os.path.exists(folder):
    os.makedirs(folder)

# FUNCTIONS ###########################################################################

# RELEVANT, DIRECT IMAGE MANIPULATION

# Saves current frame as a binary image
def getBinaryImage(frame, hand_signName):
    global folder

    # Save the image
    cv2.imwrite('captured_image.png', frame)
    image = cv2.imread('captured_image.png')

    # Convert the image to grayscale, for easier manipulation
    grayImg = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Converting the image to binary - aka turning light pixels to white and dark pixels to black
    _, binaryImg = cv2.threshold(grayImg, white_threshold, 255, cv2.THRESH_BINARY)

    # Save the manipulated image
    binary_filename = os.path.join(folder, f'{hand_signName}_binary.png')
    cv2.imwrite(binary_filename, binaryImg)
    print(f"{hand_signName} saved as '{binary_filename}'")

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

# Saves contours and defects for a hand sign - is performed for every hand sign captured
def process_hand_sign(img):
    global i, defects_hand_signs
    try:
        #Reading image
        hand_sign = img
        if hand_sign is None:
            print(f"Error: File not found.")

        # Getting contours ##################
        contourshand_sign, hierachy = cv2.findContours(hand_sign, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        #print(f'Number of contours: {len(hierachy)}') # Debug print

        # Checking if contours are present. If they are, store in contours array
        if len(contourshand_sign) == 0:
            print(f"No contours found")
        contours_refs.append(contourshand_sign)

        # Getting defects #################### 
        defectsTotal, defects = getDefects(contourshand_sign[0])
        # Save defects total for the hand_signs
        defects_hand_signs.append(defectsTotal)
        #print(f'Number of convexity defects: {defectsTotal}') # Debug print

        ### MAKING COPY WITH DRAWN DEFECTS AND HULL ###
        newImage = drawDefects(img, contourshand_sign[0], defects)
        cv2.imwrite(f'Defects_test{i}.png', newImage)
        i += 1

    except Exception as e:
        print(f"Error processing the file {img}: {e}")        

# Converts live video footage to binary
def getBinaryVideo(frame):
    # Convert the frame to grayscale, for easier manipulation
    gray_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    _, binaryImg = cv2.threshold(gray_frame, white_threshold, 255, cv2.THRESH_BINARY)

    return binaryImg

# Main function - compares contours between references and live feed
def findBestMatch(contours_refs, contours_live, defects_live):
    global defects_hand_signs

    best_match_value = float('inf')
    best_match_index = -1

    # Iterate through the contours array and compare with live contours
    for i, hand_sign_contours in enumerate(contours_refs):
        if len(hand_sign_contours) == 0:
            print(f"Warning: Empty contour encountered at index {i}. Skipping.")
            continue

        match_value = cv2.matchShapes(contours_live[0], hand_sign_contours[0], cv2.CONTOURS_MATCH_I3, 0.0)

        # Only sets a new best match if it both has a better accuracy value, and the amount of defects match
        if match_value < best_match_value and defects_hand_signs[i] == defects_live:
            best_match_value = match_value
            best_match_index = i
            
    return best_match_index, best_match_value

# Erodes to removes small pixels or clusters, then dilates back to original image
def removeNoise(frame):
    # Processen er teknisk set opening 
    erosion_iterations = 8
    dilate_iterations = 8

    kernel = np.array([[0,0,1,0,0],
                       [0,1,1,1,0],
                       [0,0,1,0,0]], dtype=np.uint8)

    # Erosion to remove bad pixels
    frame = cv2.erode(frame, kernel, iterations=erosion_iterations)

    # Dilate to get back to OG image
    frame = cv2.dilate(frame, kernel, iterations=dilate_iterations)

    return frame

# Dilates image to close potential holes in shape of hand, then erodes back to original image
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

# Retrives convexity defects and total of convexity defects for shape
def getDefects(contours):

    # Check for contours
    if len(contours) == 0:
        print('No contours found')
        return 0, None

    hull = cv2.convexHull(contours, returnPoints=False)
    defects = cv2.convexityDefects(contours,hull)

    # Check for convexity defects
    if defects is None:
        print('No defects')
        return 0, None

    distanceFilter = 8000

    # Distance filtering irrelevant points
    filter_arr = defects[:, 0, 3] > distanceFilter  # Create a boolean mask #NOTE : betyder for alle, 0 betyder for x-akse, og så det 4. element. So for hvert element i x-aksen, find fjerde element, tjek condition, og ændr værdi til true eller false. Derfor lægges vores filter array på vores defects. God forklaring: https://johnfoster.pge.utexas.edu/numerical-methods-book/ScientificPython_Numpy.html
    newDefects = defects[filter_arr]  # Apply the boolean mask to filter the defects
 
    # As defects have locations, we are only interested in the amount
    defects_total = newDefects.shape[0]

    return defects_total, newDefects

# Draws defects on image
def drawDefects(frame, contours, defects):
    frame = cv2.cvtColor(frame, cv2.COLOR_GRAY2BGR)

    # source: https://docs.opencv.org/4.x/d5/d45/tutorial_py_contours_more_functions.html
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

    return frame

# OBSOLETE source: https://stackoverflow.com/questions/58632469/how-to-find-the-orientation-of-an-object-shape-python-opencv

# QUALITY OF LIFE, SMALL FUNCTIONS ####################################################

# Displays text in top left corner of image
def displayText(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 30), font, 1, (0, 0, 255), 2, cv2.LINE_AA)
    return image

# Displays text just below the very top line
def displayTextBelow(image, text):
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, text, (10, 65), font, 1, (0, 0, 255), 2, cv2.LINE_AA)
    return image

# Displays the accuracy of the current match in the bottom left corner
def displayMatchAccuracy(image, match):
    # Display the match accuracy on the frame in the bottom left corner
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(image, f'Match: {match}', (10, image.shape[0] - 10), font, 1, (255, 255, 255), 2, cv2.LINE_AA)
    return image

# Reverse search our buffer dicitionary to find the key from the value
# source: https://www.geeksforgeeks.org/python-get-key-from-value-in-dictionary/
def get_key_from_buffer(val):
  
    for key, value in bufferDict.items():
        if val == value:
            return key

    return "key doesn't exist"

# Totals the amount values in the buffer dictionary
def get_buffer_total():
    global bufferDict

    total = 0

    for key, value in bufferDict.items():
        if value == None:
            print(f'No value found for {key}, skipping')
            continue
        total += value

    return total

# CLOSING THE APPLICATION

# Closes the applications and releases resources
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
    close_application()
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

# 1st state - captures references for all hand signs and stores their data
def state_capture_hand_signs(raw_frame, binary_frame):
    global hand_signs, hand_signIndex, key

    if hand_signIndex < len(hand_signs):
        hand_sign = hand_signs[hand_signIndex]

        contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE) 

        # Checking for contours
        if len(contoursLive) == 0:
            frame = displayText(raw_frame.copy(), 'No contours found in live feed')
            cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
            return 'capture_hand_signs'
        
        # Displaying instructions
        frame = displayText(raw_frame.copy(), f'Capturing {hand_sign}. Press "s" to save,')

        binary_frame = getBinaryVideo(raw_frame.copy())

        # Displaying the feeds
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
        cv2.imshow('Binary Feed', binary_frame)  # Updates 'Binary Feed' window

        # If user press 's' it saves the image and processes it for contours and convexity defects.
        if key == ord('s'):
            binaryImg = getBinaryImage(binary_frame, hand_sign) 

            process_hand_sign(binaryImg)
            hand_signIndex += 1  # Move to the next hand_sign

    if hand_signIndex == len(hand_signs):
        return 'match_hand_signs'  # Move to the next state after all hand_signs are captured
    else:
        return 'capture_hand_signs'  # Stay in the current state if not all hand_signs are captured       

# 2nd state - matching the captured hand_sign with the live feed
def state_match_hand_signs(raw_frame, binary_frame):
    global contours_refs, match_threshold, bufferDict, currenthand_sign, bufferTotalThreshold, hand_sign_name, bufferThreshhold, previous_hand_sign
    
    # Display the frame with no text
    frame = displayText(raw_frame.copy(), '')

    # Find the contours in the binary frame
    contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    # Checking for contours
    if len(contoursLive) == 0:
        frame = displayText(frame, 'No contours found in live feed')
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
        return 'match_hand_signs'

    # Getting defects
    defectsTotalLive, defectsLive = getDefects(contoursLive[0])

    # Checking for contours
    if defectsTotalLive is None:
        frame = displayText(frame, 'No defects found in live feed')
        cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
        return 'match_hand_signs'

    best_match_index, best_match_value = findBestMatch(contours_refs, contoursLive, defectsTotalLive)

    # Display the match accuracy on the frame
    displayMatchAccuracy(frame, round(best_match_value, 2))

######################## BUFFER ############################
    # Adding the best matched hand_sign to buffer, if it meets the threshold
    if best_match_value < match_threshold:
        hand_sign_name = hand_signs[best_match_index]
        bufferDict[hand_sign_name] += 1
    else:
        bufferDict['No hand_sign'] += 1   
    
    #print(bufferDict)

    maxBufferValue = max(bufferDict.values())

    bufferTotal = get_buffer_total()


    # Send hand_sign best matched hand_sign name to Unity
    if bufferTotal == bufferTotalThreshold:
        #print('We made it thru!')

        if best_match_index != -1 and maxBufferValue >= bufferThreshhold:
            hand_sign_name = get_key_from_buffer(maxBufferValue)

            previous_hand_sign = hand_sign_name

            #print(hand_sign_name)

            # Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

            sock.sendto(str.encode(hand_sign_name), serverAddressPort)

            currenthand_sign = hand_sign_name

        else:
            print('No certain hand_sign')

            currenthand_sign = 'No hand_sign'
            #print(f"currenthand_sign updated to: {currenthand_sign}")

            # Send hand_sign_name to Unity via UDP
            sock.sendto(str.encode(hand_sign_name), serverAddressPort)
            print(f'Sending {hand_sign_name} to Unity')

            # Reset buffer
            bufferDict = dict.fromkeys(bufferDict, 0)

    # If there is no new hand_sign, keep the same hand_sign and send it
    elif maxBufferValue < bufferThreshhold :
        # Send hand_sign_name to Unity via UDP
        sock.sendto(str.encode(previous_hand_sign), serverAddressPort)
        print(f"Sending {previous_hand_sign} to Unity")

    if best_match_value < match_threshold:
        frame = displayText(frame, f'Matched hand_sign: {hand_sign_name}')
    else:
        frame = displayText(frame, f'Matched hand_sign: No good match')
        
    ###################################################################################
    # Tester defects og tegner dem på live billede
    binary_frame = drawDefects(binary_frame, contoursLive[0], defectsLive)

    # Displaying the feeds
    cv2.imshow('Live Feed', frame)  # Updates 'Live Feed' window
    cv2.imshow('Binary Feed', binary_frame)  # Updates 'Binary Feed' window

    return 'match_hand_signs'  # Remain in the current state

# State dictionary
states = {
    'capture_hand_signs': state_capture_hand_signs,
    'match_hand_signs': state_match_hand_signs,
}

# MAIN #####################################################################################

# Open the camera
cap = cv2.VideoCapture(1)

running = True

print('Running...')

# Initial state
current_state = 'capture_hand_signs'

# main loop captures live feed, creates a binary feed, and cleans up the image before getting processed through states.
while current_state and running:
    ret, frame = cap.read()
    if not ret:
        print("Error: Failed to capture image")
        close_application()
        break

    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    # Cropping frame - only relevant due to physical setup.
    y, x, h, w = 0, 50, 550, 650
    frame = raw_frame[y:y+h, x:x+w]

    binary_frame = getBinaryVideo(frame.copy()) # Getting binary frame
    binary_frame = removeNoise(binary_frame) # Removes noise by 'opening'
    binary_frame = closingImage(binary_frame) # Closing image (should close potential holes in binary images of hands, like tattoos, and small shadows)

    # Define the key press
    key = cv2.waitKey(3) & 0xFF
    if key == ord('q'):
        close_application()
        
    # Execute the current state function
    current_state = states[current_state](frame, binary_frame)