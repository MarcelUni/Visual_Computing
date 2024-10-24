import cv2
import numpy as np

# Define available modes
modes = ["Original", "Grayscale", "Edge Detection"]

# Function to get the next unused mode
def get_next_unused_mode(current_mode, used_modes):
    # Find the next mode that hasn't been used
    for i in range(len(modes)):
        next_mode = (current_mode + i + 1) % len(modes)
        if next_mode not in used_modes:
            return next_mode
    return current_mode  # If all modes have been used, return the current mode

# Initialize video capture
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("Error: Could not open video.")
    exit()

# Set the initial mode and track used modes
current_mode = 0
used_modes = {current_mode}  # Start with the original mode as used

while True:
    ret, frame = cap.read()  # Read a frame from the video feed

    if not ret:
        print("Error: Could not read frame.")
        break

    # Display the original video feed
    cv2.imshow('Video Feed', frame)

    # Process the image based on the current mode
    if current_mode == 0:  # Original
        processed_frame = frame
        
    elif current_mode == 1:  # Grayscale
        processed_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

    elif current_mode == 2:  # Edge Detection
        processed_frame = cv2.Canny(frame, 100, 200)


    # Display the processed image
    cv2.imshow('Processed Image', processed_frame)

    # Check for key presses
    key = cv2.waitKey(1) & 0xFF

    if key == ord('n'):  # Press 'n' to switch to the next unused mode
        next_mode = get_next_unused_mode(current_mode, used_modes)
        if next_mode != current_mode:  # Only switch if we have a new mode
            current_mode = next_mode
            used_modes.add(current_mode)  # Mark the new mode as used
            print(f"Mode switched to: {modes[current_mode]}")

    elif key == ord('q'):  # Press 'q' to quit
        break

# Cleanup
cap.release()
cv2.destroyAllWindows()
