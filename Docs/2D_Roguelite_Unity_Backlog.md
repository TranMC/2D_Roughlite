# 2D_Roguelite_Unity_Backlog.xlsx

## Sheet: 📋 Tổng Quan

| Unnamed: 1                                       | Unnamed: 2                                                                                                                  | Unnamed: 3                   | Unnamed: 4   | Unnamed: 5                                                                                 |
|:-------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------------|:-----------------------------|:-------------|:-------------------------------------------------------------------------------------------|
| ⚔️  2D ROUGHLITE MANAGEMENT — UNITY GAME PROJECT | nan                                                                                                                         | nan                          | nan          | nan                                                                                        |
| Roguelite 2D  |  Unity 2D  |  3 tháng            | nan                                                                                                                         | nan                          | nan          | nan                                                                                        |
| 3                                                | 13                                                                                                                          | nan                          | 50           | 3                                                                                          |
| Epics                                            | User Stories                                                                                                                | Simulation/Decision Features | Story Points | Tháng                                                                                      |
| TECH STACK ĐỀ XUẤT                               | nan                                                                                                                         | nan                          | nan          | nan                                                                                        |
| Layer                                            | Công nghệ                                                                                                                   | nan                          | nan          | Ghi chú                                                                                    |
| Game Engine                                      | Unity 2022.3 LTS (2022.3.62f2) + C#                                                                                         | nan                          | nan          | Ưu tiên Unity 2D/URP; dễ demo và build WebGL/Windows                                       |
| Render/Visual                                    | URP 2D Renderer, Tilemap, Sprite/Placeholder                                                                                | nan                          | nan          | Asset nhân vật, khu vực có thể dùng free/mua; trọng tâm là gameflow                        |
| Data-Driven Config                               | ScriptableObject, JSON Config                                                                                               | nan                          | nan          | Dùng để cấu hình vũ khí, enemy, perk, chỉ số, level, boss mà không phải sửa code nhiều     |
| Gameplay Architecture                            | Component-Based Architecture, State Machine, Event System                                                                   | nan                          | nan          | Dễ chia module: Player, Enemy, Combat, UI, Upgrade, Level; tránh code dính chặt vào nhau   |
| Simulation Logic                                 | Unity Physics 2D, Rigidbody2D, Collider2D, Coroutine/Timer                                                                  | nan                          | nan          | Xử lý di chuyển, va chạm, tấn công, knockback, cooldown, spawn enemy                       |
| Environment Data                                 | Tilemap, Room Prefab, Spawn Point, Level Segment Data                                                                       | nan                          | nan          | Mỗi màn ghép từ nhiều phòng/segment có sẵn để tạo cảm giác random nhưng vẫn kiểm soát được |
| Decision Support                                 | Rule-based Random, Weighted Random, Difficulty Scaling                                                                      | nan                          | nan          | Dùng luật đơn giản để random phòng, enemy, perk, boss; tăng độ khó theo stage/run          |
| Testing/Build                                    | Unity Test Framework, GitHub, GitHub Projects/Trello, Windows/WebGL Build                                                   | nan                          | nan          | Test các hệ thống chính; quản lý task theo backlog; build demo                             |
| ĐIỂM ĐẶC BIỆT CỦA ĐỀ TÀI ()                      | nan                                                                                                                         | nan                          | nan          | nan                                                                                        |
| Playable Roguelite Loop                          | Người chơi bắt đầu run, vượt phòng, đánh quái, nhận perk, đánh boss, kết thúc run và dùng tài nguyên để nâng cấp vĩnh viễn. | nan                          | nan          | nan                                                                                        |
| Data-driven Upgrade & Combat System              | Quản lý vũ khí, perk, enemy, boss và chỉ số bằng ScriptableObject/JSON để dễ chỉnh sửa và cân bằng.                         | nan                          | nan          | nan                                                                                        |
| Procedural / Semi-random Level Generation        | Ghép các room/segment có sẵn để tạo màn chơi khác nhau qua từng run nhưng vẫn kiểm soát độ khó.                             | nan                          | nan          | nan                                                                                        |
| Player Choices Affect Run Strength               | Lựa chọn vũ khí, kỹ năng và perk ảnh hưởng trực tiếp đến lối chơi và khả năng vượt màn.                                     | nan                          | nan          | nan                                                                                        |
| Meta Progression & Resource Management           | Tài nguyên kiếm được sau run dùng để mở khóa nâng cấp, vũ khí hoặc kỹ năng vĩnh viễn.                                       | nan                          | nan          | nan                                                                                        |
| Run Result Report                                | Hiển thị kết quả sau run: số phòng vượt qua, enemy/boss đã hạ, tài nguyên nhận được, thời gian chơi.                        | nan                          | nan          | nan                                                                                        |
| Tài liệu bảo vệ                                  | Gồm mô tả đề tài, phạm vi MVP, kiến trúc, gameplay loop, level generation, upgrade system, demo và hướng phát triển.        | nan                          | nan          | nan                                                                                        |

