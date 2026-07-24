# Nhật ký thay đổi (Changelog)

Tất cả các thay đổi lớn và sửa lỗi trong dự án 2D Roguelite sẽ được lưu trữ tại đây.

## [v0.5.1] - 2026-07-24

### 👹 Bổ sung Tầm đánh (Attack Range Multiplier) theo Phase cho Boss (Epic E06)
*   **Bổ sung `attackRangeMultiplier` vào PhasePatternGroup**:
    *   Cập nhật [BossBase.cs](Assets/Scripts/Enemy/BossBase.cs) lưu lại `baseAttackRange` ban đầu tại `Awake()`.
    *   Tự động tính toán và nhân lại tầm đánh `attackRange` theo hệ số `attackRangeMultiplier` (mặc định lấy theo `scaleMultiplier` nếu không cấu hình) khi Boss chuyển Phase.
    *   Hiển thị hệ số nhân tầm đánh mới trong Unity Inspector và Debug Log.

---

## [v0.5.0] - 2026-07-24

### 🖼️ Giao diện Reward Card UI & Hệ thống Perk (Epic E05 - US-020)
*   **Thiết kế UI Reward Selection & Card UI**: 
    *   Tạo script `RewardCardUI.cs` điều khiển hiển thị icon, tên, mô tả và tint viền màu từ Material theo Rarity của Perk (Common, Rare, Epic).
    *   Khởi tạo Panel `Reward Selection` cho phép người chơi chọn 1 trong 3 Perk ngẫu nhiên.
    *   Thêm sự kiện `public static event Action<PerkData, int> OnPerkAdded;` trong `UpgradeManager.cs` giúp các UI khác lắng nghe mỗi khi thêm/bớt Perk.

### 👹 Nâng cấp Boss Fight & Thanh máu Boss (Epic E06 - US-023, US-025, US-026)
*   **Boss HealthBar UI**: Thiết lập UI Image / Slider cho thanh máu Boss ([BossHealthBarUI.cs](Assets/Scripts/UI/BossHealthBarUI.cs)) hiển thị tên Boss và lượng HP theo từng phase.
*   **Enraged Phase Shader & Material**: Cập nhật [Boss.cs](Assets/Scripts/Enemy/Boss.cs) sử dụng Material Outline Shader cho trạng thái Enraged thay vì đổi màu đơn thuần.
*   **Điều chỉnh Tốc độ & Scale Boss**:
    *   Tự động thay đổi Animator Speed và Transform Scale theo từng Phase.
    *   Đồng bộ tốc độ của Boss kết hợp cả Animation và Physics movement.
*   **Boss Arena & Ambient VFX**: Thêm asset Boss #2 và Particle Fog nền không khí vào phòng đấu Boss ([Room_Boss1](Assets/Prefabs/Rooms/Room_Boss1.prefab)).

### ⚔️ Hệ thống Hitbox Dựa trên Dữ liệu (Hitbox Data-Driven System - US-024)
*   **HitboxData & Controller**: Xây dựng `HitboxData.cs` và `HitboxController.cs` cho phép định nghĩa thông số Hitbox độc lập và kích hoạt thông qua Frame Index trong Animation Event.

### 🛠️ Tinh chỉnh Chiến đấu & Khắc phục Lỗi (Combat Tuning & Bug Fixes)
*   **Khắc phục lỗi baseAttackDamage**: Xử lý lỗi `baseAttackDamage` chưa được khởi tạo đúng lúc khi `UpgradeManager` đăng ký event từ sớm.
*   **Bổ sung OverlapCollider**: Thêm `OverlapCollider` vào [Attack.cs](Assets/Scripts/Combat/Attack.cs) để giải quyết triệt để lỗi đòn đánh không trúng enemy khi nhân vật đứng sát sạt.
*   **Player Attack Configs**: Tạo ScriptableObject `AttackConfigs` giúp tùy chỉnh tham số sát thương và timing của Player dễ dàng hơn.

---

## [2026-07-16]

### 👹 Nền tảng Boss Fight & Cơ chế Phân Phase (Epic E06)
*   **Thiết lập BossBase.cs**: 
    *   Tạo lớp cơ sở trừu tượng [BossBase.cs](Assets/Scripts/Enemy/BossBase.cs) kế thừa [EnemyBase.cs](Assets/Scripts/Enemy/EnemyBase.cs).
    *   Hỗ trợ cấu hình ngưỡng chuyển Phase theo mảng phần trăm HP (`phaseThresholds`).
    *   Lắng nghe sự kiện nhận sát thương để tự động chuyển phase và kích hoạt event `OnPhaseChanged`.
    *   Tự động giải phóng phòng đấu Boss khi chết bằng cách tìm kiếm [RoomManager](Assets/Scripts/RoomSystem/RoomManager.cs) trong component cha và gọi `OnRoomCleared()`.
