import cv2
import numpy as np

def getBinaryVideo(frame):
    # Convert the frame to grayscale, for easier manipulation
    gray_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    _, binaryImg = cv2.threshold(gray_frame, 150, 255, cv2.THRESH_BINARY)

    return binaryImg
def getConvexityDefects(contours, img):
    hull = cv2.convexHull(contours,returnPoints = False)
    defects = cv2.convexityDefects(contours,hull)
 
    for i in range(defects.shape[0]):
        s,e,f,d = defects[i,0]
        start = tuple(contours[s][0])
        end = tuple(contours[e][0])
        far = tuple(contours[f][0])
        cv2.line(img,start,end,[0,255,0],2)
        cv2.circle(img,far,5,[0,0,255],-1)
    return img

import cv2
import numpy as np


def close_application():
    print("Closing application")
    cap.release()
    cv2.destroyAllWindows()


# Open the camera
cap = cv2.VideoCapture(1)

while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    if not ret:
        print("Error: Failed to capture image")
        close_application()
        break

    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    

    binary_frame = getBinaryVideo(raw_frame)
    contoursLive, _ = cv2.findContours(binary_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)


    frame = getConvexityDefects(contoursLive[0], binary_frame)

    cv2.imshow('Cropped feed', frame)


    #Define the key press
    key = cv2.waitKey(1) & 0xFF
    if key == ord('q'):
        close_application()

    
