# TIẾN ĐỘ VÀ LỘ TRÌNH PHÁT TRIỂN DỰ ÁN (PROJECT ROADMAP TRACKER) - **v0.8.0**

Tài liệu này dùng để theo dõi tiến độ thực hiện các User Stories và các Epic trong suốt quá trình phát triển game 2D Roguelite.

---

## 📊 TÓM TẮT TIẾN ĐỘ CHUNG (Cập nhật: 2026-08-25)
*   **Trạng thái hiện tại**: Hoàn tất 100% các Epic từ E01–E09! Bao gồm Room/Spawner, Level Gen, Perk System, Boss Fight, Save/Load System V8 Migration, Support Weapon Buff System (tối đa 3 slots, reset loadout mỗi run, shop & achievement requirements), Permanent Upgrade Multi-tier & Milestones, Unity Editor Tools & Debug F1 Console.
*   **Tổng số Story Points (SP)**: 149 SP.
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
| **US-008** | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) | Combat | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo module `Attack.cs` quản lý hitbox trigger collider và đăng ký input `onAttack` kích hoạt hoạt ảnh tấn công của Player. Đã bổ sung đòn Air Attack riêng biệt và cơ chế chống kích hoạt đệm khi tiếp đất. |
| **US-009** | Xây dựng EnemyBase với máy trạng thái đơn giản: Patrol, Chase và Attack | Enemy | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo `EnemyBase.cs` abstract class với State Machine 5 trạng thái, tích hợp IDamageable. |

---

### 🗺️ Epic E03: Player Combat & Basic Enemy System
*Mục tiêu: Đấu quái, hệ thống khóa phòng (Room System) và cơ chế sinh quái.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-010** | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0 | Combat | 3 | 🔴 High | ✅ Hoàn thành | Đã kết nối `TakeDamage` qua giao diện `IDamageable` giữa `PlayerStats` và `EnemyBase` qua `Attack.cs`, áp dụng lực đẩy lùi (Knockback) và vô hiệu hóa/hủy thực thể khi HP <= 0. Khắc phục lỗi knockback của Enemy bị triệt tiêu do StopMovement. |
| **US-011** | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và khóa phòng | Room System | 3 | 🔴 High | ✅ Hoàn thành | Quản lý logic cửa và kích hoạt trạng thái chiến đấu phòng. |
| **US-012** | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn khi phòng bị khóa | Room System | 2 | 🔴 High | ✅ Hoàn thành | Hệ thống `EnemySpawner.cs` có thể customize vị trí sinh quái, tự vẽ gizmoz để dễ dàng kéo thả vị trí sinh quái kết hợp cùng `RoomManager.cs`. |
| **US-013** | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết | Room System | 3 | 🔴 High | ✅ Hoàn thành | `RoomManager.cs` kết hợp chung với `EnemySpawner.cs` trong mỗi phòng, đảm bảo tính độc lập giữa các phòng và chắc chắn điều kiện mở phòng (tiêu diệt hết quái). |

---

### 🧱 Epic E04: Thiết kế & Tạo Màn chơi bán ngẫu nhiên (Semi-random Level Gen)
*Mục tiêu: Thiết kế các Room Prefab và xây dựng giải thuật ghép phòng ngẫu nhiên cho màn chơi.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-014** | Thiết kế các Room Prefab cơ bản (Start, Combat, Reward, Boss) với Tilemap, Collider và các vị trí spawn quái | Level | 3 | 🔴 High | ✅ Hoàn thành | Cấu hình các prefab phòng hoàn chỉnh. |
| **US-015** | Cài đặt hệ thống sinh map bán ngẫu nhiên (Semi-random), tự động ghép nối các Room Prefab theo luồng chạy của người chơi | Architecture | 5 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện giải thuật ghép nối cửa (Doorway Alignment) tự động không chồng lấn. |
| **US-016** | Xây dựng các loại Enemy prefab khác nhau, xây dựng Pause menu UI cơ bản | Architecture | 3 | 🟡 Medium | ✅ Hoàn thành | Đã có sẵn 7 loại Enemy Prefab (`Enemy1` đến `Enemy7`) và script `PauseMenuManager.cs` điều phối giao diện tạm dừng/tùy chọn cơ bản. |
| **US-017** | Xử lý logic dịch chuyển và kết nối giữa các cửa (Doors), đảm bảo camera và Player di chuyển mượt mà qua các phòng | Gameplay | 3 | 🔴 High | ✅ Hoàn thành | Đã có Cinemachine camera, có thể bám theo người chơi, tuy nhiên chưa lock theo từng phòng, chưa đảm bảo 100% sẽ không bị quay ngoài map. |

