-- ============================================================
--  HỆ THỐNG CHẤM CÔNG NHẬN DIỆN KHUÔN MẶT
--  Phiên bản : 3.0  —  Face-Attendance Focus
--  Database  : SQLite 3.35+
--  Timezone  : Asia/Ho_Chi_Minh
--  Stack     : .NET 4.6.1 / WinForms / System.Data.SQLite
-- ============================================================
--
--  DANH SÁCH BẢNG (16 bảng / 5 nhóm):
--
--  [A] DANH MỤC (3)
--      A1. departments              — Phòng ban (cây cha-con)
--      A2. positions                — Chức vụ (có level phân quyền)
--      A3. work_shifts              — Ca làm việc
--
--  [B] NHÂN VIÊN & KHUÔN MẶT (4)
--      B1. employees                — Thông tin nhân viên
--      B2. users                    — Tài khoản đăng nhập
--      B3. face_data                — Face encoding 128-D (CORE)
--      B4. face_registration_logs   — Lịch sử đăng ký / xóa khuôn mặt
--
--  [C] THIẾT BỊ & LỊCH LÀM VIỆC (4)
--      C1. attendance_devices       — Camera / Tablet / Kiosk / Mobile
--      C2. holidays                 — Ngày lễ, nghỉ bù
--      C3. employee_shift_schedule  — Phân ca chi tiết từng ngày
--      C4. work_calendars           — Cấu hình ngày làm tuần
--
--  [D] CHẤM CÔNG (3)
--      D1. attendance_records       — Bản ghi chấm công chính (1/NV/ngày)
--      D2. attendance_logs          — Nhật ký từng lần nhận diện (audit)
--      D3. leave_requests           — Đơn nghỉ phép
--
--  [E] HỆ THỐNG (2)
--      E1. audit_logs               — Nhật ký thao tác người dùng
--      E2. system_settings          — Cấu hình key-value
--
-- ============================================================
--  SƠ ĐỒ QUAN HỆ CHÍNH:
--
--  departments ──< employees >── positions
--                     │
--            ┌────────┼──────────────────┐
--            │        │                  │
--         face_data  users    employee_shift_schedule
--            │
--   face_registration_logs
--
--  employees ──< attendance_records >── work_shifts
--                     │
--              attendance_logs >── attendance_devices
--
--  employees ──< leave_requests
-- ============================================================


-- ============================================================
--  DROP (thứ tự ngược FK)
-- ============================================================
DROP TABLE IF EXISTS AUDIT_LOGS;

DROP TABLE IF EXISTS SYSTEM_SETTINGS;

DROP TABLE IF EXISTS LEAVE_REQUESTS;

DROP TABLE IF EXISTS ATTENDANCE_LOGS;

DROP TABLE IF EXISTS ATTENDANCE_RECORDS;

DROP TABLE IF EXISTS EMPLOYEE_SHIFT_SCHEDULE;

DROP TABLE IF EXISTS WORK_CALENDARS;

DROP TABLE IF EXISTS HOLIDAYS;

DROP TABLE IF EXISTS ATTENDANCE_DEVICES;

DROP TABLE IF EXISTS FACE_REGISTRATION_LOGS;

DROP TABLE IF EXISTS FACE_DATA;

DROP TABLE IF EXISTS USERS;

DROP TABLE IF EXISTS EMPLOYEES;

DROP TABLE IF EXISTS WORK_SHIFTS;

DROP TABLE IF EXISTS POSITIONS;

DROP TABLE IF EXISTS DEPARTMENTS;
-- ============================================================
--  DROP VIEWS
-- ============================================================
DROP VIEW IF EXISTS V_TODAY_ATTENDANCE;
DROP VIEW IF EXISTS V_MONTHLY_SUMMARY;
DROP VIEW IF EXISTS V_FACE_STATUS;
DROP VIEW IF EXISTS V_ATTENDANCE_ANOMALIES;
DROP VIEW IF EXISTS V_PENDING_LEAVES;
DROP VIEW IF EXISTS V_LEAVE_BALANCE;
DROP VIEW IF EXISTS V_SUSPICIOUS_RECOGNITION;


-- ============================================================
--  NHÓM A: DANH MỤC
-- ============================================================

-- ── A1. departments ──────────────────────────────────────────
CREATE TABLE DEPARTMENTS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CODE TEXT NOT NULL UNIQUE,
    NAME TEXT NOT NULL,
    DESCRIPTION TEXT,
    PARENT_ID INT REFERENCES DEPARTMENTS(ID) ON DELETE SET NULL,
    MANAGER_ID INT, -- FK → employees(id), bổ sung sau
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
    SORT_ORDER SMALLINT NOT NULL DEFAULT 0,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ── A2. positions ─────────────────────────────────────────────
CREATE TABLE POSITIONS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CODE TEXT NOT NULL UNIQUE,
    NAME TEXT NOT NULL,
    LEVEL SMALLINT NOT NULL DEFAULT 1 CHECK (LEVEL BETWEEN 1 AND 10),
 
    -- 1=Thực tập  3=Nhân viên  5=Trưởng nhóm  7=Trưởng phòng  10=Giám đốc
    -- level dùng để routing duyệt đơn từ phía application
    DESCRIPTION TEXT,
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ── A3. work_shifts ───────────────────────────────────────────
CREATE TABLE WORK_SHIFTS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CODE TEXT NOT NULL UNIQUE,
    NAME TEXT NOT NULL,
    SHIFT_TYPE TEXT NOT NULL DEFAULT 'Fixed' CHECK (SHIFT_TYPE IN ('Fixed', 'Flexible', 'Shift')),
 
    -- Fixed    = giờ vào/ra cố định (hành chính)
    -- Flexible = linh hoạt, tính đủ standard_hours/ngày
    -- Shift    = ca xoay 3 ca
    START_TIME TEXT NOT NULL,
    END_TIME TEXT NOT NULL,
    BREAK_MINUTES SMALLINT NOT NULL DEFAULT 60 CHECK (BREAK_MINUTES >= 0),
    STANDARD_HOURS REAL NOT NULL DEFAULT 8 CHECK (STANDARD_HOURS > 0),
 
    -- Tổng giờ làm chuẩn = (end-start) - break  (business layer tính)
    LATE_THRESHOLD SMALLINT NOT NULL DEFAULT 15 CHECK (LATE_THRESHOLD >= 0),
 
    -- Phút ân hạn đến muộn; check_in <= start + threshold → vẫn đúng giờ
    EARLY_THRESHOLD SMALLINT NOT NULL DEFAULT 15 CHECK (EARLY_THRESHOLD >= 0),
 
    -- Phút ân hạn về sớm; check_out >= end - threshold → không bị về sớm
    IS_OVERNIGHT INTEGER NOT NULL DEFAULT 0,
 
    -- 1 = ca qua đêm: end_time thuộc ngày D+1 (vd 22:00→06:00)
    COLOR_CODE TEXT,
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ============================================================
--  NHÓM B: NHÂN VIÊN & KHUÔN MẶT
-- ============================================================

-- ── B1. employees ─────────────────────────────────────────────
CREATE TABLE EMPLOYEES (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CODE TEXT NOT NULL UNIQUE,
    FULL_NAME TEXT NOT NULL,
    GENDER TEXT CHECK (GENDER IN ('M', 'F', 'O')),
    DATE_OF_BIRTH TEXT,
    PHONE TEXT,
    EMAIL TEXT UNIQUE,
    IDENTITY_CARD TEXT UNIQUE, -- CCCD / CMND
    DEPARTMENT_ID INT REFERENCES DEPARTMENTS(ID) ON DELETE SET NULL,
    POSITION_ID INT REFERENCES POSITIONS(ID) ON DELETE SET NULL,
    DEFAULT_SHIFT_ID INT REFERENCES WORK_SHIFTS(ID) ON DELETE SET NULL,
 
    -- Ca mặc định — fallback khi không có employee_shift_schedule
    MANAGER_ID INT REFERENCES EMPLOYEES(ID) ON DELETE SET NULL,
 
    -- Quản lý trực tiếp — dùng routing duyệt đơn nghỉ phép
    HIRE_DATE TEXT NOT NULL DEFAULT CURRENT_DATE,
    TERMINATION_DATE TEXT,
    EMPLOYMENT_TYPE TEXT NOT NULL DEFAULT 'FullTime' CHECK (EMPLOYMENT_TYPE IN ('FullTime', 'PartTime', 'Contract', 'Intern')),
    WORK_LOCATION TEXT, -- Tên chi nhánh / văn phòng làm việc
    AVATAR_PATH TEXT, -- Ảnh đại diện (khác face_data)
    -- ── Trạng thái Face ID ──
    IS_FACE_REGISTERED INTEGER NOT NULL DEFAULT 0,
 
    -- Trigger tự động cập nhật từ face_data
    FACE_REGISTERED_AT DATETIME,
 
    -- Thời điểm đăng ký face đầu tiên thành công
    -- ── Nghỉ phép (phạm vi chấm công) ──
    ANNUAL_LEAVE_DAYS REAL NOT NULL DEFAULT 12 CHECK (ANNUAL_LEAVE_DAYS >= 0),
    USED_LEAVE_DAYS REAL NOT NULL DEFAULT 0 CHECK (USED_LEAVE_DAYS >= 0),
 
    -- Trigger tự động cộng/trừ khi leave_request thay đổi status
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME,
    CONSTRAINT CHK_EMP_DATES CHECK ( TERMINATION_DATE IS NULL OR TERMINATION_DATE >= HIRE_DATE )
);