*   **Tích hợp Boss Demo (Boss.cs)**: 
    *   Viết [Boss.cs](Assets/Scripts/Enemy/Boss.cs) kế thừa [BossBase.cs](Assets/Scripts/Enemy/BossBase.cs).
    *   Tự động đổi màu Sprite của Boss khi chuyển pha (Phase 1 -> Vàng, Phase 2 -> Đỏ) để báo hiệu trạng thái.
    *   Thêm Context Menu hữu ích để test sát thương nhanh theo tỉ lệ % máu và hạ gục Boss trực tiếp trên Inspector của Unity Editor.

### 🧬 Hệ thống Perk & Nâng cấp chỉ số (Epic E05)
*   **Cấu trúc Perk ScriptableObject**: Xây dựng [PerkData.cs](Assets/Scripts/UpgradeSystem/PerkData.cs) và các cấu trúc dữ liệu liên quan để định nghĩa thuộc tính nâng cấp, quy tắc stack và độ hiếm.
*   **Quản lý bộ chọn Perk ngẫu nhiên**: Viết [PerkPool.cs](Assets/Scripts/UpgradeSystem/PerkPool.cs) hỗ trợ thuật toán chọn ngẫu nhiên 3 Perk có trọng số theo độ hiếm và loại bỏ các Perk đã đạt giới hạn cộng dồn (Max Stack).
*   **Áp dụng chỉ số Perk**: Viết [PerkEffectApplier.cs](Assets/Scripts/UpgradeSystem/PerkEffectApplier.cs) để tính toán, cộng dồn và áp các hiệu ứng thay đổi HP, tốc độ chạy, tốc độ đánh, sát thương... (hỗ trợ cộng thẳng flat và nhân hệ số multiplier).
*   **Quản lý tiến trình nâng cấp trong Run**: Viết [UpgradeManager.cs](Assets/Scripts/UpgradeSystem/UpgradeManager.cs) lưu trữ các perk đang kích hoạt và ghi nhớ lịch sử nâng cấp phục vụ hiển thị kết quả cuối Run.

### 🖼️ Hiệu ứng Nền chuyển động (Parallax Background)
*   **Parallax Effect**: Viết [ParallaxEffect.cs](Assets/Scripts/UI/ParallaxEffect.cs) giúp các sprite nền di chuyển lệch tốc độ theo camera để tạo chiều sâu không gian 2D cho màn chơi.

---

## [2026-07-06]

### 🏃 Nhân vật chính (Player) & Chiến đấu (Combat)
*   **Air Attack riêng biệt cho Player**: 
    *   Tách biệt đòn đánh trên không (Air Attack) và đòn đánh dưới đất (Ground Attack) dựa trên trạng thái `touchingDirections.IsGrounded`.
    *   Sử dụng trigger riêng biệt `airAttack` để kích hoạt hoạt ảnh trên không.
    *   Đăng ký hằng số `airAttackTrigger` trong `Roguelite.Player.AnimationStrings` và giải quyết xung đột trùng tên class `AnimationStrings` bằng cách gọi đường dẫn namespace đầy đủ trong `PlayerController.cs`.
*   **Cơ chế chống kích hoạt đệm (Anti-buffering)**:
    *   Tự động reset cả hai trigger `attack` và `airAttack` ngay khi Player vừa chạm đất từ trên không để tránh tự động kích hoạt đòn đánh mặt đất ngoài ý muốn khi vừa tiếp đất.
    *   Cập nhật thuộc tính `IsAttacking` để bao gồm cả trạng thái `"AirAttack"`, đảm bảo hệ thống di chuyển được đồng bộ chính xác khi đang tấn công trên không.

### ⚙️ AI của Enemy (Enemy Base & Combat)
*   **Sửa lỗi Knockback của Enemy**:
    *   Khắc phục lỗi lực đẩy lùi (Knockback) của Enemy không hoạt động do hàm `StopMovement()` ở đầu coroutine `HitStaggerCoroutine()` đặt vận tốc ngang về `0f` ngay lập tức.
    *   Cập nhật `HitStaggerCoroutine` nhận tham số `hasKnockback` (mặc định là `false`). Nếu có knockback, Enemy sẽ không dừng di chuyển ngang ngay lập tức để lực đẩy có tác dụng, và chỉ gọi `StopMovement()` để dừng chuyển động sau khi kết thúc thời gian stagger.

---

## [2026-06-29]

