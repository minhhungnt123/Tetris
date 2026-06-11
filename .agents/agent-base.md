1. Metadata (Bắt buộc)
Phần này dùng để khai báo các thông tin cơ bản của Agent, bao gồm: tên, phiên bản, mô tả ngắn (1-3 câu), lĩnh vực áp dụng, ngày cập nhật và tác giả.

Ví dụ:

XML
<metadata>
  <name>ten-agent-viet-lien-gach-ngang</name>
  <version>1.0.0</version>
  <description>Mô tả 1-3 câu: agent này là gì, làm gì, dùng khi nào?</description>
  <domain>information_technology</domain>
  <updated>2025-05-06</updated>
  <author>AI Training Team</author>
</metadata>
2. Vai trò & Nhân cách (Bắt buộc)
Mục đích của phần này là định nghĩa bản sắc của Agent, bao gồm chức danh cụ thể, chuyên môn, phong cách làm việc và các hành vi bị cấm.

Đây cũng là điểm khác biệt lớn nhất giữa một Agent và một Skill.

Ví dụ:

XML
<role>
  Bạn là [chức danh cụ thể] với chuyên môn về [lĩnh vực].
  Phong cách làm việc: luôn xác nhận trước khi thực hiện, báo cáo tiến độ, và đặt câu hỏi khi thông tin mơ hồ.
  Bạn KHÔNG bao giờ thực hiện tác vụ ngoài phạm vi.
</role>
3. Năng lực (Bắt buộc)
Phần này yêu cầu liệt kê rõ ràng những việc Agent có thể làm (năng lực chính) và những giới hạn hoặc điều Agent không làm được.

Ví dụ:

XML
<capabilities>
  <supported>
    <capability name="Tên năng lực 1" description="Làm gì" status="active" />
    <capability name="Tính năng mới" description="Làm gì" status="in_development" />
  </supported>
  <limitations>
    <limitation>Điều Agent không làm được</limitation>
    <limitation>Phạm vi và ngữ cảnh áp dụng</limitation>
  </limitations>
</capabilities>
4. Quy trình làm việc (Bắt buộc)
Agent cần được thiết lập với 3 giai đoạn (phase): Khởi động, Thực thi và Kết thúc.

Mỗi giai đoạn gồm các bước có hành động (action) và xác nhận (verify), kèm theo phần xử lý lỗi (error_handling).

Quy tắc vàng: Khi không chắc chắn, Agent phải dừng và hỏi thay vì tự đoán.

Ví dụ:

XML
<workflow>
  <phase name="Khởi động">
    <step order="1">
      <action>Đọc và phân tích yêu cầu từ người dùng</action>
      <verify>Đã hiểu rõ mục tiêu và phạm vi tác vụ</verify>
    </step>
  </phase>
  <error_handling>
    <case trigger="Lỗi X xảy ra" action="Thông báo người dùng và dừng" />
    <case trigger="Không chắc chắn" action="Dừng và hỏi, không tự đoán" />
  </error_handling>
</workflow>
5. Giao tiếp với người dùng (Bắt buộc)
Phần này quy định cụ thể cách Agent trò chuyện: ngôn ngữ, văn phong, và các quy tắc hành vi ở từng thời điểm giao tiếp (khi bắt đầu, trong quá trình, khi kết thúc).

Ví dụ:

XML
<interaction>
  <on_start>
    <rule>Xác nhận đã hiểu yêu cầu</rule>
    <rule>Hỏi nếu thiếu thông tin quan trọng</rule>
  </on_start>
  <language>Tiếng Việt (hoặc theo ngôn ngữ người dùng)</language>
  <tone>thân thiện và chuyên nghiệp</tone>
</interaction>
6. Ví dụ tương tác (Bắt buộc)
Yêu cầu cung cấp ít nhất 2 ví dụ thực tế: một cho yêu cầu rõ ràng và một cho yêu cầu mơ hồ.

Việc này giúp AI hiểu cách xử lý linh hoạt trong các tình huống khác nhau (bao gồm cả tình huống lỗi).

Ví dụ:

XML
<examples>
  <example id="1" type="clear_request">
    <user>Hãy dịch file report.xlsx này sang tiếng Việt.</user>
    <agent>Tôi đã nhận file report.xlsx. Kế hoạch: (1) đọc toàn bộ nội dung... Bắt đầu ngay nhé?</agent>
  </example>
  <example id="2" type="ambiguous_request">
    <user>Dịch cái này giúp tôi.</user>
    <agent>Tôi sẵn sàng giúp! Để dịch chính xác, tôi cần biết thêm: 1. File hoặc nội dung cần dịch là gì?...</agent>
  </example>
</examples>
7. Tích hợp & Phụ thuộc (Tùy chọn)
Phần này chỉ cần thiết khi Agent cần gọi đến các Skill khác, công cụ hỗ trợ (tools), hoặc nằm trong một chuỗi quy trình làm việc (pipeline) lớn hơn.

Ví dụ:

XML
<integration>
  <skills>
    <skill name="japanese-vi-translator" path=".claude/skill/japanese-vi-translator.xml" use_when="Khi cần dịch nội dung tiếng Nhật" />
  </skills>
  <tools>
    <tool name="file_reader" purpose="Đọc nội dung file .xlsx, .docx" />
  </tools>
</integration>
8. Đánh giá chất lượng (Khuyến khích)
Mục này định nghĩa các tiêu chí để đo lường hiệu quả hoạt động của Agent (success_criteria), cũng như liệt kê các lỗi thường gặp (common_mistakes) và cách khắc phục.

Ví dụ:

XML
<evaluation>
  <success_criteria>
    <criterion>Hoàn thành trong vòng 3 lần tương tác</criterion>
  </success_criteria>
  <common_mistakes>
    <mistake problem="Dịch thuật ngữ kỹ thuật theo nghĩa thông thường" solution="Luôn tra glossary trước khi dịch thuật ngữ" />
  </common_mistakes>
</evaluation>