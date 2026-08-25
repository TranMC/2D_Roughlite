# 🎮 Dự án Game 2D Roguelite (Unity) - **v0.8.0**

Dự án phát triển game hành động **2D Roguelite** trong Unity sử dụng URP, Rigidbody2D, kiến trúc State Machine và các hệ thống lõi phục vụ lối chơi phiêu lưu, vượt ải ngẫu nhiên.

---

## 📂 CẤU TRÚC THƯ MỤC SCRIPTS THỰC TẾ

Toàn bộ mã nguồn của dự án được tổ chức khoa học trong thư mục [Assets/Scripts](Assets/Scripts):

*   **[Core/](Assets/Scripts/Core)**: Điều phối vòng lặp và trạng thái trò chơi.
    *   [GameManager.cs](Assets/Scripts/Core/GameManager.cs): Singleton quản lý trạng thái Play, Pause, GameOver, chuyển Scene, tự động reset loadout vũ khí ở lượt run mới.
    *   [RuntimeDebugConsole.cs](Assets/Scripts/Core/RuntimeDebugConsole.cs): Công cụ F1 Debug Console hỗ trợ hack tiến độ, mở khóa vũ khí, cộng vàng/quái/runs/rooms và test nhanh runtime.
    *   [CharacterEvents.cs](Assets/Scripts/Core/CharacterEvents.cs), [AnimationStrings.cs](Assets/Scripts/Core/AnimationStrings.cs), [DebugLogger.cs](Assets/Scripts/Core/DebugLogger.cs).
*   **[Player/](Assets/Scripts/Player)**: Cơ chế điều khiển và trạng thái của Player.
    *   [PlayerController.cs](Assets/Scripts/Player/PlayerController.cs): State Machine xử lý di chuyển (Idle, Move, Jump, Fall), tấn công trên không/mặt đất.
    *   [WeaponManager.cs](Assets/Scripts/Player/WeaponManager.cs): Tiếp nhận và áp dụng các chỉ số Support Weapon Buffs cộng dồn trực tiếp từ Cửa hàng Vũ khí.
    *   [PlayerStats.cs](Assets/Scripts/Player/PlayerStats.cs): Quản lý lượng HP tối đa/hiện tại, sự kiện OnHit, OnDead.
    *   [TouchingDirections.cs](Assets/Scripts/Player/TouchingDirections.cs): Kiểm tra tiếp đất, chạm tường, chạm trần.
*   **[Enemy/](Assets/Scripts/Enemy)**: Trí tuệ nhân tạo (AI) của quái vật và Boss.
    *   [EnemyBase.cs](Assets/Scripts/Enemy/EnemyBase.cs): Lớp cơ sở trừu tượng quản lý HP, trạng thái Stagger, lực đẩy lùi (Knockback) và thưởng vàng/linh hồn.
    *   [Enemy_AI.cs](Assets/Scripts/Enemy/Enemy_AI.cs): State Machine điều khiển quái đi tuần tra (Patrol Anchor), bám đuổi (Chase) và tấn công (Attack) Player.
    *   [BossBase.cs](Assets/Scripts/Enemy/BossBase.cs) & [Boss.cs](Assets/Scripts/Enemy/Boss.cs): Cấu trúc Boss đa Phase, Enraged Material Outline Shader và Animator Speed & Physics Scale theo Phase.
*   **[Combat/](Assets/Scripts/Combat)**: Hệ thống chiến đấu và Cửa hàng Vũ Khí.
    *   [WeaponData.cs](Assets/Scripts/Combat/WeaponData.cs) & [WeaponDatabase.cs](Assets/Scripts/Combat/WeaponDatabase.cs): ScriptableObject dữ liệu thuộc tính vũ khí, giá bán và điều kiện thành tựu mở bán.
    *   [WeaponShopManager.cs](Assets/Scripts/Combat/WeaponShopManager.cs): Quản lý Cửa hàng Vũ khí, thanh toán Vàng, kiểm tra thành tựu, quản lý tối đa 3 Slots Support Weapon Buffs và reset loadout mỗi run.
    *   [WeaponHitboxBridge.cs](Assets/Scripts/Combat/WeaponHitboxBridge.cs): Đồng bộ sát thương và lực hất trực tiếp sang đòn đánh của Player.
    *   [Attack.cs](Assets/Scripts/Combat/Attack.cs): Quản lý kích thước hitbox, lượng sát thương và hướng knockback.
    *   [Editor/WeaponEditorWindow.cs](Assets/Scripts/Combat/Editor/WeaponEditorWindow.cs): Công cụ Editor Window (`Tools/Roguelite/Weapon & Shop Editor`) sinh 10 vũ khí mẫu.