---

### 🧬 Epic E05: Upgrade System & Meta Progression
*Mục tiêu: Phát triển hệ thống nâng cấp chỉ số tạm thời (Perks) và lưu trữ tài nguyên nâng cấp vĩnh viễn.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-018** | Định nghĩa PerkData (ScriptableObject): id, tên, icon, loại effect (Stat modifier / Special behavior), giá trị, rarity, quy tắc stack | Data | 3 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện lớp PerkData kế thừa ScriptableObject và cấu hình các thuộc tính. |
| **US-019** | Xây dựng PerkPool + random có trọng số theo rarity, loại trừ perk đã đạt max stack khỏi vòng random | System | 3 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện PerkPool và thuật toán random có trọng số loại trừ perk max stack. |
| **US-020** | UI Reward Card: hiển thị 3 lựa chọn Perk kèm icon, mô tả, màu theo rarity; chọn bằng click hoặc phím 1/2/3 | UI | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo Panel Reward Selection & RewardCardUI hiển thị border material theo rarity và lắng nghe OnPerkAdded. |
| **US-021** | PerkEffectApplier tách riêng khỏi UpgradeManager: xử lý áp effect theo loại (cộng thẳng / nhân hệ số / effect đặc biệt) | System | 3 | 🟡 Medium | ✅ Hoàn thành | Đã hoàn thiện lớp PerkEffectApplier xử lý cộng dồn và áp dụng hiệu ứng chỉ số / đặc biệt. |
| **US-022** | UpgradeManager quản lý danh sách Perk active trong run, tự động clear khi kết thúc, lưu lịch sử để hiển thị ở màn Result | System | 5 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện UpgradeManager Singleton quản lý active perks, đồng bộ scene và tự động clear. |

---

### 👹 Epic E06: Boss Fight & Arena
*Mục tiêu: Thiết kế cơ chế chiến đấu của Boss, phân chia Phase và tạo Boss Room hoàn chỉnh.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-023** | BossBase kế thừa EnemyBase, thêm Phase theo ngưỡng % HP; dùng lại animation clip có sẵn của asset, chỉ đổi tốc độ và scale | Enemy | 5 | 🔴 High | ✅ Hoàn thành | Đã xây dựng BossBase.cs và Boss.cs với cơ chế chia pha theo lượng HP và đổi màu sprite. |
| **US-024** | Bộ 2 attack pattern phân biệt bằng hitbox và timing khác nhau trên cùng 1 animation clip | Enemy | 5 | 🔴 High | ✅ Hoàn thành | Xây dựng hệ thống Hitbox dựa trên dữ liệu (HitboxData + HitboxController) kích hoạt theo animation event frame index. |
| **US-025** | Enrage ở Phase cuối: tăng tốc độ tấn công/di chuyển + đổi màu sprite (tint) bằng Material/Shader Graph | Enemy | 3 | 🟢 Low | ✅ Hoàn thành | Tự động đổi Animator speed, physics scale và Material Outline Shader khi vào Enraged Phase. |
| **US-026** | BossHealthBar UI: tên boss, thanh máu chia phase, hiệu ứng flash/shake UI khi chuyển phase | UI | 3 | 🟡 Medium | ✅ Hoàn thành | Thiết lập UI Slider/Image cho thanh máu Boss hiển thị tên và lượng HP theo từng phase. |
| **US-027** | Tích hợp Boss Room vào level generation, khóa cửa khi vào; thêm ambient VFX và SFX riêng | Room System | 3 | 🟡 Medium | ⏳ Chưa bắt đầu | Thiết kế Boss Arena, cơ chế khóa cửa phòng và tích hợp vào sinh màn chơi. |

