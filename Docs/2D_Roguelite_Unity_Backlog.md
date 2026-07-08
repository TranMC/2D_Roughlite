# 2D Roguelite Unity Project Backlog

## Sheet: 📋 Tổng Quan

| Danh mục | Chi tiết | Phân loại | Story Points | Ghi chú |
| :--- | :--- | :--- | :---: | :--- |
| **Dự án** | ⚔️ 2D ROUGHLITE MANAGEMENT — UNITY GAME PROJECT | - | - | - |
| **Thông tin** | Roguelite 2D | Unity 2D | - | Lịch trình 3 tháng |
| **Quy mô** | 9 Epics | 49 User Stories | **149 SP** | Phân chia thành các giai đoạn thực hiện |
| **TECH STACK ĐỀ XUẤT** | | | | |
| **Game Engine** | Unity 2022.3 LTS (2022.3.62f2) + C# | - | - | Ưu tiên Unity 2D/URP; dễ demo và build WebGL/Windows |
| **Render/Visual** | URP 2D Renderer, Tilemap, Sprite/Placeholder | - | - | Asset nhân vật, khu vực có thể dùng free/mua; trọng tâm là gameflow |
| **Data-Driven Config** | ScriptableObject, JSON Config | - | - | Dùng để cấu hình vũ khí, enemy, perk, chỉ số, level, boss mà không phải sửa code nhiều |
| **Gameplay Architecture** | Component-Based Architecture, State Machine, Event System | - | - | Dễ chia module: Player, Enemy, Combat, UI, Upgrade, Level; tránh code dính chặt vào nhau |
| **Simulation Logic** | Unity Physics 2D, Rigidbody2D, Collider2D, Coroutine/Timer | - | - | Xử lý di chuyển, va chạm, tấn công, knockback, cooldown, spawn enemy |
| **Environment Data** | Tilemap, Room Prefab, Spawn Point, Level Segment Data | - | - | Mỗi màn ghép từ nhiều phòng/segment có sẵn để tạo cảm giác random nhưng vẫn kiểm soát được |
| **Decision Support** | Rule-based Random, Weighted Random, Difficulty Scaling | - | - | Dùng luật đơn giản để random phòng, enemy, perk, boss; tăng độ khó theo stage/run |
| **Testing/Build** | Unity Test Framework, GitHub, GitHub Projects/Trello, Windows/WebGL Build | - | - | Test các hệ thống chính; quản lý task theo backlog; build demo |
| **ĐIỂM ĐẶC BIỆT CỦA ĐỀ TÀI** | | | | |
| **Playable Roguelite Loop** | Người chơi bắt đầu run, vượt phòng, đánh quái, nhận perk, đánh boss, kết thúc run và dùng tài nguyên để nâng cấp vĩnh viễn. | - | - | - |
| **Data-driven Upgrade & Combat** | Quản lý vũ khí, perk, enemy, boss và chỉ số bằng ScriptableObject/JSON để dễ chỉnh sửa và cân bằng. | - | - | - |
| **Procedural Level Generation** | Ghép các room/segment có sẵn để tạo màn chơi khác nhau qua từng run nhưng vẫn kiểm soát độ khó. | - | - | - |
| **Player Choices Affect Run** | Lựa chọn vũ khí, kỹ năng và perk ảnh hưởng trực tiếp đến lối chơi và khả năng vượt màn. | - | - | - |
| **Meta Progression & Resource** | Tài nguyên kiếm được sau run dùng để mở khóa nâng cấp, vũ khí hoặc kỹ năng vĩnh viễn. | - | - | - |
| **Run Result Report** | Hiển thị kết quả sau run: số phòng vượt qua, enemy/boss đã hạ, tài nguyên nhận được, thời gian chơi. | - | - | - |
| **Tài liệu bảo vệ** | Gồm mô tả đề tài, phạm vi MVP, kiến trúc, gameplay loop, level generation, upgrade system, demo và hướng phát triển. | - | - | - |

---

## Sheet: 📌 Tất Cả Stories