-- ── B2. users ─────────────────────────────────────────────────
CREATE TABLE USERS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    USERNAME TEXT NOT NULL UNIQUE,
    PASSWORD_HASH TEXT NOT NULL,
 
    -- BCrypt hash cost=12; KHÔNG lưu plain text
    EMPLOYEE_ID INT UNIQUE REFERENCES EMPLOYEES(ID) ON DELETE SET NULL,
 
    -- UNIQUE: 1 nhân viên = tối đa 1 tài khoản
    ROLE TEXT NOT NULL DEFAULT 'Employee' CHECK (ROLE IN ('SuperAdmin', 'Admin', 'HR', 'Manager', 'Employee')),
 
    -- SuperAdmin : toàn quyền hệ thống + cấu hình
    -- Admin      : quản trị danh mục, thiết bị, ca làm
    -- HR         : xem & chỉnh sửa toàn bộ chấm công, duyệt đơn
    -- Manager    : duyệt đơn nhân viên trong phòng mình
    -- Employee   : xem lịch sử & nộp đơn của bản thân
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
    LAST_LOGIN DATETIME,
    FAILED_LOGIN_COUNT SMALLINT NOT NULL DEFAULT 0,
    LOCKED_UNTIL DATETIME,
 
    -- Khóa tạm thời sau N lần đăng nhập sai (cấu hình qua system_settings)
    REFRESH_TOKEN_HASH TEXT,
 
    -- SHA-256 hash của JWT Refresh Token
    REFRESH_TOKEN_EXPIRY DATETIME,
    MUST_CHANGE_PASSWORD INTEGER NOT NULL DEFAULT 0,
 
    -- 1 = buộc đổi mật khẩu lần đăng nhập kế tiếp
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ── B3. face_data  (CORE TABLE) ───────────────────────────────
CREATE TABLE FACE_DATA (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    EMPLOYEE_ID INT NOT NULL REFERENCES EMPLOYEES(ID) ON DELETE CASCADE,
 
    -- Xóa NV → xóa toàn bộ face data
    -- ── Dữ liệu nhận diện ──
    ENCODING TEXT NOT NULL,
 
    -- double[128] serialize → semicolon-separated string
    -- Tính bằng: dlib face_recognition / FaceRecognitionDotNet
    -- Lưu: "0.123;-0.456;0.789;..." (128 giá trị double, phân tách bởi ;)
    IMAGE_PATH TEXT NOT NULL,
 
    -- Ảnh gốc lưu server / Azure Blob / MinIO
    THUMBNAIL_PATH TEXT,
 
    -- Ảnh thu nhỏ 120×120 hiển thị UI
    -- ── Chất lượng & góc chụp ──
    IMAGE_INDEX SMALLINT NOT NULL DEFAULT 1 CHECK (IMAGE_INDEX BETWEEN 1 AND 5),
 
    -- Tối đa 5 ảnh/NV (5 góc khác nhau)
    ANGLE TEXT CHECK (ANGLE IN ('Front', 'Left', 'Right', 'Up', 'Down')),
 
    -- Góc chụp để tăng độ bao phủ nhận diện
    QUALITY_SCORE REAL NOT NULL DEFAULT 0 CHECK (QUALITY_SCORE BETWEEN 0 AND 1),
 
    -- 0.0 (xấu) → 1.0 (tốt); reject nếu < 0.6 (cấu hình system_settings)
    BRIGHTNESS REAL CHECK (BRIGHTNESS BETWEEN 0 AND 255),
 
    -- Độ sáng trung bình ảnh
    SHARPNESS REAL CHECK (SHARPNESS >= 0),
 
    -- Độ sắc nét (Laplacian variance)
    FACE_BBOX TEXT,
 
    -- Bounding box: {"x":10,"y":20,"w":100,"h":100}
    -- ── Trạng thái & kiểm duyệt ──
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
 
    -- 0 = vô hiệu hóa (không dùng nhận diện) nhưng giữ lịch sử
    IS_VERIFIED INTEGER NOT NULL DEFAULT 0,
 
    -- 1 = HR/Admin đã xem xét & xác nhận chất lượng
    VERIFIED_BY INT REFERENCES USERS(ID) ON DELETE SET NULL,
    VERIFIED_AT DATETIME,
    REGISTERED_BY INT REFERENCES USERS(ID) ON DELETE SET NULL,
 
    -- Admin tự đăng ký hoặc NV tự chụp qua kiosk
    NOTE TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME,
    CONSTRAINT UQ_FACE_EMP_INDEX UNIQUE (EMPLOYEE_ID, IMAGE_INDEX)
 -- Mỗi slot (1-5) chỉ có 1 ảnh
);

-- ── B4. face_registration_logs ────────────────────────────────
-- Ghi lại toàn bộ sự kiện đăng ký / cập nhật / xóa khuôn mặt
CREATE TABLE FACE_REGISTRATION_LOGS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    EMPLOYEE_ID INT NOT NULL REFERENCES EMPLOYEES(ID) ON DELETE CASCADE,
    FACE_DATA_ID INT REFERENCES FACE_DATA(ID) ON DELETE SET NULL,
    ACTION TEXT NOT NULL CHECK (ACTION IN ('Register', 'Update', 'Delete', 'Verify', 'Deactivate')),
 
    -- Register   = đăng ký mới
    -- Update     = chụp lại ảnh cho slot đã có
    -- Delete     = xóa vĩnh viễn
    -- Verify     = HR xác nhận chất lượng
    -- Deactivate = vô hiệu hóa (is_active = 0)
    IMAGE_INDEX SMALLINT,
    QUALITY_SCORE REAL,
 
    -- Snapshot điểm chất lượng tại thời điểm thao tác
    PERFORMED_BY INT REFERENCES USERS(ID) ON DELETE SET NULL,
    REASON TEXT,
 
    -- Lý do (bắt buộc khi Delete / Deactivate)
    IP_ADDRESS TEXT,
    DEVICE_INFO TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
--  NHÓM C: THIẾT BỊ & LỊCH LÀM VIỆC
-- ============================================================

-- ── C1. attendance_devices ────────────────────────────────────
CREATE TABLE ATTENDANCE_DEVICES (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    DEVICE_CODE TEXT NOT NULL UNIQUE,
    DEVICE_NAME TEXT NOT NULL,
    DEVICE_TYPE TEXT NOT NULL DEFAULT 'Camera' CHECK (DEVICE_TYPE IN ('Camera', 'Tablet', 'Kiosk', 'Mobile')),
 
    -- Camera  = camera IP cố định tại cổng/phòng
    -- Tablet  = tablet / kiosk cảm ứng
    -- Kiosk   = máy chuyên dụng chấm công
    -- Mobile  = ứng dụng di động (geofencing)
    LOCATION_NAME TEXT,
 
    -- "Cổng chính - Tầng 1", "Phòng IT - Tầng 3"
    IP_ADDRESS TEXT,
 
    -- Whitelist IP — chống giả mạo thiết bị nội bộ
    MAC_ADDRESS TEXT,
 
    -- Whitelist MAC
    -- ── GPS / Geofencing (dành cho Mobile) ──
    LATITUDE REAL,
    LONGITUDE REAL,
    RADIUS_METERS INT DEFAULT 100 CHECK (RADIUS_METERS > 0),
 
    -- Bán kính cho phép check-in (Mobile geofencing)
    -- ── Cấu hình nhận diện ──
    MIN_CONFIDENCE REAL NOT NULL DEFAULT 0.70 CHECK (MIN_CONFIDENCE BETWEEN 0 AND 1),
 
    -- Override ngưỡng confidence tại thiết bị này
    -- (mặc định lấy từ system_settings nếu NULL)
    CAMERA_URL TEXT,
 
    -- RTSP URL hoặc HTTP snapshot URL (Camera IP)
    IS_ONLINE INTEGER NOT NULL DEFAULT 0,
 
    -- Cập nhật bởi heartbeat job
    LAST_HEARTBEAT DATETIME,
    IS_ACTIVE INTEGER NOT NULL DEFAULT 1,
    NOTE TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ── C2. holidays ──────────────────────────────────────────────
CREATE TABLE HOLIDAYS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    HOLIDAY_DATE TEXT NOT NULL,
    NAME TEXT NOT NULL,
    HOLIDAY_TYPE TEXT NOT NULL DEFAULT 'National' CHECK (HOLIDAY_TYPE IN ('National', 'Company', 'Compensatory')),
 
    -- National     = Ngày lễ quốc gia (theo luật Việt Nam)
    -- Company      = Nghỉ riêng của công ty
    -- Compensatory = Nghỉ bù khi lễ trùng cuối tuần
    DESCRIPTION TEXT,
    IS_RECURRING INTEGER NOT NULL DEFAULT 0,
 
    -- 1 = tự tạo bản ghi năm mới (background job đầu năm)
    YEAR SMALLINT NOT NULL DEFAULT (CAST(STRFTIME('%Y', 'now') AS INTEGER)),
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_HOLIDAY_DATE_YEAR UNIQUE (HOLIDAY_DATE, YEAR)
);

-- ── C3. employee_shift_schedule ───────────────────────────────
CREATE TABLE EMPLOYEE_SHIFT_SCHEDULE (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    EMPLOYEE_ID INT NOT NULL REFERENCES EMPLOYEES(ID) ON DELETE CASCADE,
    SHIFT_ID INT NOT NULL REFERENCES WORK_SHIFTS(ID) ON DELETE RESTRICT,
    WORK_DATE TEXT NOT NULL,
    IS_DAY_OFF INTEGER NOT NULL DEFAULT 0,
 
    -- 1 = ngày nghỉ theo lịch phân công (ROT / ngày nghỉ bù riêng)
    NOTE TEXT,
    CREATED_BY INT REFERENCES USERS(ID) ON DELETE SET NULL,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_SCHEDULE_EMP_DATE UNIQUE (EMPLOYEE_ID, WORK_DATE)
 -- 1 NV chỉ có 1 lịch/ngày
);

-- ── C4. work_calendars ────────────────────────────────────────
CREATE TABLE WORK_CALENDARS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    NAME TEXT NOT NULL,
 
    -- "Hành chính T2-T6", "Vận hành T2-T7", "Nhà máy 3 ca 7 ngày"
    MONDAY INTEGER NOT NULL DEFAULT 1,
    TUESDAY INTEGER NOT NULL DEFAULT 1,
    WEDNESDAY INTEGER NOT NULL DEFAULT 1,
    THURSDAY INTEGER NOT NULL DEFAULT 1,
    FRIDAY INTEGER NOT NULL DEFAULT 1,
    SATURDAY INTEGER NOT NULL DEFAULT 0,
    SUNDAY INTEGER NOT NULL DEFAULT 0,
    EFFECTIVE_FROM TEXT NOT NULL DEFAULT CURRENT_DATE,
    EFFECTIVE_TO TEXT, -- NULL = vô thời hạn
    IS_DEFAULT INTEGER NOT NULL DEFAULT 0,
 
    -- Trigger đảm bảo chỉ 1 bản ghi is_default=1
    DESCRIPTION TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ============================================================
--  NHÓM D: CHẤM CÔNG
-- ============================================================

