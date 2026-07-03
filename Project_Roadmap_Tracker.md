# TIẾN ĐỘ VÀ LỘ TRÌNH PHÁT TRIỂN DỰ ÁN (PROJECT ROADMAP TRACKER)

Tài liệu này dùng để theo dõi tiến độ thực hiện các User Stories và các Epic trong suốt quá trình phát triển game 2D Roguelite.

---

## 📊 TÓM TẮT TIẾN ĐỘ CHUNG
*   **Trạng thái hiện tại**: Đã hoàn tất toàn bộ hệ thống Phòng và sinh quái vật (US-012, US-013 thuộc Epic E03), cùng hệ thống Menu Tạm dừng & Prefabs Quái vật (US-016 thuộc Epic E04). Chuẩn bị bước vào các giai đoạn Level Generation chính.
*   **Tổng số Story Points (SP)**: 81 SP.
*   **Đã hoàn thành**: 42 / 81 SP (52%).
*   **Đang thực hiện**: 0 / 81 SP (0%).
*   **Chưa bắt đầu**: 39 / 81 SP (48%).

---

## 📌 CHI TIẾT CÁC GIAI ĐOẠN (EPICS)

### ⚙️ Epic E01: Kiến trúc & Project Setup Unity
*Mục tiêu: Thiết lập môi trường dự án ổn định, cấu trúc thư mục, quản lý trạng thái game chung.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-001** | Khởi tạo dự án Unity 2D/URP: project settings, folder structure, scenes, input map, sorting layer | Engine | 3 | 🔴 High | ✅ Hoàn thành | Thiết lập cấu trúc thư mục code chuẩn trong `Assets/Scripts/`. |
| **US-002** | Thiết lập Git workflow: branch main/dev/feature, Git LFS cho asset, .gitignore Unity | DevOps | 3 | 🔴 High | ✅ Hoàn thành | Đã cấu hình `.gitignore`, `.gitattributes` và khởi tạo repo Git. |
| **US-003** | Tìm kiếm tài nguyên cần dùng cho game từ các nguồn khác nhau | Asset | 3 | 🔴 High | ✅ Hoàn thành | Các asset đồ họa đã được nhập vào thư mục `Assets/Arts`. |
| **US-004** | Khởi tạo các Animation cơ bản cho player | Player | 2 | 🔴 High | ✅ Hoàn thành | Người dùng đã tự thiết lập các animation cơ bản cho Player. |
| **US-005** | Khởi tạo GameManager (Singleton) để điều phối trạng thái game (Play, Pause, GameOver) | Architecture | 3 | 🔴 High | ✅ Hoàn thành | Tạo `GameManager.cs` Singleton điều phối trạng thái game và chuyển đổi scene. |

---

### 🌱 Epic E02: Core Roguelite Gameplay Loop
*Mục tiêu: Hoàn thiện nhân vật chính có thể di chuyển, tấn công và các kẻ địch cơ bản.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-006** | Xây dựng PlayerController và State Machine xử lý di chuyển (Idle, Move, Jump, Fall) bằng Rigidbody2D | Player | 3 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện PlayerController, PlayerStateMachine và các trạng thái di chuyển (Idle, Move, Jump, Fall) sử dụng Rigidbody2D. |
| **US-007** | Tạo PlayerStats để quản lí HP của player và các trạng thái Hit, Dead | Player | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo PlayerStats quản lý HP, sự kiện nhận sát thương (Hit) và vô hiệu hóa điều khiển / kích hoạt GameManager GameOver khi chết (Dead). |
| **US-008** | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) | Combat | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo module `Attack.cs` quản lý hitbox trigger collider và đăng ký input `onAttack` kích hoạt hoạt ảnh tấn công của Player. |
| **US-009** | Xây dựng EnemyBase với máy trạng thái đơn giản: Patrol, Chase và Attack | Enemy | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo `EnemyBase.cs` abstract class với State Machine 5 trạng thái, tích hợp IDamageable. |

---

### 🗺️ Epic E03: Player Combat & Basic Enemy System
*Mục tiêu: Đấu quái, hệ thống khóa phòng (Room System) và cơ chế sinh quái.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-010** | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0 | Combat | 3 | 🔴 High | ✅ Hoàn thành | Đã kết nối `TakeDamage` qua giao diện `IDamageable` giữa `PlayerStats` và `EnemyBase` qua `Attack.cs`, áp dụng lực đẩy lùi (Knockback) và vô hiệu hóa/hủy thực thể khi HP <= 0. |
| **US-011** | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và khóa phòng | Room System | 3 | 🔴 High | ✅ Hoàn thành | Quản lý logic cửa và kích hoạt trạng thái chiến đấu phòng. |
| **US-012** | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn khi phòng bị khóa | Room System | 2 | 🔴 High | ✅ Hoàn thành | Hệ thống `EnemySpawner.cs` có thể customize vị trí sinh quái, tự vẽ gizmoz để dễ dàng kéo thả vị trí sinh quái kết hợp cùng `RoomManager.cs`. |
| **US-013** | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết | Room System | 3 | 🔴 High | ✅ Hoàn thành | `RoomManager.cs` kết hợp chung với `EnemySpawner.cs` trong mỗi phòng, đảm bảo tính độc lập giữa các phòng và chắc chắn điều kiện mở phòng (tiêu diệt hết quái). |