| Epic | Story ID | User Story | Tháng | Module | SP | Priority | Tags |
| :---: | :--- | :--- | :---: | :--- | :---: | :---: | :--- |
| **E01** | **US-001** | Khởi tạo dự án Unity 2D/URP: project settings, folder structure, scenes, input map, sorting layer | Tháng 6 | Engine | 3 | 🔴 High | Engine, Infra |
| **E01** | **US-002** | Thiết lập Git workflow: branch main/dev/feature, Git LFS cho asset, .gitignore Unity, quy tắc commit | Tháng 6 | DevOps | 3 | 🔴 High | Infra |
| **E01** | **US-003** | Tìm kiếm tài nguyên cần dùng cho game từ các nguồn khác nhau | Tháng 6 | Asset | 3 | 🔴 High | Resource |
| **E01** | **US-004** | Khởi tạo các Animation cơ bản cho player | Tháng 6 | Player | 2 | 🔴 High | Animation, GD |
| **E01** | **US-005** | Khởi tạo GameManager (Singleton) để điều phối các trạng thái hoạt động của game (Play, Pause, GameOver). | Tháng 6 | Architecture | 3 | 🔴 High | Engine |
| **E02** | **US-006** | Xây dựng PlayerController và State Machine xử lý di chuyển (Idle, Move, Jump, Fall) bằng Rigidbody2D cơ bản | Tháng 6 | Player | 3 | 🔴 High | Engine, GD |
| **E02** | **US-007** | Tạo PlayerStats để quản lí HP của player (Các trạng thái như Hit, Dead) | Tháng 6 | Player | 3 | 🔴 High | Gameplay |
| **E02** | **US-008** | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) tương tác và nhận diện kẻ địch | Tháng 6 | Combat | 3 | 🔴 High | Engine, Combat |
| **E02** | **US-009** | Xây dựng EnemyBase với máy trạng thái đơn giản: tự động rượt đuổi (Chase) và tấn công (Attack). | Tháng 6 | Enemy | 3 | 🔴 High | AI, Combat |
| **E03** | **US-010** | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0. | Tháng 6 | Combat | 3 | 🔴 High | Combat |
| **E03** | **US-011** | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và kích hoạt khóa phòng. | Tháng 6 | Room System | 3 | 🔴 High | Engine, GD |
| **E03** | **US-012** | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn ngay khi phòng bị khóa. | Tháng 6 | Room System | 2 | 🔴 High | Gameplay |
| **E03** | **US-013** | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết. | Tháng 6 | Room System | 3 | 🔴 High | Gameplay |
| **E04** | **US-014** | Thiết kế các Room Prefab cơ bản (Start, Combat, Reward, Boss) với Tilemap, Collider và các vị trí spawn quái. | Tháng 7 | Level | 3 | 🔴 High | Level Design |
| **E04** | **US-015** | Cài đặt hệ thống sinh map bán ngẫu nhiên (Semi-random), tự động ghép nối các Room Prefab theo luồng chạy của người chơi. | Tháng 7 | Architecture | 5 | 🔴 High | Algorithm, Level |
| **E04** | **US-016** | Xây dựng các loại Enemy prefab khác nhau, xây dựng Pause menu UI cơ bản | Tháng 7 | Architecture | 3 | 🟡 Medium | UI, Enemy |
| **E04** | **US-017** | Xử lý logic dịch chuyển và kết nối giữa các cửa (Doors), đảm bảo camera và Player di chuyển mượt mà qua các phòng. | Tháng 7 | Gameplay | 3 | 🔴 High | Engine, Camera |
| **E05** | **US-018** | Định nghĩa PerkData (ScriptableObject): id, tên, icon, loại effect (Stat modifier / Special behavior), giá trị, rarity (Common/Rare/Epic), quy tắc stack (stackable hay unique-only) | Tháng 7 | Data | 3 | 🔴 High | Data, System |
| **E05** | **US-019** | Xây dựng PerkPool + random có trọng số theo rarity, loại trừ perk đã đạt max stack khỏi vòng random | Tháng 7 | System | 3 | 🔴 High | Logic, Random |
| **E05** | **US-020** | UI Reward Card: hiển thị 3 lựa chọn Perk kèm icon, mô tả, màu theo rarity; chọn bằng click hoặc phím 1/2/3 | Tháng 7 | UI | 3 | 🔴 High | UI, Gameplay |
| **E05** | **US-021** | PerkEffectApplier tách riêng khỏi UpgradeManager: xử lý áp effect theo loại (cộng thẳng / nhân hệ số / effect đặc biệt như lifesteal, thorns) | Tháng 7 | System | 3 | 🟡 Medium | Logic, Stats |
| **E05** | **US-022** | UpgradeManager quản lý danh sách Perk active trong run, tự động clear khi Run kết thúc (Dead/Win), lưu lịch sử Perk để hiển thị ở màn Result | Tháng 7 | System | 5 | 🔴 High | Logic, Stats |
| **E06** | **US-023** | BossBase kế thừa EnemyBase, thêm Phase theo ngưỡng % HP; dùng lại animation clip có sẵn của asset (Idle/Move/Attack/Hit), chỉ đổi tốc độ playback và scale theo phase | Tháng 7 | Enemy | 5 | 🔴 High | AI, Boss |
| **E06** | **US-024** | Bộ 2 attack pattern phân biệt bằng hitbox và timing khác nhau trên cùng 1 animation clip (VD: cùng clip "swing" nhưng 1 pattern có hitbox dài+chậm, 1 pattern hitbox ngắn+nhanh liên hoàn) | Tháng 7 | Enemy | 5 | 🔴 High | AI, Combat |
| **E06** | **US-025** | Enrage ở Phase cuối: tăng tốc độ tấn công/di chuyển + đổi màu sprite (tint) bằng Material/Shader Graph để báo hiệu | Tháng 7 | Enemy | 3 | 🟢 Low | AI, Boss |
| **E06** | **US-026** | BossHealthBar UI: tên boss, thanh máu chia phase, hiệu ứng flash/shake UI khi chuyển phase (dùng animation UI thuần code, không cần icon custom) | Tháng 7 | UI | 3 | 🟡 Medium | UI, Boss |
| **E06** | **US-027** | Tích hợp Boss Room vào level generation, khóa cửa khi vào; thêm ambient VFX (particle có sẵn trong Unity/asset free) và SFX riêng cho arena để tạo cảm giác khác biệt thay vì tileset riêng | Tháng 7 | Room System | 3 | 🟡 Medium | Level, Boss |
| **E07** | **US-028** | Thiết kế SaveData serializable: PlayerProgressData, WeaponUnlockData, AbilityUnlockData, SettingData | Tháng 7 | Data | 3 | 🟡 Medium | Save/Load, Data |
| **E07** | **US-029** | SaveManager (Singleton): đọc/ghi JSON tại persistentDataPath | Tháng 7 | Architecture | 3 | 🔴 High | Save/Load |
| **E07** | **US-030** | Luồng Load tại startup: kiểm tra file tồn tại → load, hoặc tạo SaveData mặc định nếu chưa có | Tháng 7 | Architecture | 2 | 🔴 High | Save/Load |
| **E07** | **US-031** | Tích hợp điểm gọi Save cụ thể: kết thúc run (Dead/Win), mua Permanent Upgrade, đổi Setting | Tháng 7 | Architecture | 3 | 🟡 Medium | Save/Load |
| **E07** | **US-032** | Tách riêng lưu SettingData (âm lượng, độ phân giải, key binding) khỏi luồng save tiến trình chính | Tháng 7 | System | 2 | 🔴 High | Save/Load, Settings |
| **E07** | **US-033** | Xử lý file corrupt/thiếu: fallback về SaveData mặc định, log cảnh báo | Tháng 7 | Architecture | 2 | 🟡 Medium | Save/Load, QA |
| **E07** | **US-034** | Basic integrity check (checksum/hash đơn giản) để phát hiện file save bị chỉnh tay | Tháng 7 | Architecture | 3 | 🔴 High | Save/Load, QA |
| **E08** | **US-035** | WeaponData (ScriptableObject): damage, attack speed, range, kích thước/hình hitbox, reference VFX & SFX | Tháng 7 | Data | 3 | 🔴 High | Data, Combat |
| **E08** | **US-036** | WeaponManager: quản lý vũ khí đang trang bị, chuyển đổi giữa các vũ khí đã unlock | Tháng 7 | Player | 3 | 🔴 High | Engine, Combat |
| **E08** | **US-037** | Hitbox runtime switching: thay đổi kích thước/hình dạng/thời điểm bật hitbox theo WeaponData trên cùng 1 animation clip | Tháng 7 | Combat | 3 | 🔴 High | Engine, Combat |
| **E08** | **US-038** | Vũ khí cận chiến nhanh (VD: kiếm): hitbox ngắn, tốc độ cao, kèm VFX tia lửa nhỏ + SFX riêng | Tháng 7 | Player | 3 | 🔴 High | Combat, VFX |
| **E08** | **US-039** | Vũ khí cận chiến nặng (VD: búa): hitbox lớn, tốc độ chậm, kèm screen shake + particle bụi/impact khi trúng | Tháng 7 | Player | 3 | 🔴 High | Combat, VFX |
| **E08** | **US-040** | Vũ khí tầm xa (VD: cung): logic bắn projectile riêng (spawn, di chuyển, va chạm, destroy), dùng lại sprite projectile có sẵn | Tháng 7 | Combat | 5 | 🔴 High | Engine, Combat |
| **E08** | **US-041** | Liên kết Weapon Unlock với nâng cấp vĩnh viễn (mở khóa bằng tài nguyên tại Main Menu), icon dùng sprite có sẵn | Tháng 7 | Progression | 3 | 🔴 High | System, Data |
| **E08** | **US-042** | UI hotbar hiển thị vũ khí hiện tại + phím tắt chuyển đổi (VD: Q/scroll) + icon cooldown nếu có | Tháng 7 | UI | 3 | 🔴 High | UI, Combat |
| **E09** | **US-043** | Meta-currency rơi ra từ enemy/boss đã hạ trong run, cấu hình theo từng loại enemy | Tháng 7 | System | 3 | 🔴 High | Data, Progression |
| **E09** | **US-044** | Bonus tài nguyên cho các mốc đặc biệt (VD: lần đầu hạ 1 loại boss, clear toàn bộ room 1 run) | Tháng 7 | System | 2 | 🔴 High | Data, Progression |
| **E09** | **US-045** | Màn hình Upgrade vĩnh viễn ở Main Menu: danh sách nâng cấp, giá tiền, trạng thái đã mua/chưa mua | Tháng 7 | UI | 3 | 🔴 High | UI, Progression |
| **E09** | **US-046** | PermanentUpgradeManager: áp toàn bộ chỉ số vĩnh viễn đã mua vào PlayerStats khi bắt đầu run mới | Tháng 7 | System | 3 | 🔴 High | Logic, Save/Load |
| **E09** | **US-047** | Upgrade dạng nhiều bậc (VD: Max HP cấp 1/2/3, giá tăng dần theo bậc) thay vì mua 1 lần duy nhất | Tháng 7 | System | 3 | 🔴 High | Data, Progression |
| **E09** | **US-048** | Phản hồi khi mua thành công: SFX xác nhận + hiệu ứng UI (tween scale/flash), không cần asset hình ảnh mới | Tháng 7 | UI | 2 | 🔴 High | UI, VFX |
| **E09** | **US-049** | Kiểm thử tích hợp: đảm bảo Permanent Upgrade đồng bộ đúng với SaveData qua nhiều lần chơi/tắt-mở game | Tháng 7 | System | 2 | 🔴 High | QA, Save/Load |
| - | - | **TỔNG STORY POINTS** | - | - | **149** | - | - |

