import cv2
import numpy as np
import os
import csv

filenames = []

contours_refs = []

white_threshold = 100        

def on_trackbar(val): # Tager imod en integer værdi fra trackbaren
    global white_threshold
    white_threshold = val

hand_signs = ['Forward', 'Backward']

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

    distanceFilter = 10000

    # Distance filtering irrelevant points
    filter_arr = defects[:, 0, 3] > distanceFilter  # Create a boolean mask #NOTE : betyder for alle, 0 betyder for x-akse, og så det 4. element. So for hvert element i x-aksen, find fjerde element, tjek condition, og ændr værdi til true eller false. Derfor lægges vores filter array på vores defects. God forklaring: https://johnfoster.pge.utexas.edu/numerical-methods-book/ScientificPython_Numpy.html
    newDefects = defects[filter_arr]  # Apply the boolean mask to filter the defects
 
    # As defects have locations, we are only interested in the amount
    defects_total = newDefects.shape[0]

    return defects_total, newDefects

def getBinaryImage(frame, gestureName):
    global folder

    # Save the image
    cv2.imwrite('captured_image.png', frame)
    image = cv2.imread('captured_image.png')

    # Convert the image to grayscale, for easier manipulation
    grayImg = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Converting the image to binary - aka turning light pixels to white and dark pixels to black
    _, binaryImg = cv2.threshold(grayImg, white_threshold, 255, cv2.THRESH_BINARY)

    # Save the manipulated image
    binary_filename = os.path.join(folder, f'{gestureName}_binary.png')
    cv2.imwrite(binary_filename, binaryImg)
    print(f"{gestureName} saved as '{binary_filename}'")

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
    global i, defects_gestures
    try:
        #Reading image
        gesture = img
        if gesture is None:
            print(f"Error: File not found.")

        # Getting contours ##################
        contoursGesture, hierachy = cv2.findContours(gesture, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        #print(f'Number of contours: {len(hierachy)}') # Debug print

        # Checking if contours are present. If they are, store in contours array
        if len(contoursGesture) == 0:
            print(f"No contours found")
        contours_refs.append(contoursGesture)

        # Getting defects #################### 
        defectsTotal, defects = getDefects(contoursGesture[0])
        # Save defects total for the gestures
        defects_gestures.append(defectsTotal)
        #print(f'Number of convexity defects: {defectsTotal}') # Debug print

    except Exception as e:
        print(f"Error processing the file {img}: {e}")        


#### SETUP 
cap = cv2.VideoCapture(0)

# Create a window and a trackbar
cv2.namedWindow('Binary Frame')
cv2.createTrackbar('Threshold', 'Binary Frame', white_threshold, 255, on_trackbar)

while True:
    ret, frame = cap.read()

    if ret is False:
        print('Camera issue')
        break

    frame = cv2.flip(frame, 1)


    # Nu bliver det grimt
    for hand_sign in hand_signs:

        # Get contours and defects and save them in variable
        

        # Create a file
        with open(f'{hand_sign}.csv', 'w', newline='') as file:
            writer = csv.writer(file)
            field = ["name", "age", "country"]        


    cv2.imshow('Binary Frame', frame)

    # Close on q
    if cv2.waitKey(1) & 0xFF == ord('q'):
        cap.release()
        cv2.destroyAllWindows()
        break

cap.release()
cv2.destroyAllWindows()
