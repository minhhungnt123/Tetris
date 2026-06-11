import os
import shutil

def move_to_scripts(file_name):
    """Di chuyển file script mới tạo vào thư mục Assets/_Scripts."""
    source_path = f"./{file_name}"
    dest_dir = "./Assets/_Scripts"
    
    if not os.path.exists(dest_dir):
        os.makedirs(dest_dir)
        
    if os.path.exists(source_path):
        shutil.move(source_path, os.path.join(dest_dir, file_name))
        return f"✅ Đã di chuyển {file_name} vào Assets/_Scripts."
    return "❌ Không tìm thấy file nguồn."