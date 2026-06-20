# TIẾN ĐỘ VÀ LỘ TRÌNH PHÁT TRIỂN DỰ ÁN (PROJECT ROADMAP TRACKER)

Tài liệu này dùng để theo dõi tiến độ thực hiện các User Stories và các Epic trong suốt quá trình phát triển game 2D Roguelite.

---

## 📊 TÓM TẮT TIẾN ĐỘ CHUNG
*   **Trạng thái hiện tại**: Giai đoạn thiết lập nền tảng & cấu trúc dự án.
*   **Tổng số Story Points (SP)**: 50 SP.
*   **Đã hoàn thành**: 14 / 50 SP (28%).
*   **Đang thực hiện**: 0 / 50 SP (0%).
*   **Chưa bắt đầu**: 36 / 50 SP (72%).

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
| **US-006** | Xây dựng PlayerController và State Machine xử lý di chuyển (Idle, Move, Jump, Fall) bằng Rigidbody2D | Player | 3 | 🔴 High | ⏳ Chưa bắt đầu | Di chuyển vật lý 2D, hỗ trợ cảm giác điều khiển mượt mà. |
| **US-007** | Tạo PlayerStats để quản lí HP của player và các trạng thái Hit, Dead | Player | 3 | 🔴 High | ⏳ Chưa bắt đầu | Quản lý máu, sự kiện nhận sát thương. |
| **US-008** | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) | Combat | 3 | 🔴 High | ⏳ Chưa bắt đầu | Tương tác tấn công cận chiến/tầm xa. |
| **US-009** | Xây dựng EnemyBase với máy trạng thái đơn giản: Patrol, Chase và Attack | Enemy | 3 | 🔴 High | ⏳ Chưa bắt đầu | Lớp cơ sở (Base class) cho mọi loại quái. |

---

### 🗺️ Epic E03: Player Combat & Basic Enemy System
*Mục tiêu: Đấu quái, hệ thống khóa phòng (Room System) và cơ chế sinh quái.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-010** | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0 | Combat | 3 | 🔴 High | ⏳ Chưa bắt đầu | Vòng lặp tương tác combat hoàn chỉnh. |
| **US-011** | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và khóa phòng | Room System | 3 | 🔴 High | ⏳ Chưa bắt đầu | Quản lý logic cửa và kích hoạt trạng thái chiến đấu phòng. |
| **US-012** | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn khi phòng bị khóa | Room System | 2 | 🔴 High | ⏳ Chưa bắt đầu | Tự động hóa việc spawn quái. |
| **US-013** | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết | Room System | 3 | 🔴 High | ⏳ Chưa bắt đầu | Kết thúc thử thách trong phòng và mở lối đi tiếp. |

---

## 🪵 NHẬT KÝ THAY ĐỔI (CHANGE LOG)
*   **2026-06-20**: Khởi tạo file lộ trình theo dõi tiến độ (`Project_Roadmap_Tracker.md`). Đánh dấu **US-002**, **US-003**, và **US-004** là **Hoàn thành**.
*   **2026-06-20 (tiếp tục)**: Triển khai hoàn tất **US-001** (tạo cấu trúc thư mục code) và **US-005** (viết mã nguồn GameManager Singleton).
