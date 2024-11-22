import numpy as np
import matplotlib.pyplot as plt

# Step 1: Create a 5x5 NumPy array with all values set to 0
image = np.array([
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 255, 255, 0, 0, 0, 0],
    [0, 0, 255, 255, 255, 255, 0, 0, 0, 0],
    [0, 0, 255, 255, 255, 255, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 255, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
], dtype=np.uint8)
# Step 2: Use Matplotlib to display the image with axis labels from 1 to 5
plt.imshow(image, cmap='gray', vmin=0, vmax=255)
plt.xticks(ticks=np.arange(5), labels=np.arange(1, 6))
plt.yticks(ticks=np.arange(5), labels=np.arange(1, 6))

plt.axis('off')  # Turn off the axis

# Step 3: Save the image to a folder
output_path = '10x10_img_no_axis.png'
plt.savefig(output_path, bbox_inches='tight', pad_inches=0)
plt.show()

print(f"Image saved to {output_path}")