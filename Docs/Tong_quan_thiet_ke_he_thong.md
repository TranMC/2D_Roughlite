# TỔNG QUAN THIẾT KẾ HỆ THỐNG

# 1. Tổng quan kiến trúc hệ thống

Tên diagram: System Architecture Diagram

Mục đích:
 Mô tả kiến trúc tổng thể của hệ thống game và mối quan hệ giữa các hệ thống chức năng chính.

Các thành phần chính:
 GameManager, Player System, Combat System, Enemy System, Room System, Upgrade System, Save System, UI System.

Mô tả sơ đồ:
 GameManager đóng vai trò điều phối trung tâm và quản lý trạng thái tổng thể của trò chơi. Các hệ thống như Player, Enemy, Room, Upgrade, Save và UI được tách riêng theo chức năng để giảm phụ thuộc và tăng khả năng mở rộng. Combat System chịu trách nhiệm xử lý các tương tác chiến đấu giữa người chơi và enemy.

# 2. Luồng gameplay tổng quát

Tên diagram: Gameplay Flow Diagram

Mục đích:
 Mô tả toàn bộ quá trình trải nghiệm của người chơi từ khi bắt đầu game đến khi kết thúc một lượt chơi.

Các thành phần chính:
 Main Menu, Start Run, Room, Enemy, Upgrade, Boss, Victory, Defeat.

Mô tả sơ đồ:
 Người chơi bắt đầu từ màn hình chính và tạo một lượt chơi mới. Trong quá trình chơi, người chơi vượt qua các room, tiêu diệt enemy và nhận các nâng cấp tạm thời. Sau một số room nhất định, người chơi sẽ đối đầu với boss. Lượt chơi kết thúc khi người chơi chiến thắng boss cuối cùng hoặc bị tiêu diệt.

# 3. Thiết kế các class chính

Tên diagram: Main Class Diagram

Mục đích:
 Mô tả cấu trúc các lớp chính của hệ thống và mối quan hệ giữa các lớp.

Các thành phần chính:
 GameManager, PlayerController, PlayerStats, EnemyBase, RoomManager, UpgradeManager, SaveManager, UIManager.

Mô tả sơ đồ:
 GameManager quản lý vòng đời của game và tương tác với các hệ thống khác. PlayerController điều khiển hành vi của người chơi. EnemyBase là lớp cơ sở cho các loại enemy và boss. RoomManager quản lý các room trong màn chơi. UpgradeManager quản lý hệ thống nâng cấp, trong khi SaveManager xử lý việc lưu và tải dữ liệu.

# 4. Trạng thái của nhân vật người chơi

Tên diagram: Player State Diagram

Mục đích:
 Mô tả các trạng thái hoạt động của nhân vật người chơi và điều kiện chuyển đổi giữa các trạng thái.

Các thành phần chính:
 Idle, Move, Jump, Dash, Attack, Hit, Dead.

Mô tả sơ đồ:
Nhân vật bắt đầu ở trạng thái Idle. Khi người chơi nhập lệnh di chuyển, nhân vật chuyển sang trạng thái Move và quay lại Idle khi ngừng di chuyển.

Khi thực hiện hành động nhảy, nhân vật chuyển sang trạng thái Jump. Sau khi đạt đỉnh của cú nhảy, nhân vật chuyển sang trạng thái Fall. Khi tiếp đất, nhân vật quay trở lại trạng thái Idle hoặc Move tùy theo đầu vào điều khiển của người chơi.

Trong quá trình chiến đấu, người chơi có thể thực hiện Dash để né tránh hoặc Attack để gây sát thương cho enemy. Sau khi hoàn thành hành động, nhân vật quay trở lại trạng thái Idle hoặc Move.

Khi nhận sát thương từ enemy hoặc môi trường, nhân vật chuyển sang trạng thái Hit. Nếu lượng máu còn lại lớn hơn 0, nhân vật trở về trạng thái Idle. Nếu máu giảm xuống bằng hoặc nhỏ hơn 0, nhân vật chuyển sang trạng thái Dead và kết thúc lượt chơi.

