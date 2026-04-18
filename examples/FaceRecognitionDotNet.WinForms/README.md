# FaceRecognitionDotNet WinForms

Đây là một ứng dụng WinForms riêng biệt dùng cho thư viện FaceRecognitionDotNet.

## What it does

Ứng dụng giữ nguyên thư viện lõi và cung cấp giao diện desktop để quét nhận diện khuôn mặt trên các tệp `.jpg`, `.jpeg` và `.png` trong thư mục được chọn.

Ứng dụng cũng khởi tạo PostgreSQL khi khởi động và lưu các phiên quét, ảnh đầu vào, cùng khung phát hiện khuôn mặt.

Giao diện desktop có trang xem dữ liệu để đọc lại các phiên quét và phát hiện đã lưu trong PostgreSQL.

## PostgreSQL

Hãy cập nhật [App.config](App.config) trước khi chạy ứng dụng:

- `DATABASE_URL`: URL PostgreSQL theo dạng `postgresql://postgres:Minhkhoi2204@localhost:5432`
- `PostgresAdmin`: chuỗi kết nối đến database quản trị như `postgres`
- `PostgresDatabaseName`: tên database ứng dụng sẽ tạo và dùng, ví dụ `face_recognition_winforms`

Ứng dụng sẽ tự tạo các bảng sau nếu chúng chưa tồn tại:

- `scan_sessions`
- `images`
- `detections`
- `app_users`
- `attendance_logs`

Ảnh chấm công và ảnh chụp từ webcam sẽ được sao chép vào thư mục cục bộ `img` cạnh ứng dụng, và đường dẫn lưu trong CSDL sẽ trỏ tới file đã sao chép đó.

Ảnh khuôn mặt đã đăng ký của từng tài khoản sẽ được lưu vào thư mục `face`. Mỗi tài khoản chỉ giữ đúng một ảnh khuôn mặt và chỉ được đăng ký một lần; nếu tài khoản đã có khuôn mặt thì ứng dụng sẽ không cho ghi đè.

Điều này áp dụng cho quét ảnh đơn, quét thư mục, đăng ký khuôn mặt và ảnh chấm công.

Trang chấm công cũng hỗ trợ nhận diện webcam trực tiếp với ghi nhận vào làm/tan làm tự động theo giờ Việt Nam. Người dùng có thể tự đăng ký khuôn mặt ngay trên trang này, nhưng chỉ một lần duy nhất và ảnh đăng ký phải chứa đúng một khuôn mặt. Hệ thống chỉ điểm danh khi khuôn mặt khớp với ảnh đã đăng ký của tài khoản trong thư mục `face`. Trang này có bảng tổng hợp hằng ngày với thời gian vào và ra cho từng người dùng.

Hành vi theo vai trò:

- Tài khoản `admin` có thể mở bảng điều khiển quản trị để xem toàn bộ lịch sử đăng nhập và tổng hợp chấm công.
- Tài khoản `user` chỉ thấy trang Chấm công để vào/ra bằng khuôn mặt.

Khi khởi động lần đầu, ứng dụng cũng tạo sẵn hai tài khoản mặc định nếu chưa tồn tại:

- `user123` / `user123`
- `admin123` / `admin123`

Trang Chấm công sử dụng giờ Việt Nam. `07:00` là thời điểm bắt đầu vào làm và `17:00` là thời điểm bắt đầu tan làm.

Nếu bạn muốn tạo bảng thủ công trước khi chạy ứng dụng, hãy chạy [sql/setup_postgres.sql](sql/setup_postgres.sql) trên database `face_recognition_winforms`. Đây là file SQL thuần và có thể chạy bằng `psql`, pgAdmin hoặc bất kỳ trình soạn SQL nào sau khi database đã tồn tại.

## Biên dịch

1. Mở solution.
2. Biên dịch `examples/FaceRecognitionDotNet.WinForms`.
3. Đảm bảo có thư mục `models` cạnh file thực thi.

## Chạy

1. Khởi chạy ứng dụng.
2. Chọn một thư mục có ảnh.
3. Chọn `Hog` hoặc `Cnn` và số lượng CPU.
4. Nhấn `Chạy`.

Khung kết quả sẽ hiển thị các khung mặt đã phát hiện theo định dạng gần giống CSV.
