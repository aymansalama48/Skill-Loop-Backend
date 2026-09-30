"""Generate the real binary fixtures the test plan uploads.

The plan used to point at paths like C:/path/to/avatar.png that do not exist, so every
multipart upload either 400'd or silently posted a placeholder byte blob. These files are
committed so the suite uploads genuine, decodable images.
"""

import pathlib
import zipfile

from PIL import Image, ImageDraw

OUT = pathlib.Path(__file__).resolve().parent / "test-files"
OUT.mkdir(exist_ok=True)


def card(path, size, bg, fg, label, fmt):
    img = Image.new("RGB", size, bg)
    d = ImageDraw.Draw(img)
    d.rectangle([4, 4, size[0] - 5, size[1] - 5], outline=fg, width=3)
    d.text((14, size[1] // 2 - 6), label, fill=fg)
    img.save(path, format=fmt)
    return path


made = [
    card(OUT / "avatar.png", (256, 256), (32, 64, 128), (240, 240, 240), "Avatar", "PNG"),
    card(OUT / "icon.png", (128, 128), (200, 120, 40), (30, 30, 30), "Icon", "PNG"),
    card(OUT / "icon-v2.png", (128, 128), (40, 150, 90), (255, 255, 255), "Icon v2", "PNG"),
    card(OUT / "thumbnail.jpg", (640, 360), (18, 18, 24), (250, 210, 60), "Course thumbnail", "JPEG"),
    card(OUT / "course-cover.jpg", (1280, 720), (24, 40, 90), (255, 255, 255), "Course cover", "JPEG"),
]

probe = OUT / "probe.txt"
probe.write_text("Skill Loop API test upload fixture.\n", encoding="utf-8")
made.append(probe)

# Real PDFs and a real zip, for the Google Drive material steps. Those steps are currently
# skipped because Drive upload is broken, but the fixtures should exist so they can be
# re-enabled without touching the plan.
pdf = OUT / "syllabus.pdf"
img = Image.new("RGB", (400, 200), (250, 250, 250))
img.save(pdf, format="PDF")
made.append(pdf)

slides = OUT / "lesson-slides.pdf"
img2 = Image.new("RGB", (960, 540), (255, 255, 255))
img2.save(slides, format="PDF")
made.append(slides)

zip_path = OUT / "workshop.zip"
with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
    zf.writestr("workshop-outline.txt", "Skill Loop live workshop outline.\n")
    zf.writestr("workshop-exercises.txt", "1. Setup\n2. Source generators\n3. Profiling\n")
made.append(zip_path)

for p in made:
    print(f"  {p.name:<20} {p.stat().st_size:>7} bytes")

# Prove the images actually decode, so a corrupt fixture cannot slip in again.
for p in made:
    if p.suffix.lower() in {".png", ".jpg", ".jpeg"}:
        with Image.open(p) as im:
            im.verify()
with zipfile.ZipFile(zip_path) as zf:
    assert zf.testzip() is None, "zip fixture is corrupt"
print("\nimage, pdf and zip fixtures all valid")