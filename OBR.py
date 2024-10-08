import numpy as np
from PIL import Image

class OperatorCanny:
    def __init__(self):
        self.__gauss_mask__ = np.array([[2, 4, 5, 4, 2],
                   [4, 9, 12, 9, 4],
                   [5, 12, 15, 12, 5],
                   [4, 9, 12, 9, 4],
                   [2, 4, 5, 4, 2]]) / 159

        self.__g_y__ = np.array([[1, 2, 1],
                        [0, 0, 0],
                        [-1, -2, -1]])

        self.__g_x__ = np.array([[-1, 0, 1],
                        [-2, 0, 2],
                        [-1, 0, 1]])
    
    def __get_pixel_value__(selt, image):
        return np.array(image)
    
    def __get_gray_img__(self, pixel_values):
        return np.round(0.2989 * pixel_values[:, :, 0] + 0.5870 * pixel_values[:, :, 1] + 0.1140 * pixel_values[:, :, 2]).astype(np.uint8)
    
    def __tracking__(self, hight_mask, main_mask):
        height, width = hight_mask.shape
        result = np.copy(hight_mask)
        
        for i, j in np.ndindex(height-2, width-2):
            i += 1
            j += 1
            if main_mask[i, j] == 0:
                if np.any(hight_mask[i-1:i+2, j-1:j+2]):
                    result[i, j] = 1
                else:
                    result[i, j] = 0
        
        return result
    
    def __double_threshold_filtering__(self, suppression_pixel_values, low_threshold, high_threshold):
        hight_mask = (suppression_pixel_values >= high_threshold)
        main_mask = (suppression_pixel_values >= low_threshold) & (suppression_pixel_values < high_threshold)
        return hight_mask, main_mask
    
    def __suppression_no_maximum__(self, gradient):
        height, width = gradient.shape
        result = np.zeros_like(gradient)
        
        for i, j in np.ndindex(height-2, width-2):
            i += 1
            j += 1
            direction = gradient[i, j]

            if (0 <= direction < np.pi/4) or (7*np.pi/4 <= direction <= 2*np.pi):
                neighbors = np.array([gradient[i, j+1], gradient[i, j-1]])
            elif (np.pi/4 <= direction < 3*np.pi/4):
                neighbors = np.array([gradient[i-1, j+1], gradient[i+1, j-1]])
            elif (3*np.pi/4 <= direction < 5*np.pi/4):
                neighbors = np.array([gradient[i-1, j], gradient[i+1, j]])
            else:
                neighbors = np.array([gradient[i-1, j-1], gradient[i+1, j+1]])

            if gradient[i, j] >= np.max(neighbors):
                result[i, j] = gradient[i, j]

        return result
    
    def __get_conv__(self, pixel_values, kernel):
        height, width = pixel_values.shape
        kernel_size = kernel.shape[0]
        pad_size = kernel_size // 2
        padded_image = np.pad(pixel_values, pad_size, mode='constant')
        result = np.zeros_like(pixel_values, dtype=np.float32)

        for i in range(height):
            for j in range(width):
                result[i, j] = np.sum(padded_image[i:i+kernel_size, j:j+kernel_size] * kernel)
            
        return result
    
    def __get_gradients__(self, gauss_pixel_values):
        gradient_y = self.__get_conv__(gauss_pixel_values, self.__g_y__)
        gradient_x = self.__get_conv__(gauss_pixel_values, self.__g_x__)
        gradient = np.sqrt(gradient_x**2 + gradient_y**2)
        angle = np.arctan2(gradient_y, gradient_x)

        return gradient, angle
    
    
    def start(self, image, thread_count = 1, low_threshold = -75, high_threshold = 75):     
        pixel_values = self.__get_pixel_value__(image)
        gray_pixel_values = self.__get_gray_img__(pixel_values)
        gauss_pixel_values = self.__get_conv__(gray_pixel_values, self.__gauss_mask__)
        gradient, angle = self.__get_gradients__(gauss_pixel_values)

        suppression_pixel_values = self.__suppression_no_maximum__(gradient)
        hight_mask, main_mask = self.__double_threshold_filtering__(suppression_pixel_values, low_threshold, high_threshold)
        output_pixel_values = self.__tracking__(hight_mask, main_mask)
        return Image.fromarray(output_pixel_values).convert('RGB')