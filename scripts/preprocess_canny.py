import argparse
from pathlib import Path

import cv2
import numpy as np


def read_image(path: Path):
    data = np.fromfile(str(path), dtype=np.uint8)
    image = cv2.imdecode(data, cv2.IMREAD_COLOR)
    return image


def save_image(path: Path, image):
    path.parent.mkdir(parents=True, exist_ok=True)

    ext = path.suffix.lower()
    if ext not in [".png", ".jpg", ".jpeg", ".bmp"]:
        ext = ".png"

    success, encoded = cv2.imencode(ext, image)
    if not success:
        raise RuntimeError("이미지 인코딩에 실패했습니다.")

    encoded.tofile(str(path))


def main():
    parser = argparse.ArgumentParser(description="DonkeyCar Canny Edge Preview")
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--low", type=int, default=80)
    parser.add_argument("--high", type=int, default=160)
    parser.add_argument("--blur", type=int, default=5)

    args = parser.parse_args()

    input_path = Path(args.input)
    output_path = Path(args.output)

    if not input_path.exists():
        raise FileNotFoundError(f"입력 이미지가 존재하지 않습니다: {input_path}")

    image = read_image(input_path)

    if image is None:
        raise RuntimeError(f"이미지를 읽지 못했습니다: {input_path}")

    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    blur_size = args.blur
    if blur_size < 1:
        blur_size = 1
    if blur_size % 2 == 0:
        blur_size += 1

    blurred = cv2.GaussianBlur(gray, (blur_size, blur_size), 0)
    edges = cv2.Canny(blurred, args.low, args.high)

    save_image(output_path, edges)

    print(f"Canny preview saved: {output_path}")


if __name__ == "__main__":
    main()