### ⚙️ Tinh chỉnh AI của Enemy (Enemy Base & Combat Refactor)
*   **Tách biệt cấu hình sát thương**: Loại bỏ các thuộc tính sát thương/knockback trùng lặp trực tiếp trên `EnemyBase.cs`. Đòn đánh và sát thương nay được quản lý độc lập bởi component `Attack.cs`, giúp dễ dàng thiết lập các đòn đánh khác nhau cho cùng một Enemy.
*   **Thêm giới hạn tuần tra (Patrol Range)**:
    *   Tích hợp biến `patrolAnchor` (Transform) và `patrolRange` (float) để giới hạn phạm vi tuần tra trái-phải của quái vật xung quanh tâm tuần tra (`patrolCenter`).
    *   Vẽ visual Gizmos (đoạn thẳng màu xanh lam có vạch chặn) trực tiếp trên Scene View để căn chỉnh phạm vi tuần tra dễ dàng.
    *   Hoàn toàn tương thích với hệ thống sinh quái vật tự động (US-012).

### 🏠 Hệ thống Phòng & Spawner (US-012 & US-013)
*   **Tách biệt EnemySpawner**: Tạo mới script `EnemySpawner.cs` quản lý cấu hình và logic sinh quái vật, giúp phân tách rõ trách nhiệm với `RoomManager.cs`.
*   **Tự động sinh quái vật khi khóa phòng (US-012)**: Tích hợp Spawner vào RoomManager để sinh quái vật tự động tại các tọa độ định sẵn ngay khi Player đi vào phòng.
*   **Tự động đếm và mở cửa phòng (US-013)**: Spawner tự động theo dõi sự kiện chết `OnDied` của các quái vật được sinh ra, báo về RoomManager mở cửa phòng (`OnRoomCleared()`) ngay khi toàn bộ quái vật bị tiêu diệt sạch.
*   **Tối ưu hóa cấu hình bằng tọa độ Vector2 Offset**: Sử dụng các tọa độ Vector2 Offset (lệch so với tâm phòng) thay vì dùng Transform giúp thiết kế màn chơi nhanh chóng mà không làm rác Hierarchy của Scene.

### 🛠️ Trải nghiệm Editor (Custom Editor)
*   **Tạo EnemySpawnerEditor.cs**: Script Custom Editor (nằm trong thư mục `Editor`) hiển thị các vòng tròn nhãn dán định vị và **Position Handles** (mũi tên kéo thả bằng chuột) trực tiếp trên Scene View để thiết kế điểm spawn/patrol của quái trực quan không cần gõ tọa độ thủ công.

### 🧹 Dọn dẹp Project
*   **Xóa DetectionZone.cs**: Loại bỏ script `DetectionZone.cs` dư thừa không còn sử dụng trong hệ thống AI.

### 📋 Cập nhật Lộ trình phát triển (Roadmap Sync)
*   **Đồng bộ US-016**: Cập nhật trạng thái **US-016** thành Hoàn thành trên Roadmap do trong dự án đã xây dựng sẵn 7 loại `Enemy Prefabs` cùng hệ thống Menu tạm dừng (`PauseMenuManager.cs`).

---

## [2026-06-24]

### 🏠 Hệ thống Phòng (Room System)
*   **Hoàn tất RoomManager** (**US-011**): Tạo `RoomManager.cs` (namespace `Roguelite.RoomSystem`) quản lý luồng phòng theo Flowchart:
    *   `OnTriggerEnter2D` nhận diện Player bằng LayerMask bitwise (không dùng `CompareTag`).
    *   Lock Doors: đánh dấu `isRoomLocked`, bật mảng `doors[]`, tắt `triggerCollider` tối ưu hiệu suất.
    *   Stub `SpawnEnemies()` và `Reward/Upgrade` chờ ghép nối EnemySpawner ở task kế tiếp.
    *   Hàm `OnRoomCleared()` public để gọi khi quái chết hết → mở toàn bộ cửa.

### 🧪 Kiểm thử tự động (Integration Test)
*   **Tạo RoomManager_AutoTester.cs**: Script tự động chạy 7 Test Cases (TC-05 → TC-11) trên Scene bằng Coroutine:
    *   TC-05: Đạn (Layer `Projectile`) chạm Trigger → phòng không khóa.
    *   TC-06: Quái (Layer `Enemy`) chạm Trigger → phòng không khóa.
    *   TC-07: Player chết trong phòng khóa → cửa vẫn đóng, `OnRoomCleared` không tự gọi.
    *   TC-08: Quái chết do bẫy/rớt vực → `OnRoomCleared` vẫn mở cửa bình thường.
    *   TC-09: Spawner sinh 0 quái → gọi `OnRoomCleared` ngay → cửa mở, không softlock.
    *   TC-10: Player dash escape exploit → phòng vẫn khóa, Trigger không bật lại.
    *   TC-11: Simulate reload (Disable/Enable) → `isRoomLocked` nhất quán, cửa vẫn đóng.
    *   Dùng Reflection đọc biến private `isRoomLocked` và `doors` để Assert.
    *   **Kết quả: 12 PASS / 0 FAIL.**

