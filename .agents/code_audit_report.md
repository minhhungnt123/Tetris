# BÁO CÁO PHÂN TÍCH MÃ NGUỒN (CODE AUDIT REPORT)
## Đồ án tốt nghiệp: Robot Assembly & Physics Simulator

Báo cáo này phân tích toàn bộ cấu trúc mã nguồn trong thư mục `Assets/_Scripts` dưới góc nhìn kiến trúc phần mềm (SOLID), hiệu năng Unity (Performance), mô phỏng vật lý (Physics) và trải nghiệm giao diện (UI/UX).

---

## 1. Kiến Trúc Hệ Thống & Nguyên Lý SOLID

### Vấn đề 1.1: Rò rỉ bộ nhớ (Memory Leak) và Lỗi Runtime trong `PickupNotificationUI.cs`
* **Vị trí**: [PickupNotificationUI.cs:76-83](file:///d:/Robotics/Assets/_Scripts/_UI/PickupNotificationUI.cs#L76-L83)
* **Phân tích**: 
  Trong hàm `OnEnable`, script đăng ký sự kiện `OnItemAdded`. Nếu `InventoryManager.Instance` là `null`, nó sẽ kích hoạt Coroutine trì hoãn `RegisterEventsDeferredCoroutine()`.
  Tuy nhiên, nếu GameObject hoặc Component bị vô hiệu hóa (`OnDisable`) **trước khi** `InventoryManager.Instance` được khởi tạo:
  1. `OnDisable` chạy trước và thực hiện hủy đăng ký (nhưng thực tế lúc này chưa đăng ký được vì `Instance` đang null).
  2. Coroutine vẫn tiếp tục chạy trong nền (nếu GameObject chưa bị hủy). Khi `InventoryManager.Instance` khác null, Coroutine sẽ chạy dòng tiếp theo và thực hiện đăng ký: `InventoryManager.Instance.OnItemAdded += HandleItemAdded;`
  3. Sự kiện này không bao giờ được hủy đăng ký vì `OnDisable` đã chạy từ trước.
* **Hậu quả**: Rò rỉ bộ nhớ nghiêm trọng. Khi Scene thay đổi hoặc UI bị hủy, `InventoryManager` vẫn giữ tham chiếu đến UI cũ đã bị phá hủy, dẫn đến lỗi `MissingReferenceException` khi nhặt vật phẩm mới.

### Vấn đề 1.2: Vi phạm nguyên lý Đơn nhiệm (SRP) và Phân tán Input
* **Vị trí**: [AssemblyManager.cs:22-27](file:///d:/Robotics/Assets/_Scripts/_Managers/AssemblyManager.cs#L22-L27) và [CameraController.cs:44-57](file:///d:/Robotics/Assets/_Scripts/_Controllers/CameraController.cs#L44-L57)
* **Phân tích**: 
  Mặc dù dự án đã có một `InputManager.cs` tập trung đóng vai trò Interface cho New Input System, nhưng các class `AssemblyManager` và `CameraController` lại tự ý truy cập trực tiếp các phím thông qua các biến Input System toàn cục (`Mouse.current`, `Keyboard.current`).
* **Hậu quả**: Vi phạm nguyên tắc **Single Responsibility Principle (SRP)**. Khi cần thay đổi nút bấm (remap), đổi cơ chế điều khiển (ví dụ: hỗ trợ Gamepad, Mobile Touch) hoặc tạm thời khóa tất cả các input của Player (khi đang mở Computer/UI thiết kế), bạn sẽ phải sửa code ở rất nhiều nơi, dễ bỏ sót gây lỗi xung đột input.

### Vấn đề 1.3: Quản lý tham chiếu (Cache Reference) thiếu an toàn khi Hủy đối tượng
* **Vị trí**: [InventoryUI.cs:73-87](file:///d:/Robotics/Assets/_Scripts/_Controllers/Inventory/InventoryUI.cs#L73-L87)
* **Phân tích**:
  Trong hàm `OnDestroy()`, class tìm kiếm `InventoryManager` bằng cách gọi `FindFirstObjectByType<InventoryManager>()`.
  Khi tắt game hoặc thoát Scene, thứ tự hủy các Object là ngẫu nhiên. Nếu `InventoryManager` bị hủy trước `InventoryUI`, hàm tìm kiếm này sẽ trả về `null` hoặc gây ra cảnh báo Unity, đồng thời việc gọi liên tục `FindFirstObjectByType` là không tối ưu.
* **Hậu quả**: Code rườm rà và thiếu an toàn. Nên cache trực tiếp biến `private InventoryManager inventoryManager;` trong hàm `Start`/`Awake` và sử dụng lại biến này ở `OnDestroy`.

---

## 2. Hệ Thống Vật Lý (Physics & Robot Assembly Logic)

### Vấn đề 2.1: Giật lag vật lý (Physics Explode) do di chuyển Transform trực tiếp
* **Vị trí**: [AssemblyManager.cs:66](file:///d:/Robotics/Assets/_Scripts/_Managers/AssemblyManager.cs#L66)
* **Phân tích**:
  Khi kéo thả các bộ phận robot, `AssemblyManager` đặt `selectedModule.transform.position = Vector3.Lerp(...)`. 
  Mặc dù Rigidbody của bộ phận được chọn đã được đưa về `isKinematic = true` (dòng 50), nhưng nếu bộ phận đó đang kết nối với các bộ phận con khác thông qua `Joint` (ví dụ: Khung gầm kết nối với các Bánh xe `WheelModule` vẫn đang bật vật lý `isKinematic = false` để giữ trọng lực):
  1. Việc dịch chuyển trực tiếp `transform.position` của cha sẽ ép buộc vị trí của nó thay đổi ngay lập tức ở frame đó mà không qua hệ thống tính toán vật lý (Teleportation).
  2. Các Joint của các bộ phận con sẽ bị kéo căng đột ngột với một lực cực đại để bám theo cha.
* **Hậu quả**: Gây ra hiện tượng giật lắc màn hình dữ dội (jittering) hoặc khiến robot **bay tung tóe (Physics Explosion)** do tích tụ lực đàn hồi cực lớn trong các khớp Joint ngay khi người chơi nhả chuột.

### Vấn đề 2.2: Lỗi cố định vào thế giới (World Joint Lock)
* **Vị trí**: [RobotModule.cs:37](file:///d:/Robotics/Assets/_Scripts/_Modules/RobotModule.cs#L37)
* **Phân tích**:
  Dòng code: `connectionJoint.connectedBody = parentSocket.GetComponentInParent<Rigidbody>();`
  Nếu `parentSocket` hoặc GameObject cha của nó không chứa thành phần `Rigidbody` (chẳng hạn như điểm cắm nằm trên một Object con trống không được thiết lập Rigidbody chuẩn ở root), `connectedBody` sẽ nhận giá trị `null`.
* **Hậu quả**: Trong Unity Physics, khi một Joint có `connectedBody = null`, nó sẽ tự động **khóa chặt cố định vào không gian thế giới (World Space)** tại tọa độ đó. Kết quả là robot của bạn sẽ bị đóng băng tại chỗ, không thể di chuyển hoặc rơi tự do được nữa.

---

## 3. Giao Diện & Trải Nghiệm Người Dùng (UI/UX)

### Vấn đề 3.1: Hạt sạn UX khi Drag & Drop vật phẩm trong Kho Đồ
* **Vị trí**: [InventorySlotUI.cs:125-134](file:///d:/Robotics/Assets/_Scripts/_Controllers/Inventory/InventorySlotUI.cs#L125-L134)
* **Phân tích**:
  Khi người chơi bắt đầu kéo vật phẩm (`OnBeginDrag`), script chỉ thực hiện chuyển đổi Parent của `iconImage.transform` (hình ảnh icon) để hiển thị bay theo chuột. 
  Tuy nhiên, `quantityText` (chữ hiển thị số lượng, ví dụ: "x99") lại thuộc một GameObject con độc lập và không được di chuyển theo.
* **Hậu quả**: Khi kéo vật phẩm đi, **chỉ có hình ảnh icon bay theo chuột, còn số lượng vật phẩm vẫn đứng yên tại chỗ ở ô cũ**. Điều này tạo cảm giác giao diện chắp vá, thiếu chuyên nghiệp và gây bối rối cho người chơi.

### Vấn đề 3.2: Lỗi lệch vị trí chuột khi kéo thả trên Canvas nâng cao (Camera Space)
* **Vị trí**: [InventorySlotUI.cs:151](file:///d:/Robotics/Assets/_Scripts/_Controllers/Inventory/InventorySlotUI.cs#L151)
* **Phân tích**:
  Dòng code: `iconImage.transform.position = eventData.position;`
  `eventData.position` trả về tọa độ màn hình dạng pixel (Screen Space). Việc gán trực tiếp tọa độ này vào `position` của UI chỉ đúng khi Canvas của bạn được thiết lập ở chế độ `Screen Space - Overlay`.
  Nếu đồ án nâng cấp lên giao diện cao cấp có Post-processing hoặc 3D UI, Canvas bắt buộc phải chuyển sang chế độ `Screen Space - Camera` hoặc `World Space`. Lúc này, hệ tọa độ pixel và hệ tọa độ thế giới của Canvas hoàn toàn khác nhau.
* **Hậu quả**: Icon vật phẩm khi kéo sẽ **bay lệch rất xa khỏi vị trí con trỏ chuột**, thậm chí biến mất khỏi màn hình, khiến tính năng kéo thả bị lỗi hoàn toàn.

---

## 4. Hiệu Năng & Tối Ưu Hóa (Performance)

### Vấn đề 4.1: Thao tác tìm kiếm cực kỳ đắt đỏ trong Update (`Camera.main`)
* **Vị trí**: [ShiningBillboard.cs:44-49](file:///d:/Robotics/Assets/_Scripts/_Controllers/Inventory/ShiningBillboard.cs#L44-L49)
* **Phân tích**:
  Gọi `Camera.main` trong hàm `Update()` ở mỗi frame. Trong Unity, việc truy cập `Camera.main` thực chất là engine sẽ thực hiện một câu lệnh tìm kiếm tương đương `GameObject.FindWithTag("MainCamera")` dưới tầng C++.
* **Hậu quả**: Nếu trên bản đồ có hàng chục hạt giống, vật phẩm nhặt được (Loot Items) cùng gắn script `ShiningBillboard` này, CPU sẽ bị quá tải do phải thực hiện hàng chục phép tìm kiếm GameObject mỗi khung hình, gây sụt giảm khung hình nghiêm trọng (FPS Drop) trên các máy cấu hình trung bình.

### Vấn đề 4.2: Tách rác bộ nhớ (GC.Alloc) do tìm kiếm động `FindObjectsByType`
* **Vị trí**: [AssemblyManager.cs:96](file:///d:/Robotics/Assets/_Scripts/_Managers/AssemblyManager.cs#L96)
* **Phân tích**:
  Mỗi khi người chơi thả chuột để lắp ráp robot, hàm `FindClosestSocket` lại thực hiện quét toàn bộ Scene bằng lệnh: `FindObjectsByType<RobotModule>(FindObjectsSortMode.None)`.
* **Hậu quả**: Mặc dù chỉ gọi khi nhả chuột, nhưng việc quét toàn bộ đối tượng trong Scene sẽ tạo ra mảng mới trong bộ nhớ Heap, bắt buộc bộ dọn rác (Garbage Collector) của Unity hoạt động, gây ra hiện tượng khựng game nhẹ (Lag spike) ngay tại thời điểm thả linh kiện.
