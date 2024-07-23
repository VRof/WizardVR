from collections import deque
import io
import random
import socket
import sys
import numpy as np
import os
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Conv2D, MaxPooling2D, GlobalAveragePooling2D, Dense, Dropout
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.utils import to_categorical
from sklearn.model_selection import train_test_split
from PIL import Image

# Define the model
# def create_model(num_classes):
#     model = Sequential([
#         Conv2D(32, (3, 3), activation='relu', input_shape=(128, 128, 1)),
#         MaxPooling2D((2, 2)),
#         Conv2D(64, (3, 3), activation='relu'),
#         MaxPooling2D((2, 2)),
#         Conv2D(64, (3, 3), activation='relu'),
#         MaxPooling2D((2, 2)),
#         Conv2D(128, (3, 3), activation='relu'),
#         GlobalAveragePooling2D(),
#         Dense(128, activation='relu'),
#         Dropout(0.5),
#         Dense(64, activation='relu'),
#         Dropout(0.2),
#         Dense(num_classes, activation='softmax')
#     ])
    
#     model.compile(optimizer=Adam(learning_rate=0.001),
#                   loss='categorical_crossentropy',
#                   metrics=['accuracy'])
    
#     return model

# Function to load the model
def load_saved_model(model_path):
    model = tf.keras.models.load_model(model_path)
    model.compile(optimizer=Adam(learning_rate=0.001),
                  loss='categorical_crossentropy',
                  metrics=['accuracy'])
    return model

def predict_skill(model, image, class_names):
    if isinstance(image, Image.Image):
        # Convert PIL Image to numpy array
        image = np.array(image.convert('L'))  # Convert to grayscale
        image = image.reshape(1, 128, 128, 1)  # Reshape for model input
    else:
        # Assume it's already a numpy array
        image = image.reshape(1, 128, 128, 1)
    
    image = image.astype('float32') / 255
    prediction = model.predict(image)[0]
    predicted_skill_index = np.argmax(prediction)
    confidence = prediction[predicted_skill_index]
    predicted_skill_name = class_names[predicted_skill_index]
    return predicted_skill_name, confidence, prediction

@tf.function
def train_step(model, images, labels):
    with tf.GradientTape() as tape:
        predictions = model(images, training=True)
        loss = tf.keras.losses.categorical_crossentropy(labels, predictions)
        # Sum the loss over the batch size
        loss = tf.reduce_mean(loss)
    gradients = tape.gradient(loss, model.trainable_variables)
    model.optimizer.apply_gradients(zip(gradients, model.trainable_variables))
    return loss



class ClassBuffer:
    def __init__(self, buffer_size=100, player_preference=0.7):
        self.buffer = deque(maxlen=buffer_size)
        self.player_preference = player_preference

    def add(self, image, is_player=False):
        self.buffer.append((image, is_player))

    def sample(self, num_samples):
        if len(self.buffer) == 0:
            return []

        # Calculate weights based on position and player flag
        weights = []
        for i, (_, is_player) in enumerate(self.buffer):
            weight = (i + 1) / len(self.buffer)  # Position weight
            if is_player:
                weight *= self.player_preference
            weights.append(weight)

        # Normalize weights
        total_weight = sum(weights)
        normalized_weights = [w / total_weight for w in weights]

        # Perform weighted sampling
        sampled_indices = random.choices(range(len(self.buffer)), 
                                         weights=normalized_weights, 
                                         k=min(num_samples, len(self.buffer)))
        
        return [self.buffer[i] for i in sampled_indices]

class MultiClassBuffer:
    def __init__(self, class_names, buffer_size=100, player_preference=0.7):
        self.class_names = list(class_names)
        self.class_buffers = {name: ClassBuffer(buffer_size, player_preference) for name in self.class_names}

    def add(self, image, class_name, is_player=False):
        if class_name in self.class_buffers:
            self.class_buffers[class_name].add(image, is_player)
        else:
            print(f"Warning: Unknown class '{class_name}'")

    def sample(self, batch_size):
        samples_per_class = batch_size // len(self.class_names)
        remainder = batch_size % len(self.class_names)

        samples = []
        for class_name in self.class_names:
            class_samples = self.class_buffers[class_name].sample(samples_per_class)
            samples.extend((image, self.class_names.index(class_name), is_player) 
                           for image, is_player in class_samples)

        # Distribute remainder samples
        for i in range(remainder):
            class_name = self.class_names[i]
            extra_sample = self.class_buffers[class_name].sample(1)
            if extra_sample:
                samples.append((extra_sample[0][0], self.class_names.index(class_name), extra_sample[0][1]))

        random.shuffle(samples)
        return samples

