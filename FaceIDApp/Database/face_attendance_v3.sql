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
DROP TABLE IF EXISTS audit_logs              ;
DROP TABLE IF EXISTS system_settings         ;
DROP TABLE IF EXISTS leave_requests          ;
DROP TABLE IF EXISTS attendance_logs         ;
DROP TABLE IF EXISTS attendance_records      ;
DROP TABLE IF EXISTS employee_shift_schedule ;
DROP TABLE IF EXISTS work_calendars          ;
DROP TABLE IF EXISTS holidays                ;
DROP TABLE IF EXISTS attendance_devices      ;
DROP TABLE IF EXISTS face_registration_logs  ;
DROP TABLE IF EXISTS face_data               ;
DROP TABLE IF EXISTS users                   ;
DROP TABLE IF EXISTS employees               ;
DROP TABLE IF EXISTS work_shifts             ;
DROP TABLE IF EXISTS positions               ;
DROP TABLE IF EXISTS departments             ;


-- ============================================================
--  NHÓM A: DANH MỤC
-- ============================================================

-- ── A1. departments ──────────────────────────────────────────
CREATE TABLE departments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code        TEXT  NOT NULL UNIQUE,
    name        TEXT NOT NULL,
    description TEXT,
    parent_id   INT          REFERENCES departments(id) ON DELETE SET NULL,
    manager_id  INT,                         -- FK → employees(id), bổ sung sau
    is_active   INTEGER      NOT NULL DEFAULT 1,
    sort_order  SMALLINT     NOT NULL DEFAULT 0,
    created_at  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME
);


-- ── A2. positions ─────────────────────────────────────────────
CREATE TABLE positions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code        TEXT  NOT NULL UNIQUE,
    name        TEXT NOT NULL,
    level       SMALLINT     NOT NULL DEFAULT 1 CHECK (level BETWEEN 1 AND 10),
    -- 1=Thực tập  3=Nhân viên  5=Trưởng nhóm  7=Trưởng phòng  10=Giám đốc
    -- level dùng để routing duyệt đơn từ phía application
    description TEXT,
    is_active   INTEGER      NOT NULL DEFAULT 1,
    created_at  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME
);


-- ── A3. work_shifts ───────────────────────────────────────────
CREATE TABLE work_shifts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code             TEXT    NOT NULL UNIQUE,
    name             TEXT   NOT NULL,
    shift_type       TEXT    NOT NULL DEFAULT 'Fixed'
                     CHECK (shift_type IN ('Fixed','Flexible','Shift')),
    -- Fixed    = giờ vào/ra cố định (hành chính)
    -- Flexible = linh hoạt, tính đủ standard_hours/ngày
    -- Shift    = ca xoay 3 ca
    start_time       TEXT           NOT NULL,
    end_time         TEXT           NOT NULL,
    break_minutes    SMALLINT       NOT NULL DEFAULT 60 CHECK (break_minutes >= 0),
    standard_hours   REAL   NOT NULL DEFAULT 8  CHECK (standard_hours > 0),
    -- Tổng giờ làm chuẩn = (end-start) - break  (business layer tính)
    late_threshold   SMALLINT       NOT NULL DEFAULT 15 CHECK (late_threshold >= 0),
    -- Phút ân hạn đến muộn; check_in <= start + threshold → vẫn đúng giờ
    early_threshold  SMALLINT       NOT NULL DEFAULT 15 CHECK (early_threshold >= 0),
    -- Phút ân hạn về sớm; check_out >= end - threshold → không bị về sớm
    is_overnight     INTEGER        NOT NULL DEFAULT 0,
    -- 1 = ca qua đêm: end_time thuộc ngày D+1 (vd 22:00→06:00)
    color_code       TEXT,
    is_active        INTEGER        NOT NULL DEFAULT 1,
    created_at       DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       DATETIME
);


-- ============================================================
--  NHÓM B: NHÂN VIÊN & KHUÔN MẶT
-- ============================================================

-- ── B1. employees ─────────────────────────────────────────────
CREATE TABLE employees (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code                TEXT   NOT NULL UNIQUE,
    full_name           TEXT  NOT NULL,
    gender              TEXT       CHECK (gender IN ('M','F','O')),
    date_of_birth       TEXT,
    phone               TEXT,
    email               TEXT  UNIQUE,
    identity_card       TEXT   UNIQUE,     -- CCCD / CMND
    department_id       INT           REFERENCES departments(id) ON DELETE SET NULL,
    position_id         INT           REFERENCES positions(id)   ON DELETE SET NULL,
    default_shift_id    INT           REFERENCES work_shifts(id) ON DELETE SET NULL,
    -- Ca mặc định — fallback khi không có employee_shift_schedule
    manager_id          INT           REFERENCES employees(id)   ON DELETE SET NULL,
    -- Quản lý trực tiếp — dùng routing duyệt đơn nghỉ phép
    hire_date           TEXT          NOT NULL DEFAULT CURRENT_DATE,
    termination_date    TEXT,
    employment_type     TEXT   NOT NULL DEFAULT 'FullTime'
                        CHECK (employment_type IN ('FullTime','PartTime','Contract','Intern')),
    work_location       TEXT,            -- Tên chi nhánh / văn phòng làm việc
    avatar_path         TEXT,                    -- Ảnh đại diện (khác face_data)
    -- ── Trạng thái Face ID ──
    is_face_registered  INTEGER       NOT NULL DEFAULT 0,
    -- Trigger tự động cập nhật từ face_data
    face_registered_at  DATETIME,
    -- Thời điểm đăng ký face đầu tiên thành công
    -- ── Nghỉ phép (phạm vi chấm công) ──
    annual_leave_days   REAL  NOT NULL DEFAULT 12 CHECK (annual_leave_days >= 0),
    used_leave_days     REAL  NOT NULL DEFAULT 0  CHECK (used_leave_days  >= 0),
    -- Trigger tự động cộng/trừ khi leave_request thay đổi status
    is_active           INTEGER       NOT NULL DEFAULT 1,
    created_at          DATETIME   NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME,

    CONSTRAINT chk_emp_dates CHECK (
        termination_date IS NULL OR termination_date >= hire_date
    )
);




-- ── B2. users ─────────────────────────────────────────────────
CREATE TABLE users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username             TEXT  NOT NULL UNIQUE,
    password_hash        TEXT NOT NULL,
    -- BCrypt hash cost=12; KHÔNG lưu plain text
    employee_id          INT          UNIQUE REFERENCES employees(id) ON DELETE SET NULL,
    -- UNIQUE: 1 nhân viên = tối đa 1 tài khoản
    role                 TEXT  NOT NULL DEFAULT 'Employee'
                         CHECK (role IN ('SuperAdmin','Admin','HR','Manager','Employee')),
    -- SuperAdmin : toàn quyền hệ thống + cấu hình
    -- Admin      : quản trị danh mục, thiết bị, ca làm
    -- HR         : xem & chỉnh sửa toàn bộ chấm công, duyệt đơn
    -- Manager    : duyệt đơn nhân viên trong phòng mình
    -- Employee   : xem lịch sử & nộp đơn của bản thân
    is_active            INTEGER      NOT NULL DEFAULT 1,
    last_login           DATETIME,
    failed_login_count   SMALLINT     NOT NULL DEFAULT 0,
    locked_until         DATETIME,
    -- Khóa tạm thời sau N lần đăng nhập sai (cấu hình qua system_settings)
    refresh_token_hash   TEXT,
    -- SHA-256 hash của JWT Refresh Token
    refresh_token_expiry DATETIME,
    must_change_password INTEGER      NOT NULL DEFAULT 0,
    -- 1 = buộc đổi mật khẩu lần đăng nhập kế tiếp
    created_at           DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           DATETIME
);