*   **[UpgradeSystem/](Assets/Scripts/UpgradeSystem)**: Hệ thống Nâng cấp vĩnh viễn (Meta-progression) và Perks trong trận.
    *   [PermanentUpgradeData.cs](Assets/Scripts/UpgradeSystem/PermanentUpgradeData.cs) & [PermanentUpgradeTier.cs](Assets/Scripts/UpgradeSystem/PermanentUpgradeTier.cs): Cấu trúc dữ liệu nâng cấp vĩnh viễn Multi-tier kèm Milestone Bonus ở cấp tối đa.
    *   [PermanentUpgradeManager.cs](Assets/Scripts/UpgradeSystem/PermanentUpgradeManager.cs): Quản lý mua/nâng cấp level vĩnh viễn, kiểm tra thành tựu mở bán và tính toán cộng dồn chỉ số.
    *   [StatCalculator.cs](Assets/Scripts/UpgradeSystem/StatCalculator.cs): Chuẩn hóa thứ tự tính toán chỉ số Roguelite ($\text{Final} = (\text{Base} + \text{Flat}) \times (1 + \text{Additive}) \times \text{Multiplicative}$).
    *   [PerkData.cs](Assets/Scripts/UpgradeSystem/PerkData.cs), [PerkPool.cs](Assets/Scripts/UpgradeSystem/PerkPool.cs), [UpgradeManager.cs](Assets/Scripts/UpgradeSystem/UpgradeManager.cs), [PerkEffectApplier.cs](Assets/Scripts/UpgradeSystem/PerkEffectApplier.cs): Hệ thống Perk nâng cấp trong trận.
    *   [Editor/PermanentUpgradeGeneratorWindow.cs](Assets/Scripts/UpgradeSystem/Editor/PermanentUpgradeGeneratorWindow.cs): Công cụ Editor Window (`Tools/Roguelite/Permanent Upgrade Generator`) sinh 5 nâng cấp vĩnh viễn mẫu.
*   **[SaveSystem/](Assets/Scripts/SaveSystem)**: Lưu trữ tiến trình chơi và di cư dữ liệu.
    *   [SaveData.cs](Assets/Scripts/SaveSystem/SaveData.cs) & [SaveManager.cs](Assets/Scripts/SaveSystem/SaveManager.cs): Singleton quản lý 3 slots save JSON tại `persistentDataPath`, di cư tự động lên **Save Version 8**.
    *   [WeaponUnlockData.cs](Assets/Scripts/SaveSystem/WeaponUnlockData.cs): Quản lý danh sách vũ khí đã sở hữu và mảng `equippedWeaponIds` (tối đa 3 slots).
*   **[UI/](Assets/Scripts/UI)**: Các thành phần giao diện.
    *   [WeaponShopUIController.cs](Assets/Scripts/UI/WeaponShopUIController.cs) & [WeaponShopItemUI.cs](Assets/Scripts/UI/WeaponShopItemUI.cs): Giao diện Cửa Hàng Vũ Khí với bộ đếm slot `(X/3)`, nút `+ Trang Bị Support` / `✓ Gỡ Support`.
    *   [PermanentUpgradeUIController.cs](Assets/Scripts/UI/PermanentUpgradeUIController.cs) & [PermanentUpgradeItemUI.cs](Assets/Scripts/UI/PermanentUpgradeItemUI.cs): Giao diện Cửa hàng Nâng cấp vĩnh viễn với các bộ lọc Category.
    *   [RewardCardUI.cs](Assets/Scripts/UI/RewardCardUI.cs), [BossHealthBarUI.cs](Assets/Scripts/UI/BossHealthBarUI.cs), [PauseMenuManager.cs](Assets/Scripts/UI/PauseMenuManager.cs), [ParallaxEffect.cs](Assets/Scripts/UI/ParallaxEffect.cs).

---

## ⌨️ CÁC PHÍM TẮT ĐỂ KIỂM THỬ (TEST HOTKEYS & UTILS)

Để hỗ trợ kiểm thử nhanh trong quá trình phát triển trên Unity Editor:

### 1. Phím tắt mở Console Debug Tool F1:
*   **Phím F1**: Bật/tắt giao diện **Runtime Debug Console**:
    *   Bảng theo dõi và hack Vàng, Số quái diệt, Số lượt Run, Cấp phòng sâu nhất.
    *   Thao tác mở khóa tất cả vũ khí, gỡ/thêm nhanh 3 slots Support Weapon Buff.
    *   Lệnh CLI: `weapon unlockall`, `weapon reset`, `weapon buy <id>`, `weapon equip <id>`, `addkills <num>`, `addruns <num>`.

### 2. Phím tắt điều khiển trạng thái Player:
*   **Phím T**: Gây **10 sát thương** trực tiếp cho Player (để test hiệu ứng Stagger/Hit).
*   **Phím Y**: Gây **100 sát thương** (hạ gục Player lập tức để test GameOver Screen).
*   **Phím U**: Hồi **10 HP** cho Player.

---

> [!NOTE]
> Để cập nhật tiến trình phát triển và lưu trữ lịch sử thay đổi, hãy đồng bộ hóa tại [Project_Roadmap_Tracker.md](Project_Roadmap_Tracker.md) và [CHANGELOG.md](CHANGELOG.md).