def load_image(image_path):
    try:
        with Image.open(image_path) as img:
            return img.convert('RGB')
    except Exception as e:
        print(f"Error loading {image_path}: {str(e)}")
        return None


def load_initial_buffer(buffer, folder_name='BufferImages'):
    # Get the directory where the script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    # Construct the path to the BufferImages folder
    base_path = os.path.join(script_dir, folder_name)
    for class_name in buffer.class_names:
        print(class_name)
        class_path = os.path.join(base_path, class_name)
        if os.path.exists(class_path):
            for file_name in os.listdir(class_path):
                if file_name.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp', '.gif')):
                    image_path = os.path.join(class_path, file_name)
                    image = load_image(image_path)
                    if image:
                        buffer.add(image, class_name, is_player=False)
        else:
            print(f"Warning: Path not found for class {class_name} at {class_path}")

def preprocess_image(image):
    if not isinstance(image, Image.Image):
        raise ValueError("Input must be a PIL Image object")
    image = image.convert('L')  # Convert to grayscale
    image = image.resize((128, 128))  # Resize to match model input
    image_array = np.array(image).reshape(1, 128, 128, 1)
    return image_array.astype('float32') / 255

def update_model_with_buffer(model, buffer, batch_size, class_names):
    samples = buffer.sample(batch_size)
    images = []
    labels = []

    for image, class_index, _ in samples:
        preprocessed_image = preprocess_image(image)
        images.append(preprocessed_image)
        label = np.zeros((1, len(class_names)))
        label[0, class_index] = 1
        labels.append(label)

    images = np.vstack(images)
    labels = np.vstack(labels)

    image_tensor = tf.convert_to_tensor(images, dtype=tf.float32)
    label_tensor = tf.convert_to_tensor(labels, dtype=tf.float32)

    loss = train_step(model, image_tensor, label_tensor)
    return loss

def update_model(model, buffer, image, class_name, batch_size=32):
    if isinstance(image, str):
        image = load_image(image)
    if image is not None:
        buffer.add(image, class_name, is_player=True)
        return update_model_with_buffer(model, buffer, batch_size, buffer.class_names)
    else:
        print(f"Failed to add image for class {class_name}")
        return None

# Main execution
if __name__ == "__main__":
    model_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
    base_path = os.path.join(model_dir,"spell_recognition_model.h5")
    model_save_path = base_path  # Path to save the model

    class_names = ['fireball', 'frostbeam', 'heal', 'meteor', 'others', 'shield', 'summon', 'teleport']

    # Load the model
    loaded_model = load_saved_model(model_save_path)

    buffer_size = 100
    player_preference = 0.7  # preference for player images
    buffer = MultiClassBuffer(class_names, buffer_size,player_preference)    # Load initial images from folders
    load_initial_buffer(buffer)

    try:
        host, port = "127.0.0.1", 25001
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.connect((host, port))
    except Exception as e:
        print(f"Error connecting to socket: {e}")
        exit(1)

    while True:
        received_data = sock.recv(10000)
        if not received_data:
            continue
        else:
            try:
                # Convert received data to PIL Image
                drawn_image = Image.open(io.BytesIO(received_data))
            
                predicted_skill, confidence, prediction_array  = predict_skill(loaded_model, drawn_image, class_names)
                prediction_array = prediction_array*100;
                print(f"Predicted skill: {predicted_skill}")
                print(f"Confidence: {confidence}")

                sock.sendall(str([f"{name:} {value:.2f}%" for name,value in zip(class_names ,prediction_array)]).encode("UTF-8"))# send to unity
                if predicted_skill != "others":
                    if confidence > 0.95:
                        buffer.add(drawn_image, predicted_skill, is_player=True)
                    if confidence > 0.90 and confidence < 0.95:
                        loss = update_model(loaded_model, buffer, drawn_image, predicted_skill)
            except: #if not image then it text
                label_from_user = received_data.decode('utf-8')
                loss = update_model(loaded_model, buffer, drawn_image, label_from_user)
                sock.sendall("Model updated".encode("UTF-8"))
                
            # except Exception as e:
            #     print(f"Error during prediction: {e}")
            #     sock.sendall(f"Error during prediction: {e}".encode("UTF-8"))
                # Don't exit here, continue the loop


    # Don't forget to save the updated model periodically if you're updating it during gameplay
    # loaded_model.save(model_save_path)