-- ── B3. face_data  (CORE TABLE) ───────────────────────────────
CREATE TABLE face_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id     INT         NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    -- Xóa NV → xóa toàn bộ face data

    -- ── Dữ liệu nhận diện ──
    encoding        TEXT        NOT NULL,
    -- double[128] serialize → semicolon-separated string
    -- Tính bằng: dlib face_recognition / FaceRecognitionDotNet
    -- Lưu: "0.123;-0.456;0.789;..." (128 giá trị double, phân tách bởi ;)
    image_path      TEXT        NOT NULL,
    -- Ảnh gốc lưu server / Azure Blob / MinIO
    thumbnail_path  TEXT,
    -- Ảnh thu nhỏ 120×120 hiển thị UI

    -- ── Chất lượng & góc chụp ──
    image_index     SMALLINT    NOT NULL DEFAULT 1 CHECK (image_index BETWEEN 1 AND 5),
    -- Tối đa 5 ảnh/NV (5 góc khác nhau)
    angle           TEXT CHECK (angle IN ('Front','Left','Right','Up','Down')),
    -- Góc chụp để tăng độ bao phủ nhận diện
    quality_score   REAL        NOT NULL DEFAULT 0 CHECK (quality_score BETWEEN 0 AND 1),
    -- 0.0 (xấu) → 1.0 (tốt); reject nếu < 0.6 (cấu hình system_settings)
    brightness      REAL        CHECK (brightness BETWEEN 0 AND 255),
    -- Độ sáng trung bình ảnh
    sharpness       REAL        CHECK (sharpness >= 0),
    -- Độ sắc nét (Laplacian variance)
    face_bbox       TEXT,
    -- Bounding box: {"x":10,"y":20,"w":100,"h":100}

    -- ── Trạng thái & kiểm duyệt ──
    is_active       INTEGER     NOT NULL DEFAULT 1,
    -- 0 = vô hiệu hóa (không dùng nhận diện) nhưng giữ lịch sử
    is_verified     INTEGER     NOT NULL DEFAULT 0,
    -- 1 = HR/Admin đã xem xét & xác nhận chất lượng
    verified_by     INT         REFERENCES users(id) ON DELETE SET NULL,
    verified_at     DATETIME,
    registered_by   INT         REFERENCES users(id) ON DELETE SET NULL,
    -- Admin tự đăng ký hoặc NV tự chụp qua kiosk
    note            TEXT,
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME,

    CONSTRAINT uq_face_emp_index UNIQUE (employee_id, image_index)
    -- Mỗi slot (1-5) chỉ có 1 ảnh
);


-- ── B4. face_registration_logs ────────────────────────────────
-- Ghi lại toàn bộ sự kiện đăng ký / cập nhật / xóa khuôn mặt
CREATE TABLE face_registration_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id     INT         NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    face_data_id    INT         REFERENCES face_data(id) ON DELETE SET NULL,
    action          TEXT NOT NULL
                    CHECK (action IN ('Register','Update','Delete','Verify','Deactivate')),
    -- Register   = đăng ký mới
    -- Update     = chụp lại ảnh cho slot đã có
    -- Delete     = xóa vĩnh viễn
    -- Verify     = HR xác nhận chất lượng
    -- Deactivate = vô hiệu hóa (is_active = 0)
    image_index     SMALLINT,
    quality_score   REAL,
    -- Snapshot điểm chất lượng tại thời điểm thao tác
    performed_by    INT         REFERENCES users(id) ON DELETE SET NULL,
    reason          TEXT,
    -- Lý do (bắt buộc khi Delete / Deactivate)
    ip_address      TEXT,
    device_info     TEXT,
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);


-- ============================================================
--  NHÓM C: THIẾT BỊ & LỊCH LÀM VIỆC
-- ============================================================

-- ── C1. attendance_devices ────────────────────────────────────
CREATE TABLE attendance_devices (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    device_code     TEXT  NOT NULL UNIQUE,
    device_name     TEXT NOT NULL,
    device_type     TEXT  NOT NULL DEFAULT 'Camera'
                    CHECK (device_type IN ('Camera','Tablet','Kiosk','Mobile')),
    -- Camera  = camera IP cố định tại cổng/phòng
    -- Tablet  = tablet / kiosk cảm ứng
    -- Kiosk   = máy chuyên dụng chấm công
    -- Mobile  = ứng dụng di động (geofencing)
    location_name   TEXT,
    -- "Cổng chính - Tầng 1", "Phòng IT - Tầng 3"
    ip_address      TEXT,
    -- Whitelist IP — chống giả mạo thiết bị nội bộ
    mac_address     TEXT,
    -- Whitelist MAC
    -- ── GPS / Geofencing (dành cho Mobile) ──
    latitude        REAL,
    longitude       REAL,
    radius_meters   INT          DEFAULT 100 CHECK (radius_meters > 0),
    -- Bán kính cho phép check-in (Mobile geofencing)
    -- ── Cấu hình nhận diện ──
    min_confidence  REAL         NOT NULL DEFAULT 0.70 CHECK (min_confidence BETWEEN 0 AND 1),
    -- Override ngưỡng confidence tại thiết bị này
    -- (mặc định lấy từ system_settings nếu NULL)
    camera_url      TEXT,
    -- RTSP URL hoặc HTTP snapshot URL (Camera IP)
    is_online       INTEGER      NOT NULL DEFAULT 0,
    -- Cập nhật bởi heartbeat job
    last_heartbeat  DATETIME,
    is_active       INTEGER      NOT NULL DEFAULT 1,
    note            TEXT,
    created_at      DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME
);


-- ── C2. holidays ──────────────────────────────────────────────
CREATE TABLE holidays (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    holiday_date    TEXT         NOT NULL,
    name            TEXT NOT NULL,
    holiday_type    TEXT  NOT NULL DEFAULT 'National'
                    CHECK (holiday_type IN ('National','Company','Compensatory')),
    -- National     = Ngày lễ quốc gia (theo luật Việt Nam)
    -- Company      = Nghỉ riêng của công ty
    -- Compensatory = Nghỉ bù khi lễ trùng cuối tuần
    description     TEXT,
    is_recurring    INTEGER      NOT NULL DEFAULT 0,
    -- 1 = tự tạo bản ghi năm mới (background job đầu năm)
    year            SMALLINT     NOT NULL DEFAULT (CAST(strftime('%Y', 'now') AS INTEGER)),
    created_at      DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_holiday_date_year UNIQUE (holiday_date, year)
);


-- ── C3. employee_shift_schedule ───────────────────────────────
CREATE TABLE employee_shift_schedule (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id INT         NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    shift_id    INT         NOT NULL REFERENCES work_shifts(id) ON DELETE RESTRICT,
    work_date   TEXT        NOT NULL,
    is_day_off  INTEGER     NOT NULL DEFAULT 0,
    -- 1 = ngày nghỉ theo lịch phân công (ROT / ngày nghỉ bù riêng)
    note        TEXT,
    created_by  INT         REFERENCES users(id) ON DELETE SET NULL,
    created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_schedule_emp_date UNIQUE (employee_id, work_date)
    -- 1 NV chỉ có 1 lịch/ngày
);


-- ── C4. work_calendars ────────────────────────────────────────
CREATE TABLE work_calendars (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name           TEXT NOT NULL,
    -- "Hành chính T2-T6", "Vận hành T2-T7", "Nhà máy 3 ca 7 ngày"
    monday         INTEGER      NOT NULL DEFAULT 1,
    tuesday        INTEGER      NOT NULL DEFAULT 1,
    wednesday      INTEGER      NOT NULL DEFAULT 1,
    thursday       INTEGER      NOT NULL DEFAULT 1,
    friday         INTEGER      NOT NULL DEFAULT 1,
    saturday       INTEGER      NOT NULL DEFAULT 0,
    sunday         INTEGER      NOT NULL DEFAULT 0,
    effective_from TEXT         NOT NULL DEFAULT CURRENT_DATE,
    effective_to   TEXT,        -- NULL = vô thời hạn
    is_default     INTEGER      NOT NULL DEFAULT 0,
    -- Trigger đảm bảo chỉ 1 bản ghi is_default=1
    description    TEXT,
    created_at     DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     DATETIME
);


-- ============================================================
--  NHÓM D: CHẤM CÔNG
-- ============================================================

