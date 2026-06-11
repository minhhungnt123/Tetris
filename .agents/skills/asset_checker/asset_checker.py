import os

def check_assets_integrity(target_folder="./Assets/_Models"):
    """
    Quét đệ quy thư mục để tìm các file 3D/Material thiếu file .meta đi kèm.
    """
    if not os.path.exists(target_folder):
        return f"❌ Lỗi: Không tìm thấy thư mục {target_folder}."

    # Các định dạng tài nguyên phổ biến trong đồ án Unity
    target_extensions = ('.fbx', '.obj', '.blend', '.mat', '.prefab', '.png', '.jpg')
    missing_meta_files = []

    # os.walk giúp quét cả các thư mục con (ví dụ: _Models/RobotArms)
    for root, _, files in os.walk(target_folder):
        for file in files:
            # Bỏ qua chính các file .meta
            if file.endswith('.meta'):
                continue
                
            if file.lower().endswith(target_extensions):
                file_path = os.path.join(root, file)
                meta_path = f"{file_path}.meta"
                
                if not os.path.exists(meta_path):
                    missing_meta_files.append(file_path)

    if missing_meta_files:
        report = "⚠️ PHÁT HIỆN LỖI THIẾU FILE .META:\n"
        for missing in missing_meta_files:
            report += f"  - {missing}\n"
        report += "-> Vui lòng mở Unity Editor để hệ thống tự động sinh lại file .meta, tránh lỗi Missing Reference."
        return report

    return "✅ Tuyệt vời! Tất cả model và tài nguyên đều có file .meta đầy đủ và an toàn."

# Cho phép Agent gọi trực tiếp
if __name__ == "__main__":
    result = check_assets_integrity()
    print(result)