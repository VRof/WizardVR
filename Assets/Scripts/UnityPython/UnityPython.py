import io
import socket
import os
import sys
import warnings
import numpy as np
from PIL import Image
import logging
from matplotlib import pyplot as plt
from matplotlib import image as mpimg

warnings.filterwarnings("ignore")
logging.getLogger('absl').setLevel(logging.ERROR)
os.environ['TF_ENABLE_ONEDNN_OPTS'] = '0'
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'

from keras.api.preprocessing.image import img_to_array
from keras.api.models import load_model
from keras.api.utils import to_categorical

log_file = "script_log.txt"
try:
    script_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
    script_path = os.path.join(script_dir, log_file)
    if os.path.exists(script_path) and os.path.getsize(script_path) > 0:
        with open(script_path, 'w', encoding='utf-8') as f:
            f.write('')
except Exception:
    pass


def log_message(message):
    with open(script_path, 'a', encoding='utf-8') as folder:
        folder.write(message + '\n')


def preprocess_image(image_bytes):
    try:
        img = Image.open(io.BytesIO(image_bytes))


        upscale_factor = 1
        new_width = int(img.width * upscale_factor)
        new_height = int(img.height * upscale_factor)
        img = img.resize((new_width, new_height))
        left = (new_width - 250) / 2
        top = (new_height - 250) / 2
        right = (new_width + 250) / 2
        bottom = (new_height + 250) / 2
        img = img.crop((left, top, right, bottom))
        new_size = (128, 128)
        img = img.resize(new_size)
        # plt.imshow(img)
        # plt.show()
        img_array = img_to_array(img) / 255.0
        img_array = np.expand_dims(img_array, axis=0)
        return img_array
    except Exception as e:
        log_message(f"Error in preprocessing image: {e}")
        exit(1)


# Update the model with new data
def update_model(model, images, labels):
    try:
        model.fit(images, labels, epochs=1, batch_size=len(images), verbose=0)
    except Exception as e:
        log_message(f"Error in updating model: {e}")
        exit(1)


def save_model(model_path):
    try:
        model.save(model_path)
    except Exception as e:
        log_message(f"Error in saving model: {e}")
        exit(1)


def image_to_bytes(image_path):
    try:
        with open(image_path, 'rb') as image_file:
            image = Image.open(image_file)
            byte_arr = io.BytesIO()
            image.save(byte_arr, format=image.format)
            return byte_arr.getvalue()
    except Exception as e:
        log_message(f"{e}")
        exit(1)


# class_labels = ['heal', 'summon', 'frostbolt', 'fireball', 'teleport', 'shield', 'shock', 'none', 'meteor'] # for trained_model_new*
class_labels = ['none', 'heal', 'summon', 'frostbolt', 'fireball', 'teleport', 'shield', 'shock',
                'meteor']  # for trained_model_3

image_batch = []
label_batch = []

# load base images for image batch
try:
    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "none_base.jpg")))[0])
    label_batch.append(0)

    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "heal_base.jpg")))[0])
    label_batch.append(1)

    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "summon_base.jpg")))[
                           0])
    label_batch.append(2)

    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "frost_bolt.jpg")))[
                           0])
    label_batch.append(3)

    image_batch.append(preprocess_image(image_to_bytes(
        os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "fireball_base.jpg")))[0])
    label_batch.append(4)

    image_batch.append(preprocess_image(image_to_bytes(
        os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "teleport_base.jpg")))[0])
    label_batch.append(5)

    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "shield_base.jpg")))[
                           0])
    label_batch.append(6)

    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "shock_base.jpg")))[
                           0])
    label_batch.append(7)

    image_batch.append(preprocess_image(
        image_to_bytes(os.path.join(os.path.dirname(os.path.abspath(sys.argv[0])), "base_images", "meteor_base.jpg")))[
                           0])
    label_batch.append(8)
except Exception as e:
    log_message(f"{e}")
    exit(1)

log_message("base images inserted")

# create socket
try:
    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((host, port))
except Exception as e:
    log_message(f"error in socket {e}")
    exit(1)

# load model
try:
    model_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
    model_path = os.path.join(model_dir, "trained_model_3.h5")
    model = load_model(model_path, compile=False)
    model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])
except Exception as e:
    log_message(f"Error in loading model : {e}")
    exit(1)

# start listening
while True:
    received_data = sock.recv(10000)
    if not received_data:
        continue
    else:
        try:
            img_array = preprocess_image(received_data)
            predictions = model.predict(img_array, verbose=0)
            predicted_index = np.argmax(predictions)
            predicted_class = class_labels[predicted_index]
            probability = predictions[0][predicted_index]
            log_message(f"Predicted Class: {predicted_class}, Probability: {probability:.4f}")
            if probability > 0.90:
                sock.sendall(predicted_class.encode("UTF-8"))  # send to unity
                image_batch.pop(predicted_index)
                image_batch.insert(predicted_index, img_array[0])
                image_batch_array = np.array(image_batch)
                label_batch_to_categorical = to_categorical(label_batch, num_classes=len(class_labels))

                update_model(model, image_batch_array, label_batch_to_categorical)
                save_model(model_path)
                log_message(f"Model Updated")
            else:
                sock.sendall("not recognized".encode("UTF-8"))
                log_message(f"Model Not Updated")
        except Exception as e:
            log_message(f"Error during prediction: {e}")
            sock.sendall(f"Error during prediction: {e}".encode("UTF-8"))
            exit(1)
