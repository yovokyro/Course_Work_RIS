import asyncio
import websockets
from PIL import Image
import io
import time
import json
import base64

from OBR import OperatorCanny

start_time = 0

async def handle_client(websocket):
    client_data = bytearray()

    try:  
        async for message in websocket:
            client_data.extend(message) 

            if client_data.endswith(b'EOF'):
                image = Image.open(io.BytesIO(client_data[:-3]))
                imgFormat = image.format
                print("Image received successfully")
                start_time = time.time()   
                
                oc = OperatorCanny()
                output_img = oc.start(image, thread_count=3)
                
                image_bytes = io.BytesIO()
                output_img.save(image_bytes, format=imgFormat)
                image_bytes = image_bytes.getvalue()
                
                result_time = time.time() - start_time
                
                packet = {
                    'image': base64.b64encode(image_bytes).decode('utf-8'),
                    'time': str(result_time),
                }
                
                await websocket.send(json.dumps(packet))
                client_data = bytearray()
    except:
        del client_data

start_server = websockets.serve(handle_client, "localhost", 8888)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()

