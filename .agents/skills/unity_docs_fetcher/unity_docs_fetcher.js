// Cần cài đặt Node.js để chạy script này
const args = process.argv.slice(2);

function fetchUnityDoc(query) {
    if (!query) {
        return "❌ Lỗi: Vui lòng cung cấp tên Component hoặc Class (Ví dụ: Rigidbody, ConfigurableJoint).";
    }

    // Làm sạch đầu vào (xóa khoảng trắng thừa)
    const cleanQuery = query.trim();

    // Tạo URL trỏ thẳng tới ScriptReference của Unity
    const url = `https://docs.unity3d.com/ScriptReference/${cleanQuery}.html`;

    // Tạo cấu trúc trả về rõ ràng cho LLM đọc
    const response = [
        `🔍 Báo cáo tra cứu tài liệu Unity:`,
        `Tóm tắt: Đây là đường dẫn tài liệu chính thức cho [${cleanQuery}].`,
        `URL: ${url}`,
        `Lưu ý cho Agent: Hãy dùng tool duyệt web (nếu có) để đọc nội dung URL này, hoặc sử dụng kiến thức sẵn có để giải thích.`
    ].join('\n');

    return response;
}

// Thực thi dựa trên tham số dòng lệnh: node unity-docs-fetcher.js ConfigurableJoint
const result = fetchUnityDoc(args[0]);
console.log(result);