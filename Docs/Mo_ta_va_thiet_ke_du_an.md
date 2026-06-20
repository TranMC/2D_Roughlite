Mô tả + GDP + MVP

# TÀI LIỆU MÔ TẢ VÀ THIẾT KẾ SƠ BỘ ĐỀ TÀI

## 1. Thông tin chung

Tên đề tài: Phát triển game hành động 2D Roguelite bằng Unity

Thể loại: Game hành động 2D, Roguelite, phiêu lưu theo màn chơi.

Nền tảng phát triển: Unity 2D.

Thời gian thực hiện: Khoảng 3 tháng.

## 2. Tóm tắt đề tài

Đề tài hướng đến việc xây dựng một game hành động 2D thuộc thể loại Roguelite, lấy cảm hứng từ các game như Dead Cells nhưng được giản lược hoá để phù hợp với tốc độ và tiêu chí làm việc của nhóm và đồ án tốt nghiệp

Người chơi sẽ điều khiển nhân vật vượt qua các màn chơi, chiến đấu với quái, thu thập nâng cấp tạm thời trong mỗi lượt chơi và mở khóa các nâng cấp vĩnh viễn (ví dụ như các trang bị, một số perk vĩnh viễn -  tham khảo hệ thống skill và đồ trong dead cell) để nhân vật mạnh hơn ở những lần chơi tiếp theo. Sau một số màn nhất định, người chơi sẽ gặp boss với độ khó cao hơn.

Sản phẩm cuối cùng dự kiến là một bản game prototype/MVP có đầy đủ vòng lặp gameplay chính, bao gồm: di chuyển, chiến đấu, quái, boss, nâng cấp, lưu tiến trình và giao diện cơ bản.

## 3. Lý do chọn đề tài

Game 2D Roguelite là thể loại phù hợp để thực hiện trong phạm vi đồ án tốt nghiệp vì có thể áp dụng nhiều kiến thức về lập trình game, thiết kế hệ thống, quản lý dữ liệu, AI của kẻ địch và boss có thể hoạt động đơn giản, giao diện người dùng và kiểm thử phần mềm nhanh được.

Ngoài ra, đề tài có tính thực hành cao, dễ thể hiện kết quả thông qua bản demo trực tiếp, đồng thời vẫn có đủ độ phức tạp để đánh giá năng lực phân tích, thiết kế và triển khai của nhóm.

## 4. Mục tiêu đề tài

### 4.1. Mục tiêu tổng quát

Xây dựng một game hành động 2D Roguelite bằng Unity, có vòng lặp chơi hoàn chỉnh từ bắt đầu màn chơi, chiến đấu, nhận nâng cấp, gặp boss, kết thúc lượt chơi và lưu lại một phần tiến trình của người chơi.

### 4.2. Mục tiêu cụ thể

Xây dựng hệ thống điều khiển nhân vật 2D.

Xây dựng hệ thống chiến đấu giữa người chơi và quái.

Xây dựng hệ thống màn chơi theo dạng room hoặc stage.

Xây dựng hệ thống nâng cấp tạm thời trong mỗi lượt chơi.

Xây dựng hệ thống nâng cấp vĩnh viễn sau mỗi lượt chơi.

Xây dựng ít nhất một boss có hành vi tấn công riêng.

Xây dựng hệ thống lưu và tải dữ liệu tiến trình.

Xây dựng giao diện cơ bản như menu, HUD, pause, game over.

Hoàn thiện bản build demo có thể chạy được trên máy tính.

## 5. Ý tưởng gameplay

### 5.1. Vòng lặp gameplay chính

Người chơi bắt đầu từ màn chính, chọn bắt đầu lượt chơi mới. Trong mỗi lượt chơi, người chơi sẽ đi qua các màn nhỏ, chiến đấu với quái, nhận phần thưởng hoặc nâng cấp tạm thời. Sau khi vượt qua một số màn nhất định, người chơi sẽ gặp boss.

Nếu người chơi thất bại, lượt chơi kết thúc. Tuy nhiên, một số tài nguyên hoặc nâng cấp vĩnh viễn vẫn được giữ lại để hỗ trợ cho những lượt chơi sau.

Vòng lặp chính:

Menu chính → Bắt đầu lượt chơi → Vào màn → Đánh quái → Nhận perk → Qua màn tiếp theo → Gặp boss → Thắng hoặc thua → Cập nhật tiến trình → Quay lại menu

### 5.2. Cơ chế điều khiển nhân vật

Nhân vật chính có các hành động cơ bản:

Di chuyển trái/phải.