## Sheet: 📌 Tất Cả Stories

| Unnamed: 1                                                      | Unnamed: 2   | Unnamed: 3                                                                                                     | Unnamed: 4   | Unnamed: 5   | Unnamed: 6   | Unnamed: 7   | Unnamed: 8     |
|:----------------------------------------------------------------|:-------------|:---------------------------------------------------------------------------------------------------------------|:-------------|:-------------|:-------------|:-------------|:---------------|
| MASTER BACKLOG — 2D ROGUELITE ACTION GAME UNITY ( USER STORIES) | nan          | nan                                                                                                            | nan          | nan          | nan          | nan          | nan            |
| Epic                                                            | Story ID     | User Story                                                                                                     | Tháng        | Module       | SP           | Priority     | Tags           |
| E01                                                             | US-001       | Khởi tạo dự án Unity 2D/URP: project settings, folder structure, scenes, input map, sorting layer              | Tháng 6      | Engine       | 3            | 🔴 High       | Engine, Infra  |
| nan                                                             | US-002       | Thiết lập Git workflow: branch main/dev/feature, Git LFS cho asset, .gitignore Unity, quy tắc commit           | Tháng 6      | DevOps       | 3            | 🔴 High       | Infra          |
| nan                                                             | US-003       | Tìm kiếm tài nguyên cần dùng cho game từ các nguồn khác nhau                                                   | Tháng 6      | Asset        | 3            | 🔴 High       | Resource       |
| nan                                                             | US-004       | Khởi tạo các Animation cơ bản cho player                                                                       | Tháng 6      | Player       | 2            | 🔴 High       | Animation, GD  |
| nan                                                             | US-005       | Khởi tạo GameManager (Singleton) để điều phối các trạng thái hoạt động của game (Play, Pause, GameOver).       | Tháng 6      | Architecture | 3            | 🔴 High       | Engine         |
| E02                                                             | US-006       | Xây dựng PlayerController và State Machine xử lý di chuyển (Idle, Move, Jump, Fall) bằng Rigidbody2D cơ bản    | Tháng 6      | Player       | 3            | 🔴 High       | Engine, GD     |
| nan                                                             | US-007       | Tạo PlayerStats để quản lí HP của player (Các trạng thái như Hit, Dead)                                        | Tháng 6      | Player       | 3            | 🔴 High       | Gameplay       |
| nan                                                             | US-008       | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) tương tác và nhận diện kẻ địch | Tháng 6      | Combat       | 3            | 🔴 High       | Engine, Combat |
| nan                                                             | US-009       | Xây dựng EnemyBase với máy trạng thái đơn giản: tự động rượt đuổi (Chase) và tấn công (Attack).                | Tháng 6      | Enemy        | 3            | 🔴 High       | AI, Combat     |
| E03                                                             | US-010       | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0.                         | Tháng 6      | Combat       | 3            | 🔴 High       | Combat         |
| nan                                                             | US-011       | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và kích hoạt khóa phòng.                   | Tháng 6      | Room System  | 3            | 🔴 High       | Engine, GD     |
| nan                                                             | US-012       | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn ngay khi phòng bị khóa.                | Tháng 6      | Room System  | 2            | 🔴 High       | Gameplay       |
| nan                                                             | US-013       | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết.             | Tháng 6      | Room System  | 3            | 🔴 High       | Gameplay       |
| nan                                                             | nan          | nan                                                                                                            | Tháng 6      | nan          | 3            | 🔴 High       | nan            |
| nan                                                             | nan          | nan                                                                                                            | Tháng 6      | nan          | 3            | 🔴 High       | nan            |
| nan                                                             | nan          | nan                                                                                                            | Tháng 6      | nan          | 3            | 🔴 High       | nan            |
| nan                                                             | nan          | nan                                                                                                            | Tháng 6      | nan          | 2            | 🔴 High       | nan            |
| nan                                                             | nan          | nan                                                                                                            | Tháng 6      | nan          | 2            | 🟡 Medium     | nan            |
| TỔNG STORY POINTS                                               | nan          | nan                                                                                                            | nan          | nan          | 50           | nan          | nan            |

