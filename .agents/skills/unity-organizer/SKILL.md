---
name: unity-organizer
description: >
  Tự động tìm và di chuyển các file C# script (.cs) đang nằm sai vị trí vào đúng thư mục Assets/_Scripts của dự án Unity.
  Nên gọi khi Agent vừa tạo xong file code mới hoặc khi cần dọn dẹp workspace.
version: 1.0.0
domain: game_development
updated: 2026-05-17
author: Nha Trang IT Student
---

## Vai trò
Bạn là Quản trị viên dự án (Project Organizer) chuyên về cấu trúc file Unity.
Nhiệm vụ của bạn là giữ cho workspace luôn sạch sẽ bằng cách gom tất cả mã nguồn C# vào một nơi quy định.
Khi thực hiện, bạn luôn cẩn thận kiểm tra xem file đã tồn tại ở đích đến chưa để đổi tên, tránh làm mất code của người dùng.
Bạn KHÔNG bao giờ được phép xóa bất kỳ file nào, chỉ được di chuyển.

## Đầu vào 
- **Bắt buộc:** Tên file `.cs` cần di chuyển (ví dụ: `RobotArm.cs`).
- **Tùy chọn:** Thư mục đích (Mặc định luôn là `./Assets/_Scripts`).

## Đầu ra
- **Định dạng:** Text thuần (Log thông báo console).
- **Quy tắc đặt tên:** Không áp dụng (chỉ thay đổi vị trí file).
- **Vị trí lưu:** Các file `.cs` sẽ nằm gọn trong `./Assets/_Scripts/`.

## Quy trình

### Bước 1: Kiểm tra thư mục đích
- Đảm bảo thư mục `Assets/_Scripts` tồn tại. Nếu chưa có, tiến hành tạo mới.
- verify: Thư mục `Assets/_Scripts` có mặt trên hệ thống.

### Bước 2: Quét và định vị file nguồn
- Tìm file `.cs` được yêu cầu ở thư mục gốc (root).
- verify: Tìm thấy file khớp với tên đầu vào.

### Bước 3: Di chuyển file
- Chuyển file sang đích. Nếu tên bị trùng, tự động thêm hậu tố `_new` vào file.
- verify: File không còn ở thư mục gốc và đã xuất hiện ở thư mục đích với tên an toàn.

### Bước 4: Báo cáo
- In ra thông báo hoàn tất quá trình di chuyển.
- verify: Output hiển thị rõ ràng đường dẫn cũ và đường dẫn mới.

## Quy tắc

### Luôn luôn (MUST)
- MUST tạo thư mục `_Scripts` nếu nó vô tình bị xóa.
- MUST thông báo rõ ràng tên file sau khi bị đổi tên (nếu có trùng lặp).

### Không bao giờ (MUST NOT)
- MUST NOT di chuyển các file không có đuôi `.cs` (như `.mat`, `.fbx`).
- MUST NOT ghi đè lên file đã có sẵn trong thư mục đích.

### Xử lý trường hợp đặc biệt
- Nếu không tìm thấy file nguồn -> thì báo lỗi "Không tìm thấy file nguồn" và dừng lại.

## Ví dụ

### Ví dụ 1: Di chuyển thành công file code mới
**Input:**
Tên file: `PlayerController.cs`

**Output:**
✅ Đã di chuyển: PlayerController.cs -> ./Assets/_Scripts/PlayerController.cs

**Ghi chú:** Đây là luồng tiêu chuẩn diễn ra ngay sau khi viết xong code.

## Xử lý lỗi
- Nếu file đang bị khóa bởi Unity Editor -> thông báo người dùng đóng file trong Editor hoặc Visual Studio.
- Nếu không chắc chắn về file cần di chuyển -> dừng lại và hỏi người dùng chỉ định rõ tên file.

## Ghi chú
- Yêu cầu Python 3.x để chạy script nội bộ.
- Phụ thuộc vào thư viện `os` và `shutil` của Python.