---

### 🧱 Epic E04: Thiết kế & Tạo Màn chơi bán ngẫu nhiên (Semi-random Level Gen)
*Mục tiêu: Thiết kế các Room Prefab và xây dựng giải thuật ghép phòng ngẫu nhiên cho màn chơi.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-014** | Thiết kế các Room Prefab cơ bản (Start, Combat, Reward, Boss) với Tilemap, Collider và các vị trí spawn quái | Level | 3 | 🔴 High | ⏳ Chưa bắt đầu | Cấu hình các prefab phòng hoàn chỉnh. |
| **US-015** | Cài đặt hệ thống sinh map bán ngẫu nhiên (Semi-random), tự động ghép nối các Room Prefab theo luồng chạy của người chơi | Architecture | 5 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện giải thuật ghép nối cửa (Doorway Alignment) tự động không chồng lấn. |
| **US-016** | Xây dựng các loại Enemy prefab khác nhau, xây dựng Pause menu UI cơ bản | Architecture | 3 | 🟡 Medium | ✅ Hoàn thành | Đã có sẵn 7 loại Enemy Prefab (`Enemy1` đến `Enemy7`) và script `PauseMenuManager.cs` điều phối giao diện tạm dừng/tùy chọn cơ bản. |
| **US-017** | Xử lý logic dịch chuyển và kết nối giữa các cửa (Doors), đảm bảo camera và Player di chuyển mượt mà qua các phòng | Gameplay | 3 | 🔴 High | ⏳ Chưa bắt đầu | Chuyển cảnh mượt mà giữa các phòng bằng Cinemachine/Camera transition. |

---

### 🧬 Epic E05: Upgrade System & Meta Progression
*Mục tiêu: Phát triển hệ thống nâng cấp chỉ số tạm thời (Perks) và lưu trữ tài nguyên nâng cấp vĩnh viễn.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-018** | Tạo cấu trúc ScriptableObject để quản lý dữ liệu cấu hình của các loại Perk tạm thời (Tăng HP, Tăng Damage, Tốc đánh) | Data | 3 | 🔴 High | ⏳ Chưa bắt đầu | Data-driven configuration cho Perks. |
| **US-019** | Xây dựng màn hình UI Reward hiển thị ngẫu nhiên các lựa chọn Perk nâng cấp cho Player sau khi clear Room | UI | 3 | 🔴 High | ⏳ Chưa bắt đầu | Giao diện lựa chọn phần thưởng ngẫu nhiên. |
| **US-020** | Lập trình UpgradeManager xử lý cộng dồn chỉ số từ Perk vào PlayerStats và tự động reset trạng thái sau khi kết thúc Run (Dead/Win) | System | 3 | 🔴 High | ⏳ Chưa bắt đầu | Quản lý tác dụng của Perk trong run chơi. |
| **US-021** | Xây dựng hệ thống quản lý tài nguyên Meta-progression thu thập được để mở khóa nâng cấp vĩnh viễn ở Main Menu | Progression | 5 | 🟡 Medium | ⏳ Chưa bắt đầu | Nâng cấp vĩnh viễn ngoài màn hình chính. |

---

### 👹 Epic E06: Boss Fight & UX Polish & Save/Load System
*Mục tiêu: Thiết kế Boss chiến đấu, hệ thống lưu game JSON và hoàn thiện trải nghiệm người dùng toàn diện.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-022** | Lập trình AI cho Boss dựa trên EnemyBase với các State chiến đấu phức tạp hơn (VD: Attack diện rộng, Enrage khi máu thấp) | Enemy | 5 | 🔴 High | ⏳ Chưa bắt đầu | Boss AI hành vi đặc trưng. |
| **US-023** | Tích hợp Boss Room vào khâu Level Generation và cấu hình thanh HP Bar chuyên dụng hiển thị trên UI khi đối đầu Boss | Boss | 3 | 🔴 High | ⏳ Chưa bắt đầu | Giao diện thanh máu Boss và chèn phòng Boss ở cuối hành trình. |
| **US-024** | Cài đặt hệ thống Save/Load dữ liệu (JSON) để lưu trữ vĩnh viễn tiến trình mở khóa nâng cấp và tài nguyên của người chơi | Architecture | 5 | 🔴 High | ⏳ Chưa bắt đầu | Lưu/tải dữ liệu cục bộ an toàn. |
| **US-025** | Hoàn thiện luồng UX toàn game: Kết nối Main Menu vào vòng lặp Run, hiển thị thông số chi tiết ở màn hình Result (Victory/Defeat) | UI | 3 | 🔴 High | ⏳ Chưa bắt đầu | Hoàn thiện vòng lặp chơi đầy đủ và giao diện kết thúc màn. |

---

## 🪵 NHẬT KÝ THAY ĐỔI (CHANGE LOG)
Xem chi tiết toàn bộ lịch sử thay đổi và cập nhật của dự án tại tệp tin [CHANGELOG.md](https://github.com/TranMC/2D_Roughlite/CHANGELOG.md).