-- ── D1. attendance_records ────────────────────────────────────
-- Bảng trung tâm: mỗi nhân viên tối đa 1 bản ghi / ngày
CREATE TABLE attendance_records (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id          INT          NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    attendance_date      TEXT         NOT NULL DEFAULT CURRENT_DATE,
    shift_id             INT          REFERENCES work_shifts(id) ON DELETE SET NULL,
    -- Ca áp dụng hôm đó (business layer: lấy từ schedule → default_shift)

    -- ── Dữ liệu Check-In ──
    check_in             DATETIME,
    check_in_device_id   INT          REFERENCES attendance_devices(id) ON DELETE SET NULL,
    check_in_image_path  TEXT,        -- Ảnh chụp lúc check-in (lưu để xem lại)
    check_in_method      TEXT  DEFAULT 'Face'
                         CHECK (check_in_method IN ('Face','Manual','QRCode','NFC','Mobile')),
    check_in_confidence  REAL         CHECK (check_in_confidence  IS NULL OR check_in_confidence  BETWEEN 0 AND 1),
    -- Độ tin cậy khuôn mặt: 0.0 → 1.0
    check_in_latitude    REAL,
    check_in_longitude   REAL,
    -- GPS tọa độ lúc check-in (Mobile)

    -- ── Dữ liệu Check-Out ──
    check_out            DATETIME,
    check_out_device_id  INT          REFERENCES attendance_devices(id) ON DELETE SET NULL,
    check_out_image_path TEXT,
    check_out_method     TEXT
                         CHECK (check_out_method IN ('Face','Manual','QRCode','NFC','Mobile')),
    check_out_confidence REAL         CHECK (check_out_confidence IS NULL OR check_out_confidence BETWEEN 0 AND 1),
    check_out_latitude   REAL,
    check_out_longitude  REAL,

    -- ── Kết quả tính toán (Business Layer → lưu vào DB) ──
    status               TEXT  NOT NULL DEFAULT 'NotYet'
                         CHECK (status IN (
                             'Present',       -- Có mặt đúng giờ
                             'Late',          -- Đi muộn (quá late_threshold)
                             'EarlyLeave',    -- Về sớm (trước early_threshold)
                             'LateAndEarly',  -- Vừa muộn vừa về sớm
                             'Absent',        -- Vắng không phép
                             'Leave',         -- Nghỉ phép (có đơn Approved)
                             'Holiday',       -- Ngày lễ
                             'DayOff',        -- Ngày nghỉ theo lịch phân công
                             'NotYet'         -- Chưa đến (trong ngày làm việc)
                         )),
    late_minutes         SMALLINT     NOT NULL DEFAULT 0 CHECK (late_minutes   >= 0),
    early_minutes        SMALLINT     NOT NULL DEFAULT 0 CHECK (early_minutes  >= 0),
    working_minutes      INT          NOT NULL DEFAULT 0 CHECK (working_minutes >= 0),
    -- Phút làm thực tế = (check_out - check_in) - break_minutes
    -- Dùng phút thay vì decimal giờ để tránh sai số làm tròn

    -- ── Điều chỉnh thủ công ──
    is_manual_edit       INTEGER      NOT NULL DEFAULT 0,
    manual_edit_by       INT          REFERENCES users(id) ON DELETE SET NULL,
    manual_edit_at       DATETIME,
    manual_edit_reason   TEXT,
    -- Bắt buộc khi is_manual_edit = 1 (check tại app / trigger)
    note                 TEXT,

    created_at           DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           DATETIME,

    CONSTRAINT uq_attendance_emp_date UNIQUE (employee_id, attendance_date),
    CONSTRAINT chk_checkout_after_checkin CHECK (
        check_out IS NULL OR check_in IS NULL OR check_out >= check_in
    ),
    CONSTRAINT chk_manual_reason CHECK (
        is_manual_edit = 0
        OR (is_manual_edit = 1 AND manual_edit_reason IS NOT NULL)
    )
);


-- ── D2. attendance_logs ───────────────────────────────────────
-- Nhật ký từng lần camera nhận diện / quẹt thẻ
-- Không thể DELETE / UPDATE — append-only audit trail
CREATE TABLE attendance_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    attendance_id   BIGINT       REFERENCES attendance_records(id) ON DELETE SET NULL,
    -- NULL = chưa map được vào bản ghi (NV chưa đăng ký face, nhận diện thất bại)
    employee_id     INT          REFERENCES employees(id)          ON DELETE SET NULL,
    device_id       INT          REFERENCES attendance_devices(id) ON DELETE SET NULL,

    log_time        DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    log_type        TEXT  NOT NULL
                    CHECK (log_type IN ('CheckIn','CheckOut','Unknown')),
    method          TEXT  NOT NULL DEFAULT 'Face'
                    CHECK (method IN ('Face','Manual','QRCode','NFC','Mobile')),

    -- ── Kết quả nhận diện khuôn mặt ──
    matched_face_id  INT         REFERENCES face_data(id) ON DELETE SET NULL,
    -- Face slot nào đã match
    confidence       REAL        CHECK (confidence IS NULL OR confidence BETWEEN 0 AND 1),
    face_distance    REAL        CHECK (face_distance IS NULL OR face_distance >= 0),
    -- Khoảng cách Euclidean: nhỏ hơn = giống hơn (< 0.4 = rất giống)
    image_path       TEXT,       -- Ảnh chụp tại thời điểm nhận diện (evidence)

    -- ── Vị trí ──
    latitude        REAL,
    longitude       REAL,
    ip_address      TEXT,

    -- ── Kết quả xử lý ──
    result          TEXT  NOT NULL DEFAULT 'Success'
                    CHECK (result IN (
                        'Success',     -- Nhận diện thành công
                        'Failed',      -- Không khớp khuôn mặt nào
                        'Suspicious',  -- Khớp nhưng confidence thấp (cần xem lại)
                        'Duplicate',   -- Check-in trùng trong khoảng duplicate_window
                        'Spoofing',    -- Phát hiện ảnh giả / video playback
                        'DeviceError'  -- Lỗi thiết bị / không lấy được frame
                    )),
    fail_reason     TEXT,
    -- Chi tiết lỗi khi result != 'Success'
    raw_payload     TEXT,
    -- Dữ liệu thô từ thiết bị gửi lên (debug, không dùng business logic)
    created_at      DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP
    -- KHÔNG có updated_at — append-only
);


-- ── D3. leave_requests ────────────────────────────────────────
-- Đơn nghỉ phép — ảnh hưởng trực tiếp đến status chấm công
CREATE TABLE leave_requests (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id     INT          NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    leave_type      TEXT  NOT NULL
                    CHECK (leave_type IN (
                        'Annual',       -- Nghỉ phép năm
                        'Sick',         -- Nghỉ ốm (có giấy bác sĩ)
                        'Maternity',    -- Thai sản (nữ)
                        'Paternity',    -- Nghỉ vợ đẻ (nam)
                        'Marriage',     -- Nghỉ kết hôn
                        'Bereavement',  -- Nghỉ tang
                        'Unpaid',       -- Không lương
                        'WFH',          -- Làm từ xa / WFH
                        'Other'         -- Khác
                    )),
    start_date      TEXT         NOT NULL,
    end_date        TEXT         NOT NULL,
    total_days      REAL NOT NULL CHECK (total_days > 0),
    -- 0.5 = bán ngày; business layer tính (loại trừ T7, CN, ngày lễ)
    is_half_day     INTEGER      NOT NULL DEFAULT 0,
    half_day_period TEXT  CHECK (half_day_period IN ('Morning','Afternoon')),
    reason          TEXT         NOT NULL,
    document_path   TEXT,
    -- File đính kèm: giấy nghỉ ốm, giấy kết hôn... (PDF/JPG)
    status          TEXT  NOT NULL DEFAULT 'Pending'
                    CHECK (status IN ('Pending','Approved','Rejected','Cancelled')),
    approved_by     INT          REFERENCES employees(id) ON DELETE SET NULL,
    approved_at     DATETIME,
    reject_reason   TEXT,
    note            TEXT,
    created_at      DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME,

    CONSTRAINT chk_leave_dates   CHECK (end_date >= start_date),
    CONSTRAINT chk_half_day      CHECK (
        is_half_day = 0
        OR (start_date = end_date AND half_day_period IS NOT NULL)
    ),
    CONSTRAINT chk_leave_approved CHECK (
        status != 'Approved'
        OR (approved_by IS NOT NULL AND approved_at IS NOT NULL)
    )
);


-- ============================================================
--  NHÓM E: HỆ THỐNG & KIỂM TOÁN
-- ============================================================