---
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
| **US-008** | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) | Combat | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo module `Attack.cs` quản lý hitbox trigger collider và đăng ký input `onAttack` kích hoạt hoạt ảnh tấn công của Player. Đã bổ sung đòn Air Attack riêng biệt và cơ chế chống kích hoạt đệm khi tiếp đất. |
| **US-009** | Xây dựng EnemyBase với máy trạng thái đơn giản: Patrol, Chase và Attack | Enemy | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo `EnemyBase.cs` abstract class với State Machine 5 trạng thái, tích hợp IDamageable. |

---

### 🗺️ Epic E03: Player Combat & Basic Enemy System
*Mục tiêu: Đấu quái, hệ thống khóa phòng (Room System) và cơ chế sinh quái.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-010** | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0 | Combat | 3 | 🔴 High | ✅ Hoàn thành | Đã kết nối `TakeDamage` qua giao diện `IDamageable` giữa `PlayerStats` và `EnemyBase` qua `Attack.cs`, áp dụng lực đẩy lùi (Knockback) và vô hiệu hóa/hủy thực thể khi HP <= 0. Khắc phục lỗi knockback của Enemy bị triệt tiêu do StopMovement. |
| **US-011** | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và khóa phòng | Room System | 3 | 🔴 High | ✅ Hoàn thành | Quản lý logic cửa và kích hoạt trạng thái chiến đấu phòng. |
| **US-012** | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn khi phòng bị khóa | Room System | 2 | 🔴 High | ✅ Hoàn thành | Hệ thống `EnemySpawner.cs` có thể customize vị trí sinh quái, tự vẽ gizmoz để dễ dàng kéo thả vị trí sinh quái kết hợp cùng `RoomManager.cs`. |
| **US-013** | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết | Room System | 3 | 🔴 High | ✅ Hoàn thành | `RoomManager.cs` kết hợp chung với `EnemySpawner.cs` trong mỗi phòng, đảm bảo tính độc lập giữa các phòng và chắc chắn điều kiện mở phòng (tiêu diệt hết quái). |

---

### 🧱 Epic E04: Thiết kế & Tạo Màn chơi bán ngẫu nhiên (Semi-random Level Gen)
*Mục tiêu: Thiết kế các Room Prefab và xây dựng giải thuật ghép phòng ngẫu nhiên cho màn chơi.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-014** | Thiết kế các Room Prefab cơ bản (Start, Combat, Reward, Boss) với Tilemap, Collider và các vị trí spawn quái | Level | 3 | 🔴 High | ✅ Hoàn thành | Cấu hình các prefab phòng hoàn chỉnh. |
| **US-015** | Cài đặt hệ thống sinh map bán ngẫu nhiên (Semi-random), tự động ghép nối các Room Prefab theo luồng chạy của người chơi | Architecture | 5 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện giải thuật ghép nối cửa (Doorway Alignment) tự động không chồng lấn. |
| **US-016** | Xây dựng các loại Enemy prefab khác nhau, xây dựng Pause menu UI cơ bản | Architecture | 3 | 🟡 Medium | ✅ Hoàn thành | Đã có sẵn 7 loại Enemy Prefab (`Enemy1` đến `Enemy7`) và script `PauseMenuManager.cs` điều phối giao diện tạm dừng/tùy chọn cơ bản. |
| **US-017** | Xử lý logic dịch chuyển và kết nối giữa các cửa (Doors), đảm bảo camera và Player di chuyển mượt mà qua các phòng | Gameplay | 3 | 🔴 High | ✅ Hoàn thành | Đã có Cinemachine camera, có thể bám theo người chơi, tuy nhiên chưa lock theo từng phòng, chưa đảm bảo 100% sẽ không bị quay ngoài map. |

---

