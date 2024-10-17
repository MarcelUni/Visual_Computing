import cv2

# Initialize video capture
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("Error: Could not open video.")
    exit()

try:
    # First loop: Show the video in color
    while True:
        # Capture frame-by-frame
        ret, frame = cap.read()
        if not ret:
            print("Failed to grab frame.")
            break

        # Display the frame in color
        cv2.imshow('Video Feed (Color)', frame)

        # Check for key press events
        key = cv2.waitKey(1) & 0xFF

        # If 'n' is pressed, break this loop and go to the grayscale loop
        if key == ord('n'):
            print("Switching to grayscale mode.")
            cv2.destroyWindow('Video Feed (Color)')  # Close the color video window
            break

        # If 'q' is pressed, quit the program entirely
        if key == ord('q'):
            print("Exiting video feed...")
            cap.release()
            cv2.destroyAllWindows()
            exit()  # Exit the entire program

    # Second loop: Show the video in grayscale
    while True:
        # Capture frame-by-frame
        ret, frame = cap.read()
        if not ret:
            print("Failed to grab frame.")
            break

        # Convert the frame to grayscale
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

        # Display the frame in grayscale
        cv2.imshow('Video Feed (Grayscale)', frame)

        # Check for key press events
        key = cv2.waitKey(1) & 0xFF

        # If 'q' is pressed, quit the program entirely
        if key == ord('q'):
            print("Exiting video feed...")
            break

finally:
    # Release the video capture object and close any OpenCV windows
    cap.release()
    cv2.destroyAllWindows()
