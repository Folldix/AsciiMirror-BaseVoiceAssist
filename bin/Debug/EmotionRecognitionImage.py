from deepface import DeepFace
import cv2
import argparse
import os
import sys
import tensorflow as tf

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'  # Прибираємо зайві логи TensorFlow
import tensorflow as tf
tf.get_logger().setLevel('ERROR')  # Вимикаємо логи Keras

# Вимкнення зайвих логів TensorFlow
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'  
tf.get_logger().setLevel('ERROR')  

def recognize_emotion_from_image(image_path):
    if not os.path.exists(image_path):
        print("Помилка: файл не знайдено", file=sys.stderr)
        return
    
    img = cv2.imread(image_path)
    if img is None:
        print("Помилка: не вдалося завантажити зображення", file=sys.stderr)
        return
    
    try:
        result = DeepFace.analyze(img, actions=['emotion'], enforce_detection=False)
        emotion = result[0]['dominant_emotion']
        print(emotion)  # Тільки емоція, без зайвого тексту
    except Exception as e:
        print("Помилка розпізнавання:", e, file=sys.stderr)

# Обробка аргументів командного рядка
parser = argparse.ArgumentParser(description='Розпізнавання емоцій')
parser.add_argument('--image', type=str, help="Шлях до зображення")
args = parser.parse_args()

if args.image:
    recognize_emotion_from_image(args.image)
