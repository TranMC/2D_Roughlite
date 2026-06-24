# 2D Roguelite Unity Project Backlog

## Sheet: 📋 Tổng Quan

| Danh mục | Chi tiết | Phân loại | Story Points | Ghi chú |
| :--- | :--- | :--- | :---: | :--- |
| **Dự án** | ⚔️ 2D ROUGHLITE MANAGEMENT — UNITY GAME PROJECT | - | - | - |
| **Thông tin** | Roguelite 2D | Unity 2D | - | Lịch trình 3 tháng |
| **Quy mô** | 6 Epics | 25 User Stories | **81 SP** | Phân chia thành các giai đoạn thực hiện |
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
| **E05** | **US-018** | Tạo cấu trúc ScriptableObject để quản lý dữ liệu cấu hình của các loại Perk tạm thời (Tăng HP, Tăng Damage, Tốc đánh). | Tháng 7 | Data | 3 | 🔴 High | Data, System |
| **E05** | **US-019** | Xây dựng màn hình UI Reward hiển thị ngẫu nhiên các lựa chọn Perk nâng cấp cho Player sau khi clear Room. | Tháng 7 | UI | 3 | 🔴 High | UI, Gameplay |
| **E05** | **US-020** | Lập trình UpgradeManager xử lý cộng dồn chỉ số từ Perk vào PlayerStats và tự động reset trạng thái sau khi kết thúc Run (Dead/Win). | Tháng 7 | System | 3 | 🔴 High | Logic, Stats |
| **E05** | **US-021** | Xây dựng hệ thống quản lý tài nguyên Meta-progression thu thập được để mở khóa nâng cấp vĩnh viễn ở Main Menu. | Tháng 7 | Progression | 5 | 🟡 Medium | System, Data |
| **E06** | **US-022** | Lập trình AI cho Boss dựa trên EnemyBase với các State chiến đấu phức tạp hơn (VD: Attack diện rộng, Enrage khi máu thấp). | Tháng 7 | Enemy | 5 | 🔴 High | AI, Boss |
| **E06** | **US-023** | Tích hợp Boss Room vào khâu Level Generation và cấu hình thanh HP Bar chuyên dụng hiển thị trên UI khi đối đầu Boss. | Tháng 7 | Boss | 3 | 🔴 High | UI, Level |
| **E06** | **US-024** | Cài đặt hệ thống Save/Load dữ liệu (JSON) để lưu trữ vĩnh viễn tiến trình mở khóa nâng cấp và tài nguyên của người chơi. | Tháng 7 | Architecture | 5 | 🔴 High | Save/Load, Data |
| **E06** | **US-025** | Hoàn thiện luồng UX toàn game: Kết nối Main Menu vào vòng lặp Run, hiển thị thông số chi tiết ở màn hình Result (Victory/Defeat). | Tháng 7 | UI | 3 | 🔴 High | UX, HUD |
| - | - | **TỔNG STORY POINTS** | - | - | **81** | - | - |

---

## Sheet: 🗓️ Roadmap
### 📅 Lộ trình 3 tháng — 2D Roguelite Action Game Unity

| Epic | Tên Epic | Nội dung chính | Quy mô (Story Points) |
| :---: | :--- | :--- | :---: |
| **E01** | Kiến trúc & Project Setup Unity | Khởi tạo project Unity, cấu trúc thư mục, scene, kiến trúc quản lý game, event system và workflow Git | 14 SP |
| **E02** | Core Roguelite Gameplay Loop | Xây dựng nhân vật, di chuyển cơ bản, trạng thái máu và quái đơn giản | 12 SP |
| **E03** | Player Combat & Basic Enemy System | Đấu quái, hệ thống khóa phòng (Room System) và cơ chế sinh quái | 11 SP |
| **E04** | Thiết kế & Tạo Màn chơi bán ngẫu nhiên (Semi-random Level Gen) | Thiết kế Room Prefab, giải thuật sinh map bán ngẫu nhiên, kết nối cửa phòng và camera mượt mà | 14 SP |
| **E05** | Upgrade System & Meta Progression | Cấu hình Perks tạm thời (ScriptableObject), UI Reward nâng cấp, hệ thống nâng cấp vĩnh viễn ở Menu | 14 SP |
| **E06** | Boss Fight & UX Polish & Save/Load | AI Boss nâng cao, tích hợp phòng Boss, hệ thống lưu game JSON và hoàn thiện luồng UX/UI toàn game | 16 SP |

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
| **US-018** | Tạo cấu trúc ScriptableObject để quản lý dữ liệu cấu hình của các loại Perk tạm thời (Tăng HP, Tăng Damage, Tốc đánh). | Data | Data, System | 3 | Tạo data-driven cho danh sách nâng cấp lâm thời trong run chơi. |
| **US-019** | Xây dựng màn hình UI Reward hiển thị ngẫu nhiên các lựa chọn Perk nâng cấp cho Player sau khi clear Room. | UI | UI, Gameplay | 3 | UI hiển thị thẻ bài lựa chọn phần thưởng ngẫu nhiên. |
| **US-020** | Lập trình UpgradeManager xử lý cộng dồn chỉ số từ Perk vào PlayerStats và tự động reset trạng thái sau khi kết thúc Run (Dead/Win). | System | Logic, Stats | 3 | Quản lý logic nâng cấp chỉ số động trong suốt một run chơi. |
| **US-021** | Xây dựng hệ thống quản lý tài nguyên Meta-progression thu thập được để mở khóa nâng cấp vĩnh viễn ở Main Menu. | Progression | System, Data | 5 | Tạo cơ chế tích lũy tài nguyên sau khi kết thúc run và giao diện nâng cấp vĩnh viễn. |
| - | - | **TỔNG STORY POINTS** | - | **14** | |

---

## Sheet: E06
### 👹 Epic E06: Boss Fight & UX Polish & Save/Load System (Tháng 7 | Độ ưu tiên: 🔴 High)

| Story ID | User Story | Module | Tags | SP | Ghi chú / Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **US-022** | Lập trình AI cho Boss dựa trên EnemyBase với các State chiến đấu phức tạp hơn (VD: Attack diện rộng, Enrage khi máu thấp). | Enemy | AI, Boss | 5 | Xây dựng AI Boss có cơ chế chia pha (Phase) và các kỹ năng tấn công diện rộng. |
| **US-023** | Tích hợp Boss Room vào khâu Level Generation và cấu hình thanh HP Bar chuyên dụng hiển thị trên UI khi đối đầu Boss. | Boss | UI, Level | 3 | Tích hợp phòng trùm cuối và UI thanh máu Boss động trên HUD. |
| **US-024** | Cài đặt hệ thống Save/Load dữ liệu (JSON) để lưu trữ vĩnh viễn tiến trình mở khóa nâng cấp và tài nguyên của người chơi. | Architecture | Save/Load, Data | 5 | Hệ thống lưu trữ dữ liệu vĩnh viễn xuống bộ nhớ thiết bị. |
| **US-025** | Hoàn thiện luồng UX toàn game: Kết nối Main Menu vào vòng lặp Run, hiển thị thông số chi tiết ở màn hình Result (Victory/Defeat). | UI | UX, HUD | 3 | Đồng bộ hóa toàn bộ luồng trải nghiệm người chơi từ menu, bắt đầu run, kết thúc run. |
| - | - | **TỔNG STORY POINTS** | - | **16** | |

---

## Sheet: E07 đến E13 (Chưa định hình)

Các Epic từ E07 đến E13 hiện chưa có User Story chi tiết và sẽ được phân rã ở các giai đoạn sau của dự án.
