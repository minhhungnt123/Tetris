# 🕹️ Tetris (Unity 6 Game Project)

Chào mừng bạn đến với repository **Tetris**! Đây là dự án game Tetris cá nhân được phát triển trên nền tảng **Unity 6**, xây dựng theo chuẩn **Tetris Guideline hiện đại** kết hợp với kiến trúc mã nguồn sạch (**Clean Architecture / SOLID**).

Tài liệu này đóng vai trò hướng dẫn tổng quan, quy định chuẩn quản lý mã nguồn, quy chuẩn đặt tên nhánh (**Branching**), viết commit (**Conventional Commits**) và cấu trúc thư mục của dự án.

---

## 📌 Tổng quan dự án (Project Overview)

- **Engine:** Unity Editor `6000.3.10f1` (Unity 6).
- **Ngôn ngữ:** C# (.NET Standard / Unity Assembly Definition).
- **Cơ chế Gameplay (Tetris Guideline):**
  - Hệ thống sinh khối 7-Bag Randomizer.
  - Xoay SRS (Super Rotation System) kèm bảng Wall Kick đầy đủ.
  - Ghost Piece (hình chiếu vị trí rơi), Hold Piece (giữ khối).
  - Hard Drop (thả tức thì), Soft Drop (hạ nhanh).
  - Xử lý mượt mà chuyển động ngang với DAS (Delayed Auto Shift) và ARR (Auto Repeat Rate).
  - Tính điểm theo chuẩn Guideline: Single, Double, Triple, Tetris, T-Spin, Combo & Back-to-Back multipliers.
  - Hiệu ứng nhấp nháy (Flash animation) khi xóa hàng trước khi hạ các hàng trên xuống.
- **Tech Stack & Packages:**
  - **Unity Input System** (`com.unity.inputsystem`): Tách biệt hoàn toàn input phần cứng qua Action Maps (`Assets/InputAction.inputactions`).
  - **2D Tilemap** (`com.unity.modules.tilemap`): Quản lý ma trận bàn cờ và vẽ khối gạch hiệu năng cao.
  - **uGUI & TextMesh Pro**: Hiển thị điểm số, level, hàng đã xóa, high score và bảng điều khiển.

---

## 🚨 3 Quy tắc vàng khi làm việc với Unity (Golden Rules)

1. **Quy tắc file `.meta` (Tối quan trọng):**
   * Trong Unity, mỗi asset (script, tile, audio, scene...) luôn đi kèm một file metadata `.meta` chứa GUID duy nhất.
   * ❌ **TUYỆT ĐỐI KHÔNG** đổi tên, di chuyển hoặc xóa asset trực tiếp bằng Windows Explorer.
   * ✅ **LUÔN LUÔN** thao tác đổi tên, di chuyển file **ngay bên trong cửa sổ Project của Unity Editor** để Unity tự động đồng bộ file `.meta`.
2. **Quy tắc Scene & Tránh xung đột (Scene & Conflict Rule):**
   * File scene `.unity` là file tuần tự hóa YAML phức tạp. Hạn chế chỉnh sửa đồng thời nhiều logic chưa hoàn thiện trực tiếp trên scene chính (`Tetris.unity`).
   * Tách biệt logic vào các Prefab và ScriptableObject thay vì hardcode trực tiếp trên Scene Hierarchy.