-- ── E1. audit_logs ────────────────────────────────────────────
-- Append-only — KHÔNG bao giờ UPDATE / DELETE
CREATE TABLE audit_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id     INT          REFERENCES users(id)     ON DELETE SET NULL,
    employee_id INT          REFERENCES employees(id) ON DELETE SET NULL,
    action      TEXT  NOT NULL,
    -- LOGIN, LOGOUT, CREATE, UPDATE, DELETE, APPROVE, REJECT,
    -- FACE_REGISTER, FACE_DELETE, ATTENDANCE_EDIT, EXPORT ...
    table_name  TEXT,
    record_id   TEXT,  -- ID bản ghi bị tác động (TEXT để linh hoạt)
    old_values  TEXT,         -- Snapshot trước thay đổi
    new_values  TEXT,         -- Snapshot sau thay đổi
    ip_address  TEXT,
    user_agent  TEXT,
    description TEXT,
    created_at  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP
);


-- ── E2. system_settings ───────────────────────────────────────
CREATE TABLE system_settings (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    key         TEXT NOT NULL UNIQUE,
    value       TEXT         NOT NULL,
    value_type  TEXT  NOT NULL DEFAULT 'String'
                CHECK (value_type IN ('String','Integer','Decimal','Boolean','Json')),
    category    TEXT  NOT NULL DEFAULT 'General'
                CHECK (category IN ('General','FaceRecognition','Attendance','Security','Notification')),
    description TEXT,
    is_editable INTEGER      NOT NULL DEFAULT 1,
    updated_by  INT          REFERENCES users(id) ON DELETE SET NULL,
    created_at  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME
);


-- ============================================================
--  INDEXES
-- ============================================================

-- employees
CREATE INDEX idx_emp_dept      ON employees(department_id);
CREATE INDEX idx_emp_position  ON employees(position_id);
CREATE INDEX idx_emp_shift     ON employees(default_shift_id);
CREATE INDEX idx_emp_manager   ON employees(manager_id);
CREATE INDEX idx_emp_active    ON employees(is_active) WHERE is_active = 1;
CREATE INDEX idx_emp_face      ON employees(is_face_registered, is_active);

-- users
CREATE INDEX idx_usr_employee  ON users(employee_id);
CREATE INDEX idx_usr_role      ON users(role);

-- face_data  ← hot table (load vào memory cache)
CREATE INDEX idx_face_emp      ON face_data(employee_id, is_active) WHERE is_active = 1;
CREATE INDEX idx_face_verified ON face_data(is_verified) WHERE is_verified = 0;

-- face_registration_logs
CREATE INDEX idx_frlog_emp     ON face_registration_logs(employee_id, created_at DESC);
CREATE INDEX idx_frlog_action  ON face_registration_logs(action, created_at DESC);

-- attendance_devices
CREATE INDEX idx_dev_type      ON attendance_devices(device_type, is_active);
CREATE INDEX idx_dev_online    ON attendance_devices(is_online) WHERE is_online = 1;

-- employee_shift_schedule
CREATE INDEX idx_sched_emp     ON employee_shift_schedule(employee_id, work_date);
CREATE INDEX idx_sched_date    ON employee_shift_schedule(work_date);

-- holidays
CREATE INDEX idx_holiday_year  ON holidays(year, holiday_date);

-- attendance_records  ← most queried
CREATE INDEX idx_att_emp_date  ON attendance_records(employee_id, attendance_date DESC);
CREATE INDEX idx_att_date      ON attendance_records(attendance_date);
CREATE INDEX idx_att_status    ON attendance_records(status)
    WHERE status NOT IN ('Present','Holiday','DayOff');
CREATE INDEX idx_att_month     ON attendance_records(employee_id, attendance_date);
CREATE INDEX idx_att_manual    ON attendance_records(is_manual_edit) WHERE is_manual_edit = 1;
CREATE INDEX idx_att_no_out    ON attendance_records(employee_id, attendance_date)
    WHERE check_out IS NULL AND check_in IS NOT NULL;
    -- Phát hiện quên check-out

-- attendance_logs  ← append-only, query giảm dần
CREATE INDEX idx_alog_time     ON attendance_logs(log_time DESC);
CREATE INDEX idx_alog_emp      ON attendance_logs(employee_id, log_time DESC);
CREATE INDEX idx_alog_device   ON attendance_logs(device_id, log_time DESC);
CREATE INDEX idx_alog_result   ON attendance_logs(result) WHERE result != 'Success';
CREATE INDEX idx_alog_att      ON attendance_logs(attendance_id);

-- leave_requests
CREATE INDEX idx_leave_emp     ON leave_requests(employee_id, status);
CREATE INDEX idx_leave_dates   ON leave_requests(start_date, end_date);
CREATE INDEX idx_leave_pending ON leave_requests(status, created_at) WHERE status = 'Pending';

-- audit_logs
CREATE INDEX idx_audit_user    ON audit_logs(user_id, created_at DESC);
CREATE INDEX idx_audit_table   ON audit_logs(table_name, record_id);
CREATE INDEX idx_audit_time    ON audit_logs(created_at DESC);


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
CREATE VIEW v_today_attendance AS
SELECT
    e.id                                             AS employee_id,
    e.code                                           AS employee_code,
    e.full_name,
    d.name                                           AS department_name,
    p.name                                           AS position_name,
    ws.name                                          AS shift_name,
    ws.start_time, ws.end_time,
    ws.late_threshold, ws.early_threshold,
    a.check_in, a.check_out,
    a.check_in_method, a.check_out_method,
    a.check_in_confidence, a.check_out_confidence,
    a.status,
    a.late_minutes, a.early_minutes,
    ROUND((a.working_minutes / 60.0), 2)     AS working_hours,
    a.is_manual_edit,
    dev_in.device_name                               AS check_in_device,
    dev_out.device_name                              AS check_out_device,
    e.is_face_registered
FROM employees e
LEFT JOIN attendance_records a
    ON e.id = a.employee_id AND a.attendance_date = CURRENT_DATE
LEFT JOIN employee_shift_schedule ess
    ON e.id = ess.employee_id AND ess.work_date = CURRENT_DATE
LEFT JOIN work_shifts ws
    ON COALESCE(a.shift_id, ess.shift_id, e.default_shift_id) = ws.id
LEFT JOIN departments d         ON e.department_id       = d.id
LEFT JOIN positions   p         ON e.position_id         = p.id
LEFT JOIN attendance_devices dev_in  ON a.check_in_device_id  = dev_in.id
LEFT JOIN attendance_devices dev_out ON a.check_out_device_id = dev_out.id
WHERE e.is_active = 1;


-- V2. Tổng hợp chấm công theo tháng
CREATE VIEW v_monthly_summary AS
SELECT
    e.id                                                    AS employee_id,
    e.code, e.full_name,
    d.name                                                  AS department_name,
    date(a.attendance_date, 'start of month')                  AS month,
    COUNT(*)                                                AS total_records,
    SUM(CASE WHEN a.status = 'Present' THEN 1 ELSE 0 END)            AS present_days,
    SUM(CASE WHEN a.status = 'Late' THEN 1 ELSE 0 END)               AS late_days,
    SUM(CASE WHEN a.status = 'EarlyLeave' THEN 1 ELSE 0 END)         AS early_leave_days,
    SUM(CASE WHEN a.status = 'LateAndEarly' THEN 1 ELSE 0 END)       AS late_and_early_days,
    SUM(CASE WHEN a.status = 'Absent' THEN 1 ELSE 0 END)             AS absent_days,
    SUM(CASE WHEN a.status = 'Leave' THEN 1 ELSE 0 END)              AS leave_days,
    SUM(CASE WHEN a.status = 'Holiday' THEN 1 ELSE 0 END)            AS holiday_days,
    SUM(CASE WHEN a.status = 'DayOff' THEN 1 ELSE 0 END)             AS day_off_days,
    -- Ngày công hợp lệ = có mặt + trễ/sớm + nghỉ phép
    SUM(CASE WHEN a.status IN (
        'Present','Late','EarlyLeave','LateAndEarly','Leave'
    ) THEN 1 ELSE 0 END)                                                      AS actual_work_days,
    SUM(a.late_minutes)                                     AS total_late_minutes,
    SUM(a.early_minutes)                                    AS total_early_minutes,
    ROUND((SUM(a.working_minutes) / 60.0), 2)      AS total_working_hours,
    SUM(CASE WHEN a.is_manual_edit = 1 THEN 1 ELSE 0 END)         AS manual_edit_count