---

## Sheet: 🗓️ Roadmap
### 📅 Lộ trình 3 tháng — 2D Roguelite Action Game Unity

| Epic | Tên Epic | Nội dung chính | Quy mô (Story Points) |
| :---: | :--- | :--- | :---: |
| **E01** | Kiến trúc & Project Setup Unity | Khởi tạo project Unity, cấu trúc thư mục, scene, kiến trúc quản lý game, event system và workflow Git | 14 SP |
| **E02** | Core Roguelite Gameplay Loop | Xây dựng nhân vật, di chuyển cơ bản, trạng thái máu và quái đơn giản | 12 SP |
| **E03** | Player Combat & Basic Enemy System | Đấu quái, hệ thống khóa phòng (Room System) và cơ chế sinh quái | 11 SP |
| **E04** | Thiết kế & Tạo Màn chơi bán ngẫu nhiên (Semi-random Level Gen) | Thiết kế Room Prefab, giải thuật sinh map bán ngẫu nhiên, kết nối cửa phòng và camera mượt mà | 14 SP |
| **E05** | Upgrade System & Meta Progression | Cấu hình Perks (ScriptableObject), PerkPool có trọng số, UI Reward Card, PerkEffectApplier tách biệt và UpgradeManager | 17 SP |
| **E06** | Boss Fight & Arena | AI Boss phân chia phase, attack patterns, thanh HP UI chuyên dụng, cơ chế Enrage và tích hợp Boss Room | 19 SP |
| **E07** | Save/Load System | Thiết kế SaveData, triển khai SaveManager lưu trữ JSON, quản lý file lỗi/chỉnh sửa trái phép | 18 SP |
| **E08** | Weapon System | Quản lý WeaponData ScriptableObject, WeaponManager đổi vũ khí, hitbox runtime và projectile tầm xa | 26 SP |
| **E09** | Meta-currency & Permanent Upgrades | Tích lũy meta-currency, nâng cấp vĩnh viễn nhiều bậc tại Main Menu, áp chỉ số vào PlayerStats | 18 SP |