# 5. Trạng thái của enemy

Tên diagram: Enemy State Diagram

Mục đích:
 Mô tả hành vi của enemy dưới dạng máy trạng thái nhằm đơn giản hóa việc xử lý AI.

Các thành phần chính:
 Patrol, Detect, Chase, Attack, Hit, Dead.

Mô tả sơ đồ:
 Enemy bắt đầu ở trạng thái tuần tra (Patrol). Khi phát hiện người chơi, enemy chuyển sang Chase để tiếp cận mục tiêu. Khi đủ khoảng cách, enemy thực hiện Attack. Nếu bị tấn công, enemy chuyển sang Hit. Khi máu giảm về 0, enemy chuyển sang Dead và bị loại khỏi room.

# 6. Luồng xử lý một room

Tên diagram: Room Processing Flow Diagram

Mục đích:
 Mô tả quá trình hoạt động của một room từ khi người chơi bước vào cho đến khi room được hoàn thành.

Các thành phần chính:
 Player Enter Room, Spawn Enemies, Combat, Room Cleared, Reward, Open Exit.

Mô tả sơ đồ:
 Khi người chơi bước vào room, các lối ra tạm thời bị khóa và enemy được sinh ra. Người chơi phải tiêu diệt toàn bộ enemy trong room. Sau khi room được hoàn thành, hệ thống phát thưởng hoặc hiển thị lựa chọn nâng cấp, sau đó mở đường sang room tiếp theo.

# 7. Thiết kế hệ thống nâng cấp

Tên diagram: Upgrade System Class Diagram

Mục đích:
 Mô tả cấu trúc dữ liệu và mối quan hệ giữa các loại nâng cấp trong trò chơi.

Các thành phần chính:
 UpgradeManager, UpgradeData, TemporaryUpgrade, PermanentUpgrade, PlayerStats, SaveData.

Mô tả sơ đồ:
 Hệ thống nâng cấp được chia thành hai nhóm chính: nâng cấp tạm thời và nâng cấp vĩnh viễn. TemporaryUpgrade chỉ có hiệu lực trong một lượt chơi hiện tại và bị xóa khi người chơi chết. PermanentUpgrade được lưu trong SaveData và tiếp tục tồn tại giữa nhiều lượt chơi khác nhau.

# 8. Thiết kế dữ liệu lưu trữ

Tên diagram: Save Data Class Diagram

Mục đích:
 Mô tả cấu trúc dữ liệu được lưu trữ để duy trì tiến trình của người chơi.

Các thành phần chính:
 SaveData, PlayerProgressData, WeaponUnlockData, AbilityUnlockData, SettingData.

Mô tả sơ đồ:
 SaveData là lớp dữ liệu trung tâm chứa toàn bộ tiến trình vĩnh viễn của người chơi. Dữ liệu bao gồm tài nguyên tích lũy, cấp độ nâng cấp chỉ số, danh sách vũ khí đã mở khóa, kỹ năng đã mở khóa và các thiết lập hệ thống như âm thanh hoặc đồ họa.

# 9. Luồng lưu và tải dữ liệu

Tên diagram: Save and Load Flow Diagram

Mục đích:
 Mô tả quy trình lưu và tải dữ liệu trong trò chơi.

Các thành phần chính:
 Game Start, Check Save File, Load SaveData, Create New SaveData, Save Progress, Update Settings.

Mô tả sơ đồ:
 Khi khởi động trò chơi, hệ thống kiểm tra sự tồn tại của tệp lưu. Nếu dữ liệu đã tồn tại, SaveData được tải và áp dụng vào game. Nếu chưa có dữ liệu, hệ thống tạo SaveData mới với các giá trị mặc định. Khi người chơi kết thúc lượt chơi hoặc thay đổi thiết lập, dữ liệu sẽ được cập nhật và lưu xuống bộ nhớ để sử dụng trong các lần chơi tiếp theo.