FROM employees e
JOIN attendance_records a ON e.id = a.employee_id
LEFT JOIN departments d   ON e.department_id = d.id
WHERE e.is_active = 1
GROUP BY e.id, e.code, e.full_name, d.name,
         date(a.attendance_date, 'start of month');


-- V3. Nhân viên chưa đăng ký / cần cập nhật Face ID
CREATE VIEW v_face_status AS
SELECT
    e.id, e.code, e.full_name,
    d.name                                          AS department_name,
    e.is_face_registered,
    e.face_registered_at,
    COUNT(fd.id)                                    AS total_faces,
    SUM(CASE WHEN fd.is_active = 1 THEN 1 ELSE 0 END) AS active_faces,
    SUM(CASE WHEN fd.is_verified = 1 AND fd.is_active = 1 THEN 1 ELSE 0 END) AS verified_faces,
    ROUND((AVG(CASE WHEN fd.is_active = 1 THEN fd.quality_score ELSE NULL END)), 3) AS avg_quality,
    MIN(CASE WHEN fd.is_active = 1 THEN fd.quality_score ELSE NULL END)           AS min_quality
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
LEFT JOIN face_data fd  ON e.id = fd.employee_id
WHERE e.is_active = 1
GROUP BY e.id, e.code, e.full_name, d.name,
         e.is_face_registered, e.face_registered_at;


-- V4. Bất thường chấm công cần xem xét
CREATE VIEW v_attendance_anomalies AS
SELECT
    a.id            AS attendance_id,
    e.code, e.full_name,
    d.name          AS department_name,
    a.attendance_date,
    a.status,
    a.check_in, a.check_out,
    a.late_minutes, a.early_minutes,
    ROUND((a.working_minutes / 60.0), 2) AS working_hours,
    a.check_in_confidence,
    a.check_out_confidence,
    CASE
        WHEN a.check_in_confidence  < 0.70  THEN 'Check-in confidence thấp < 0.70'
        WHEN a.check_out_confidence < 0.70  THEN 'Check-out confidence thấp < 0.70'
        WHEN a.late_minutes  > 60           THEN 'Đi muộn > 60 phút'
        WHEN a.early_minutes > 60           THEN 'Về sớm > 60 phút'
        WHEN a.check_out IS NULL
             AND a.check_in IS NOT NULL
             AND CURRENT_TIMESTAMP > datetime(a.attendance_date, '+1 day')
                                            THEN 'Quên check-out'
        WHEN a.working_minutes < 240 AND a.status = 'Present'
                                            THEN 'Giờ làm < 4h nhưng status Present'
        WHEN a.is_manual_edit = 1        THEN 'Điều chỉnh thủ công'
        ELSE 'Bất thường'
    END             AS anomaly_type
FROM attendance_records a
JOIN employees   e ON a.employee_id   = e.id
JOIN departments d ON e.department_id = d.id
WHERE
    (a.check_in_confidence  IS NOT NULL AND a.check_in_confidence  < 0.70) OR
    (a.check_out_confidence IS NOT NULL AND a.check_out_confidence < 0.70) OR
    a.late_minutes > 60 OR a.early_minutes > 60 OR
    (a.check_out IS NULL AND a.check_in IS NOT NULL
     AND CURRENT_TIMESTAMP > datetime(a.attendance_date, '+1 day')) OR
    (a.working_minutes < 240 AND a.status = 'Present') OR
    a.is_manual_edit = 1;


-- V5. Đơn nghỉ phép đang chờ duyệt
CREATE VIEW v_pending_leaves AS
SELECT
    lr.id, e.code, e.full_name,
    d.name          AS department_name,
    mgr.full_name   AS manager_name,
    lr.leave_type,
    lr.start_date, lr.end_date, lr.total_days,
    lr.is_half_day, lr.half_day_period,
    lr.reason,
    CASE WHEN lr.document_path IS NOT NULL THEN 1 ELSE 0 END AS has_document,
    lr.created_at
FROM leave_requests lr
JOIN employees e    ON lr.employee_id = e.id
LEFT JOIN departments d   ON e.department_id = d.id
LEFT JOIN employees mgr   ON e.manager_id    = mgr.id
WHERE lr.status = 'Pending'
ORDER BY lr.created_at;


-- V6. Số dư nghỉ phép
CREATE VIEW v_leave_balance AS
SELECT
    e.id, e.code, e.full_name,
    d.name                                   AS department_name,
    e.annual_leave_days,
    e.used_leave_days,
    e.annual_leave_days - e.used_leave_days  AS remaining_days,
    CAST(strftime('%Y', 'now') AS INTEGER)     AS year
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
WHERE e.is_active = 1;


-- V7. Nhật ký nhận diện khuôn mặt nghi ngờ (Suspicious / Spoofing)
CREATE VIEW v_suspicious_recognition AS
SELECT
    al.id       AS log_id,
    al.log_time,
    e.code      AS employee_code,
    e.full_name,
    d.name      AS department_name,
    al.result,
    al.confidence,
    al.face_distance,
    al.method,
    al.ip_address,
    dev.device_name,
    dev.location_name,
    al.image_path,
    al.fail_reason
FROM attendance_logs al
LEFT JOIN employees         e   ON al.employee_id = e.id
LEFT JOIN departments       d   ON e.department_id = d.id
LEFT JOIN attendance_devices dev ON al.device_id   = dev.id
WHERE al.result IN ('Suspicious','Spoofing','Failed')
ORDER BY al.log_time DESC;


-- ============================================================
--  DỮ LIỆU HỆ THỐNG
-- ============================================================

INSERT INTO system_settings (key, value, value_type, category, description) VALUES
-- Face Recognition
('face.confidence_threshold',        '0.70',  'Decimal', 'FaceRecognition', 'Ngưỡng confidence tối thiểu chấp nhận nhận diện (0-1)'),
('face.max_slots_per_employee',      '5',     'Integer', 'FaceRecognition', 'Số ảnh tối đa mỗi nhân viên (slot 1-5)'),
('face.min_quality_score',           '0.60',  'Decimal', 'FaceRecognition', 'Điểm chất lượng ảnh tối thiểu khi đăng ký'),
('face.max_distance',                '0.40',  'Decimal', 'FaceRecognition', 'Khoảng cách Euclidean tối đa (< giá trị này = khớp)'),
('face.anti_spoofing_enabled',       '1',  'Boolean', 'FaceRecognition', 'Bật liveness detection chống ảnh giả/video playback'),
('face.duplicate_window_minutes',    '5',     'Integer', 'FaceRecognition', 'Khoảng thời gian chặn check-in trùng (phút)'),
('face.require_verification',        '1',  'Boolean', 'FaceRecognition', 'Bắt buộc HR xác nhận ảnh trước khi đưa vào nhận diện'),
('face.min_face_size_pixels',        '80',    'Integer', 'FaceRecognition', 'Kích thước khuôn mặt tối thiểu trong ảnh (pixel)'),
-- Attendance
('attendance.auto_absent_hour',      '22',    'Integer', 'Attendance',      'Giờ tự động đánh Absent nếu chưa check-in (22:00)'),
('attendance.allow_mobile_checkin',  '1',  'Boolean', 'Attendance',      'Cho phép chấm công qua mobile app'),
('attendance.geofence_enabled',      '1',  'Boolean', 'Attendance',      'Kiểm tra GPS khi check-in Mobile'),
('attendance.manual_edit_notify',    '1',  'Boolean', 'Attendance',      'Thông báo nhân viên khi HR sửa chấm công'),
-- Security
('security.max_login_attempts',      '5',     'Integer', 'Security',        'Số lần đăng nhập sai trước khi khóa tài khoản'),
('security.lockout_minutes',         '30',    'Integer', 'Security',        'Thời gian khóa tài khoản (phút)'),
('security.jwt_expire_minutes',      '60',    'Integer', 'Security',        'Thời gian hết hạn JWT Access Token (phút)'),
('security.refresh_token_days',      '7',     'Integer', 'Security',        'Thời gian hết hạn Refresh Token (ngày)'),
('security.password_min_length',     '8',     'Integer', 'Security',        'Độ dài mật khẩu tối thiểu'),
-- Notification
('notification.email_enabled',       '1',  'Boolean', 'Notification',    'Gửi email thông báo'),
('notification.late_alert_enabled',  '1',  'Boolean', 'Notification',    'Cảnh báo đi muộn qua email/push'),
('notification.approval_notify',     '1',  'Boolean', 'Notification',    'Thông báo kết quả duyệt đơn');