-- ── D1. attendance_records ────────────────────────────────────
-- Bảng trung tâm: mỗi nhân viên tối đa 1 bản ghi / ngày
CREATE TABLE ATTENDANCE_RECORDS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    EMPLOYEE_ID INT NOT NULL REFERENCES EMPLOYEES(ID) ON DELETE RESTRICT,
    ATTENDANCE_DATE TEXT NOT NULL DEFAULT CURRENT_DATE,
    SHIFT_ID INT REFERENCES WORK_SHIFTS(ID) ON DELETE SET NULL,
 
    -- Ca áp dụng hôm đó (business layer: lấy từ schedule → default_shift)
    -- ── Dữ liệu Check-In ──
    CHECK_IN DATETIME,
    CHECK_IN_DEVICE_ID INT REFERENCES ATTENDANCE_DEVICES(ID) ON DELETE SET NULL,
    CHECK_IN_IMAGE_PATH TEXT, -- Ảnh chụp lúc check-in (lưu để xem lại)
    CHECK_IN_METHOD TEXT DEFAULT 'Face' CHECK (CHECK_IN_METHOD IN ('Face', 'Manual', 'QRCode', 'NFC', 'Mobile')),
    CHECK_IN_CONFIDENCE REAL CHECK (CHECK_IN_CONFIDENCE IS NULL OR CHECK_IN_CONFIDENCE BETWEEN 0 AND 1),
 
    -- Độ tin cậy khuôn mặt: 0.0 → 1.0
    CHECK_IN_LATITUDE REAL,
    CHECK_IN_LONGITUDE REAL,
 
    -- GPS tọa độ lúc check-in (Mobile)
    -- ── Dữ liệu Check-Out ──
    CHECK_OUT DATETIME,
    CHECK_OUT_DEVICE_ID INT REFERENCES ATTENDANCE_DEVICES(ID) ON DELETE SET NULL,
    CHECK_OUT_IMAGE_PATH TEXT,
    CHECK_OUT_METHOD TEXT CHECK (CHECK_OUT_METHOD IN ('Face', 'Manual', 'QRCode', 'NFC', 'Mobile')),
    CHECK_OUT_CONFIDENCE REAL CHECK (CHECK_OUT_CONFIDENCE IS NULL OR CHECK_OUT_CONFIDENCE BETWEEN 0 AND 1),
    CHECK_OUT_LATITUDE REAL,
    CHECK_OUT_LONGITUDE REAL,
 
    -- ── Kết quả tính toán (Business Layer → lưu vào DB) ──
    STATUS TEXT NOT NULL DEFAULT 'NotYet' CHECK (STATUS IN ( 'Present', -- Có mặt đúng giờ
    'Late', -- Đi muộn (quá late_threshold)
    'EarlyLeave', -- Về sớm (trước early_threshold)
    'LateAndEarly', -- Vừa muộn vừa về sớm
    'Absent', -- Vắng không phép
    'Leave', -- Nghỉ phép (có đơn Approved)
    'Holiday', -- Ngày lễ
    'DayOff', -- Ngày nghỉ theo lịch phân công
    'NotYet' -- Chưa đến (trong ngày làm việc)
    )),
    LATE_MINUTES SMALLINT NOT NULL DEFAULT 0 CHECK (LATE_MINUTES >= 0),
    EARLY_MINUTES SMALLINT NOT NULL DEFAULT 0 CHECK (EARLY_MINUTES >= 0),
    WORKING_MINUTES INT NOT NULL DEFAULT 0 CHECK (WORKING_MINUTES >= 0),
 
    -- Phút làm thực tế = (check_out - check_in) - break_minutes
    -- Dùng phút thay vì decimal giờ để tránh sai số làm tròn
    -- ── Điều chỉnh thủ công ──
    IS_MANUAL_EDIT INTEGER NOT NULL DEFAULT 0,
    MANUAL_EDIT_BY INT REFERENCES USERS(ID) ON DELETE SET NULL,
    MANUAL_EDIT_AT DATETIME,
    MANUAL_EDIT_REASON TEXT,
 
    -- Bắt buộc khi is_manual_edit = 1 (check tại app / trigger)
    NOTE TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME,
    CONSTRAINT UQ_ATTENDANCE_EMP_DATE UNIQUE (EMPLOYEE_ID, ATTENDANCE_DATE),
    CONSTRAINT CHK_CHECKOUT_AFTER_CHECKIN CHECK ( CHECK_OUT IS NULL OR CHECK_IN IS NULL OR CHECK_OUT >= CHECK_IN ),
    CONSTRAINT CHK_MANUAL_REASON CHECK ( IS_MANUAL_EDIT = 0 OR (IS_MANUAL_EDIT = 1 AND MANUAL_EDIT_REASON IS NOT NULL) )
);

-- ── D2. attendance_logs ───────────────────────────────────────
-- Nhật ký từng lần camera nhận diện / quẹt thẻ
-- Không thể DELETE / UPDATE — append-only audit trail
CREATE TABLE ATTENDANCE_LOGS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    ATTENDANCE_ID BIGINT REFERENCES ATTENDANCE_RECORDS(ID) ON DELETE SET NULL,
 
    -- NULL = chưa map được vào bản ghi (NV chưa đăng ký face, nhận diện thất bại)
    EMPLOYEE_ID INT REFERENCES EMPLOYEES(ID) ON DELETE SET NULL,
    DEVICE_ID INT REFERENCES ATTENDANCE_DEVICES(ID) ON DELETE SET NULL,
    LOG_TIME DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LOG_TYPE TEXT NOT NULL CHECK (LOG_TYPE IN ('CheckIn', 'CheckOut', 'Unknown')),
    METHOD TEXT NOT NULL DEFAULT 'Face' CHECK (METHOD IN ('Face', 'Manual', 'QRCode', 'NFC', 'Mobile')),
 
    -- ── Kết quả nhận diện khuôn mặt ──
    MATCHED_FACE_ID INT REFERENCES FACE_DATA(ID) ON DELETE SET NULL,
 
    -- Face slot nào đã match
    CONFIDENCE REAL CHECK (CONFIDENCE IS NULL OR CONFIDENCE BETWEEN 0 AND 1),
    FACE_DISTANCE REAL CHECK (FACE_DISTANCE IS NULL OR FACE_DISTANCE >= 0),
 
    -- Khoảng cách Euclidean: nhỏ hơn = giống hơn (< 0.4 = rất giống)
    IMAGE_PATH TEXT, -- Ảnh chụp tại thời điểm nhận diện (evidence)
    -- ── Vị trí ──
    LATITUDE REAL,
    LONGITUDE REAL,
    IP_ADDRESS TEXT,
 
    -- ── Kết quả xử lý ──
    RESULT TEXT NOT NULL DEFAULT 'Success' CHECK (RESULT IN ( 'Success', -- Nhận diện thành công
    'Failed', -- Không khớp khuôn mặt nào
    'Suspicious', -- Khớp nhưng confidence thấp (cần xem lại)
    'Duplicate', -- Check-in trùng trong khoảng duplicate_window
    'Spoofing', -- Phát hiện ảnh giả / video playback
    'DeviceError' -- Lỗi thiết bị / không lấy được frame
    )),
    FAIL_REASON TEXT,
 
    -- Chi tiết lỗi khi result != 'Success'
    RAW_PAYLOAD TEXT,
 
    -- Dữ liệu thô từ thiết bị gửi lên (debug, không dùng business logic)
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
 -- KHÔNG có updated_at — append-only
);

-- ── D3. leave_requests ────────────────────────────────────────
-- Đơn nghỉ phép — ảnh hưởng trực tiếp đến status chấm công
CREATE TABLE LEAVE_REQUESTS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    EMPLOYEE_ID INT NOT NULL REFERENCES EMPLOYEES(ID) ON DELETE RESTRICT,
    LEAVE_TYPE TEXT NOT NULL CHECK (LEAVE_TYPE IN ( 'Annual', -- Nghỉ phép năm
    'Sick', -- Nghỉ ốm (có giấy bác sĩ)
    'Maternity', -- Thai sản (nữ)
    'Paternity', -- Nghỉ vợ đẻ (nam)
    'Marriage', -- Nghỉ kết hôn
    'Bereavement', -- Nghỉ tang
    'Unpaid', -- Không lương
    'WFH', -- Làm từ xa / WFH
    'Other' -- Khác
    )),
    START_DATE TEXT NOT NULL,
    END_DATE TEXT NOT NULL,
    TOTAL_DAYS REAL NOT NULL CHECK (TOTAL_DAYS > 0),
 
    -- 0.5 = bán ngày; business layer tính (loại trừ T7, CN, ngày lễ)
    IS_HALF_DAY INTEGER NOT NULL DEFAULT 0,
    HALF_DAY_PERIOD TEXT CHECK (HALF_DAY_PERIOD IN ('Morning', 'Afternoon')),
    REASON TEXT NOT NULL,
    DOCUMENT_PATH TEXT,
 
    -- File đính kèm: giấy nghỉ ốm, giấy kết hôn... (PDF/JPG)
    STATUS TEXT NOT NULL DEFAULT 'Pending' CHECK (STATUS IN ('Pending', 'Approved', 'Rejected', 'Cancelled')),
    APPROVED_BY INT REFERENCES EMPLOYEES(ID) ON DELETE SET NULL,
    APPROVED_AT DATETIME,
    REJECT_REASON TEXT,
    NOTE TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME,
    CONSTRAINT CHK_LEAVE_DATES CHECK (END_DATE >= START_DATE),
    CONSTRAINT CHK_HALF_DAY CHECK ( IS_HALF_DAY = 0 OR (START_DATE = END_DATE AND HALF_DAY_PERIOD IS NOT NULL) ),
    CONSTRAINT CHK_LEAVE_APPROVED CHECK ( STATUS != 'Approved' OR (APPROVED_BY IS NOT NULL AND APPROVED_AT IS NOT NULL) )
);

-- ============================================================
--  NHÓM E: HỆ THỐNG & KIỂM TOÁN
-- ============================================================

-- ── E1. audit_logs ────────────────────────────────────────────
-- Append-only — KHÔNG bao giờ UPDATE / DELETE
CREATE TABLE AUDIT_LOGS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    USER_ID INT REFERENCES USERS(ID) ON DELETE SET NULL,
    EMPLOYEE_ID INT REFERENCES EMPLOYEES(ID) ON DELETE SET NULL,
    ACTION TEXT NOT NULL,
 
    -- LOGIN, LOGOUT, CREATE, UPDATE, DELETE, APPROVE, REJECT,
    -- FACE_REGISTER, FACE_DELETE, ATTENDANCE_EDIT, EXPORT ...
    TABLE_NAME TEXT,
    RECORD_ID TEXT, -- ID bản ghi bị tác động (TEXT để linh hoạt)
    OLD_VALUES TEXT, -- Snapshot trước thay đổi
    NEW_VALUES TEXT, -- Snapshot sau thay đổi
    IP_ADDRESS TEXT,
    USER_AGENT TEXT,
    DESCRIPTION TEXT,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ── E2. system_settings ───────────────────────────────────────
