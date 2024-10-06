import asyncio
import websockets
from PIL import Image
import io
import numpy as np

gauss_mask = np.array([[2, 4, 5, 4, 2],
                   [4, 9, 12, 9, 4],
                   [5, 12, 15, 12, 5],
                   [4, 9, 12, 9, 4],
                   [2, 4, 5, 4, 2]]) / 159

g_y = np.array([[1, 2, 1],
                [0, 0, 0],
                [-1, -2, -1]])

g_x = np.array([[-1, 0, 1],
                [-2, 0, 2],
                [-1, 0, 1]])

def tracking(hight_mask, main_mask):
    height, width = hight_mask.shape
    result = np.copy(hight_mask)
    
    for i in range(1, height-1):
        for j in range(1, width-1):
            if main_mask[i, j] == 0:
                if np.any(hight_mask[i-1:i+2, j-1:j+2]):
                    result[i, j] = 1
                else:
                    result[i, j] = 0
    
    return result

def double_threshold_filtering(suppression_pixel_values, low_threshold, high_threshold):
    hight_mask = (suppression_pixel_values >= high_threshold)
    main_mask = (suppression_pixel_values >= low_threshold) & (suppression_pixel_values < high_threshold)
    return hight_mask, main_mask

def suppression_no_maximum(gradient):
    height, width = gradient.shape
    result = np.zeros_like(gradient)
    
    for i in range(1, height-1):
        for j in range(1, width-1):
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

def get_conv(pixel_values, kernel):
    height, width = pixel_values.shape
    kernel_size = kernel.shape[0]
    pad_size = kernel_size // 2
    padded_image = np.pad(pixel_values, pad_size, mode='constant')
    result = np.zeros_like(pixel_values, dtype=np.float32)

    for i in range(height):
        for j in range(width):
            result[i, j] = np.sum(padded_image[i:i+kernel_size, j:j+kernel_size] * kernel)

    return result

def get_gradients(gauss_pixel_values):
    gradient_y = get_conv(gauss_pixel_values, g_y)
    gradient_x = get_conv(gauss_pixel_values, g_x)
    gradient = np.sqrt(gradient_x**2 + gradient_y**2)
    angle = np.arctan2(gradient_y, gradient_x)

    return gradient, angle

def get_gray_img(pixel_values):
    return np.round(0.2989 * pixel_values[:, :, 0] + 0.5870 * pixel_values[:, :, 1] + 0.1140 * pixel_values[:, :, 2]).astype(np.uint8)

def get_pixel_value(image):
    return np.array(image)

async def handle_client(websocket):
    while True:
        async for message in websocket:
            image = Image.open(io.BytesIO(message))
            imgFormat = image.format

            pixel_values = get_pixel_value(image)

            #градация серого
            gray_pixel_values = get_gray_img(pixel_values)

            #сглаживание по гауссу
            gauss_pixel_values = get_conv(gray_pixel_values, gauss_mask)

            #нахождение градиентов
            gradient, angle = get_gradients(gauss_pixel_values)

            #подавление не максимумов
            suppression_pixel_values = suppression_no_maximum(gradient)

            #двойная пороговая фильтрация
            hight_mask, main_mask = double_threshold_filtering(suppression_pixel_values, -30, 90)

            #трассировка области неоднозначности
            output_pixel_values = tracking(hight_mask, main_mask)

            output_img = Image.fromarray(output_pixel_values).convert('RGB')
            image_bytes = io.BytesIO()
            output_img.save(image_bytes, format=imgFormat)
            image_bytes = image_bytes.getvalue()

            await websocket.send(image_bytes)

start_server = websockets.serve(handle_client, "localhost", 8888)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()