-- ============================================================
--  DỮ LIỆU MẪU (Seed)
--  Mật khẩu mặc định: admin / admin123  |  user / user123
-- ============================================================

-- Phòng ban (7 phòng)
INSERT INTO departments (code, name, description, sort_order) VALUES
('BOD',   'Ban Giám đốc',           'Điều hành chung công ty',          1),
('HR',    'Phòng Nhân sự',          'Quản lý nhân sự, tuyển dụng',     2),
('IT',    'Phòng Công nghệ',        'Phát triển và vận hành hệ thống', 3),
('SALES', 'Phòng Kinh doanh',       'Bán hàng và chăm sóc khách',     4),
('ACCT',  'Phòng Kế toán',          'Quản lý tài chính, kế toán',     5),
('MKT',   'Phòng Marketing',        'Truyền thông và quảng cáo',      6),
('QA',    'Phòng Kiểm thử',         'Đảm bảo chất lượng sản phẩm',   7);

-- Chức vụ (7 cấp)
INSERT INTO positions (code, name, level) VALUES
('CEO',  'Giám đốc điều hành', 10),
('VP',   'Phó giám đốc',        9),
('DIR',  'Trưởng phòng',        7),
('LEAD', 'Trưởng nhóm',         5),
('SR',   'Nhân viên cao cấp',   4),
('JR',   'Nhân viên',           3),
('INT',  'Thực tập sinh',       1);

-- Ca làm việc (5 ca)
INSERT INTO work_shifts
    (code, name, shift_type, start_time, end_time,
     break_minutes, standard_hours, late_threshold, early_threshold, is_overnight, color_code)
VALUES
('MAIN',  'Ca hành chính', 'Fixed',    '08:00', '17:00', 60, 8.0, 15, 15, 0, '#4A90D9'),
('MORN',  'Ca sáng',       'Shift',    '06:00', '14:00', 30, 8.0, 10, 10, 0, '#F5A623'),
('AFT',   'Ca chiều',      'Shift',    '14:00', '22:00', 30, 8.0, 10, 10, 0, '#7ED321'),
('NIGHT', 'Ca đêm',        'Shift',    '22:00', '06:00', 30, 8.0, 10, 10, 1, '#9B59B6'),
('FLEX',  'Ca linh hoạt',  'Flexible', '07:00', '19:00', 60, 8.0,  0,  0, 0, '#1ABC9C');

-- Lịch làm tuần (3 loại)
INSERT INTO work_calendars (name, saturday, sunday, is_default) VALUES
('Hành chính T2-T6', 0, 0, 1),
('Vận hành T2-T7',   1, 0, 0),
('Sản xuất 7 ngày',  1, 1, 0);

-- Ngày lễ 2026
INSERT INTO holidays (holiday_date, name, holiday_type, is_recurring, year) VALUES
('2026-01-01', 'Tết Dương lịch',              'National', 1, 2026),
('2026-02-17', 'Nghỉ Tết Nguyên đán (bù)',    'National', 0, 2026),
('2026-02-18', 'Tết Nguyên đán – Mùng 1',     'National', 0, 2026),
('2026-02-19', 'Tết Nguyên đán – Mùng 2',     'National', 0, 2026),
('2026-02-20', 'Tết Nguyên đán – Mùng 3',     'National', 0, 2026),
('2026-04-06', 'Giỗ Tổ Hùng Vương (10/3 ÂL)', 'National', 0, 2026),
('2026-04-30', 'Ngày Giải phóng miền Nam',     'National', 1, 2026),
('2026-05-01', 'Quốc tế Lao động',             'National', 1, 2026),
('2026-09-02', 'Quốc khánh',                   'National', 1, 2026),
('2026-09-03', 'Nghỉ bù Quốc khánh',          'National', 0, 2026);

-- ── Nhân viên mẫu (10 người) ──
INSERT INTO employees
    (code, full_name, gender, date_of_birth, phone, email, identity_card,
     department_id, position_id, default_shift_id, employment_type, hire_date, annual_leave_days)