---

## Sheet: ⚒️ Phân công

### 👥 Phân công nhiệm vụ chính của thành viên

| STT | Tên thành viên | Vai trò chính | Nhiệm vụ phụ trách | Kết quả dự kiến |
| :---: | :--- | :--- | :--- | :--- |
| 1 | Trần Việt Anh | Leader / Gameplay Dev | Quản lý tiến độ, chia task, tổng hợp build; xây dựng core gameplay loop; phối hợp các module chính | Game loop hoàn chỉnh: Start Run → Clear Room → Nhận Reward → Boss → End Run |
| 2 | Phùng Phạm Gia Bảo | Game Designer / Gameplay Dev | Thiết kế và lập trình cơ chế chiến đấu, Xây dựng AI cho quái thường và Boss; Thiết kế các module chính | Hệ thống combat mượt mà, Có ít nhất 1-2 loại quái với hành vi AI khác nhau và 1 Boss hoàn chỉnh có bộ kỹ năng riêng. |
| 3 | Nguyễn Mạnh Thắng | UI / Data / Documentation | Làm UI menu, HUD, pause, result screen; quản lý ScriptableObject/JSON; viết tài liệu, slide, báo cáo | Có giao diện chơi được, màn hình kết quả run, tài liệu mô tả hệ thống |
| 4 | Phạm Bình Định | Progression / System Dev | Xây dựng hệ thống tài nguyên, perk/reward, nâng cấp vĩnh viễn, save/load tiến trình; hỗ trợ cân bằng chỉ số gameplay | Có hệ thống nâng cấp hoạt động, người chơi nhận tài nguyên sau run, mở khóa nâng cấp vĩnh viễn và lưu được tiến trình |
| 5 | Đặng Đình Tuyên | Level / System Designer | Thiết kế room prefab, tilemap, spawn point, hệ thống ghép phòng ngẫu nhiên/bán ngẫu nhiên, cân bằng màn chơi | Có hệ thống tạo màn chơi theo room/segment, mỗi run có bố cục khác nhau |