CREATE TABLE SYSTEM_SETTINGS (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    KEY TEXT NOT NULL UNIQUE,
    VALUE TEXT NOT NULL,
    VALUE_TYPE TEXT NOT NULL DEFAULT 'String' CHECK (VALUE_TYPE IN ('String', 'Integer', 'Decimal', 'Boolean', 'Json')),
    CATEGORY TEXT NOT NULL DEFAULT 'General' CHECK (CATEGORY IN ('General', 'FaceRecognition', 'Attendance', 'Security', 'Notification')),
    DESCRIPTION TEXT,
    IS_EDITABLE INTEGER NOT NULL DEFAULT 1,
    UPDATED_BY INT REFERENCES USERS(ID) ON DELETE SET NULL,
    CREATED_AT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATED_AT DATETIME
);

-- ============================================================
--  INDEXES
-- ============================================================

-- employees
CREATE INDEX IDX_EMP_DEPT ON EMPLOYEES(DEPARTMENT_ID);

CREATE INDEX IDX_EMP_POSITION ON EMPLOYEES(POSITION_ID);

CREATE INDEX IDX_EMP_SHIFT ON EMPLOYEES(DEFAULT_SHIFT_ID);

CREATE INDEX IDX_EMP_MANAGER ON EMPLOYEES(MANAGER_ID);

CREATE INDEX IDX_EMP_ACTIVE ON EMPLOYEES(IS_ACTIVE) WHERE IS_ACTIVE = 1;

CREATE INDEX IDX_EMP_FACE ON EMPLOYEES(IS_FACE_REGISTERED, IS_ACTIVE);

-- users
CREATE INDEX IDX_USR_EMPLOYEE ON USERS(EMPLOYEE_ID);

CREATE INDEX IDX_USR_ROLE ON USERS(ROLE);

-- face_data  ← hot table (load vào memory cache)
CREATE INDEX IDX_FACE_EMP ON FACE_DATA(EMPLOYEE_ID, IS_ACTIVE) WHERE IS_ACTIVE = 1;

CREATE INDEX IDX_FACE_VERIFIED ON FACE_DATA(IS_VERIFIED) WHERE IS_VERIFIED = 0;

-- face_registration_logs
CREATE INDEX IDX_FRLOG_EMP ON FACE_REGISTRATION_LOGS(EMPLOYEE_ID, CREATED_AT DESC);

CREATE INDEX IDX_FRLOG_ACTION ON FACE_REGISTRATION_LOGS(ACTION, CREATED_AT DESC);

-- attendance_devices
CREATE INDEX IDX_DEV_TYPE ON ATTENDANCE_DEVICES(DEVICE_TYPE, IS_ACTIVE);

CREATE INDEX IDX_DEV_ONLINE ON ATTENDANCE_DEVICES(IS_ONLINE) WHERE IS_ONLINE = 1;

-- employee_shift_schedule
CREATE INDEX IDX_SCHED_EMP ON EMPLOYEE_SHIFT_SCHEDULE(EMPLOYEE_ID, WORK_DATE);

CREATE INDEX IDX_SCHED_DATE ON EMPLOYEE_SHIFT_SCHEDULE(WORK_DATE);

-- holidays
CREATE INDEX IDX_HOLIDAY_YEAR ON HOLIDAYS(YEAR, HOLIDAY_DATE);

-- attendance_records  ← most queried
CREATE INDEX IDX_ATT_EMP_DATE ON ATTENDANCE_RECORDS(EMPLOYEE_ID, ATTENDANCE_DATE DESC);

CREATE INDEX IDX_ATT_DATE ON ATTENDANCE_RECORDS(ATTENDANCE_DATE);

CREATE INDEX IDX_ATT_STATUS ON ATTENDANCE_RECORDS(STATUS)
    WHERE STATUS NOT IN ('Present', 'Holiday', 'DayOff');

CREATE INDEX IDX_ATT_MONTH ON ATTENDANCE_RECORDS(EMPLOYEE_ID, ATTENDANCE_DATE);

CREATE INDEX IDX_ATT_MANUAL ON ATTENDANCE_RECORDS(IS_MANUAL_EDIT) WHERE IS_MANUAL_EDIT = 1;

CREATE INDEX IDX_ATT_NO_OUT ON ATTENDANCE_RECORDS(EMPLOYEE_ID, ATTENDANCE_DATE)
    WHERE CHECK_OUT IS NULL AND CHECK_IN IS NOT NULL;

-- Phát hiện quên check-out

-- attendance_logs  ← append-only, query giảm dần
CREATE INDEX IDX_ALOG_TIME ON ATTENDANCE_LOGS(LOG_TIME DESC);

CREATE INDEX IDX_ALOG_EMP ON ATTENDANCE_LOGS(EMPLOYEE_ID, LOG_TIME DESC);

CREATE INDEX IDX_ALOG_DEVICE ON ATTENDANCE_LOGS(DEVICE_ID, LOG_TIME DESC);

CREATE INDEX IDX_ALOG_RESULT ON ATTENDANCE_LOGS(RESULT) WHERE RESULT != 'Success';

CREATE INDEX IDX_ALOG_ATT ON ATTENDANCE_LOGS(ATTENDANCE_ID);

-- leave_requests
CREATE INDEX IDX_LEAVE_EMP ON LEAVE_REQUESTS(EMPLOYEE_ID, STATUS);

CREATE INDEX IDX_LEAVE_DATES ON LEAVE_REQUESTS(START_DATE, END_DATE);

CREATE INDEX IDX_LEAVE_PENDING ON LEAVE_REQUESTS(STATUS, CREATED_AT) WHERE STATUS = 'Pending';

-- audit_logs
CREATE INDEX IDX_AUDIT_USER ON AUDIT_LOGS(USER_ID, CREATED_AT DESC);

CREATE INDEX IDX_AUDIT_TABLE ON AUDIT_LOGS(TABLE_NAME, RECORD_ID);

CREATE INDEX IDX_AUDIT_TIME ON AUDIT_LOGS(CREATED_AT DESC);

-- ============================================================
--  TRIGGERS
-- ============================================================

-- T1. Auto-update updated_at



-- T2. Đồng bộ employees.is_face_registered & face_registered_at




-- T3. Đồng bộ employees.used_leave_days khi leave_request thay đổi status




-- T4. Chỉ 1 work_calendar is_default = 1




-- T5. Tự động ghi face_registration_logs khi face_data thay đổi




-- ============================================================
--  VIEWS
-- ============================================================

-- V1. Dashboard chấm công hôm nay
CREATE VIEW V_TODAY_ATTENDANCE AS
    SELECT
        E.ID                                 AS EMPLOYEE_ID,
        E.CODE                               AS EMPLOYEE_CODE,
        E.FULL_NAME,
        D.NAME                               AS DEPARTMENT_NAME,
        P.NAME                               AS POSITION_NAME,
        WS.NAME                              AS SHIFT_NAME,
        WS.START_TIME,
        WS.END_TIME,
        WS.LATE_THRESHOLD,
        WS.EARLY_THRESHOLD,
        A.CHECK_IN,
        A.CHECK_OUT,
        A.CHECK_IN_METHOD,
        A.CHECK_OUT_METHOD,
        A.CHECK_IN_CONFIDENCE,
        A.CHECK_OUT_CONFIDENCE,
        A.STATUS,
        A.LATE_MINUTES,
        A.EARLY_MINUTES,
        ROUND((A.WORKING_MINUTES / 60.0), 2) AS WORKING_HOURS,
        A.IS_MANUAL_EDIT,
        DEV_IN.DEVICE_NAME                   AS CHECK_IN_DEVICE,
        DEV_OUT.DEVICE_NAME                  AS CHECK_OUT_DEVICE,
        E.IS_FACE_REGISTERED
    FROM
        EMPLOYEES               E
        LEFT JOIN ATTENDANCE_RECORDS A
        ON E.ID = A.EMPLOYEE_ID
        AND A.ATTENDANCE_DATE = DATE('now', '+7 hours')
        LEFT JOIN EMPLOYEE_SHIFT_SCHEDULE ESS
        ON E.ID = ESS.EMPLOYEE_ID
        AND ESS.WORK_DATE = CURRENT_DATE
        LEFT JOIN WORK_SHIFTS WS
        ON COALESCE(A.SHIFT_ID,
        ESS.SHIFT_ID,
        E.DEFAULT_SHIFT_ID) = WS.ID
        LEFT JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
        LEFT JOIN POSITIONS P
        ON E.POSITION_ID = P.ID
        LEFT JOIN ATTENDANCE_DEVICES DEV_IN
        ON A.CHECK_IN_DEVICE_ID = DEV_IN.ID
        LEFT JOIN ATTENDANCE_DEVICES DEV_OUT
        ON A.CHECK_OUT_DEVICE_ID = DEV_OUT.ID
    WHERE
        E.IS_ACTIVE = 1;

-- V2. Tổng hợp chấm công theo tháng
CREATE VIEW V_MONTHLY_SUMMARY AS
    SELECT
        E.ID                                      AS EMPLOYEE_ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME                                    AS DEPARTMENT_NAME,
        DATE(A.ATTENDANCE_DATE, 'start of month') AS MONTH,
        COUNT(*)                                  AS TOTAL_RECORDS,
        SUM(
            CASE
                WHEN A.STATUS = 'Present' THEN
                    1
                ELSE
                    0
            END)                                  AS PRESENT_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'Late' THEN
                    1
                ELSE
                    0
            END)                                  AS LATE_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'EarlyLeave' THEN
                    1
                ELSE
                    0
            END)                                  AS EARLY_LEAVE_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'LateAndEarly' THEN
                    1
                ELSE
                    0
            END)                                  AS LATE_AND_EARLY_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'Absent' THEN
                    1
                ELSE
                    0
            END)                                  AS ABSENT_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'Leave' THEN
                    1
                ELSE
                    0
            END)                                  AS LEAVE_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'Holiday' THEN
                    1
                ELSE
                    0
            END)                                  AS HOLIDAY_DAYS,
        SUM(
            CASE
                WHEN A.STATUS = 'DayOff' THEN
                    1
                ELSE
                    0
            END)                                  AS DAY_OFF_DAYS,
 
        -- Ngày công hợp lệ = có mặt + trễ/sớm + nghỉ phép
        SUM(
            CASE
                WHEN A.STATUS IN (
                'Present', 'Late', 'EarlyLeave', 'LateAndEarly', 'Leave'
                ) THEN
                    1
                ELSE
                    0
            END)                                  AS ACTUAL_WORK_DAYS,
        SUM(A.LATE_MINUTES)                       AS TOTAL_LATE_MINUTES,
        SUM(A.EARLY_MINUTES)                      AS TOTAL_EARLY_MINUTES,
        ROUND((SUM(A.WORKING_MINUTES) / 60.0), 2) AS TOTAL_WORKING_HOURS,
        SUM(
            CASE
                WHEN A.IS_MANUAL_EDIT = 1 THEN
                    1
                ELSE
                    0
            END)                                  AS MANUAL_EDIT_COUNT
    FROM
        EMPLOYEES          E
        JOIN ATTENDANCE_RECORDS A
        ON E.ID = A.EMPLOYEE_ID
        LEFT JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
    WHERE
        E.IS_ACTIVE = 1
    GROUP BY
        E.ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME,
        DATE(A.ATTENDANCE_DATE, 'start of month');