### 🧬 Epic E05: Upgrade System & Meta Progression
*Mục tiêu: Phát triển hệ thống nâng cấp chỉ số tạm thời (Perks) và lưu trữ tài nguyên nâng cấp vĩnh viễn.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-018** | Định nghĩa PerkData (ScriptableObject): id, tên, icon, loại effect (Stat modifier / Special behavior), giá trị, rarity, quy tắc stack | Data | 3 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện lớp PerkData kế thừa ScriptableObject và cấu hình các thuộc tính. |
| **US-019** | Xây dựng PerkPool + random có trọng số theo rarity, loại trừ perk đã đạt max stack khỏi vòng random | System | 3 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện PerkPool và thuật toán random có trọng số loại trừ perk max stack. |
| **US-020** | UI Reward Card: hiển thị 3 lựa chọn Perk kèm icon, mô tả, màu theo rarity; chọn bằng click hoặc phím 1/2/3 | UI | 3 | 🔴 High | ✅ Hoàn thành | Đã tạo Panel Reward Selection & RewardCardUI hiển thị border material theo rarity và lắng nghe OnPerkAdded. |
| **US-021** | PerkEffectApplier tách riêng khỏi UpgradeManager: xử lý áp effect theo loại (cộng thẳng / nhân hệ số / effect đặc biệt) | System | 3 | 🟡 Medium | ✅ Hoàn thành | Đã hoàn thiện lớp PerkEffectApplier xử lý cộng dồn và áp dụng hiệu ứng chỉ số / đặc biệt. |
| **US-022** | UpgradeManager quản lý danh sách Perk active trong run, tự động clear khi kết thúc, lưu lịch sử để hiển thị ở màn Result | System | 5 | 🔴 High | ✅ Hoàn thành | Đã hoàn thiện UpgradeManager Singleton quản lý active perks, đồng bộ scene và tự động clear. |

---

### 👹 Epic E06: Boss Fight & Arena
*Mục tiêu: Thiết kế cơ chế chiến đấu của Boss, phân chia Phase và tạo Boss Room hoàn chỉnh.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-023** | BossBase kế thừa EnemyBase, thêm Phase theo ngưỡng % HP; dùng lại animation clip có sẵn của asset, chỉ đổi tốc độ và scale | Enemy | 5 | 🔴 High | ✅ Hoàn thành | Đã xây dựng BossBase.cs và Boss.cs với cơ chế chia pha theo lượng HP và đổi màu sprite. |
| **US-024** | Bộ 2 attack pattern phân biệt bằng hitbox và timing khác nhau trên cùng 1 animation clip | Enemy | 5 | 🔴 High | ✅ Hoàn thành | Xây dựng hệ thống Hitbox dựa trên dữ liệu (HitboxData + HitboxController) kích hoạt theo animation event frame index. |
| **US-025** | Enrage ở Phase cuối: tăng tốc độ tấn công/di chuyển + đổi màu sprite (tint) bằng Material/Shader Graph | Enemy | 3 | 🟢 Low | ✅ Hoàn thành | Tự động đổi Animator speed, physics scale và Material Outline Shader khi vào Enraged Phase. |
| **US-026** | BossHealthBar UI: tên boss, thanh máu chia phase, hiệu ứng flash/shake UI khi chuyển phase | UI | 3 | 🟡 Medium | ✅ Hoàn thành | Thiết lập UI Slider/Image cho thanh máu Boss hiển thị tên và lượng HP theo từng phase. |
| **US-027** | Tích hợp Boss Room vào level generation, khóa cửa khi vào; thêm ambient VFX và SFX riêng | Room System | 3 | 🟡 Medium | ⏳ Chưa bắt đầu | Thiết kế Boss Arena, cơ chế khóa cửa phòng và tích hợp vào sinh màn chơi. |

---