### ⚙️ Cấu hình dự án
*   Thêm Layer `Enemy` và `Projectile` vào **Project Settings > Tags and Layers**.

---

## [2026-06-23]

### ⚙️ AI của Enemy (Enemy Base & Enemy AI)
*   **Đơn giản hóa trạng thái tuần tra**: Thay thế trạng thái tuần tra liên tục bằng chu kỳ đi tuần xen kẽ đứng nghỉ tự nhiên (`Idle` ↔ `Patrol`).
    *   **Idle State**: Enemy đứng nghỉ trong `idleDuration` (2s), phát hiện Player để đuổi theo, hoặc hết thời gian sẽ quay đầu (`Flip`) đi tuần.
    *   **Patrol State**: Enemy di chuyển ngang trong `patrolDuration` (3s), chạm tường/vực hoặc hết thời gian sẽ chuyển sang `Idle` nghỉ ngơi.
*   **Tự động hóa Animation `isMoving`**: Animator tự động cập nhật tham số `isMoving` dựa trên vận tốc `Rigidbody2D` thực tế (`Mathf.Abs(rb.velocity.x) > 0.1f`).
*   **Khắc phục lỗi tấn công xác Player**: Thêm phương thức kiểm tra `IsTargetAlive()` trong `DetectPlayer()` và `ChaseLogic()` của Enemy để tự động bỏ bám đuổi và quay về đi tuần khi Player chết.
*   **Khắc phục lỗi tấn công xác Enemy**: Cập nhật hàm `HandleDeath()` trong `EnemyBase.cs` để vô hiệu hóa toàn bộ các Collider2D của Enemy (`GetComponentsInChildren<Collider2D>()`) tránh việc vũ khí của Player tiếp tục va chạm vật lý với xác quái.

### 🏃 Vật lý & Trạng thái Player (Player Stats & Controller)
*   **Sửa lỗi di chuyển khi bị trúng đòn**: Khắc phục lỗi Player bị kẹt không di chuyển được sau khi bị hit (`LockVelocity` không tự tắt). Đã bổ sung logic giải phóng `LockVelocity = false` khi thời gian bất tử/khựng kết thúc.
*   **Sửa lỗi kẹt lặp hoạt ảnh chết khi đang rơi**: Vô hiệu hóa component `TouchingDirections` khi chết và đặt lại các tham số Animator về trạng thái tĩnh để tránh Animator bị kẹt lặp lại trạng thái `Fall` từ Any State.
*   **Thêm cơ chế rơi tự do khi chết trên không**: Thay đổi hàm `Die()` để Player rơi tự nhiên xuống đất (vẫn chịu tác dụng của trọng lực nhưng khóa di chuyển ngang) rồi mới tắt mô phỏng vật lý và collider sau khi đã tiếp đất hoàn toàn.

### 🛠️ Lỗi biên dịch & Khác
*   **Sửa lỗi thiếu namespace**: Bổ sung chỉ thị `using System.Collections;` trong `PlayerStats.cs` để sửa lỗi thiếu kiểu `IEnumerator` dùng cho Coroutine.

---

## [2026-06-21]

### 🏃 Nhân vật chính (Player)
*   **Hoàn tất hệ thống Player Stats** (**US-007**): Tạo `PlayerStats` để quản lý HP, các sự kiện nhận sát thương (Hit) và chết (Dead).
*   **Player Controller hoàn chỉnh** (**US-006**): Tích hợp State Machine xử lý di chuyển (Idle, Move, Jump, Fall) sử dụng Rigidbody2D.

---

## [2026-06-20]

### ⚙️ Kiến trúc & Thiết lập dự án
*   **Khởi tạo GameManager** (**US-005**): Viết GameManager Singleton quản lý trạng thái trò chơi (Play, Pause, GameOver) và chuyển Scene.
*   **Khởi tạo Lộ trình phát triển**: Tạo file theo dõi tiến độ dự án `Project_Roadmap_Tracker.md`.
*   **Cấu trúc thư mục dự án** (**US-001**): Tạo cấu trúc thư mục chuẩn cho Unity 2D/URP.
*   **Git Workflow & Assets** (**US-002**, **US-003**, **US-004**): Khởi tạo Git repository, `.gitignore`, `.gitattributes`, nhập asset đồ họa và chuẩn bị animation cơ bản.