-- V3. Nhân viên chưa đăng ký / cần cập nhật Face ID
CREATE VIEW V_FACE_STATUS AS
    SELECT
        E.ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME               AS DEPARTMENT_NAME,
        E.IS_FACE_REGISTERED,
        E.FACE_REGISTERED_AT,
        COUNT(FD.ID)         AS TOTAL_FACES,
        SUM(
            CASE
                WHEN FD.IS_ACTIVE = 1 THEN
                    1
                ELSE
                    0
            END)             AS ACTIVE_FACES,
        SUM(
            CASE
                WHEN FD.IS_VERIFIED = 1 AND FD.IS_ACTIVE = 1 THEN
                    1
                ELSE
                    0
            END)             AS VERIFIED_FACES,
        ROUND((AVG(
            CASE
                WHEN FD.IS_ACTIVE = 1 THEN
                    FD.QUALITY_SCORE
                ELSE
                    NULL
            END)), 3)        AS AVG_QUALITY,
        MIN(
            CASE
                WHEN FD.IS_ACTIVE = 1 THEN
                    FD.QUALITY_SCORE
                ELSE
                    NULL
            END)             AS MIN_QUALITY
    FROM
        EMPLOYEES   E
        LEFT JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
        LEFT JOIN FACE_DATA FD
        ON E.ID = FD.EMPLOYEE_ID
    WHERE
        E.IS_ACTIVE = 1
    GROUP BY
        E.ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME,
        E.IS_FACE_REGISTERED,
        E.FACE_REGISTERED_AT;

-- V4. Bất thường chấm công cần xem xét
CREATE VIEW V_ATTENDANCE_ANOMALIES AS
    SELECT
        A.ID                                 AS ATTENDANCE_ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME                               AS DEPARTMENT_NAME,
        A.ATTENDANCE_DATE,
        A.STATUS,
        A.CHECK_IN,
        A.CHECK_OUT,
        A.LATE_MINUTES,
        A.EARLY_MINUTES,
        ROUND((A.WORKING_MINUTES / 60.0), 2) AS WORKING_HOURS,
        A.CHECK_IN_CONFIDENCE,
        A.CHECK_OUT_CONFIDENCE,
        CASE
            WHEN A.CHECK_IN_CONFIDENCE < 0.70 THEN
                'Check-in confidence thấp < 0.70'
            WHEN A.CHECK_OUT_CONFIDENCE < 0.70 THEN
                'Check-out confidence thấp < 0.70'
            WHEN A.LATE_MINUTES > 60 THEN
                'Đi muộn > 60 phút'
            WHEN A.EARLY_MINUTES > 60 THEN
                'Về sớm > 60 phút'
            WHEN A.CHECK_OUT IS NULL
            AND A.CHECK_IN IS NOT NULL
            AND CURRENT_TIMESTAMP > DATETIME(A.ATTENDANCE_DATE, '+1 day')
            THEN
                'Quên check-out'
            WHEN A.WORKING_MINUTES < 240 AND A.STATUS = 'Present'
            THEN
                'Giờ làm < 4h nhưng status Present'
            WHEN A.IS_MANUAL_EDIT = 1 THEN
                'Điều chỉnh thủ công'
            ELSE
                'Bất thường'
        END                                  AS ANOMALY_TYPE
    FROM
        ATTENDANCE_RECORDS A
        JOIN EMPLOYEES E
        ON A.EMPLOYEE_ID = E.ID
        JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
    WHERE
        (A.CHECK_IN_CONFIDENCE IS NOT NULL
        AND A.CHECK_IN_CONFIDENCE < 0.70)
        OR (A.CHECK_OUT_CONFIDENCE IS NOT NULL
        AND A.CHECK_OUT_CONFIDENCE < 0.70)
        OR A.LATE_MINUTES > 60
        OR A.EARLY_MINUTES > 60
        OR (A.CHECK_OUT IS NULL
        AND A.CHECK_IN IS NOT NULL
        AND CURRENT_TIMESTAMP > DATETIME(A.ATTENDANCE_DATE, '+1 day'))
        OR (A.WORKING_MINUTES < 240
        AND A.STATUS = 'Present')
        OR A.IS_MANUAL_EDIT = 1;

-- V5. Đơn nghỉ phép đang chờ duyệt
CREATE VIEW V_PENDING_LEAVES AS
    SELECT
        LR.ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME             AS DEPARTMENT_NAME,
        MGR.FULL_NAME      AS MANAGER_NAME,
        LR.LEAVE_TYPE,
        LR.START_DATE,
        LR.END_DATE,
        LR.TOTAL_DAYS,
        LR.IS_HALF_DAY,
        LR.HALF_DAY_PERIOD,
        LR.REASON,
        CASE
            WHEN LR.DOCUMENT_PATH IS NOT NULL THEN
                1
            ELSE
                0
        END                AS HAS_DOCUMENT,
        LR.CREATED_AT
    FROM
        LEAVE_REQUESTS LR
        JOIN EMPLOYEES E
        ON LR.EMPLOYEE_ID = E.ID
        LEFT JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
        LEFT JOIN EMPLOYEES MGR
        ON E.MANAGER_ID = MGR.ID
    WHERE
        LR.STATUS = 'Pending'
    ORDER BY
        LR.CREATED_AT;

-- V6. Số dư nghỉ phép
CREATE VIEW V_LEAVE_BALANCE AS
    SELECT
        E.ID,
        E.CODE,
        E.FULL_NAME,
        D.NAME                                  AS DEPARTMENT_NAME,
        E.ANNUAL_LEAVE_DAYS,
        E.USED_LEAVE_DAYS,
        E.ANNUAL_LEAVE_DAYS - E.USED_LEAVE_DAYS AS REMAINING_DAYS,
        CAST(STRFTIME('%Y', 'now') AS INTEGER)  AS YEAR
    FROM
        EMPLOYEES   E
        LEFT JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
    WHERE
        E.IS_ACTIVE = 1;

-- V7. Nhật ký nhận diện khuôn mặt nghi ngờ (Suspicious / Spoofing)
CREATE VIEW V_SUSPICIOUS_RECOGNITION AS
    SELECT
        AL.ID             AS LOG_ID,
        AL.LOG_TIME,
        E.CODE            AS EMPLOYEE_CODE,
        E.FULL_NAME,
        D.NAME            AS DEPARTMENT_NAME,
        AL.RESULT,
        AL.CONFIDENCE,
        AL.FACE_DISTANCE,
        AL.METHOD,
        AL.IP_ADDRESS,
        DEV.DEVICE_NAME,
        DEV.LOCATION_NAME,
        AL.IMAGE_PATH,
        AL.FAIL_REASON
    FROM
        ATTENDANCE_LOGS    AL
        LEFT JOIN EMPLOYEES E
        ON AL.EMPLOYEE_ID = E.ID
        LEFT JOIN DEPARTMENTS D
        ON E.DEPARTMENT_ID = D.ID
        LEFT JOIN ATTENDANCE_DEVICES DEV
        ON AL.DEVICE_ID = DEV.ID
    WHERE
        AL.RESULT IN ('Suspicious', 'Spoofing', 'Failed')
    ORDER BY
        AL.LOG_TIME DESC;

-- ============================================================
--  DỮ LIỆU HỆ THỐNG
-- ============================================================

INSERT INTO SYSTEM_SETTINGS (
    KEY,
    VALUE,
    VALUE_TYPE,
    CATEGORY,
    DESCRIPTION
) VALUES
 -- Face Recognition
(
    'face.confidence_threshold',
    '0.70',
    'Decimal',
    'FaceRecognition',
    'Ngưỡng confidence tối thiểu chấp nhận nhận diện (0-1)'
),
(
    'face.max_slots_per_employee',
    '5',
    'Integer',
    'FaceRecognition',
    'Số ảnh tối đa mỗi nhân viên (slot 1-5)'
),
(
    'face.min_quality_score',
    '0.60',
    'Decimal',
    'FaceRecognition',
    'Điểm chất lượng ảnh tối thiểu khi đăng ký'
),
(
    'face.max_distance',
    '0.40',
    'Decimal',
    'FaceRecognition',
    'Khoảng cách Euclidean tối đa (< giá trị này = khớp)'
),
(
    'face.anti_spoofing_enabled',
    '1',
    'Boolean',
    'FaceRecognition',
    'Bật liveness detection chống ảnh giả/video playback'
),
(
    'face.duplicate_window_minutes',
    '5',
    'Integer',
    'FaceRecognition',
    'Khoảng thời gian chặn check-in trùng (phút)'
),
(
    'face.require_verification',
    '1',
    'Boolean',
    'FaceRecognition',
    'Bắt buộc HR xác nhận ảnh trước khi đưa vào nhận diện'
),
(
    'face.min_face_size_pixels',
    '80',
    'Integer',
    'FaceRecognition',
    'Kích thước khuôn mặt tối thiểu trong ảnh (pixel)'
),
 
-- Attendance
(
    'attendance.auto_absent_hour',
    '22',
    'Integer',
    'Attendance',
    'Giờ tự động đánh Absent nếu chưa check-in (22:00)'
),
(
    'attendance.allow_mobile_checkin',
    '1',
    'Boolean',
    'Attendance',
    'Cho phép chấm công qua mobile app'
),
(
    'attendance.geofence_enabled',
    '1',
    'Boolean',
    'Attendance',
    'Kiểm tra GPS khi check-in Mobile'
),
(
    'attendance.manual_edit_notify',
    '1',
    'Boolean',
    'Attendance',
    'Thông báo nhân viên khi HR sửa chấm công'
),
 
-- Security
(
    'security.max_login_attempts',
    '5',
    'Integer',
    'Security',
    'Số lần đăng nhập sai trước khi khóa tài khoản'
),
(
    'security.lockout_minutes',
    '30',
    'Integer',
    'Security',
    'Thời gian khóa tài khoản (phút)'
),
(
    'security.jwt_expire_minutes',
    '60',
    'Integer',
    'Security',
    'Thời gian hết hạn JWT Access Token (phút)'
),
(
    'security.refresh_token_days',
    '7',
    'Integer',
    'Security',
    'Thời gian hết hạn Refresh Token (ngày)'
),
(
    'security.password_min_length',
    '8',
    'Integer',
    'Security',
    'Độ dài mật khẩu tối thiểu'
),
 