### 🧩 Phân chia Module phụ

| Module | Người phụ trách chính | Người hỗ trợ | Mức ưu tiên | Ghi chú |
| :--- | :--- | :--- | :---: | :--- |
| **Core Gameplay Loop** | Phùng Phạm Gia Bảo | Trần Việt Anh | 🔴 Cao | Bắt buộc có trong MVP |
| **Player Movement & Combat** | Phùng Phạm Gia Bảo | Đặng Đình Tuyên | 🔴 Cao | Là phần cảm giác chơi chính |
| **Enemy System** | Trần Việt Anh | Nguyễn Mạnh Thắng | 🔴 Cao | Enemy thường cần xong trước boss |
| **Boss System** | Trần Việt Anh | Phùng Phạm Gia Bảo | 🔴 Cao | Chỉ cần 1 boss tốt cho demo |
| **Level Generation** | Phùng Phạm Gia Bảo | Đặng Đình Tuyên | 🔴 Cao | Ưu tiên semi-random, không cần procedural quá phức tạp |
| **Weapon System** | Đặng Đình Tuyên | Nguyễn Mạnh Thắng | 🔴 Cao | Có thể bắt đầu với 2–3 vũ khí |
| **Perk / Temporary Upgrade** | Nguyễn Mạnh Thắng | Phạm Bình Định | 🔴 Cao | Là đặc trưng roguelite quan trọng |
| **Permanent Upgrade / Save System** | Đặng Đình Tuyên | Trần Việt Anh | 🟡 Trung bình | Lưu tài nguyên, mở khóa nâng cấp |
| **UI / HUD** | Nguyễn Mạnh Thắng | Phạm Bình Định | 🔴 Cao | Máu, tài nguyên, perk, pause, result |
| **Art / Placeholder / Audio** | Đặng Đình Tuyên | Phạm Bình Định | 🟡 Trung bình | Có thể dùng asset free trước |
| **Testing / Bug fix** | Phùng Phạm Gia Bảo | Trần Việt Anh | 🔴 Cao | Test build mỗi giai đoạn hoặc thường xuyên |
| **Báo cáo / Slide / Demo Script** | Phạm Bình Định | Nguyễn Mạnh Thắng | 🔴 Cao | Chuẩn bị cho mentor và bảo vệ |

---

## Sheet: E01
### ⚙️ Epic E01: Kiến trúc & Project Setup Unity (Tháng 6 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-001** | Khởi tạo dự án Unity 2D/URP: project settings, folder structure, scenes, input map, sorting layer | Engine | Engine, Infra | 3 | Thiết lập cấu trúc thư mục code chuẩn trong `Assets/Scripts/`. |
| **US-002** | Thiết lập Git workflow: branch main/dev/feature, Git LFS cho asset, .gitignore Unity, quy tắc commit | DevOps | Infra | 3 | Đã cấu hình `.gitignore`, `.gitattributes` và khởi tạo repo Git. |
| **US-003** | Tìm kiếm tài nguyên cần dùng cho game từ các nguồn khác nhau | Asset | Resource | 3 | Các asset đồ họa đã được nhập vào thư mục `Assets/Arts`. |
| **US-004** | Khởi tạo các Animation cơ bản cho player | Player | Animation, GD | 2 | Người dùng đã tự thiết lập các animation cơ bản cho Player. |
| **US-005** | Khởi tạo GameManager (Singleton) để điều phối các trạng thái hoạt động của game (Play, Pause, GameOver). | Architecture | Engine | 3 | Tạo `GameManager.cs` Singleton điều phối trạng thái game và chuyển đổi scene. |
| - | - | **TỔNG STORY POINTS** | - | **14** | |

---