## Sheet: 🗓️ Roadmap

| Unnamed: 1                                                  | Unnamed: 2                         | Unnamed: 3                                                                                            | Unnamed: 4   |
|:------------------------------------------------------------|:-----------------------------------|:------------------------------------------------------------------------------------------------------|:-------------|
| ROADMAP 3 THÁNG — SMART FARM MANAGEMENT UNITY               | nan                                | nan                                                                                                   | nan          |
| THÁNG 1 — CORE ENGINE, FARMING LOOP & SIMULATION FOUNDATION | nan                                | nan                                                                                                   | nan          |
| Epic                                                        | Tên                                | Nội dung chính                                                                                        | Story Points |
| E01                                                         | Kiến trúc & Project Setup Unity    | Khởi tạo project Unity, cấu trúc thư mục, scene, kiến trúc quản lý game, event system và workflow Git | 17 sp        |
| E02                                                         | Core Roguelite Gameplay Loop       | Xây dựng vòng lặp chơi chính: bắt đầu run, vào màn, clear room, nhận reward, kết thúc run             | 17 sp        |
| E03                                                         | Player Combat & Basic Enemy System | Xây dựng nhân vật, điều khiển, tấn công, nhận damage, enemy thường và combat cơ bản                   | 16 sp        |

## Sheet: ⚒️ Phân công

