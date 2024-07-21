import io
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
def create_model(num_classes):
    model = Sequential([
        Conv2D(32, (3, 3), activation='relu', input_shape=(128, 128, 1)),
        MaxPooling2D((2, 2)),
        Conv2D(64, (3, 3), activation='relu'),
        MaxPooling2D((2, 2)),
        Conv2D(64, (3, 3), activation='relu'),
        MaxPooling2D((2, 2)),
        Conv2D(128, (3, 3), activation='relu'),
        GlobalAveragePooling2D(),
        Dense(128, activation='relu'),
        Dropout(0.5),
        # Dense(64, activation='relu'),
        # Dropout(0.2),
        Dense(num_classes, activation='softmax')
    ])
    
    model.compile(optimizer=Adam(learning_rate=0.0001),
                  loss='categorical_crossentropy',
                  metrics=['accuracy'])
    
    return model

# # Load images and labels from directories
# def load_data(base_path):
#     images = []
#     labels = []
#     class_names = []
    
#     for class_index, class_name in enumerate(sorted(os.listdir(base_path))):
#         class_path = os.path.join(base_path, class_name)
#         if os.path.isdir(class_path):
#             class_names.append(class_name)
#             for image_name in os.listdir(class_path):
#                 image_path = os.path.join(class_path, image_name)
#                 try:
#                     image = Image.open(image_path).convert('L')  # Convert to grayscale
#                     image = image.resize((128, 128))
#                     image = np.array(image)
#                     images.append(image)
#                     labels.append(len(class_names) - 1)  # Use the current length of class_names as the label
#                 except Exception as e:
#                     print(f"Error loading image {image_path}: {e}")
    
#     return np.array(images), np.array(labels), class_names

# Train and save the model
def train_and_save_model(images, labels, class_names, model_save_path):
    # Convert labels to categorical
    num_classes = len(class_names)
    labels_categorical = to_categorical(labels, num_classes=num_classes)

    # Split the data into training and validation sets
    X_train, X_val, y_train, y_val = train_test_split(images, labels_categorical, test_size=0.2, random_state=42)

    # Normalize the data
    X_train = X_train.reshape(X_train.shape[0], 128, 128, 1).astype('float32') / 255
    X_val = X_val.reshape(X_val.shape[0], 128, 128, 1).astype('float32') / 255

    # Initialize the model
    model = create_model(num_classes)

    # Data augmentation for training
    datagen = ImageDataGenerator(
        rotation_range=10,
        width_shift_range=0.1,
        height_shift_range=0.1,
        zoom_range=0.1,
        horizontal_flip=False,
        vertical_flip=False
    )

    # Train the model
    batch_size = 32
    epochs = 50

    history = model.fit(
        datagen.flow(X_train, y_train, batch_size=batch_size),
        steps_per_epoch=len(X_train) // batch_size,
        epochs=epochs,
        validation_data=(X_val, y_val)
    )

    # Save the model
    model.save(model_save_path)
    print(f"Model saved to {model_save_path}")

    return model, history

# Function to load the model
def load_saved_model(model_path):
    model = tf.keras.models.load_model(model_path)
    model.compile(optimizer=Adam(learning_rate=0.0001),
                  loss='categorical_crossentropy',
                  metrics=['accuracy'])
    return model

# Function to predict the skill
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
    return prediction
    # predicted_skill_index = np.argmax(prediction)
    # confidence = prediction[predicted_skill_index]
    # predicted_skill_name = class_names[predicted_skill_index]
    # return predicted_skill_name, confidence

@tf.function
def train_step(model, images, labels):
    with tf.GradientTape() as tape:
        predictions = model(images, training=True)
        loss = tf.keras.losses.categorical_crossentropy(labels, predictions)
    gradients = tape.gradient(loss, model.trainable_variables)
    model.optimizer.apply_gradients(zip(gradients, model.trainable_variables))
    return loss

def update_model(model, image, skill_index, class_names):
    # Preprocess the image
    image = image.reshape(1, 128, 128, 1).astype('float32') / 255
    
    # Create a one-hot encoded label
    label = np.zeros((1, len(class_names)))
    label[0, skill_index] = 1
    
    # Convert to tensors
    image_tensor = tf.convert_to_tensor(image)
    label_tensor = tf.convert_to_tensor(label)
    
    # Update the model
    loss = train_step(model, image_tensor, label_tensor)
    return loss

# Main execution
if __name__ == "__main__":
    model_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
    base_path = os.path.join(model_dir,"spell_recognition_model.h5")
    model_save_path = base_path  # Path to save the model

    class_names = np.array(['fireball', 'frostbeam', 'heal', 'meteor', 'others', 'shield', 'summon', 'teleport'])

    # Load the model
    loaded_model = load_saved_model(model_save_path)

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
                
                
                # predicted_skill, confidence = predict_skill(loaded_model, drawn_image, class_names)
                # print(f"Predicted skill: {predicted_skill}")
                # print(f"Confidence: {confidence}")

                prediction_array = predict_skill(loaded_model, drawn_image, class_names)*100
                sock.sendall(str([f"{name:} {value:.2f}%" for name,value in zip(class_names ,prediction_array)]).encode("UTF-8"))
                # if confidence > 0.90 and predicted_skill != "others":
                #     sock.sendall(predicted_skill.encode("UTF-8"))  # send to unity
                #     #update
                # else:
                #     sock.sendall("not recognized".encode("UTF-8"))
            except Exception as e:
                print(f"Error during prediction: {e}")
                sock.sendall(f"Error during prediction: {e}".encode("UTF-8"))
                # Don't exit here, continue the loop


    # Don't forget to save the updated model periodically if you're updating it during gameplay
    # loaded_model.save(model_save_path)