## Sheet: E02
### 🌱 Epic E02: Core Roguelite Gameplay Loop (Tháng 6 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-006** | Xây dựng PlayerController và State Machine xử lý di chuyển (Idle, Move, Jump, Fall) bằng Rigidbody2D cơ bản | Player | Engine, GD | 3 | Đã hoàn thiện PlayerController, PlayerStateMachine và các trạng thái di chuyển (Idle, Move, Jump, Fall) sử dụng Rigidbody2D. |
| **US-007** | Tạo PlayerStats để quản lí HP của player (Các trạng thái như Hit, Dead) | Player | Gameplay | 3 | Đã tạo PlayerStats quản lý HP, sự kiện nhận sát thương (Hit) và vô hiệu hóa điều khiển / kích hoạt GameManager GameOver khi chết (Dead). |
| **US-008** | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) tương tác và nhận diện kẻ địch | Combat | Engine, Combat | 3 | Đã tạo module `Attack.cs` quản lý hitbox trigger collider và đăng ký input `onAttack` kích hoạt hoạt ảnh tấn công của Player. |
| **US-009** | Xây dựng EnemyBase với máy trạng thái đơn giản: tự động rượt đuổi (Chase) và tấn công (Attack). | Enemy | AI, Combat | 3 | Đã tạo `EnemyBase.cs` abstract class với State Machine 5 trạng thái, tích hợp IDamageable. |
| - | - | **TỔNG STORY POINTS** | - | **12** | |

---

## Sheet: E03
### 🗺️ Epic E03: Player Combat & Basic Enemy System (Tháng 6 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-010** | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0. | Combat | Combat | 3 | Đã kết nối `TakeDamage` qua giao diện `IDamageable` giữa `PlayerStats` và `EnemyBase` qua `Attack.cs`, áp dụng lực đẩy lùi (Knockback) và vô hiệu hóa/hủy thực thể khi HP <= 0. |
| **US-011** | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và kích hoạt khóa phòng. | Room System | Engine, GD | 3 | Quản lý logic cửa và kích hoạt trạng thái chiến đấu phòng. |
| **US-012** | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn ngay khi phòng bị khóa. | Room System | Gameplay | 2 | Tự động hóa việc spawn quái. |
| **US-013** | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết. | Room System | Gameplay | 3 | Kết thúc thử thách trong phòng và mở lối đi tiếp. |
| - | - | **TỔNG STORY POINTS** | - | **11** | |

---

## Sheet: E04
### 🧱 Epic E04: Thiết kế & Tạo Màn chơi bán ngẫu nhiên (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-014** | Thiết kế các Room Prefab cơ bản (Start, Combat, Reward, Boss) với Tilemap, Collider và các vị trí spawn quái. | Level | Level Design | 3 | Cấu hình các prefab phòng hoàn chỉnh với chướng ngại vật và cửa ra vào. |
| **US-015** | Cài đặt hệ thống sinh map bán ngẫu nhiên (Semi-random), tự động ghép nối các Room Prefab theo luồng chạy của người chơi. | Architecture | Algorithm, Level | 5 | Thiết lập hệ thống tạo màn dựa trên cấu hình phòng có sẵn. |
| **US-016** | Xây dựng các loại Enemy prefab khác nhau, xây dựng Pause menu UI cơ bản | Architecture | UI, Enemy | 3 | Tạo biến thể các loại quái thường và xây dựng màn hình Menu Pause. |
| **US-017** | Xử lý logic dịch chuyển và kết nối giữa các cửa (Doors), đảm bảo camera và Player di chuyển mượt mà qua các phòng. | Gameplay | Engine, Camera | 3 | Đồng bộ hóa vị trí camera và người chơi thông qua cửa dịch chuyển. |
| - | - | **TỔNG STORY POINTS** | - | **14** | |

---

## Sheet: E05
### 🧬 Epic E05: Upgrade System & Meta Progression (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-018** | Định nghĩa PerkData (ScriptableObject): id, tên, icon, loại effect (Stat modifier / Special behavior), giá trị, rarity (Common/Rare/Epic), quy tắc stack (stackable hay unique-only) | Data | Data, System | 3 | Tạo ScriptableObject cấu hình chi tiết cho các Perk với đầy đủ thuộc tính và quy tắc stack. |
| **US-019** | Xây dựng PerkPool + random có trọng số theo rarity, loại trừ perk đã đạt max stack khỏi vòng random | System | Logic, Random | 3 | Lập trình bể PerkPool hỗ trợ weighted random theo tỷ lệ rarity và bộ lọc bỏ các Perk đã tối đa lượt nâng cấp. |
| **US-020** | UI Reward Card: hiển thị 3 lựa chọn Perk kèm icon, mô tả, màu theo rarity; chọn bằng click hoặc phím 1/2/3 | UI | UI, Gameplay | 3 | Giao diện lựa chọn Perk chuyên nghiệp, hỗ trợ tương tác chuột và phím tắt số 1/2/3. |
| **US-021** | PerkEffectApplier tách riêng khỏi UpgradeManager: xử lý áp effect theo loại (cộng thẳng / nhân hệ số / effect đặc biệt như lifesteal, thorns) | System | Logic, Stats | 3 | Tách riêng lớp applier chịu trách nhiệm tính toán và cộng dồn các kiểu chỉ số phức tạp hoặc hiệu ứng đặc biệt. |
| **US-022** | UpgradeManager quản lý danh sách Perk active trong run, tự động clear khi Run kết thúc (Dead/Win), lưu lịch sử Perk để hiển thị ở màn Result | System | Logic, Stats | 5 | Lớp quản lý vòng đời Perk hoạt động trong màn chơi, tự động reset chỉ số và lưu lại vết thông tin Perks đã chọn. |
| - | - | **TỔNG STORY POINTS** | - | **17** | |