Nhảy hoặc né tránh.

Tấn công thường.

Sử dụng kỹ năng hoặc vũ khí.

Nhận sát thương.

Chết khi hết máu.

### 5.3. Cơ chế chiến đấu

Người chơi chiến đấu với các loại quái trong màn. Mỗi quái có máu, sát thương và hành vi riêng. Khi bị đánh bại, quái có thể rơi tài nguyên, điểm kinh nghiệm hoặc mở đường sang khu vực tiếp theo.

Boss có lượng máu cao hơn, nhiều kiểu tấn công hơn và đóng vai trò là thử thách chính sau một số màn chơi.

### 5.4. Cơ chế nâng cấp

Game có hai loại nâng cấp chính:

Nâng cấp tạm thời: Chỉ có hiệu lực trong một lượt chơi, ví dụ tăng sát thương, tăng tốc độ đánh, hồi máu, tăng tốc độ di chuyển.

Nâng cấp vĩnh viễn: Được giữ lại sau khi người chơi thất bại, ví dụ mở khóa vũ khí mới, tăng máu tối đa, tăng sát thương cơ bản hoặc mở khóa kỹ, perk buff năng mới.

## 6. Phạm vi thực hiện

### 6.1. Phạm vi bắt buộc — MVP

Các tính năng bắt buộc cần hoàn thành:

### 6.2. Tính năng mở rộng nếu còn thời gian

### 6.3. Tính năng không nằm trong phạm vi

Để đảm bảo tiến độ, nhóm không triển khai các tính năng sau trong giai đoạn đồ án:

Multiplayer.

Online leaderboard.

Hệ thống màn chơi sinh ngẫu nhiên quá phức tạp như game thương mại.

Cốt truyện dài nhiều chương.

Số lượng lớn vũ khí, boss và quái.

Hệ thống tài khoản online.

## 7. Kết quả dự kiến

Sau khi hoàn thành, đề tài dự kiến đạt được các kết quả sau:

Một bản game 2D Roguelite có thể chơi được.

Có vòng lặp gameplay rõ ràng.

Có nhân vật, enemy, boss, perk và nâng cấp.

Có hệ thống lưu tiến trình.

Có giao diện cơ bản phục vụ trải nghiệm chơi.

Có bản build demo để trình bày với giảng viên.

Có tài liệu báo cáo, slide và video demo đi kèm.

## 8. Tiêu chí đánh giá hoàn thành

Đề tài được xem là hoàn thành ở mức MVP khi đáp ứng các tiêu chí:

Người chơi có thể bắt đầu game, điều khiển nhân vật và chiến đấu.

Enemy và boss hoạt động đúng theo thiết kế.

Người chơi có thể nhận nâng cấp trong lượt chơi.

Dữ liệu nâng cấp vĩnh viễn được lưu lại sau khi kết thúc lượt chơi.

Game có màn hình menu, HUD, pause và game over.

Game có thể build và chạy ổn định trên máy tính.

Nhóm có thể demo được một lượt chơi hoàn chỉnh từ đầu đến cuối.



| STT | Tính năng | Mô tả |

|---|---|---|

| 1 | Điều khiển nhân vật | Nhân vật có thể di chuyển, nhảy/né và tấn công |

| 2 | Hệ thống máu và sát thương | Player và enemy có HP, nhận sát thương và chết |

| 3 | Enemy cơ bản | Có ít nhất 2–3 loại quái với hành vi đơn giản |

| 4 | Màn chơi dạng room/stage | Người chơi có thể đi qua nhiều khu vực |

| 5 | Hệ thống perk tạm thời | Người chơi nhận buff trong mỗi lượt chơi |

| 6 | Hệ thống nâng cấp vĩnh viễn | Sau khi chết vẫn giữ một số nâng cấp |

| 7 | Boss | Có ít nhất 1 boss hoàn chỉnh |

| 8 | Save/Load | Lưu tiến trình nâng cấp của người chơi |

| 9 | UI cơ bản | Menu, HUD, pause, game over |

| 10 | Bản build demo | Game có thể chạy và demo được |





| Tính năng | Mô tả |

|---|---|

| Nhiều loại vũ khí | Thêm 2–3 loại vũ khí khác nhau |

| Nhiều boss | Thêm boss thứ hai hoặc boss nâng cấp |

| Shop trong game | Người chơi mua vật phẩm hoặc perk |

| Nhiều biome | Có nhiều môi trường màn chơi khác nhau |

| Achievement đơn giản | Ghi nhận một số thành tựu của người chơi |

