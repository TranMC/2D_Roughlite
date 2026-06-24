# 🎮 2D Roguelite Project

Dự án game 2D Roguelite trong Unity sử dụng Rigidbody2D, State Machine, và các cơ chế tương tác combat cơ bản.

---

## 🎹 CÁC PHÍM TẮT ĐỂ KIỂM THỬ (TEST HOTKEYS)

Trong Unity Editor, bạn có thể nhấn các phím sau trên bàn phím để kiểm thử nhanh các trạng thái của Player:
*   **Phím T**: Gây **10 sát thương** cho nhân vật (Kích hoạt sự kiện `OnHit` và Trigger `hit` của Animator, khóa di chuyển tạm thời).
*   **Phím Y**: Gây **100 sát thương** (Tiêu diệt nhân vật, kích hoạt sự kiện `OnDead`, Trigger `die` của Animator, vô hiệu hóa điều khiển, cho phép rơi tự do nếu ở trên không và thông báo GameOver).
*   **Phím U**: Hồi **10 máu** (Kích hoạt sự kiện `OnHealthChanged` tăng HP).

---

> [!WARNING]
> Chú ý thường xuyên cập nhật Roadmap (`Project_Roadmap_Tracker.md`) và Changelog (`CHANGELOG.md`) để đảm bảo tiến trình phát triển không bị lẫn lộn.