---

## Sheet: E06
### 👹 Epic E06: Boss Fight & Arena (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-023** | BossBase kế thừa EnemyBase, thêm Phase theo ngưỡng % HP; dùng lại animation clip có sẵn của asset (Idle/Move/Attack/Hit), chỉ đổi tốc độ playback và scale theo phase | Enemy | AI, Boss | 5 | Tạo lớp `BossBase.cs` thừa kế `EnemyBase.cs`, kiểm tra ngưỡng HP để chuyển đổi Phase động. |
| **US-024** | Bộ 2 attack pattern phân biệt bằng hitbox và timing khác nhau trên cùng 1 animation clip (VD: cùng clip "swing" nhưng 1 pattern có hitbox dài+chậm, 1 pattern hitbox ngắn+nhanh liên hoàn) | Enemy | AI, Combat | 5 | Triển khai cơ chế tùy biến hitbox và thời điểm kích hoạt hitbox cho các chuỗi đòn đánh của Boss. |
| **US-025** | Enrage ở Phase cuối: tăng tốc độ tấn công/di chuyển + đổi màu sprite (tint) bằng Material/Shader Graph để báo hiệu | Enemy | AI, Boss | 3 | Áp dụng hiệu ứng đồ họa (Shader/Material) và điều chỉnh tốc độ di chuyển/tấn công khi Boss enrage. |
| **US-026** | BossHealthBar UI: tên boss, thanh máu chia phase, hiệu ứng flash/shake UI khi chuyển phase (dùng animation UI thuần code, không cần icon custom) | UI | UI, Boss | 3 | UI hiển thị thanh HP đặc biệt cho Boss kèm hiệu ứng chuyển pha sinh động. |
| **US-027** | Tích hợp Boss Room vào level generation, khóa cửa khi vào; thêm ambient VFX (particle có sẵn trong Unity/asset free) và SFX riêng cho arena để tạo cảm giác khác biệt thay vì tileset riêng | Room System | Level, Boss | 3 | Tích hợp phòng Boss vào hệ thống sinh màn, tự động khóa cửa và kích hoạt hiệu ứng VFX/SFX khi bắt đầu đấu Boss. |
| - | - | **TỔNG STORY POINTS** | - | **19** | |

---

## Sheet: E07
### 💾 Epic E07: Save/Load System (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-028** | Thiết kế SaveData serializable: PlayerProgressData, WeaponUnlockData, AbilityUnlockData, SettingData | Data | Save/Load, Data | 3 | Xây dựng các struct/class chứa dữ liệu save có thuộc tính `[System.Serializable]`. |
| **US-029** | SaveManager (Singleton): đọc/ghi JSON tại persistentDataPath | Architecture | Save/Load | 3 | Tạo `SaveManager` ghi dữ liệu đã được serialize thành JSON xuống ổ đĩa. |
| **US-030** | Luồng Load tại startup: kiểm tra file tồn tại → load, hoặc tạo SaveData mặc định nếu chưa có | Architecture | Save/Load | 2 | Tự động kiểm tra và khởi tạo file lưu trữ mặc định nếu chưa tồn tại ở startup. |
| **US-031** | Tích hợp điểm gọi Save cụ thể: kết thúc run (Dead/Win), mua Permanent Upgrade, đổi Setting | Architecture | Save/Load | 3 | Đăng ký sự kiện tự động lưu tại các điểm mốc quan trọng của gameflow. |
| **US-032** | Tách riêng lưu SettingData (âm lượng, độ phân giải, key binding) khỏi luồng save tiến trình chính | System | Save/Load, Settings | 2 | Tách biệt cấu hình thiết lập hệ thống và tiến trình chơi game thành 2 file lưu trữ độc lập. |
| **US-033** | Xử lý file corrupt/thiếu: fallback về SaveData mặc định, log cảnh báo | Architecture | Save/Load, QA | 2 | Bắt ngoại lệ khi đọc file lỗi và tự động khôi phục cấu hình mặc định để tránh crash game. |
| **US-034** | Basic integrity check (checksum/hash đơn giản) để phát hiện file save bị chỉnh tay | Architecture | Save/Load, QA | 3 | Triển khai mã hóa băm hoặc checksum đơn giản để bảo mật file save không bị sửa đổi tùy tiện. |
| - | - | **TỔNG STORY POINTS** | - | **18** | |

---

