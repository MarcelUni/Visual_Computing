import cv2
import numpy as np


def close_application():
    print("Closing application")
    cap.release()
    cv2.destroyAllWindows()


# Open the camera
cap = cv2.VideoCapture(0)

while True:
    # Capture frame-by-frame
    ret, frame = cap.read()
    if not ret:
        print("Error: Failed to capture image")
        close_application()
        break

    raw_frame = cv2.flip(frame, 1)  # Flip the frame horizontally (mirror effect)
    
    #Cropping frame
    y = 50
    x = 50
    h = 300
    w = 300
    raw_frame = raw_frame[y:y+h, x:x+w]

    #Define the key press
    key = cv2.waitKey(1) & 0xFF
    if key == ord('q'):
        close_application()

    