-- Notification
(
    'notification.email_enabled',
    '1',
    'Boolean',
    'Notification',
    'Gửi email thông báo'
),
(
    'notification.late_alert_enabled',
    '1',
    'Boolean',
    'Notification',
    'Cảnh báo đi muộn qua email/push'
),
(
    'notification.approval_notify',
    '1',
    'Boolean',
    'Notification',
    'Thông báo kết quả duyệt đơn'
);

-- ============================================================
--  DỮ LIỆU MẪU (Seed)
-- Mật khẩu mặc định: admin / admin123  |  user / user123
-- ============================================================

-- Phòng ban (7 phòng)
INSERT INTO DEPARTMENTS (
    CODE,
    NAME,
    DESCRIPTION,
    SORT_ORDER
) VALUES (
    'BOD',
    'Ban Giám đốc',
    'Điều hành chung công ty',
    1
),
(
    'HR',
    'Phòng Nhân sự',
    'Quản lý nhân sự, tuyển dụng',
    2
),
(
    'IT',
    'Phòng Công nghệ',
    'Phát triển và vận hành hệ thống',
    3
),
(
    'SALES',
    'Phòng Kinh doanh',
    'Bán hàng và chăm sóc khách',
    4
),
(
    'ACCT',
    'Phòng Kế toán',
    'Quản lý tài chính, kế toán',
    5
),
(
    'MKT',
    'Phòng Marketing',
    'Truyền thông và quảng cáo',
    6
),
(
    'QA',
    'Phòng Kiểm thử',
    'Đảm bảo chất lượng sản phẩm',
    7
);


-- Chức vụ (7 cấp)
INSERT INTO POSITIONS (
    CODE,
    NAME,
    LEVEL
) VALUES (
    'CEO',
    'Giám đốc điều hành',
    10
),
(
    'VP',
    'Phó giám đốc',
    '9'
),
(
    'DIR',
    'Trưởng phòng',
    7
),
(
    'LEAD',
    'Trưởng nhóm',
    5
),
(
    'SR',
    'Nhân viên cao cấp',
    4
),
(
    'JR',
    'Nhân viên',
    3
),
(
    'INT',
    'Thực tập sinh',
    1
);

-- Ca làm việc (5 ca)
INSERT INTO WORK_SHIFTS (
    CODE,
    NAME,
    SHIFT_TYPE,
    START_TIME,
    END_TIME,
    BREAK_MINUTES,
    STANDARD_HOURS,
    LATE_THRESHOLD,
    EARLY_THRESHOLD,
    IS_OVERNIGHT,
    COLOR_CODE
) VALUES (
    'MAIN',
    'Ca hành chính',
    'Fixed',
    '08:00',
    '17:00',
    60,
    8.0,
    15,
    15,
    0,
    '#4A90D9'
),
(
    'MORN',
    'Ca sáng',
    'Shift',
    '06:00',
    '14:00',
    30,
    8.0,
    10,
    10,
    0,
    '#F5A623'
),
(
    'AFT',
    'Ca chiều',
    'Shift',
    '14:00',
    '22:00',
    30,
    8.0,
    10,
    10,
    0,
    '#7ED321'
),
(
    'NIGHT',
    'Ca đêm',
    'Shift',
    '22:00',
    '06:00',
    30,
    8.0,
    10,
    10,
    1,
    '#9B59B6'
),

(
    'FLEX',
    'Ca linh hoạt',
    'Flexible',
    '07:00',
    '19:00',
    60,
    8.0,
    0,
    0,
    0,
    '#1ABC9C'
);

-- Lịch làm tuần (3 loại)
-- Lịch làm tuần (3 loại)
INSERT INTO WORK_CALENDARS (
    NAME,
    SATURDAY,
    SUNDAY,
    IS_DEFAULT
) VALUES (
    'Hành chính T2-T6',
    0,
    0,
    1
),
(
    'Vận hành T2-T7',
    1,
    0,
    0
),
(
    'Sản xuất 7 ngày',
    1,
    1,
    0
);

-- Ngày lễ 2026
INSERT INTO HOLIDAYS (
    HOLIDAY_DATE,
    NAME,
    HOLIDAY_TYPE,
    IS_RECURRING,
    YEAR
) VALUES (
    '2026-01-01',
    'Tết Dương lịch',
    'National',
    1,
    2026
),
(
    '2026-02-17',
    'Nghỉ Tết Nguyên đán (bù)',
    'National',
    0,
    2026
),
(
    '2026-02-18',
    'Tết Nguyên đán – Mùng 1',
    'National',
    0,
    2026
),
(
    '2026-02-19',
    'Tết Nguyên đán – Mùng 2',
    'National',
    0,
    2026
),
(
    '2026-02-20',
    'Tết Nguyên đán – Mùng 3',
    'National',
    0,
    2026
),
(
    '2026-04-06',
    'Giỗ Tổ Hùng Vương (10/3 ÂL)',
    'National',
    0,
    2026
),
(
    '2026-04-30',
    'Ngày Giải phóng miền Nam',
    'National',
    1,
    2026
),
(
    '2026-05-01',
    'Quốc tế Lao động',
    'National',
    1,
    2026
),
(
    '2026-09-02',
    'Quốc khánh',
    'National',
    1,
    2026
),
(
    '2026-09-03',
    'Nghỉ bù Quốc khánh',
    'National',
    0,
    2026
);

