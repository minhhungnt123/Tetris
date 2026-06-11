import os
import shutil

def organize_scripts(source_dir=".", target_dir="./Assets/_Scripts"):
    """
    Tìm và di chuyển các file .cs từ thư mục nguồn sang thư mục đích.
    """
    # Đảm bảo thư mục đích tồn tại
    if not os.path.exists(target_dir):
        os.makedirs(target_dir)
        print(f"📁 Đã tạo thư mục: {target_dir}")

    moved_files = []
    
    # Quét các file trong thư mục gốc
    for file in os.listdir(source_dir):
        if file.endswith(".cs"):
            source_path = os.path.join(source_dir, file)
            target_path = os.path.join(target_dir, file)
            
            # Nếu file đã tồn tại ở đích, thêm hậu tố để tránh ghi đè mất code
            if os.path.exists(target_path):
                base_name, ext = os.path.splitext(file)
                target_path = os.path.join(target_dir, f"{base_name}_new{ext}")

            shutil.move(source_path, target_path)
            moved_files.append(file)
            print(f"✅ Đã di chuyển: {file} -> {target_path}")

    if not moved_files:
        return "ℹ️ Không tìm thấy file script C# nào cần di chuyển."
    
    return f"🚀 Hoàn tất! Đã sắp xếp {len(moved_files)} file vào {target_dir}."

# Cho phép Agent gọi trực tiếp từ Terminal
if __name__ == "__main__":
    result = organize_scripts()
    print(result)