---
name: docs-generator
description: Trích xuất thông tin từ codebase dự án Unity và tạo nội dung tự động cho báo cáo đồ án tốt nghiệp.
---

# docs-generator

## Mục đích
Skill này được thiết kế để tự động hóa việc tạo tài liệu (docs) và viết báo cáo cho đồ án tốt nghiệp dựa trên thông tin thực tế từ codebase.

## Cách sử dụng
- Agent (đặc biệt là `ThesisReporterAgent`) sẽ gọi skill này khi cần trích xuất thông tin về cấu trúc thư mục, chức năng của các script C#, hoặc cần viết tài liệu kỹ thuật cho dự án.
- Skill này giúp tóm tắt chức năng của các class/module và định dạng chuẩn Markdown để dễ dàng đưa vào báo cáo Word/PDF.

## Quy trình thực hiện
1. **Quét và Phân tích**: Quét thư mục `Assets/_Scripts` và các thư mục liên quan để phân loại các thành phần kiến trúc (Controllers, Managers, Modules).
2. **Trích xuất Context**: Lấy thông tin từ các docstrings, summary, và logic của các class/method quan trọng, phối hợp với `CodeBaseAgent`.
3. **Định dạng & Xuất bản**: Khởi tạo và cập nhật các file tài liệu định dạng Markdown trong thư mục `Assets/_Report` (ví dụ: `Draft_Chapter_X.md`).

## Tiêu chuẩn chất lượng
- Luôn giữ nguyên ngữ cảnh kỹ thuật khi diễn đạt bằng tiếng Việt.
- Tuân thủ văn phong của một báo cáo học thuật chuyên nghiệp (khách quan, súc tích).
- Nêu bật được việc áp dụng các nguyên lý Clean Code và SOLID vào dự án.
