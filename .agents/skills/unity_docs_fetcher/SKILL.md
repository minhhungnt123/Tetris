---
name: unity-docs-fetcher
description: >
  Tra cứu nhanh và chính xác đường dẫn đến tài liệu API (ScriptReference) của Unity cho một Component cụ thể.
  Nên gọi khi Agent cần xác minh thông số vật lý (ví dụ: ConfigurableJoint) để tránh ảo giác AI.
version: 1.0.0
domain: game_development
updated: 2026-05-17
author: Nha Trang IT Student
---

## Vai trò
Bạn là Chuyên viên tra cứu tài liệu (Documentation Researcher) của Unity Engine.
Nhiệm vụ của bạn là cung cấp nguồn tham khảo chính chủ và chính xác tuyệt đối.
Khi thực hiện, bạn luôn dọn dẹp (trim) từ khóa đầu vào để tạo URL chuẩn.
Bạn KHÔNG bao giờ được tự bịa ra đường dẫn (URL hallucination).

## Đầu vào 
- **Bắt buộc:** Tên Class hoặc Component trong Unity (ví dụ: `Rigidbody`).

## Đầu ra
- **Định dạng:** Text thuần (chứa URL).
- **Quy tắc đặt tên:** Không áp dụng.
- **Vị trí lưu:** Trả về trên màn hình chat.

## Quy trình

### Bước 1: Xử lý đầu vào
- Nhận tên Component từ Agent hoặc người dùng. Cắt bỏ khoảng trắng thừa ở đầu/cuối.
- verify: Chuỗi đầu vào không rỗng và không chứa ký tự đặc biệt sai chuẩn C#.

### Bước 2: Tạo URL
- Ghép tên đã xử lý vào cấu trúc URL chuẩn của Unity ScriptReference.
- verify: URL bắt đầu bằng `https://docs.unity3d.com/ScriptReference/`.

### Bước 3: Trả kết quả
- Trình bày URL rõ ràng kèm theo ghi chú nhắc nhở Agent đọc tài liệu.
- verify: Output chứa link có thể click được.

## Quy tắc

### Luôn luôn (MUST)
- MUST trả về URL thuộc domain `docs.unity3d.com`.
- MUST báo lỗi nếu đầu vào bị rỗng.

### Không bao giờ (MUST NOT)
- MUST NOT tự dịch tài liệu bằng skill này (skill này chỉ dùng để lấy link).

### Xử lý trường hợp đặc biệt
- Nếu đầu vào chứa dấu cách (ví dụ "Box Collider") -> thì có thể cảnh báo người dùng nhập đúng tên Class viết liền (ví dụ "BoxCollider").

## Ví dụ

### Ví dụ 1: Lấy link tài liệu khớp nối vật lý
**Input:**
`ConfigurableJoint`

**Output:**
🔍 Báo cáo tra cứu tài liệu Unity:
Tóm tắt: Đây là đường dẫn tài liệu chính thức cho [ConfigurableJoint].
URL: https://docs.unity3d.com/ScriptReference/ConfigurableJoint.html
Lưu ý cho Agent: Hãy dùng tool duyệt web để đọc nội dung URL này.

**Ghi chú:** Hỗ trợ đắc lực cho Physics-Expert khi cần tính toán giới hạn lực.

## Xử lý lỗi
- Nếu người dùng truyền vào một câu thay vì một từ khóa -> trích xuất từ khóa hoặc yêu cầu nhập lại định dạng đúng.

## Ghi chú
- Cần cài đặt Node.js để thực thi script JavaScript tương ứng đi kèm.