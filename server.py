import asyncio
import websockets
from PIL import Image
import io
import numpy as np

async def get_gray_img(pixel_values):
    return np.round(0.2989 * pixel_values[:, :, 0] + 0.5870 * pixel_values[:, :, 1] + 0.1140 * pixel_values[:, :, 2]).astype(np.uint8)


async def get_pixel_value(image):
    return np.array(image)

async def handle_client(websocket, path):
    while True:
        message = await websocket.recv()
        image = Image.open(io.BytesIO(message))
        imgFormat = image.format

        pixel_values = await get_pixel_value(image)
        gray_img = await get_gray_img(pixel_values)

        new_img = Image.fromarray(gray_img)
        image_bytes = io.BytesIO()
        new_img.save(image_bytes, format=imgFormat)
        image_bytes = image_bytes.getvalue()

        await websocket.send(image_bytes)

start_server = websockets.serve(handle_client, "localhost", 8888)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()