-- ── Nhân viên mẫu (10 người) ──
-- ── Nhân viên mẫu (10 người) ──
INSERT INTO EMPLOYEES (
    CODE,
    FULL_NAME,
    GENDER,
    DATE_OF_BIRTH,
    PHONE,
    EMAIL,
    IDENTITY_CARD,
    DEPARTMENT_ID,
    POSITION_ID,
    DEFAULT_SHIFT_ID,
    EMPLOYMENT_TYPE,
    HIRE_DATE,
    ANNUAL_LEAVE_DAYS
) VALUES (
    'NV001',
    'Trần Minh Nhật',
    'M',
    '2004-08-15',
    '0901000001',
    'nhat.tm@company.com',
    '079204001001',
    3,
    4,
    1,
    'FullTime',
    '2023-06-01',
    14
),
(
    'NV002',
    'Trần Dương Yến Nhi',
    'F',
    '2003-12-25',
    '0901000002',
    'nhi.tdy@company.com',
    '079203002002',
    2,
    5,
    1,
    'FullTime',
    '2023-01-10',
    14
),
(
    'NV003',
    'Trần Nguyễn Minh Khôi',
    'M',
    '2004-03-20',
    '0901000003',
    'khoi.tnm@company.com',
    '079204003003',
    3,
    6,
    1,
    'FullTime',
    '2024-02-15',
    12
),
(
    'NV004',
    'Nguyễn Văn An',
    'M',
    '1985-05-10',
    '0901000004',
    'an.nv@company.com',
    '079185004004',
    1,
    1,
    1,
    'FullTime',
    '2015-01-01',
    18
),
(
    'NV005',
    'Lê Thị Hồng Nhung',
    'F',
    '1990-11-08',
    '0901000005',
    'nhung.lth@company.com',
    '079190005005',
    2,
    3,
    1,
    'FullTime',
    '2018-03-15',
    16
),
(
    'NV006',
    'Phạm Quốc Bảo',
    'M',
    '1992-07-22',
    '0901000006',
    'bao.pq@company.com',
    '079192006006',
    3,
    3,
    1,
    'FullTime',
    '2019-08-01',
    15
),
(
    'NV007',
    'Võ Ngọc Trâm',
    'F',
    '1995-04-18',
    '0901000007',
    'tram.vn@company.com',
    '079195007007',
    4,
    5,
    1,
    'FullTime',
    '2020-06-10',
    14
),
(
    'NV008',
    'Huỳnh Đức Trí',
    'M',
    '1998-09-30',
    '0901000008',
    'tri.hd@company.com',
    '079198008008',
    5,
    5,
    1,
    'FullTime',
    '2021-11-01',
    13
),
(
    'NV009',
    'Đặng Thùy Linh',
    'F',
    '2000-02-14',
    '0901000009',
    'linh.dt@company.com',
    '079200009009',
    6,
    6,
    1,
    'FullTime',
    '2022-07-20',
    12
),
(
    'NV010',
    'Bùi Thanh Tùng',
    'M',
    '2001-06-05',
    '0901000010',
    'tung.bt@company.com',
    '079201010010',
    7,
    7,
    1,
    'PartTime',
    '2025-01-15',
    10
),
(
    'NV011',
    'Đoàn Ngọc Linh',
    'F',
    '1994-12-16',
    '0901207089',
    'linh.dn@company.com',
    '079104111842',
    4,
    4,
    1,
    'FullTime',
    '2022-09-27',
    12
),
(
    'NV012',
    'Hồ Nhật Anh',
    'F',
    '1999-09-12',
    '0901965730',
    'anh.hn@company.com',
    '079132599779',
    6,
    7,
    1,
    'FullTime',
    '2024-11-19',
    14
),
(
    'NV013',
    'Nguyễn Vũ Gia Bảo',
    'M',
    '2005-11-01',
    '0901364857',
    'bao.nvg@company.com',
    '079194299400',
    2,
    7,
    1,
    'FullTime',
    '2024-01-10',
    16
),
(
    'NV014',
    'Nguyễn Hữu Bình',
    'M',
    '2001-07-14',
    '0901614094',
    'binh.nh@company.com',
    '079135107313',
    4,
    7,
    1,
    'FullTime',
    '2023-07-28',
    15
),
(
    'NV015',
    'Trần Lê Anh Đại',
    'F',
    '1991-12-06',
    '0901454196',
    'dai.tla@company.com',
    '079166260980',
    4,
    4,
    1,
    'FullTime',
    '2021-04-20',
    15
),
(
    'NV016',
    'Nguyễn Thành Đạt',
    'M',
    '1996-03-03',
    '0901104739',
    'dat.nt@company.com',
    '079108896132',
    3,
    7,
    1,
    'FullTime',
    '2022-10-14',
    12
),
(
    'NV017',
    'Nguyễn Hoàng Tiến Đạt',
    'M',
    '1994-07-18',
    '0901571068',
    'dat.nht@company.com',
    '079104492177',
    4,
    6,
    1,
    'FullTime',
    '2020-03-24',
    12
),
(
    'NV018',
    'Bùi Hải Đường',
    'M',
    '1992-01-27',
    '0901688802',
    'duong.bh@company.com',
    '079125933155',
    2,
    7,
    1,
    'FullTime',
    '2023-03-05',
    16
),
(
    'NV019',
    'Trương Đại Hải',
    'M',
    '1993-07-24',
    '0901105276',
    'hai.td@company.com',
    '079185686429',
    5,
    4,
    1,
    'FullTime',
    '2022-05-22',
    15
),
(
    'NV020',
    'Hà Chí Hân',
    'M',
    '1995-11-03',
    '0901912225',
    'han.hc@company.com',
    '079185372333',
    5,
    5,
    1,
    'FullTime',
    '2023-08-12',
    12
),
(
    'NV021',
    'Đặng Ngọc Châu',
    'M',
    '2002-02-19',
    '0901624941',
    'chau.dn@company.com',
    '079128401461',
    7,
    6,
    1,
    'FullTime',
    '2020-02-07',
    14
),
(
    'NV022',
    'Trương Văn Huế',
    'M',
    '2001-11-12',
    '0901525631',
    'hue.tv@company.com',
    '079178166149',
    6,
    4,
    1,
    'FullTime',
    '2022-08-03',
    15
),
(
    'NV023',
    'Nguyễn Tấn Hưng',
    'M',
    '1996-10-25',
    '0901382472',
    'hung.nt@company.com',
    '079144348328',
    6,
    5,
    1,
    'FullTime',
    '2023-02-15',
    12
),
(
    'NV024',
    'Lê Thanh Khải',
    'M',
    '1996-10-10',
    '0901425030',
    'khai.lt@company.com',
    '079178440514',
    2,
    7,
    1,
    'FullTime',
    '2023-02-23',
    15
),
(
    'NV025',
    'Lê Tuấn Kiệt',
    'M',
    '2000-02-16',
    '0901705111',
    'kiet.lt@company.com',
    '079195731097',
    2,
    4,
    1,
    'FullTime',
    '2021-01-22',
    12
),
(
    'NV026',
    'Nguyễn Hoàng Kỳ',
    'M',
    '1991-11-18',
    '0901910492',
    'ky.nh@company.com',
    '079146803029',
    2,
    4,
    1,
    'FullTime',
    '2020-11-06',
    16
),
(
    'NV027',
    'Nguyễn Thị Kim Liên',
    'F',
    '1997-09-04',
    '0901557405',
    'lien.ntk@company.com',
    '079159640477',
    4,
    7,
    1,
    'FullTime',
    '2024-06-02',
    14
),
(
    'NV028',
    'Nguyễn Văn Toàn',
    'M',
    '2004-05-12',
    '0901329782',
    'toan.nv@company.com',
    '079111765625',
    4,
    7,
    1,
    'FullTime',
    '2020-12-01',
    12
),
(
    'NV029',
    'Trần Anh Nhân',
    'F',
    '1997-12-06',
    '0901650437',
    'nhan.ta@company.com',
    '079154737949',
    7,
    5,
    1,
    'FullTime',
    '2023-02-24',
    15
),
(
    'NV030',
    'Đặng Minh Nhật',
    'M',
    '1997-04-23',
    '0901118939',
    'nhat.dm@company.com',
    '079121543964',
    4,
    4,
    1,
    'FullTime',
    '2023-12-08',
    15
),
(
    'NV031',
    'Nguyễn Văn Phú',
    'M',
    '1998-11-02',
    '0901395679',
    'phu.nv@company.com',
    '079179731823',
    3,
    7,
    1,
    'FullTime',
    '2021-06-08',
    14
),
(
    'NV032',
    'Đoàn Phúc',
    'M',
    '1992-12-12',
    '0901130960',
    'phuc.d@company.com',
    '079115136797',
    7,
    7,
    1,
    'FullTime',
    '2021-05-17',
    12
),
(
    'NV033',
    'Bùi Đắc Quí',
    'M',
    '1999-01-22',
    '0901351204',
    'qui.bd@company.com',
    '079168652495',
    5,
    4,
    1,
    'FullTime',
    '2024-11-25',
    14
),
(
    'NV034',
    'Nguyễn Thành Thái',
    'M',
    '1996-09-17',
    '0901677981',
    'thai.nt@company.com',
    '079131541936',
    4,
    4,
    1,
    'FullTime',
    '2020-08-03',
    14
),
(
    'NV035',
    'Hồ Hữu Thịnh',
    'F',
    '2003-09-26',
    '0901920354',
    'thinh.hh@company.com',
    '079118350421',
    4,
    4,
    1,
    'FullTime',
    '2022-04-28',
    16
),
(
    'NV036',
    'Lâm Diệu Tinh',
    'F',
    '2005-10-04',
    '0901883557',
    'tinh.ld@company.com',
    '079122345251',
    7,
    6,
    1,
    'FullTime',
    '2020-03-26',
    16
),
(
    'NV037',
    'Trần Thanh Phương',
    'M',
    '1993-04-02',
    '0901974389',
    'phuong.tt@company.com',
    '079121251414',
    2,
    4,
    1,
    'FullTime',
    '2024-10-08',
    16
),
(
    'NV038',
    'Phạm Gia Bảo',
    'M',
    '1999-09-03',
    '0901835971',
    'bao.pg@company.com',
    '079154997403',
    7,
    5,
    1,
    'FullTime',
    '2023-09-08',
    16
),
(
    'NV039',
    'Nguyễn Trần Hữu Đức',
    'M',
    '2005-12-28',
    '0901464031',
    'duc.nth@company.com',
    '079178354917',
    5,
    6,
    1,
    'FullTime',
    '2021-08-27',
    15
),
(
    'NV040',
    'Triệu Ngọc Hào',
    'M',
    '1999-03-04',
    '0901706069',
    'hao.tn@company.com',
    '079184482350',
    2,
    6,
    1,
    'FullTime',
    '2024-05-01',
    16
),
(
    'NV041',
    'Trần Trung Hiếu',
    'M',
    '2005-03-11',
    '0901663980',
    'hieu.tt@company.com',
    '079142674087',
    3,
    7,
    1,
    'FullTime',
    '2022-08-11',
    12
),
(
    'NV042',
    'Lâm Thái Hòa',
    'M',
    '2004-09-09',
    '0901444251',
    'hoa.lt@company.com',
    '079112876963',
    7,
    5,
    1,
    'FullTime',
    '2022-05-02',
    14
),
(
    'NV043',
    'Huỳnh Đông Huy',
    'M',
    '1995-09-07',
    '0901552496',
    'huy.hd@company.com',
    '079199509373',
    3,
    5,
    1,
    'FullTime',
    '2024-09-08',
    12
),
(
    'NV044',
    'Nguyễn Quốc Khánh',
    'M',
    '1996-09-10',
    '0901232611',
    'khanh.nq@company.com',
    '079144801745',
    5,
    5,
    1,
    'FullTime',
    '2022-11-22',
    14
),
(
    'NV045',
    'Đặng Văn Khoa',
    'M',
    '2005-06-27',
    '0901582681',
    'khoa.dv@company.com',
    '079158634238',
    4,
    4,
    1,
    'FullTime',
    '2021-01-23',
    12
),
(
    'NV046',
    'Lê Trung Kiên',
    'M',
    '2002-01-23',
    '0901848806',
    'kien.lt@company.com',
    '079175733988',
    7,
    7,
    1,
    'FullTime',
    '2022-01-11',
    12
),
(
    'NV047',
    'Nguyễn Tấn Kiệt',
    'M',
    '1999-07-04',
    '0901884574',
    'kiet.nt@company.com',
    '079109619039',
    3,
    7,
    1,
    'FullTime',
    '2023-10-01',
    15
),
(
    'NV048',
    'Hoàng Công Trường Lộc',
    'M',
    '2001-09-05',
    '0901748651',
    'loc.hct@company.com',
    '079165131085',
    5,
    5,
    1,
    'FullTime',
    '2024-04-23',
    12
),
(
    'NV049',
    'Lưu Gia Luân',
    'M',
    '2000-05-07',
    '0901577919',
    'luan.lg@company.com',
    '079138862633',
    3,
    5,
    1,
    'FullTime',
    '2024-03-09',
    15
),
(
    'NV050',
    'Lương Hào Minh',
    'M',
    '1992-10-28',
    '0901359107',
    'minh.lh@company.com',
    '079197113679',
    5,
    7,
    1,
    'FullTime',
    '2024-02-07',
    12
),
(
    'NV051',
    'Nguyễn Hoàng Phúc',
    'M',
    '2000-11-25',
    '0901657951',
    'phuc.nh@company.com',
    '079176943958',
    4,
    7,
    1,
    'FullTime',
    '2022-01-27',
    14
),
(
    'NV052',
    'Võ Xuân Thắng',
    'M',
    '1991-08-16',
    '0901469188',
    'thang.vx@company.com',
    '079174769868',
    4,
    7,
    1,
    'FullTime',
    '2023-03-22',
    16
),
(
    'NV053',
    'Hồ Ngọc Thái Thông',
    'M',
    '1998-11-08',
    '0901487363',
    'thong.hnt@company.com',
    '079168158615',
    5,
    5,
    1,
    'FullTime',
    '2021-01-14',
    15
),
(
    'NV054',
    'Nguyễn Lê Anh Trúc',
    'F',
    '2002-04-26',
    '0901854407',
    'truc.nla@company.com',
    '079172736107',
    4,
    4,
    1,
    'FullTime',
    '2022-11-13',
    12
),
(
    'NV055',
    'Nguyễn Văn Trường',
    'M',
    '1990-06-07',
    '0901497071',
    'truong.nv@company.com',
    '079193506468',
    5,
    4,
    1,
    'FullTime',
    '2021-06-01',
    15
),
(
    'NV056',
    'Nguyễn Thanh Tú',
    'M',
    '2005-09-09',
    '0901124859',
    'tu.nt@company.com',
    '079118589802',
    2,
    7,
    1,
    'FullTime',
    '2020-04-02',
    15
),
(
    'NV057',
    'Nguyễn Hoàng Vĩ',
    'M',
    '2004-12-15',
    '0901985062',
    'vi.nh@company.com',
    '079170796862',
    5,
    6,
    1,
    'FullTime',
    '2021-10-01',
    15
),
(
    'NV058',
    'Trần Hồ Quang Vinh',
    'M',
    '2002-01-04',
    '0901530620',
    'vinh.thq@company.com',
    '079121300210',
    5,
    7,
    1,
    'FullTime',
    '2021-07-02',
    16
),
(
    'NV059',
    'Đinh Thị Thảo An',
    'F',
    '1996-01-16',
    '0901321158',
    'an.dtt@company.com',
    '079182628871',
    2,
    7,
    1,
    'FullTime',
    '2023-09-03',
    14
),
(
    'NV060',
    'Phạm Hữu Ân',
    'M',
    '2000-09-08',
    '0901388445',
    'an.ph@company.com',
    '079147436466',
    3,
    6,
    1,
    'FullTime',
    '2021-07-08',
    15
),
(
    'NV061',
    'Lê Văn Bình',
    'M',
    '2003-06-09',
    '0901765104',
    'binh.lv@company.com',
    '079173253225',
    4,
    6,
    1,
    'FullTime',
    '2020-01-18',
    14
),
(
    'NV062',
    'Nguyễn Ngọc Thùy Dương',
    'F',
    '1997-05-26',
    '0901968247',
    'duong.nnt@company.com',
    '079178944019',
    7,
    4,
    1,
    'FullTime',
    '2021-07-19',
    12
);