3. **Quy tắc không sửa file Auto-Generated:**
   * ❌ **KHÔNG** chỉnh sửa thủ công các file do Unity tự sinh: `*.csproj`, `*.sln`, thư mục `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
   * Các thay đổi về cấu hình thư viện cần được quản lý qua `Packages/manifest.json` hoặc Project Settings.

---

## 🏷️ Branch & Commit Standards (Sentry & Conventional Commits)

Toàn bộ nhánh (branch) và lịch sử commit của dự án tuân thủ nghiêm ngặt chuẩn **Conventional Commits** và chuẩn đặt tên nhánh:

### 1. Branch Naming Convention
* **Format:** `<type>/<short-description>` hoặc `<username>/<type>/<short-description>`
* **Quy tắc:** Viết thường toàn bộ (lowercase), phân tách bằng dấu gạch ngang (kebab-case), độ dài từ 2 đến 5 từ ngắn gọn, súc tích.
* **Các loại branch thông dụng:**
  * `feat`    : Tính năng gameplay, cơ chế mới (ví dụ: `feat/hold-piece`, `feat/ghost-piece`, `feat/das-arr-movement`)
  * `fix`     : Sửa lỗi logic, sửa lỗi hiển thị, va chạm (ví dụ: `fix/srs-wall-kick-i-piece`, `fix/line-clear-freeze`)
  * `refactor`: Tối ưu hóa mã nguồn, áp dụng SOLID/Clean Code mà không làm thay đổi tính năng (ví dụ: `refactor/input-manager-events`, `refactor/board-clear-logic`)
  * `style`   : Tinh chỉnh giao diện UI, font chữ, màu sắc khối, animation (ví dụ: `style/neon-theme-ui`, `style/game-over-modal`)
  * `chore`   : Cập nhật thư viện, dọn dẹp asset dư thừa, cấu hình git/editor (ví dụ: `chore/update-input-system-package`, `chore/clean-unused-tiles`)
  * `docs`    : Cập nhật tài liệu, README, hướng dẫn kỹ thuật (ví dụ: `docs/update-architecture-readme`)

---

### 2. Commit Message Convention
* **Format:**
  ```text
  <type>(<scope>): <Subject>

  <body> (tùy chọn: giải thích WHAT và WHY)

  <footer> (tùy chọn: liên kết issue / task, ví dụ: Refs #12)
  ```

* **Quy tắc phần Header:**
  * `<type>`: `feat`, `fix`, `refactor`, `style`, `chore`, `docs`, `perf`, `test`.
  * `<scope>`: Module mục tiêu (ví dụ: `core`, `board`, `piece`, `input`, `ui`, `audio`, `data`, `animation`).
  * `<Subject>`: Dùng thể mệnh lệnh ("Add" thay vì "Added", "Fix" thay vì "Fixed"), viết hoa chữ cái đầu tiên, **KHÔNG** có dấu chấm ở cuối, giới hạn tối đa 70 ký tự.

* **Ví dụ chuẩn cho dự án Tetris:**
  * ✅ `feat(piece): Implement SRS rotation system and wall kick tables`
  * ✅ `feat(board): Add 7-bag randomizer for tetromino sequence`
  * ✅ `fix(input): Prevent piece movement during line clear animation`
  * ✅ `refactor(core): Decouple score calculations into GameManager`
  * ✅ `style(ui): Add flash blink effect on line clear`
  * ✅ `chore(project): Upgrade Input System package to 1.18.0`
  * ❌ *Cần tránh: `fix bug`, `update code`, `[Tetris] Add something`, `WIP`*

---

## 🏗️ Kiến trúc mã nguồn (Code Architecture)

Mã nguồn C# được bố trí trong thư mục `Assets/Scripts/` theo cấu trúc tách biệt trách nhiệm, tuân thủ nguyên lý **SOLID**:

```
Assets/Scripts/
├── Core/                         # Dữ liệu & Quy tắc nghiệp vụ cốt lõi (Domain Logic & Abstractions)
│   ├── Tetromino.cs              # Enum định nghĩa 7 khối gạch & struct TetrominoData
│   ├── Data.cs                   # Ma trận tọa độ các khối & bảng Wall Kick SRS
│   ├── GameMode.cs               # Enum định nghĩa các chế độ chơi (Marathon, Sprint,...)
│   ├── IGameModeRule.cs          # Interface trừu tượng hóa quy tắc từng chế độ chơi
│   ├── MarathonModeRule.cs       # Logic chế độ Marathon (chơi tăng dần tốc độ theo level)
│   ├── SprintModeRule.cs         # Logic chế độ Sprint (chạy đua xóa 40 dòng)
│   ├── IPieceRandomizer.cs       # Interface trừu tượng hóa thuật toán sinh khối
│   ├── SevenBagRandomizer.cs     # Triển khai thuật toán 7-Bag Randomizer độc lập
│   ├── IScoreService.cs          # Interface xử lý tính điểm và hệ số combo
│   ├── TetrisGameSettingsSO.cs   # ScriptableObject cấu hình gameplay, DAS/ARR, trọng lực
│   └── TetrisScoringSettingsSO.cs# ScriptableObject cấu hình bảng điểm, combo & back-to-back
├── Managers/                     # Quản lý trạng thái và dịch vụ toàn cục (Singleton & C# Events)
│   ├── GameManager.cs            # Điều phối trạng thái vòng đời game (Start, Pause, GameOver, GameMode)
│   ├── Board.cs                  # Quản lý bàn cờ Tilemap, kiểm tra va chạm, line clear animation
│   ├── ScoreManager.cs           # Quản lý điểm số, level, lines cleared (triển khai IScoreService)
│   ├── InputManager.cs           # Điểm duy nhất lắng nghe Input Action và phát C# Events
│   └── UIManager.cs              # Tầng view giao diện, lắng nghe sự kiện từ các Managers
├── Controllers/                  # Hành vi điều khiển gắn trên GameObjects
│   ├── Piece.cs                  # Điều khiển khối đang rơi (trọng lực, lock delay, xoay SRS, hard drop)
│   ├── Ghost.cs                  # Khối bóng chiếu vị trí tiếp đất của khối hiện tại
│   ├── PlayerInputController.cs  # Xử lý DAS, ARR và chuyển đổi input từ InputManager tới Piece
│   ├── HoldPieceManager.cs       # Quản lý logic giữ khối (Hold Piece) tách biệt
│   └── OnlineLobbyStub.cs        # Stub chuẩn bị mở rộng chế độ nhiều người chơi (Multiplayer)
└── Utils/                        # Tiện ích bổ trợ và hiệu ứng trực quan
    ├── FloatingScoreEffect.cs    # Hiệu ứng chuyển động & mờ dần của số điểm bay
    └── FloatingScoreSpawner.cs   # Spawner quản lý khởi tạo Floating Score trên màn hình
```

### Nguyên tắc lập trình cốt lõi:
1. **Single Responsibility & Dependency Inversion:** Logic tính điểm (`ScoreManager`/`IScoreService`), sinh khối (`SevenBagRandomizer`/`IPieceRandomizer`) và luật chơi (`IGameModeRule`) được tách rời khỏi `GameManager`, giúp dễ dàng mở rộng và viết unit test.
2. **Data-Driven Configuration:** Thông số gameplay (tốc độ rơi, thời gian DAS/ARR, bảng điểm) được lưu trữ tại `Assets/Settings/` qua các ScriptableObject (`TetrisGameSettingsSO`, `TetrisScoringSettingsSO`), cho phép cân bằng game trực tiếp trên Inspector mà không cần sửa code.
3. **Board mutation pattern:** Mọi thay đổi vị trí hoặc góc xoay của `Piece` phải tuân thủ nghiêm ngặt:
   `Board.Clear(piece)` ➡️ thay đổi trạng thái/tọa độ ➡️ `Board.Set(piece)`.
4. **Event-driven communication:** Các Manager giao tiếp qua C# Events (`System.Action`), không dùng polling trong `Update()`.
5. **Input encapsulation:** Chỉ `InputManager` được phép đọc trực tiếp `InputActionReference`; các script khác chỉ đăng ký nhận sự kiện.

---

## 🔄 Quy trình làm việc hàng ngày (Daily Workflow)

| Bước | Mục tiêu | Lệnh Git CLI / Thao tác |
| :---: | :--- | :--- |
| **1** | **Đồng bộ nhánh chính** | `git checkout main` (hoặc `develop`)<br>`git pull origin main` |
| **2** | **Tạo nhánh tính năng** | `git checkout -b feat/t-spin-detection` |
| **3** | **Lập trình & Kiểm thử** | Viết code trong IDE ➡️ Chuyển sang Unity Editor để compile ➡️ Test trực tiếp tại scene `Tetris.unity` |
| **4** | **Kiểm tra trạng thái** | `git status` (đảm bảo file mã nguồn và file `.meta` đi kèm đều được theo dõi) |
| **5** | **Commit & Push** | `git add .`<br>`git commit -m "feat(core): Add T-spin corner detection logic"`<br>`git push -u origin feat/t-spin-detection` |
| **6** | **Merge về nhánh chính** | Tạo Pull Request hoặc merge trực tiếp sau khi hoàn thành kiểm thử |

---

## 💻 Git CLI Cheat Sheet

```bash
# 1. Tạo nhánh mới và chuyển sang nhánh đó
git checkout -b feat/ghost-piece-rendering

# 2. Kiểm tra các file thay đổi (bao gồm cả .meta)
git status

# 3. Stage và commit theo chuẩn Conventional Commits
git add .
git commit -m "feat(controller): Implement ghost piece drop calculation"

# 4. Push nhánh lên remote
git push -u origin feat/ghost-piece-rendering

# 5. Hủy bỏ thay đổi chưa commit trên một file cụ thể
git restore Assets/Scripts/Controllers/Piece.cs

# 6. Hủy bỏ toàn bộ thay đổi chưa stage
git restore .
```

---

## 📁 Cấu trúc thư mục Assets (Assets Folder Structure)

```
Assets/
├── Animations/                  # Animation clips & controllers
├── Audio/                       # Nhạc nền (BGM) và hiệu ứng âm thanh (SFX)
├── Materials/                   # Material hiển thị
├── Prefabs/                     # Các prefab game (Blocks, FloatingScoreEffect, UI Elements)
├── Scenes/                      # Scenes của game (chính: Tetris.unity)
├── Screenshots/                 # Ảnh chụp màn hình tiến độ / kiểm thử giao diện
├── Scripts/                     # Toàn bộ mã nguồn C# (Core, Managers, Controllers, Utils)
├── Settings/                    # ScriptableObject cấu hình gameplay & scoring (.asset)
├── TextMesh Pro/                # Resource & fonts cho UI TextMesh Pro
├── Textures/                    # Hình ảnh sprite, icon, background
├── Tiles/                       # Unity Tileset dùng để vẽ các khối gạch lên Tilemap
├── InputAction.inputactions     # Cấu hình Unity Input System Actions (Action Maps)
└── InputSystem_Actions.inputactions # Cấu hình Input Actions mặc định của Unity
```

---

## 🎮 Hướng dẫn mở dự án & Trải nghiệm Game

1. Mở **Unity Hub** và chọn **Add project from disk**.
2. Chọn thư mục `Tetris`. Đảm bảo phiên bản Unity Editor là **Unity 6 (`6000.3.10f1`)**.
3. Mở scene tại đường dẫn: `Assets/Scenes/Tetris.unity`.
4. Nhấn nút **Play** trên Unity Editor để bắt đầu chơi.
5. **Phím điều khiển mặc định:**
   - `A` / `D` hoặc `Phím Mũi tên Trái / Phải`: Di chuyển ngang (hỗ trợ DAS/ARR).
   - `S` hoặc `Mũi tên Xuống`: Soft Drop (hạ nhanh).
   - `Space` hoặc `Mũi tên Lên`: Hard Drop (thả ngay lập tức).
   - `W` hoặc `Up` / `X`: Xoay theo chiều kim đồng hồ (Clockwise).
   - `Z`: Xoay ngược chiều kim đồng hồ (Counter-Clockwise).
   - `C` hoặc `Left Shift`: Hold Piece (giữ khối).
   - `Escape`: Tạm dừng / Tiếp tục (Pause/Resume).
