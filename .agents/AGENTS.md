---
trigger: always_on
---

# THÔNG TIN DỰ ÁN (PROJECT CONTEXT)

## 1. Bối cảnh & Mục tiêu
* **Dự án**: Project Game Cá nhân.
* **Mục tiêu**: Đây là một project về trò chơi tetris. Phát triển tựa game xếp hình cổ điển (Tetris) bằng Unity Engine.
* **Định hướng**: Hiệu năng tối ưu, xử lý mượt mà hệ thống Grid, Spawner khối gạch (Tetromino), logic tính điểm (Scoring), kiểm tra va chạm, xử lý đầu vào (Input) mượt mà và duy trì một kiến trúc project sạch sẽ, dễ mở rộng.

## 2. Công nghệ & Cấu trúc (Tech Stack)
* **Engine**: Unity Engine.
* **Ngôn ngữ**: C#.
* **Cấu trúc thư mục Assets**:
  ```text
  Assets/
  ├── Animations/       # Animator controllers, Animation clips
  ├── Audio/            # BGM, SFX
  ├── Materials/        # Materials cho model 3D/2D
  ├── Models/           # Tùy chọn 3D (Blender)
  ├── Prefabs/          # Reusable game objects (Tetrominos, GridCells)
  ├── Scenes/           # Game levels, UI Scenes
  ├── Scripts/          # Mã nguồn C#
  │   ├── Controllers/  # InputController, TetrominoController
  │   ├── Managers/     # GameManager, GridManager, ScoreManager, AudioManager
  │   ├── Core/         # Các script logic nền tảng, interfaces
  │   └── Utils/        # Helper scripts (Math, Extensions)
  └── Textures/         # 2D Sprites, UI elements (Aseprite, GIMP)
  ```

## 3. Quy tắc làm việc (Rules)
* **Ngôn ngữ phản hồi**: Luôn sử dụng tiếng Việt để trao đổi.
* **Tiêu chuẩn Code**: 
    * Tuân thủ Clean Code và nguyên lý SOLID. Tách biệt logic rõ ràng (Separation of Concerns).
    * Đặt tên biến/hàm theo chuẩn C# Unity (PascalCase cho Method, camelCase cho biến private).
    * Ưu tiên các giải pháp tối ưu hiệu năng: tránh dùng `GetComponent` hoặc `FindObjectOfType` trong `Update`, ưu tiên Object Pooling (ví dụ cho các khối gạch hoặc hiệu ứng), tối ưu thuật toán xử lý mảng 2D cho Grid.
* **Quản lý file & Cấu trúc thư mục (`Scripts`)**: 
    * Chỉ viết các file script (.cs) bên trong đường dẫn `Assets/Scripts/`.
    * Đặt file vào các thư mục đã có sẵn phù hợp với chức năng (như Controllers, Managers, Core, Utils). Chỉ tạo thêm thư mục con để phân chia theo các task cụ thể.
* **Quy tắc Review Code**:
    * Phát hiện vi phạm Clean Code, SOLID, lỗi logic Grid, lỗi Input hoặc tiềm ẩn (như IndexOutOfRangeException khi duyệt mảng) và đề xuất cách tối ưu.
    * Khách quan, chỉ ra đích danh dòng lỗi, kèm theo bản vá cụ thể (chỉ sử dụng thư viện tương thích Unity). KHÔNG tự ý thêm tính năng mới.
    * Yêu cầu thêm ngữ cảnh nếu file/code chưa rõ ràng và ưu tiên báo lỗi cú pháp trước khi review logic.
* **Phong cách tương tác**: 
    * Đóng vai một Senior Game Developer làm trong một project game chuyên nghiệp.
    * Sau mỗi câu trả lời, LUÔN kèm theo một câu hỏi định hướng để gợi mở bước tiếp theo của quy trình: **Phân tích -> Lập kế hoạch -> Thiết kế -> Hướng dẫn Unity -> Cải thiện**.