## Sheet: E08
### ⚔️ Epic E08: Weapon System (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-035** | WeaponData (ScriptableObject): damage, attack speed, range, kích thước/hình hitbox, reference VFX & SFX | Data | Data, Combat | 3 | Tạo ScriptableObject định nghĩa toàn bộ thuộc tính cấu hình cho từng vũ khí. |
| **US-036** | WeaponManager: quản lý vũ khí đang trang bị, chuyển đổi giữa các vũ khí đã unlock | Player | Engine, Combat | 3 | Quản lý kho vũ khí của người chơi và cho phép chuyển đổi nhanh trong trận đấu. |
| **US-037** | Hitbox runtime switching: thay đổi kích thước/hình dạng/thời điểm bật hitbox theo WeaponData trên cùng 1 animation clip | Combat | Engine, Combat | 3 | Đồng bộ hóa thay đổi hitbox runtime dựa trên thông số của vũ khí hiện tại đang sử dụng. |
| **US-038** | Vũ khí cận chiến nhanh (VD: kiếm): hitbox ngắn, tốc độ cao, kèm VFX tia lửa nhỏ + SFX riêng | Player | Combat, VFX | 3 | Tích hợp vũ khí tấn công nhanh với các hiệu ứng âm thanh/hình ảnh tương ứng. |
| **US-039** | Vũ khí cận chiến nặng (VD: búa): hitbox lớn, tốc độ chậm, kèm screen shake + particle bụi/impact khi trúng | Player | Combat, VFX | 3 | Tích hợp vũ khí tấn công chậm có lực sát thương lớn kèm hiệu ứng rung màn hình và bụi hạt. |
| **US-040** | Vũ khí tầm xa (VD: cung): logic bắn projectile riêng (spawn, di chuyển, va chạm, destroy), dùng lại sprite projectile có sẵn | Combat | Engine, Combat | 5 | Lập trình logic đạn bay (Projectile) hoàn chỉnh bao gồm tốc độ bay và xử lý va chạm. |
| **US-041** | Liên kết Weapon Unlock với nâng cấp vĩnh viễn (mở khóa bằng tài nguyên tại Main Menu), icon dùng sprite có sẵn | Progression | System, Data | 3 | Tích hợp mở khóa vũ khí mới vào hệ thống nâng cấp vĩnh viễn ngoài Menu. |
| **US-042** | UI hotbar hiển thị vũ khí hiện tại + phím tắt chuyển đổi (VD: Q/scroll) + icon cooldown nếu có | UI | UI, Combat | 3 | Thiết kế thanh Hotbar trên HUD hiển thị vũ khí đang dùng và hỗ trợ phím tắt chuyển đổi nhanh. |
| - | - | **TỔNG STORY POINTS** | - | **26** | |

---

## Sheet: E09
### 🪙 Epic E09: Meta-currency & Permanent Upgrades (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-043** | Meta-currency rơi ra từ enemy/boss đã hạ trong run, cấu hình theo từng loại enemy | System | Data, Progression | 3 | Thiết lập cơ chế rơi tài nguyên (meta-currency) từ kẻ thù khi bị tiêu diệt. |
| **US-044** | Bonus tài nguyên cho các mốc đặc biệt (VD: lần đầu hạ 1 loại boss, clear toàn bộ room 1 run) | System | Data, Progression | 2 | Tặng thêm tài nguyên cho người chơi khi đạt được các thành tựu cụ thể trong quá trình chơi. |
| **US-045** | Màn hình Upgrade vĩnh viễn ở Main Menu: danh sách nâng cấp, giá tiền, trạng thái đã mua/chưa mua | UI | UI, Progression | 3 | Xây dựng giao diện menu nâng cấp vĩnh viễn rõ ràng, trực quan cho người chơi. |
| **US-046** | PermanentUpgradeManager: áp toàn bộ chỉ số vĩnh viễn đã mua vào PlayerStats khi bắt đầu run mới | System | Logic, Save/Load | 3 | Đồng bộ hóa các chỉ số nâng cấp vĩnh viễn đã lưu vào thực thể Player khi khởi tạo run chơi mới. |
| **US-047** | Upgrade dạng nhiều bậc (VD: Max HP cấp 1/2/3, giá tăng dần theo bậc) thay vì mua 1 lần duy nhất | System | Data, Progression | 3 | Thiết lập cấu trúc nâng cấp lũy tiến theo từng cấp độ và tăng dần chi phí mua. |
| **US-048** | Phản hồi khi mua thành công: SFX xác nhận + hiệu ứng UI (tween scale/flash), không cần asset hình ảnh mới | UI | UI, VFX | 2 | Tạo hiệu ứng thị giác và âm thanh phản hồi sống động khi người chơi thực hiện giao dịch nâng cấp thành công. |
| **US-049** | Kiểm thử tích hợp: đảm bảo Permanent Upgrade đồng bộ đúng với SaveData qua nhiều lần chơi/tắt-mở game | System | QA, Save/Load | 2 | Đảm bảo tính nhất quán dữ liệu nâng cấp vĩnh viễn xuyên suốt vòng đời ứng dụng. |
| - | - | **TỔNG STORY POINTS** | - | **18** | |
