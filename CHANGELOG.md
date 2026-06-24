# Nhật ký thay đổi (Changelog)

Tất cả các thay đổi lớn và sửa lỗi trong dự án 2D Roguelite sẽ được lưu trữ tại đây.

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