| Unnamed: 1         | Unnamed: 2         | Unnamed: 3                   | Unnamed: 4                                                                                                           | Unnamed: 5                                                                                                            | Unnamed: 8                      | Unnamed: 9            | Unnamed: 10        | Unnamed: 11   | Unnamed: 12                                            |
|:-------------------|:-------------------|:-----------------------------|:---------------------------------------------------------------------------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------|:--------------------------------|:----------------------|:-------------------|:--------------|:-------------------------------------------------------|
| PHÂN CÔNG NHIỆM VỤ | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | PHÂN CHIA MODULE PHỤ            | nan                   | nan                | nan           | nan                                                    |
| STT                | Tên thành viên     | Vai trò chính                | Nhiệm vụ phụ trách                                                                                                   | Kết quả dự kiến                                                                                                       | Module                          | Người phụ trách chính | Người hỗ trợ       | Mức ưu tiên   | Ghi chú                                                |
| 1                  | Trần Việt Anh      | Leader / Gameplay Dev        | Quản lý tiến độ, chia task, tổng hợp build; xây dựng core gameplay loop; phối hợp các module chính                   | Game loop hoàn chỉnh: Start Run → Clear Room → Nhận Reward → Boss → End Run                                           | Core Gameplay Loop              | Phùng Phạm Gia Bảo    | Trần Việt Anh      | 🔴 Cao         | Bắt buộc có trong MVP                                  |
| 2                  | Phùng Phạm Gia Bảo | Game Designer / Gameplay Dev | Thiết kế và lập trình cơ chế chiến đấu, Xây dựng AI cho quái thường và Boss.; Thiết kế các module chính              | Hệ thống combat mượt mà, Có ít nhất 1-2 loại quái với hành vi AI khác nhau và 1 Boss hoàn chỉnh có bộ kỹ năng riêng.  | Player Movement & Combat        | Phùng Phạm Gia Bảo    | Đặng Đình Tuyên    | 🔴 Cao         | Là phần cảm giác chơi chính                            |
| 3                  | Nguyễn Mạnh Thắng  | UI / Data / Documentation    | Làm UI menu, HUD, pause, result screen; quản lý ScriptableObject/JSON; viết tài liệu, slide, báo cáo                 | Có giao diện chơi được, màn hình kết quả run, tài liệu mô tả hệ thống                                                 | Enemy System                    | Trần Việt Anh         | Nguyễn Mạnh Thắng  | 🔴 Cao         | Enemy thường cần xong trước boss                       |
| 4                  | Phạm Bình Định     | Progression / System Dev     | Xây dựng hệ thống tài nguyên, perk/reward, nâng cấp vĩnh viễn, save/load tiến trình; hỗ trợ cân bằng chỉ số gameplay | Có hệ thống nâng cấp hoạt động, người chơi nhận tài nguyên sau run, mở khóa nâng cấp vĩnh viễn và lưu được tiến trình | Boss System                     | Trần Việt Anh         | Phùng Phạm Gia Bảo | 🔴 Cao         | Chỉ cần 1 boss tốt cho demo                            |
| 5                  | Đặng Đình Tuyên    | Level / System Designer      | Thiết kế room prefab, tilemap, spawn point, hệ thống ghép phòng ngẫu nhiên/bán ngẫu nhiên, cân bằng màn chơi         | Có hệ thống tạo màn chơi theo room/segment, mỗi run có bố cục khác nhau                                               | Level Generation                | Phùng Phạm Gia Bảo    | Đặng Đình Tuyên    | 🔴 Cao         | Ưu tiên semi-random, không cần procedural quá phức tạp |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | Weapon System                   | Đặng Đình Tuyên       | Nguyễn Mạnh Thắng  | 🔴 Cao         | Có thể bắt đầu với 2–3 vũ khí                          |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | Perk / Temporary Upgrade        | Nguyễn Mạnh Thắng     | Phạm Bình Định     | 🔴 Cao         | Là đặc trưng roguelite quan trọng                      |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | Permanent Upgrade / Save System | Đặng Đình Tuyên       | Trần Việt Anh      | 🟡 Trung bình  | Lưu tài nguyên, mở khóa nâng cấp                       |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | UI / HUD                        | Nguyễn Mạnh Thắng     | Phạm Bình Định     | 🔴 Cao         | Máu, tài nguyên, perk, pause, result                   |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | Art / Placeholder / Audio       | Đặng Đình Tuyên       | Phạm Bình Định     | 🟡 Trung bình  | Có thể dùng asset free trước                           |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | Testing / Bug fix               | Phùng Phạm Gia Bảo    | Trần Việt Anh      | 🔴 Cao         | Test build mỗi giai đoạn hoặc thường xuyên             |
| nan                | nan                | nan                          | nan                                                                                                                  | nan                                                                                                                   | Báo cáo / Slide / Demo Script   | Phạm Bình Định        | Nguyễn Mạnh Thắng  | 🔴 Cao         | Chuẩn bị cho mentor và bảo vệ                          |

## Sheet: E01

| Unnamed: 1                                                | Unnamed: 2                                                                                               | Unnamed: 3   | Unnamed: 4    | Unnamed: 5   | Unnamed: 6                    |
|:----------------------------------------------------------|:---------------------------------------------------------------------------------------------------------|:-------------|:--------------|:-------------|:------------------------------|
| ⚙️ Kiến trúc & Project Setup Unity  |  Tháng 6  |  🔴 High | nan                                                                                                      | nan          | nan           | nan          | nan                           |
| Story ID                                                  | User Story                                                                                               | Module       | Tags          | SP           | Ghi chú / Acceptance Criteria |
| US-001                                                    | Khởi tạo dự án Unity 2D/URP: project settings, folder structure, scenes, input map, sorting layer        | Engine       | Engine, Infra | 3            | nan                           |
| US-002                                                    | Thiết lập Git workflow: branch main/dev/feature, Git LFS cho asset, .gitignore Unity, quy tắc commit     | DevOps       | Infra         | 3            | nan                           |
| US-003                                                    | Tìm kiếm tài nguyên cần dùng cho game từ các nguồn khác nhau                                             | Asset        | Resource      | 3            | nan                           |
| US-004                                                    | Khởi tạo các Animation cơ bản cho player                                                                 | Player       | Animation, GD | 2            | nan                           |
| US-005                                                    | Khởi tạo GameManager (Singleton) để điều phối các trạng thái hoạt động của game (Play, Pause, GameOver). | Architecture | Engine        | 3            | nan                           |
| TỔNG STORY POINTS                                         | nan                                                                                                      | nan          | nan           | 14           | nan                           |

