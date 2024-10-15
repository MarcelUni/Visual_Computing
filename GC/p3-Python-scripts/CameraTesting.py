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
    
    cv2.imshow('Feed', raw_frame)

    #Cropping frame
    cam_x, cam_y, cam_w, cam_h = cv2.getWindowImageRect('Feed')

    y = 0
    x = 50
    h = 300
    w = 500
    cropped = raw_frame[y:y+cam_h, x:x+w]

    cv2.imshow('Cropped feed', cropped)


    print(f'x = {cam_x}')
    print(f'y = {cam_y}')
    print(f'w = {cam_w}')
    print(f'h = {cam_h}')

    #Define the key press
    key = cv2.waitKey(1) & 0xFF
    if key == ord('q'):
        close_application()

    
