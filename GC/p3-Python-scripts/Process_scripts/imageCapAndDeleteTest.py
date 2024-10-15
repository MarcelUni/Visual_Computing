import cv2
import os

# Function to manipulate the image (example implementation)
def manipulate_image(image):
    # Example manipulation: convert to grayscale
    return cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

# Initialize video capture
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("Error: Could not open video.")
    exit()

while True:
    ret, frame = cap.read()  # Read a frame from the video feed

    if not ret:
        print("Error: Could not read frame.")
        break

    cv2.imshow('Video Feed', frame)  # Display the frame

    # Wait for a key press (1 ms delay)
    key = cv2.waitKey(1) & 0xFF

    if key == ord('s'):  # Press 's' to save the image
        cv2.imwrite('captured_image.png', frame)
        print("Image saved as 'captured_image.png'")

        # Manipulate the image
        manipulated_image = manipulate_image(frame)
        
        # Save the manipulated image
        cv2.imwrite('manipulated_image.png', manipulated_image)
        print("Manipulated image saved as 'manipulated_image.png'")

        # Delete the original captured image
        try:
            os.remove('captured_image.png')
            print("Original image deleted.")
        except FileNotFoundError:
            print("Error: Original image file not found.")
        except Exception as e:
            print(f"Error deleting the image: {e}")

    elif key == ord('q'):  # Press 'q' to quit
        break

# Cleanup
cap.release()
cv2.destroyAllWindows()