VALUES
('NV001', 'Trần Minh Nhật',         'M', '2004-08-15', '0901000001', 'nhat.tm@company.com',  '079204001001', 3, 4, 1, 'FullTime', '2023-06-01', 14),
('NV002', 'Trần Dương Yến Nhi',     'F', '2003-12-25', '0901000002', 'nhi.tdy@company.com',  '079203002002', 2, 5, 1, 'FullTime', '2023-01-10', 14),
('NV003', 'Trần Nguyễn Minh Khôi',  'M', '2004-03-20', '0901000003', 'khoi.tnm@company.com', '079204003003', 3, 6, 1, 'FullTime', '2024-02-15', 12),
('NV004', 'Nguyễn Văn An',          'M', '1985-05-10', '0901000004', 'an.nv@company.com',    '079185004004', 1, 1, 1, 'FullTime', '2015-01-01', 18),
('NV005', 'Lê Thị Hồng Nhung',      'F', '1990-11-08', '0901000005', 'nhung.lth@company.com','079190005005', 2, 3, 1, 'FullTime', '2018-03-15', 16),
('NV006', 'Phạm Quốc Bảo',          'M', '1992-07-22', '0901000006', 'bao.pq@company.com',  '079192006006', 3, 3, 1, 'FullTime', '2019-08-01', 15),
('NV007', 'Võ Ngọc Trâm',           'F', '1995-04-18', '0901000007', 'tram.vn@company.com', '079195007007', 4, 5, 1, 'FullTime', '2020-06-10', 14),
('NV008', 'Huỳnh Đức Trí',          'M', '1998-09-30', '0901000008', 'tri.hd@company.com',  '079198008008', 5, 5, 1, 'FullTime', '2021-11-01', 13),
('NV009', 'Đặng Thùy Linh',         'F', '2000-02-14', '0901000009', 'linh.dt@company.com', '079200009009', 6, 6, 1, 'FullTime', '2022-07-20', 12),
('NV010', 'Bùi Thanh Tùng',         'M', '2001-06-05', '0901000010', 'tung.bt@company.com', '079201010010', 7, 7, 1, 'PartTime', '2025-01-15', 10),
('NV011', 'Đoàn Ngọc Linh', 'F', '1994-12-16', '0901207089', 'linh.dn@company.com', '079104111842', 4, 4, 1, 'FullTime', '2022-09-27', 12),
('NV012', 'Hồ Nhật Anh', 'F', '1999-09-12', '0901965730', 'anh.hn@company.com', '079132599779', 6, 7, 1, 'FullTime', '2024-11-19', 14),
('NV013', 'Nguyễn Vũ Gia Bảo', 'M', '2005-11-01', '0901364857', 'bao.nvg@company.com', '079194299400', 2, 7, 1, 'FullTime', '2024-01-10', 16),
('NV014', 'Nguyễn Hữu Bình', 'M', '2001-07-14', '0901614094', 'binh.nh@company.com', '079135107313', 4, 7, 1, 'FullTime', '2023-07-28', 15),
('NV015', 'Trần Lê Anh Đại', 'F', '1991-12-06', '0901454196', 'dai.tla@company.com', '079166260980', 4, 4, 1, 'FullTime', '2021-04-20', 15),
('NV016', 'Nguyễn Thành Đạt', 'M', '1996-03-03', '0901104739', 'dat.nt@company.com', '079108896132', 3, 7, 1, 'FullTime', '2022-10-14', 12),
('NV017', 'Nguyễn Hoàng Tiến Đạt', 'M', '1994-07-18', '0901571068', 'dat.nht@company.com', '079104492177', 4, 6, 1, 'FullTime', '2020-03-24', 12),
('NV018', 'Bùi Hải Đường', 'M', '1992-01-27', '0901688802', 'duong.bh@company.com', '079125933155', 2, 7, 1, 'FullTime', '2023-03-05', 16),
('NV019', 'Trương Đại Hải', 'M', '1993-07-24', '0901105276', 'hai.td@company.com', '079185686429', 5, 4, 1, 'FullTime', '2022-05-22', 15),
('NV020', 'Hà Chí Hân', 'M', '1995-11-03', '0901912225', 'han.hc@company.com', '079185372333', 5, 5, 1, 'FullTime', '2023-08-12', 12),
('NV021', 'Đặng Ngọc Châu', 'M', '2002-02-19', '0901624941', 'chau.dn@company.com', '079128401461', 7, 6, 1, 'FullTime', '2020-02-07', 14),
('NV022', 'Trương Văn Huế', 'M', '2001-11-12', '0901525631', 'hue.tv@company.com', '079178166149', 6, 4, 1, 'FullTime', '2022-08-03', 15),
('NV023', 'Nguyễn Tấn Hưng', 'M', '1996-10-25', '0901382472', 'hung.nt@company.com', '079144348328', 6, 5, 1, 'FullTime', '2023-02-15', 12),
('NV024', 'Lê Thanh Khải', 'M', '1996-10-10', '0901425030', 'khai.lt@company.com', '079178440514', 2, 7, 1, 'FullTime', '2023-02-23', 15),
('NV025', 'Lê Tuấn Kiệt', 'M', '2000-02-16', '0901705111', 'kiet.lt@company.com', '079195731097', 2, 4, 1, 'FullTime', '2021-01-22', 12),
('NV026', 'Nguyễn Hoàng Kỳ', 'M', '1991-11-18', '0901910492', 'ky.nh@company.com', '079146803029', 2, 4, 1, 'FullTime', '2020-11-06', 16),
('NV027', 'Nguyễn Thị Kim Liên', 'F', '1997-09-04', '0901557405', 'lien.ntk@company.com', '079159640477', 4, 7, 1, 'FullTime', '2024-06-02', 14),
('NV028', 'Nguyễn Văn Toàn', 'M', '2004-05-12', '0901329782', 'toan.nv@company.com', '079111765625', 4, 7, 1, 'FullTime', '2020-12-01', 12),
('NV029', 'Trần Anh Nhân', 'F', '1997-12-06', '0901650437', 'nhan.ta@company.com', '079154737949', 7, 5, 1, 'FullTime', '2023-02-24', 15),
('NV030', 'Đặng Minh Nhật', 'M', '1997-04-23', '0901118939', 'nhat.dm@company.com', '079121543964', 4, 4, 1, 'FullTime', '2023-12-08', 15),
('NV031', 'Nguyễn Văn Phú', 'M', '1998-11-02', '0901395679', 'phu.nv@company.com', '079179731823', 3, 7, 1, 'FullTime', '2021-06-08', 14),
('NV032', 'Đoàn Phúc', 'M', '1992-12-12', '0901130960', 'phuc.d@company.com', '079115136797', 7, 7, 1, 'FullTime', '2021-05-17', 12),
('NV033', 'Bùi Đắc Quí', 'M', '1999-01-22', '0901351204', 'qui.bd@company.com', '079168652495', 5, 4, 1, 'FullTime', '2024-11-25', 14),
('NV034', 'Nguyễn Thành Thái', 'M', '1996-09-17', '0901677981', 'thai.nt@company.com', '079131541936', 4, 4, 1, 'FullTime', '2020-08-03', 14),
('NV035', 'Hồ Hữu Thịnh', 'F', '2003-09-26', '0901920354', 'thinh.hh@company.com', '079118350421', 4, 4, 1, 'FullTime', '2022-04-28', 16),
('NV036', 'Lâm Diệu Tinh', 'F', '2005-10-04', '0901883557', 'tinh.ld@company.com', '079122345251', 7, 6, 1, 'FullTime', '2020-03-26', 16),
('NV037', 'Trần Thanh Phương', 'M', '1993-04-02', '0901974389', 'phuong.tt@company.com', '079121251414', 2, 4, 1, 'FullTime', '2024-10-08', 16),
('NV038', 'Phạm Gia Bảo', 'M', '1999-09-03', '0901835971', 'bao.pg@company.com', '079154997403', 7, 5, 1, 'FullTime', '2023-09-08', 16),
('NV039', 'Nguyễn Trần Hữu Đức', 'M', '2005-12-28', '0901464031', 'duc.nth@company.com', '079178354917', 5, 6, 1, 'FullTime', '2021-08-27', 15),
('NV040', 'Triệu Ngọc Hào', 'M', '1999-03-04', '0901706069', 'hao.tn@company.com', '079184482350', 2, 6, 1, 'FullTime', '2024-05-01', 16),
('NV041', 'Trần Trung Hiếu', 'M', '2005-03-11', '0901663980', 'hieu.tt@company.com', '079142674087', 3, 7, 1, 'FullTime', '2022-08-11', 12),
('NV042', 'Lâm Thái Hòa', 'M', '2004-09-09', '0901444251', 'hoa.lt@company.com', '079112876963', 7, 5, 1, 'FullTime', '2022-05-02', 14),
('NV043', 'Huỳnh Đông Huy', 'M', '1995-09-07', '0901552496', 'huy.hd@company.com', '079199509373', 3, 5, 1, 'FullTime', '2024-09-08', 12),
('NV044', 'Nguyễn Quốc Khánh', 'M', '1996-09-10', '0901232611', 'khanh.nq@company.com', '079144801745', 5, 5, 1, 'FullTime', '2022-11-22', 14),
('NV045', 'Đặng Văn Khoa', 'M', '2005-06-27', '0901582681', 'khoa.dv@company.com', '079158634238', 4, 4, 1, 'FullTime', '2021-01-23', 12),
('NV046', 'Lê Trung Kiên', 'M', '2002-01-23', '0901848806', 'kien.lt@company.com', '079175733988', 7, 7, 1, 'FullTime', '2022-01-11', 12),
('NV047', 'Nguyễn Tấn Kiệt', 'M', '1999-07-04', '0901884574', 'kiet.nt@company.com', '079109619039', 3, 7, 1, 'FullTime', '2023-10-01', 15),
('NV048', 'Hoàng Công Trường Lộc', 'M', '2001-09-05', '0901748651', 'loc.hct@company.com', '079165131085', 5, 5, 1, 'FullTime', '2024-04-23', 12),
('NV049', 'Lưu Gia Luân', 'M', '2000-05-07', '0901577919', 'luan.lg@company.com', '079138862633', 3, 5, 1, 'FullTime', '2024-03-09', 15),
('NV050', 'Lương Hào Minh', 'M', '1992-10-28', '0901359107', 'minh.lh@company.com', '079197113679', 5, 7, 1, 'FullTime', '2024-02-07', 12),
('NV051', 'Nguyễn Hoàng Phúc', 'M', '2000-11-25', '0901657951', 'phuc.nh@company.com', '079176943958', 4, 7, 1, 'FullTime', '2022-01-27', 14),
('NV052', 'Võ Xuân Thắng', 'M', '1991-08-16', '0901469188', 'thang.vx@company.com', '079174769868', 4, 7, 1, 'FullTime', '2023-03-22', 16),
('NV053', 'Hồ Ngọc Thái Thông', 'M', '1998-11-08', '0901487363', 'thong.hnt@company.com', '079168158615', 5, 5, 1, 'FullTime', '2021-01-14', 15),
('NV054', 'Nguyễn Lê Anh Trúc', 'F', '2002-04-26', '0901854407', 'truc.nla@company.com', '079172736107', 4, 4, 1, 'FullTime', '2022-11-13', 12),
('NV055', 'Nguyễn Văn Trường', 'M', '1990-06-07', '0901497071', 'truong.nv@company.com', '079193506468', 5, 4, 1, 'FullTime', '2021-06-01', 15),
('NV056', 'Nguyễn Thanh Tú', 'M', '2005-09-09', '0901124859', 'tu.nt@company.com', '079118589802', 2, 7, 1, 'FullTime', '2020-04-02', 15),
('NV057', 'Nguyễn Hoàng Vĩ', 'M', '2004-12-15', '0901985062', 'vi.nh@company.com', '079170796862', 5, 6, 1, 'FullTime', '2021-10-01', 15),
('NV058', 'Trần Hồ Quang Vinh', 'M', '2002-01-04', '0901530620', 'vinh.thq@company.com', '079121300210', 5, 7, 1, 'FullTime', '2021-07-02', 16),
('NV059', 'Đinh Thị Thảo An', 'F', '1996-01-16', '0901321158', 'an.dtt@company.com', '079182628871', 2, 7, 1, 'FullTime', '2023-09-03', 14),
('NV060', 'Phạm Hữu Ân', 'M', '2000-09-08', '0901388445', 'an.ph@company.com', '079147436466', 3, 6, 1, 'FullTime', '2021-07-08', 15),
('NV061', 'Lê Văn Bình', 'M', '2003-06-09', '0901765104', 'binh.lv@company.com', '079173253225', 4, 6, 1, 'FullTime', '2020-01-18', 14),
('NV062', 'Nguyễn Ngọc Thùy Dương', 'F', '1997-05-26', '0901968247', 'duong.nnt@company.com', '079178944019', 7, 4, 1, 'FullTime', '2021-07-19', 12);