### 💾 Epic E07: Save/Load System
*Mục tiêu: Xây dựng hệ thống lưu trữ tiến trình chơi, thiết lập và bảo mật dữ liệu save.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-028** | Thiết kế SaveData serializable: PlayerProgressData, WeaponUnlockData, AbilityUnlockData, SettingData | Data | 3 | 🟡 Medium | ✅ Hoàn thành | Đã tạo các lớp dữ liệu serializable cho tiến trình, vũ khí, ability/perk và thiết lập. |
| **US-029** | SaveManager (Singleton): đọc/ghi JSON tại persistentDataPath | Architecture | 3 | 🔴 High | ✅ Hoàn thành | `SaveManager` lưu/tải JSON tại `Application.persistentDataPath`, hỗ trợ 3 save slot và file backup `.bak`. |
| **US-030** | Luồng Load tại startup: kiểm tra file tồn tại → load, hoặc tạo SaveData mặc định nếu chưa có | Architecture | 2 | 🔴 High | ✅ Hoàn thành | `GameManager` bảo đảm SaveManager được tạo lúc khởi động; SaveManager tự tải hoặc tạo dữ liệu mặc định. |
| **US-031** | Tích hợp điểm gọi Save cụ thể: kết thúc run (Dead/Win), mua Permanent Upgrade, đổi Setting | Architecture | 3 | 🟡 Medium | 🚧 Đang thực hiện | Đã kích hoạt autosave có debounce khi `GameOver`/`Victory`; còn điểm gọi khi mua Permanent Upgrade và thay đổi Setting. |
| **US-032** | Tách riêng lưu SettingData (âm lượng, độ phân giải, key binding) khỏi luồng save tiến trình chính | System | 2 | 🔴 High | ✅ Hoàn thành | `SettingData` được lưu riêng trong `settings.json` và ghi nhớ save slot dùng gần nhất. |
| **US-033** | Xử lý file corrupt/thiếu: fallback về SaveData mặc định, log cảnh báo | Architecture | 2 | 🟡 Medium | ✅ Hoàn thành | Tạo dữ liệu mặc định khi file thiếu; file save lỗi sẽ thử phục hồi từ backup trước khi fallback và ghi log. |
*Mục tiêu: Phát triển cơ chế meta-progression, tích lũy tài nguyên và mua nâng cấp vĩnh viễn.*

| ID | User Story | Module | SP | Độ ưu tiên | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **US-043** | Meta-currency rơi ra từ enemy/boss đã hạ trong run, cấu hình theo từng loại enemy | System | 3 | 🔴 High | ⏳ Chưa bắt đầu | Logic rớt tiền sau khi hạ kẻ địch. |
| **US-044** | Bonus tài nguyên cho các mốc đặc biệt (VD: lần đầu hạ boss, clear toàn bộ room 1 run) | System | 2 | 🔴 High | ⏳ Chưa bắt đầu | Thưởng thêm tài nguyên khi đạt mốc thành tựu trong run. |
| **US-045** | Màn hình Upgrade vĩnh viễn ở Main Menu: danh sách nâng cấp, giá tiền, trạng thái đã mua/chưa mua | UI | 3 | 🔴 High | ⏳ Chưa bắt đầu | UI nâng cấp chỉ số vĩnh viễn ngoài màn hình chính. |
| **US-046** | PermanentUpgradeManager: áp toàn bộ chỉ số vĩnh viễn đã mua vào PlayerStats khi bắt đầu run mới | System | 3 | 🔴 High | ⏳ Chưa bắt đầu | Cộng dồn các chỉ số đã nâng cấp vào stats của nhân vật khi bắt đầu run. |
| **US-047** | Upgrade dạng nhiều bậc (VD: Max HP cấp 1/2/3, giá tăng dần theo bậc) | System | 3 | 🔴 High | ⏳ Chưa bắt đầu | Thiết kế nâng cấp lũy tiến theo bậc. |
| **US-048** | Phản hồi khi mua thành công: SFX xác nhận + hiệu ứng UI (tween scale/flash) | UI | 2 | 🔴 High | ⏳ Chưa bắt đầu | Phản hồi nghe nhìn sinh động khi mua thành công. |
| **US-049** | Kiểm thử tích hợp: đảm bảo Permanent Upgrade đồng bộ đúng với SaveData qua nhiều lần chơi | System | 2 | 🔴 High | ⏳ Chưa bắt đầu | Đảm bảo tính toàn vẹn và đồng bộ dữ liệu nâng cấp. |

---

## 🪵 NHẬT KÝ THAY ĐỔI (CHANGE LOG)
Xem chi tiết toàn bộ lịch sử thay đổi và cập nhật của dự án tại tệp tin [CHANGELOG.md](https://github.com/TranMC/2D_Roughlite/CHANGELOG.md).