-- Quản lý trực tiếp
UPDATE EMPLOYEES
SET
    MANAGER_ID = 4
WHERE
    ID IN (1, 2, 3, 5, 6, 7, 8, 9, 10);

UPDATE EMPLOYEES
SET
    MANAGER_ID = 4
WHERE
    ID BETWEEN 11 AND 62;

-- Tạm thời gán CEO quản lý hết để dễ test
UPDATE EMPLOYEES
SET
    MANAGER_ID = 6
WHERE
    ID IN (1, 3)
    OR (ID BETWEEN 11
    AND 62
    AND DEPARTMENT_ID = 3);

UPDATE EMPLOYEES
SET
    MANAGER_ID = 5
WHERE
    ID = 2
    OR (ID BETWEEN 11
    AND 62
    AND DEPARTMENT_ID = 2);

UPDATE EMPLOYEES
SET
    MANAGER_ID = 4
WHERE
    ID IN (1, 2, 3, 5, 6);

UPDATE EMPLOYEES
SET
    MANAGER_ID = 6
WHERE
    ID IN (1, 3);

UPDATE EMPLOYEES
SET
    MANAGER_ID = 5
WHERE
    ID = 2;

-- ── Tài khoản đăng nhập (hash sẽ được DatabaseBootstrapper cập nhật) ──
-- ── Tài khoản đăng nhập (hash sẽ được DatabaseBootstrapper cập nhật) ──
INSERT INTO USERS (
    USERNAME,
    PASSWORD_HASH,
    ROLE,
    EMPLOYEE_ID,
    MUST_CHANGE_PASSWORD
) VALUES (
    'admin',
    'SEED_WILL_BE_REHASHED',
    'Admin',
    NULL,
    0
),
(
    'user',
    'SEED_WILL_BE_REHASHED',
    'Employee',
    1,
    0
);

-- ── Thiết bị (5 thiết bị) ──
-- ── Thiết bị (5 thiết bị) ──
INSERT INTO ATTENDANCE_DEVICES (
    DEVICE_CODE,
    DEVICE_NAME,
    DEVICE_TYPE,
    LOCATION_NAME,
    IP_ADDRESS,
    LATITUDE,
    LONGITUDE,
    RADIUS_METERS,
    MIN_CONFIDENCE,
    CAMERA_URL
) VALUES (
    'CAM-GATE-01',
    'Camera Cổng chính',
    'Camera',
    'Cổng chính – Tầng 1',
    '192.168.1.101',
    10.77690,
    106.70090,
    50,
    0.72,
    'rtsp://192.168.1.101:554/stream'
),
(
    'CAM-IT-01',
    'Camera Phòng IT',
    'Camera',
    'Phòng IT – Tầng 3',
    '192.168.1.102',
    10.77700,
    106.70100,
    30,
    0.70,
    'rtsp://192.168.1.102:554/stream'
),
(
    'CAM-SALES',
    'Camera Phòng KD',
    'Camera',
    'Phòng KD – Tầng 2',
    '192.168.1.103',
    10.77680,
    106.70080,
    30,
    0.70,
    'rtsp://192.168.1.103:554/stream'
),
(
    'KSK-HR-01',
    'Kiosk Phòng HR',
    'Kiosk',
    'HR – Tầng 2',
    '192.168.1.110',
    10.77685,
    106.70085,
    30,
    0.70,
    NULL
),
(
    'MOB-APP',
    'Mobile Application',
    'Mobile',
    'Di động / WFH',
    NULL,
    NULL,
    NULL,
    100,
    0.68,
    NULL
);

-- ── Bản ghi chấm công mẫu ──
-- ── Bản ghi chấm công mẫu ──
INSERT INTO ATTENDANCE_RECORDS (
    EMPLOYEE_ID,
    ATTENDANCE_DATE,
    SHIFT_ID,
    CHECK_IN,
    CHECK_OUT,
    CHECK_IN_DEVICE_ID,
    CHECK_OUT_DEVICE_ID,
    CHECK_IN_METHOD,
    CHECK_OUT_METHOD,
    CHECK_IN_CONFIDENCE,
    CHECK_OUT_CONFIDENCE,
    STATUS,
    LATE_MINUTES,
    EARLY_MINUTES,
    WORKING_MINUTES
) VALUES (
    1,
    DATE('now'),
    1,
    DATETIME('now', 'start of day', '+7 hours', '+55 minutes'),
    NULL,
    2,
    NULL,
    'Face',
    NULL,
    0.96,
    NULL,
    'Present',
    0,
    0,
    0
),
(
    2,
    DATE('now'),
    1,
    DATETIME('now', 'start of day', '+8 hours', '+22 minutes'),
    NULL,
    1,
    NULL,
    'Face',
    NULL,
    0.93,
    NULL,
    'Late',
    22,
    0,
    0
),
(
    3,
    DATE('now'),
    1,
    DATETIME('now', 'start of day', '+7 hours', '+58 minutes'),
    NULL,
    2,
    NULL,
    'Face',
    NULL,
    0.95,
    NULL,
    'Present',
    0,
    0,
    0
),
(
    4,
    DATE('now'),
    1,
    DATETIME('now', 'start of day', '+7 hours', '+50 minutes'),
    NULL,
    1,
    NULL,
    'Face',
    NULL,
    0.97,
    NULL,
    'Present',
    0,
    0,
    0
),
(
    7,
    DATE('now'),
    1,
    DATETIME('now', 'start of day', '+8 hours', '+0 minutes'),
    NULL,
    3,
    NULL,
    'Face',
    NULL,
    0.90,
    NULL,
    'Present',
    0,
    0,
    0
),
(
    1,
    DATE('now', '-1 day'),
    1,
    DATETIME('now', '-1 day', 'start of day', '+7 hours', '+52 minutes'),
    DATETIME('now', '-1 day', 'start of day', '+17 hours', '+8 minutes'),
    2,
    2,
    'Face',
    'Face',
    0.95,
    0.93,
    'Present',
    0,
    0,
    496
),
(
    2,
    DATE('now', '-1 day'),
    1,
    DATETIME('now', '-1 day', 'start of day', '+8 hours', '+0 minutes'),
    DATETIME('now', '-1 day', 'start of day', '+17 hours', '+0 minutes'),
    1,
    1,
    'Face',
    'Face',
    0.92,
    0.90,
    'Present',
    0,
    0,
    480
),
(
    3,
    DATE('now', '-1 day'),
    1,
    DATETIME('now', '-1 day', 'start of day', '+8 hours', '+5 minutes'),
    DATETIME('now', '-1 day', 'start of day', '+17 hours', '+15 minutes'),
    2,
    2,
    'Face',
    'Face',
    0.94,
    0.91,
    'Present',
    5,
    0,
    490
),
(
    5,
    DATE('now', '-1 day'),
    1,
    DATETIME('now', '-1 day', 'start of day', '+7 hours', '+45 minutes'),
    DATETIME('now', '-1 day', 'start of day', '+17 hours', '+30 minutes'),
    1,
    1,
    'Face',
    'Face',
    0.96,
    0.95,
    'Present',
    0,
    0,
    525
),
(
    6,
    DATE('now', '-2 days'),
    1,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Absent',
    0,
    0,
    0
);

-- ── Log nhận diện ──
INSERT INTO ATTENDANCE_LOGS (
    ATTENDANCE_ID,
    EMPLOYEE_ID,
    DEVICE_ID,
    LOG_TIME,
    LOG_TYPE,
    METHOD,
    MATCHED_FACE_ID,
    CONFIDENCE,
    FACE_DISTANCE,
    RESULT
) VALUES (
    1,
    1,
    2,
    DATETIME('now', 'start of day', '+7 hours', '+55 minutes'),
    'CheckIn',
    'Face',
    NULL,
    0.96,
    0.25,
    'Success'
),
(
    1,
    1,
    2,
    DATETIME('now', 'start of day', '+17 hours', '+10 minutes'),
    'CheckOut',
    'Face',
    NULL,
    0.94,
    0.28,
    'Success'
),
(
    2,
    2,
    1,
    DATETIME('now', 'start of day', '+8 hours', '+22 minutes'),
    'CheckIn',
    'Face',
    NULL,
    0.93,
    0.27,
    'Success'
),
(
    2,
    2,
    1,
    DATETIME('now', 'start of day', '+17 hours', '+5 minutes'),
    'CheckOut',
    'Face',
    NULL,
    0.91,
    0.32,
    'Success'
);

-- ── Đơn nghỉ phép mẫu (3 đơn) ──
-- ── Đơn nghỉ phép mẫu (3 đơn) ──
INSERT INTO LEAVE_REQUESTS (
    EMPLOYEE_ID,
    LEAVE_TYPE,
    START_DATE,
    END_DATE,
    TOTAL_DAYS,
    REASON,
    STATUS,
    APPROVED_BY,
    APPROVED_AT
) VALUES (
    3,
    'Annual',
    DATE('now', '+10 days'),
    DATE('now', '+11 days'),
    2,
    'Nghỉ phép về quê',
    'Pending',
    NULL,
    NULL
),
(
    2,
    'Sick',
    DATE('now', '-3 days'),
    DATE('now', '-3 days'),
    1,
    'Khám sức khỏe định kỳ',
    'Approved',
    5,
    DATETIME('now', '-2 days')
),
(
    9,
    'Other',
    DATE('now', '+5 days'),
    DATE('now', '+5 days'),
    1,
    'Có việc gia đình cần giải quyết',
    'Pending',
    NULL,
    NULL
);

-- ============================================================
--  GHI CHÚ KỸ THUẬT
-- ============================================================
--
-- Database   : SQLite 3.x (System.Data.SQLite.Core)
-- Framework  : .NET Framework 4.6.1 / WinForms
-- Password   : BCrypt.Net-Next (AuthPasswordHasher)
-- Face AI    : FaceRecognitionDotNet (dlib wrapper) -- float[128] encoding
-- Mật khẩu mặc định tất cả tài khoản mẫu: admin123
--
-- ============================================================
-- KẾT THÚC SCRIPT
-- ============================================================