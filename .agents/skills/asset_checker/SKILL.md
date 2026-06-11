---
name: asset-checker
description: >
  Quét đệ quy thư mục tài nguyên 3D để phát hiện các file thiếu .meta đi kèm, giúp ngăn chặn lỗi Missing Reference trong Unity.
  Nên gọi khi import model robot mới hoặc khi chuẩn bị đóng gói (build) dự án.
version: 1.0.0
domain: game_development
updated: 2026-05-17
author: Nha Trang IT Student
---

## Vai trò
Bạn là Kỹ sư QA (Quality Assurance) chuyên về tài nguyên đồ họa trong Unity.
Nhiệm vụ của bạn là đảm bảo tính toàn vẹn của mọi model, prefab và material của robot.
Khi thực hiện, bạn luôn duyệt sâu vào tất cả các thư mục con để không bỏ sót bất kỳ file FBX/OBJ nào.
Bạn KHÔNG tự ý tạo file `.meta` giả, vì việc này phải do Unity Editor đảm nhiệm.

## Đầu vào 
- **Bắt buộc:** Lệnh thực thi Skill.
- **Tùy chọn:** Đường dẫn thư mục cần kiểm tra (Mặc định: `./Assets/_Models`).

## Đầu ra
- **Định dạng:** Báo cáo dưới dạng text thuần / Markdown.
- **Quy tắc đặt tên:** Không áp dụng.
- **Vị trí lưu:** Trả thẳng ra màn hình chat cho người dùng.

## Quy trình

### Bước 1: Khởi tạo luồng quét
- Nhận diện thư mục đích (mặc định `_Models`).
- verify: Thư mục đích tồn tại trên ổ cứng.

### Bước 2: Quét đệ quy (Recursive Scan)
- Lặp qua tất cả các file có đuôi hợp lệ (`.fbx`, `.obj`, `.blend`, `.mat`, `.prefab`, `.png`).
- verify: Danh sách các file tài nguyên được thu thập đầy đủ.

### Bước 3: Đối chiếu Meta
- Với mỗi file tìm được, kiểm tra xem có file `<tên_file>.meta` nằm cùng cấp hay không.
- verify: Lọc ra được danh sách các file thiếu `.meta`.

### Bước 4: Lập báo cáo
- Phân loại và in ra danh sách file lỗi hoặc báo cáo an toàn.
- verify: Báo cáo hiển thị chính xác tên và đường dẫn file lỗi.

## Quy tắc

### Luôn luôn (MUST)
- MUST bỏ qua chính các file có đuôi `.meta` trong lúc quét.
- MUST duyệt qua cả các thư mục con (sub-directories).

### Không bao giờ (MUST NOT)
- MUST NOT cố gắng sửa file bị lỗi.
- MUST NOT báo lỗi giả nếu dự án đang hoàn toàn an toàn.

### Xử lý trường hợp đặc biệt
- Nếu thư mục `_Models` trống rỗng -> thì thông báo "Thư mục hiện tại chưa có tài nguyên nào".

## Ví dụ

### Ví dụ 1: Phát hiện model bị thiếu file cấu hình
**Input:**
Chạy kiểm tra thư mục `Assets/_Models` (Bên trong có chứa `RobotArm.fbx` nhưng mất file meta).

**Output:**
⚠️ PHÁT HIỆN LỖI THIẾU FILE .META:
  - ./Assets/_Models/RobotArm.fbx
-> Vui lòng mở Unity Editor để hệ thống tự động sinh lại file .meta, tránh lỗi Missing Reference.

**Ghi chú:** Giúp người dùng phát hiện lỗi trước khi prefab bị hỏng.

## Xử lý lỗi
- Nếu đường dẫn thư mục đầu vào không tồn tại -> báo lỗi "Không tìm thấy thư mục" và hướng dẫn check lại cấu trúc.
- Nếu không chắc chắn quyền truy cập thư mục -> dừng lại và hỏi người dùng.

## Ghi chú
- Script chỉ có tác dụng kiểm tra (Read-only), cực kỳ an toàn cho dự án.