## Sheet: E02

| Unnamed: 1                                            | Unnamed: 2                                                                                                     | Unnamed: 3   | Unnamed: 4     | Unnamed: 5   | Unnamed: 6                    |
|:------------------------------------------------------|:---------------------------------------------------------------------------------------------------------------|:-------------|:---------------|:-------------|:------------------------------|
| 🌱 Core Roguelite Gameplay Loop  |  Tháng 6  |  🔴 High | nan                                                                                                            | nan          | nan            | nan          | nan                           |
| Story ID                                              | User Story                                                                                                     | Module       | Tags           | SP           | Ghi chú / Acceptance Criteria |
| US-006                                                | Xây dựng PlayerController và State Machine xử lý di chuyển (Idle, Move, Jump, Fall) bằng Rigidbody2D cơ bản    | Player       | Engine, GD     | 3            | nan                           |
| US-007                                                | Tạo PlayerStats để quản lí HP của player (Các trạng thái như Hit, Dead)                                        | Player       | Gameplay       | 3            | nan                           |
| US-008                                                | Thêm trạng thái Attack cho Player, thiết lập hệ thống hitbox (Trigger Collider) tương tác và nhận diện kẻ địch | Combat       | Engine, Combat | 3            | nan                           |
| US-009                                                | Xây dựng EnemyBase với máy trạng thái đơn giản: tự động rượt đuổi (Chase) và tấn công (Attack).                | Enemy        | AI, Combat     | 3            | nan                           |
| TỔNG STORY POINTS                                     | nan                                                                                                            | nan          | nan            | 12           | nan                           |

## Sheet: E03

| Unnamed: 1                                                  | Unnamed: 2                                                                                         | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------------------------------------------------|:---------------------------------------------------------------------------------------------------|:-------------|:-------------|:-------------|:------------------------------|
| 🗺️ Player Combat & Basic Enemy System |  Tháng 6  |  🔴 High | nan                                                                                                | nan          | nan          | nan          | nan                           |
| Story ID                                                    | User Story                                                                                         | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| US-010                                                      | Ghép nối TakeDamage giữa Player và Enemy, xử lý Knockback và hủy quái vật khi HP <= 0.             | Combat       | Combat       | 3            | nan                           |
| US-011                                                      | Tạo RoomManager, dùng Collider2D ở cửa để nhận diện Player bước vào và kích hoạt khóa phòng.       | Room System  | Engine, GD   | 3            | nan                           |
| US-012                                                      | Cấu hình Enemy Spawner để tự động sinh quái vật tại các vị trí định sẵn ngay khi phòng bị khóa.    | Room System  | Gameplay     | 2            | nan                           |
| US-013                                                      | Đếm lượng quái trong phòng; tự động chuyển trạng thái Cleared và mở cửa khi quái bị tiêu diệt hết. | Room System  | Gameplay     | 3            | nan                           |
| TỔNG STORY POINTS                                           | nan                                                                                                | nan          | nan          | 11           | nan                           |

## Sheet: E04

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E05

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E06

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E07

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E08

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E09

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E10

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E11

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E12

| Unnamed: 1        | Unnamed: 2   | Unnamed: 3   | Unnamed: 4   | Unnamed: 5   | Unnamed: 6                    |
|:------------------|:-------------|:-------------|:-------------|:-------------|:------------------------------|
| Story ID          | User Story   | Module       | Tags         | SP           | Ghi chú / Acceptance Criteria |
| TỔNG STORY POINTS | nan          | nan          | nan          | 0            | nan                           |

## Sheet: E13

| Unnamed: 1        |   Unnamed: 5 |
|:------------------|-------------:|
| TỔNG STORY POINTS |            0 |
