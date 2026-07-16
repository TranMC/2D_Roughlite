# 🎮 Dự án Game 2D Roguelite (Unity)

Dự án phát triển game hành động **2D Roguelite** trong Unity sử dụng URP, Rigidbody2D, kiến trúc State Machine và các hệ thống lõi phục vụ lối chơi phiêu lưu, vượt ải ngẫu nhiên.

---

## 📂 CẤU TRÚC THƯ MỤC SCRIPTS THỰC TẾ

Toàn bộ mã nguồn của dự án được tổ chức khoa học trong thư mục [Assets/Scripts](Assets/Scripts):

*   **[Core/](Assets/Scripts/Core)**: Điều phối vòng lặp và trạng thái trò chơi.
    *   [GameManager.cs](Assets/Scripts/Core/GameManager.cs): Singleton quản lý trạng thái Play, Pause, GameOver, chuyển Scene.
    *   [CharacterEvents.cs](Assets/Scripts/Core/CharacterEvents.cs), [AnimationStrings.cs](Assets/Scripts/Core/AnimationStrings.cs), [DebugLogger.cs](Assets/Scripts/Core/DebugLogger.cs).
*   **[Player/](Assets/Scripts/Player)**: Cơ chế điều khiển và trạng thái của Player.
    *   [PlayerController.cs](Assets/Scripts/Player/PlayerController.cs): State Machine xử lý di chuyển (Idle, Move, Jump, Fall), tấn công trên không/mặt đất.
    *   [PlayerStats.cs](Assets/Scripts/Player/PlayerStats.cs): Quản lý lượng HP tối đa/hiện tại, sự kiện OnHit, OnDead.
    *   [TouchingDirections.cs](Assets/Scripts/Player/TouchingDirections.cs): Kiểm tra tiếp đất, chạm tường, chạm trần.
*   **[Enemy/](Assets/Scripts/Enemy)**: Trí tuệ nhân tạo (AI) của quái vật và Boss.
    *   [EnemyBase.cs](Assets/Scripts/Enemy/EnemyBase.cs): Lớp cơ sở trừu tượng quản lý HP, trạng thái Stagger, lực đẩy lùi (Knockback).
    *   [Enemy_AI.cs](Assets/Scripts/Enemy/Enemy_AI.cs): State Machine điều khiển quái đi tuần tra (Patrol Anchor), bám đuổi (Chase) và tấn công (Attack) Player.
    *   [BossBase.cs](Assets/Scripts/Enemy/BossBase.cs) & [Boss.cs](Assets/Scripts/Enemy/Boss.cs): Cấu trúc Boss đa Phase theo ngưỡng máu và tự động mở cửa phòng Boss khi chết.
*   **[Combat/](Assets/Scripts/Combat)**: Hệ thống chiến đấu và gây sát thương.
    *   [Attack.cs](Assets/Scripts/Combat/Attack.cs): Quản lý kích thước hitbox, lượng sát thương và hướng knockback.
    *   [IDamageable.cs](Assets/Scripts/Combat/IDamageable.cs): Giao diện nhận sát thương chung cho cả Player và Enemy.
*   **[RoomSystem/](Assets/Scripts/RoomSystem)**: Hệ thống phòng và sinh quái tự động.
    *   [RoomManager.cs](Assets/Scripts/RoomSystem/RoomManager.cs): Quản lý trạng thái khóa/mở phòng khi người chơi đi qua cửa.
    *   [EnemySpawner.cs](Assets/Scripts/RoomSystem/EnemySpawner.cs): Tự động sinh quái vật theo offset và kiểm đếm lượng quái còn lại.
    *   [MapGenerator.cs](Assets/Scripts/RoomSystem/MapGenerator.cs): Giải thuật sinh bản đồ bán ngẫu nhiên (Semi-random Level Gen).
    *   [Editor/EnemySpawnerEditor.cs](Assets/Scripts/RoomSystem/Editor/EnemySpawnerEditor.cs): Custom Editor trực quan hóa các vị trí spawn trong Unity Scene View.
*   **[UpgradeSystem/](Assets/Scripts/UpgradeSystem)**: Hệ thống Perk và Nâng cấp trong lượt chơi (Run).
    *   [PerkData.cs](Assets/Scripts/UpgradeSystem/PerkData.cs): ScriptableObject lưu trữ thông số của từng Perk.
    *   [PerkPool.cs](Assets/Scripts/UpgradeSystem/PerkPool.cs): Quản lý kho Perk khả dụng và random có trọng số loại trừ trùng lặp.
    *   [UpgradeManager.cs](Assets/Scripts/UpgradeSystem/UpgradeManager.cs): Quản lý danh sách các Perk đang hoạt động của Player và lịch sử nâng cấp.
    *   [PerkEffectApplier.cs](Assets/Scripts/UpgradeSystem/PerkEffectApplier.cs): Áp dụng các chỉ số thay đổi thực tế vào thuộc tính của Player.
*   **[UI/](Assets/Scripts/UI)**: Các thành phần giao diện và hiệu ứng đồ họa.
    *   [PauseMenuManager.cs](Assets/Scripts/UI/PauseMenuManager.cs): Quản lý tạm dừng game.
    *   [ParallaxEffect.cs](Assets/Scripts/UI/ParallaxEffect.cs): Hiệu ứng nền di chuyển song song theo camera của Player.

---

## ⌨️ CÁC PHÍM TẮT ĐỂ KIỂM THỬ (TEST HOTKEYS & UTILS)

Để hỗ trợ kiểm thử nhanh trong quá trình phát triển trên Unity Editor:

### 1. Phím tắt điều khiển trạng thái Player:
*   **Phím T**: Gây **10 sát thương** trực tiếp cho Player (để test hiệu ứng Stagger/Hit).
*   **Phím Y**: Gây **100 sát thương** (hạ gục Player lập tức để test GameOver Screen).
*   **Phím U**: Hồi **10 HP** cho Player.

### 2. Kiểm thử Boss nhanh trong Inspector:
Chuột phải (Context Menu) vào Component `Boss` của GameObject Boss trong Scene View để chạy các hàm debug:
*   `Debug/Gây 20% maxHP sát thương`: Gây mất máu nhanh để kiểm thử hiệu ứng đổi màu Sprite theo Phase (Phase 1 -> Vàng, Phase 2 -> Đỏ).
*   `Debug/Hạ gục Boss ngay lập tức`: Tiêu diệt Boss để kiểm tra sự kiện mở cửa phòng tự động thông qua [RoomManager](Assets/Scripts/RoomSystem/RoomManager.cs).

---

> [!NOTE]
> Để cập nhật tiến trình phát triển và lưu trữ lịch sử thay đổi, hãy đồng bộ hóa tại [Project_Roadmap_Tracker.md](Project_Roadmap_Tracker.md) và [CHANGELOG.md](CHANGELOG.md).