-- Quản lý trực tiếp
UPDATE employees SET manager_id = 4 WHERE id IN (1, 2, 3, 5, 6, 7, 8, 9, 10);
UPDATE employees SET manager_id = 4 WHERE id BETWEEN 11 AND 62; -- Tạm thời gán CEO quản lý hết để dễ test
UPDATE employees SET manager_id = 6 WHERE id IN (1, 3) OR (id BETWEEN 11 AND 62 AND department_id = 3);
UPDATE employees SET manager_id = 5 WHERE id = 2 OR (id BETWEEN 11 AND 62 AND department_id = 2);

-- ── Tài khoản đăng nhập (hash sẽ được DatabaseBootstrapper cập nhật) ──
INSERT INTO users (username, password_hash, role, employee_id, must_change_password) VALUES
('admin', 'SEED_WILL_BE_REHASHED', 'Admin',    NULL, 0),
('user',  'SEED_WILL_BE_REHASHED', 'Employee', 1,    0);

-- ── Thiết bị (5 thiết bị) ──
INSERT INTO attendance_devices
    (device_code, device_name, device_type, location_name, ip_address,
     latitude, longitude, radius_meters, min_confidence, camera_url)
VALUES
('CAM-GATE-01', 'Camera Cổng chính',  'Camera', 'Cổng chính – Tầng 1', '192.168.1.101', 10.77690, 106.70090,  50, 0.72, 'rtsp://192.168.1.101:554/stream'),
('CAM-IT-01',   'Camera Phòng IT',    'Camera', 'Phòng IT – Tầng 3',   '192.168.1.102', 10.77700, 106.70100,  30, 0.70, 'rtsp://192.168.1.102:554/stream'),
('CAM-SALES',   'Camera Phòng KD',    'Camera', 'Phòng KD – Tầng 2',   '192.168.1.103', 10.77680, 106.70080,  30, 0.70, 'rtsp://192.168.1.103:554/stream'),
('KSK-HR-01',   'Kiosk Phòng HR',     'Kiosk',  'HR – Tầng 2',         '192.168.1.110', 10.77685, 106.70085,  30, 0.70, NULL),
('MOB-APP',     'Mobile Application', 'Mobile', 'Di động / WFH',        NULL,            NULL,     NULL,       100, 0.68, NULL);

-- ── Bản ghi chấm công mẫu ──
INSERT INTO attendance_records
    (employee_id, attendance_date, shift_id,
     check_in, check_out, check_in_device_id, check_out_device_id,
     check_in_method, check_out_method,
     check_in_confidence, check_out_confidence,
     status, late_minutes, early_minutes, working_minutes)
VALUES
(1, date('now'), 1,
 datetime('now', 'start of day', '+7 hours', '+55 minutes'), datetime('now', 'start of day', '+17 hours', '+10 minutes'),
 2, 2, 'Face', 'Face', 0.96, 0.94, 'Present', 0, 0, 495),
(2, date('now'), 1,
 datetime('now', 'start of day', '+8 hours', '+22 minutes'), datetime('now', 'start of day', '+17 hours', '+5 minutes'),
 1, 1, 'Face', 'Face', 0.93, 0.91, 'Late', 22, 0, 463),
(3, date('now'), 1,
 datetime('now', 'start of day', '+7 hours', '+58 minutes'), datetime('now', 'start of day', '+16 hours', '+30 minutes'),
 2, 2, 'Face', 'Face', 0.95, 0.92, 'EarlyLeave', 0, 30, 452),
(4, date('now'), 1,
 datetime('now', 'start of day', '+7 hours', '+50 minutes'), datetime('now', 'start of day', '+17 hours', '+20 minutes'),
 1, 1, 'Face', 'Face', 0.97, 0.96, 'Present', 0, 0, 510),
(7, date('now'), 1,
 datetime('now', 'start of day', '+8 hours', '+0 minutes'), datetime('now', 'start of day', '+17 hours', '+0 minutes'),
 3, 3, 'Face', 'Face', 0.90, 0.88, 'Present', 0, 0, 480),
(1, date('now', '-1 day'), 1,
 datetime('now', '-1 day', 'start of day', '+7 hours', '+52 minutes'), datetime('now', '-1 day', 'start of day', '+17 hours', '+8 minutes'),
 2, 2, 'Face', 'Face', 0.95, 0.93, 'Present', 0, 0, 496),
(2, date('now', '-1 day'), 1,
 datetime('now', '-1 day', 'start of day', '+8 hours', '+0 minutes'), datetime('now', '-1 day', 'start of day', '+17 hours', '+0 minutes'),
 1, 1, 'Face', 'Face', 0.92, 0.90, 'Present', 0, 0, 480),
(3, date('now', '-1 day'), 1,
 datetime('now', '-1 day', 'start of day', '+8 hours', '+5 minutes'), datetime('now', '-1 day', 'start of day', '+17 hours', '+15 minutes'),
 2, 2, 'Face', 'Face', 0.94, 0.91, 'Present', 5, 0, 490),
(5, date('now', '-1 day'), 1,
 datetime('now', '-1 day', 'start of day', '+7 hours', '+45 minutes'), datetime('now', '-1 day', 'start of day', '+17 hours', '+30 minutes'),
 1, 1, 'Face', 'Face', 0.96, 0.95, 'Present', 0, 0, 525),
(6, date('now', '-2 days'), 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Absent', 0, 0, 0);

-- ── Log nhận diện ──
INSERT INTO attendance_logs
    (attendance_id, employee_id, device_id, log_type, method,
     matched_face_id, confidence, face_distance, result)
VALUES
(1, 1, 2, 'CheckIn',  'Face', NULL, 0.96, 0.25, 'Success'),
(1, 1, 2, 'CheckOut', 'Face', NULL, 0.94, 0.28, 'Success'),
(2, 2, 1, 'CheckIn',  'Face', NULL, 0.93, 0.30, 'Success'),
(2, 2, 1, 'CheckOut', 'Face', NULL, 0.91, 0.32, 'Success'),
(3, 3, 2, 'CheckIn',  'Face', NULL, 0.95, 0.27, 'Success'),
(3, 3, 2, 'CheckOut', 'Face', NULL, 0.92, 0.29, 'Success');

-- ── Đơn nghỉ phép mẫu (3 đơn) ──
INSERT INTO leave_requests
    (employee_id, leave_type, start_date, end_date, total_days, reason, status, approved_by, approved_at)
VALUES
(3, 'Annual',   date('now', '+10 days'), date('now', '+11 days'), 2, 'Nghỉ phép về quê',                    'Pending', NULL, NULL),
(2, 'Sick',     date('now', '-3 days'),  date('now', '-3 days'),  1, 'Khám sức khỏe định kỳ',               'Approved', 5, datetime('now', '-2 days')),
(9, 'Other',    date('now', '+5 days'),  date('now', '+5 days'),  1, 'Có việc gia đình cần giải quyết',     'Pending', NULL, NULL);


-- ============================================================
-- ============================================================
--  GHI CHÚ KỸ THUẬT
-- ============================================================
--
-- Database   : SQLite 3.x (System.Data.SQLite.Core)
-- Framework  : .NET Framework 4.6.1 / WinForms
-- Password   : BCrypt.Net-Next (AuthPasswordHasher)
-- Face AI    : FaceRecognitionDotNet (dlib wrapper) — float[128] encoding
-- Mật khẩu mặc định tất cả tài khoản mẫu: admin123
--
-- ============================================================
-- KẾT THÚC SCRIPT
-- ============================================================
