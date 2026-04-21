-- ============================================================
--  HỆ THỐNG CHẤM CÔNG NHẬN DIỆN KHUÔN MẶT
--  Phiên bản : 3.0  —  Face-Attendance Focus
--  Database  : PostgreSQL 15+
--  Timezone  : Asia/Ho_Chi_Minh
--  Stack     : .NET 8 / ASP.NET Core / EF Core / SignalR
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

SET timezone = 'Asia/Ho_Chi_Minh';

-- ============================================================
--  DROP (thứ tự ngược FK)
-- ============================================================
DROP TABLE IF EXISTS audit_logs               CASCADE;
DROP TABLE IF EXISTS system_settings          CASCADE;
DROP TABLE IF EXISTS leave_requests           CASCADE;
DROP TABLE IF EXISTS attendance_logs          CASCADE;
DROP TABLE IF EXISTS attendance_records       CASCADE;
DROP TABLE IF EXISTS employee_shift_schedule  CASCADE;
DROP TABLE IF EXISTS work_calendars           CASCADE;
DROP TABLE IF EXISTS holidays                 CASCADE;
DROP TABLE IF EXISTS attendance_devices       CASCADE;
DROP TABLE IF EXISTS face_registration_logs   CASCADE;
DROP TABLE IF EXISTS face_data                CASCADE;
DROP TABLE IF EXISTS users                    CASCADE;
DROP TABLE IF EXISTS employees                CASCADE;
DROP TABLE IF EXISTS work_shifts              CASCADE;
DROP TABLE IF EXISTS positions                CASCADE;
DROP TABLE IF EXISTS departments              CASCADE;


-- ============================================================
--  NHÓM A: DANH MỤC
-- ============================================================

-- ── A1. departments ──────────────────────────────────────────
CREATE TABLE departments (
    id          SERIAL       PRIMARY KEY,
    code        VARCHAR(20)  NOT NULL UNIQUE,
    name        VARCHAR(100) NOT NULL,
    description TEXT,
    parent_id   INT          REFERENCES departments(id) ON DELETE SET NULL,
    manager_id  INT,                         -- FK → employees(id), bổ sung sau
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    sort_order  SMALLINT     NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMPTZ
);
COMMENT ON TABLE  departments           IS 'Phòng ban — hỗ trợ cấu trúc cây qua parent_id';
COMMENT ON COLUMN departments.parent_id IS 'Phòng ban cha; NULL = gốc';
COMMENT ON COLUMN departments.manager_id IS 'Trưởng phòng — FK employees(id), thêm sau';


-- ── A2. positions ─────────────────────────────────────────────
CREATE TABLE positions (
    id          SERIAL       PRIMARY KEY,
    code        VARCHAR(20)  NOT NULL UNIQUE,
    name        VARCHAR(100) NOT NULL,
    level       SMALLINT     NOT NULL DEFAULT 1 CHECK (level BETWEEN 1 AND 10),
    -- 1=Thực tập  3=Nhân viên  5=Trưởng nhóm  7=Trưởng phòng  10=Giám đốc
    -- level dùng để routing duyệt đơn từ phía application
    description TEXT,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMPTZ
);
COMMENT ON COLUMN positions.level IS '1-10: dùng routing duyệt đơn (app logic)';


-- ── A3. work_shifts ───────────────────────────────────────────
CREATE TABLE work_shifts (
    id               SERIAL         PRIMARY KEY,
    code             VARCHAR(20)    NOT NULL UNIQUE,
    name             VARCHAR(100)   NOT NULL,
    shift_type       VARCHAR(20)    NOT NULL DEFAULT 'Fixed'
                     CHECK (shift_type IN ('Fixed','Flexible','Shift')),
    -- Fixed    = giờ vào/ra cố định (hành chính)
    -- Flexible = linh hoạt, tính đủ standard_hours/ngày
    -- Shift    = ca xoay 3 ca
    start_time       TIME           NOT NULL,
    end_time         TIME           NOT NULL,
    break_minutes    SMALLINT       NOT NULL DEFAULT 60 CHECK (break_minutes >= 0),
    standard_hours   DECIMAL(4,2)   NOT NULL DEFAULT 8  CHECK (standard_hours > 0),
    -- Tổng giờ làm chuẩn = (end-start) - break  (business layer tính)
    late_threshold   SMALLINT       NOT NULL DEFAULT 15 CHECK (late_threshold >= 0),
    -- Phút ân hạn đến muộn; check_in <= start + threshold → vẫn đúng giờ
    early_threshold  SMALLINT       NOT NULL DEFAULT 15 CHECK (early_threshold >= 0),
    -- Phút ân hạn về sớm; check_out >= end - threshold → không bị về sớm
    is_overnight     BOOLEAN        NOT NULL DEFAULT FALSE,
    -- TRUE = ca qua đêm: end_time thuộc ngày D+1 (vd 22:00→06:00)
    color_code       VARCHAR(7),
    is_active        BOOLEAN        NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMPTZ
);
COMMENT ON TABLE  work_shifts               IS 'Ca làm việc: Fixed / Flexible / Shift';
COMMENT ON COLUMN work_shifts.late_threshold  IS 'Phút ân hạn vào muộn — check_in ≤ start+N vẫn Present';
COMMENT ON COLUMN work_shifts.early_threshold IS 'Phút ân hạn về sớm — check_out ≥ end-N vẫn Present';
COMMENT ON COLUMN work_shifts.is_overnight    IS 'Ca đêm: end_time thuộc ngày hôm sau';


-- ============================================================
--  NHÓM B: NHÂN VIÊN & KHUÔN MẶT
-- ============================================================

-- ── B1. employees ─────────────────────────────────────────────
CREATE TABLE employees (
    id                  SERIAL        PRIMARY KEY,
    code                VARCHAR(20)   NOT NULL UNIQUE,
    full_name           VARCHAR(100)  NOT NULL,
    gender              CHAR(1)       CHECK (gender IN ('M','F','O')),
    date_of_birth       DATE,
    phone               VARCHAR(15),
    email               VARCHAR(100)  UNIQUE,
    identity_card       VARCHAR(20)   UNIQUE,     -- CCCD / CMND
    department_id       INT           REFERENCES departments(id) ON DELETE SET NULL,
    position_id         INT           REFERENCES positions(id)   ON DELETE SET NULL,
    default_shift_id    INT           REFERENCES work_shifts(id) ON DELETE SET NULL,
    -- Ca mặc định — fallback khi không có employee_shift_schedule
    manager_id          INT           REFERENCES employees(id)   ON DELETE SET NULL,
    -- Quản lý trực tiếp — dùng routing duyệt đơn nghỉ phép
    hire_date           DATE          NOT NULL DEFAULT CURRENT_DATE,
    termination_date    DATE,
    employment_type     VARCHAR(20)   NOT NULL DEFAULT 'FullTime'
                        CHECK (employment_type IN ('FullTime','PartTime','Contract','Intern')),
    work_location       VARCHAR(100),            -- Tên chi nhánh / văn phòng làm việc
    avatar_path         TEXT,                    -- Ảnh đại diện (khác face_data)
    -- ── Trạng thái Face ID ──
    is_face_registered  BOOLEAN       NOT NULL DEFAULT FALSE,
    -- Trigger tự động cập nhật từ face_data
    face_registered_at  TIMESTAMPTZ,
    -- Thời điểm đăng ký face đầu tiên thành công
    -- ── Nghỉ phép (phạm vi chấm công) ──
    annual_leave_days   DECIMAL(5,1)  NOT NULL DEFAULT 12 CHECK (annual_leave_days >= 0),
    used_leave_days     DECIMAL(5,1)  NOT NULL DEFAULT 0  CHECK (used_leave_days  >= 0),
    -- Trigger tự động cộng/trừ khi leave_request thay đổi status
    is_active           BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ,

    CONSTRAINT chk_emp_dates CHECK (
        termination_date IS NULL OR termination_date >= hire_date
    )
);

ALTER TABLE departments
    ADD CONSTRAINT fk_dept_manager
    FOREIGN KEY (manager_id) REFERENCES employees(id) ON DELETE SET NULL;

COMMENT ON TABLE  employees                   IS 'Thông tin nhân viên';
COMMENT ON COLUMN employees.default_shift_id  IS 'Ca fallback khi không có lịch phân ca';
COMMENT ON COLUMN employees.manager_id        IS 'Quản lý trực tiếp — routing duyệt đơn';
COMMENT ON COLUMN employees.is_face_registered IS 'Trigger tự động từ face_data';
COMMENT ON COLUMN employees.face_registered_at IS 'Lần đầu đăng ký face thành công';


-- ── B2. users ─────────────────────────────────────────────────
CREATE TABLE users (
    id                   SERIAL       PRIMARY KEY,
    username             VARCHAR(50)  NOT NULL UNIQUE,
    password_hash        VARCHAR(255) NOT NULL,
    -- BCrypt hash cost=12; KHÔNG lưu plain text
    employee_id          INT          UNIQUE REFERENCES employees(id) ON DELETE SET NULL,
    -- UNIQUE: 1 nhân viên = tối đa 1 tài khoản
    role                 VARCHAR(20)  NOT NULL DEFAULT 'Employee'
                         CHECK (role IN ('SuperAdmin','Admin','HR','Manager','Employee')),
    -- SuperAdmin : toàn quyền hệ thống + cấu hình
    -- Admin      : quản trị danh mục, thiết bị, ca làm
    -- HR         : xem & chỉnh sửa toàn bộ chấm công, duyệt đơn
    -- Manager    : duyệt đơn nhân viên trong phòng mình
    -- Employee   : xem lịch sử & nộp đơn của bản thân
    is_active            BOOLEAN      NOT NULL DEFAULT TRUE,
    last_login           TIMESTAMPTZ,
    failed_login_count   SMALLINT     NOT NULL DEFAULT 0,
    locked_until         TIMESTAMPTZ,
    -- Khóa tạm thời sau N lần đăng nhập sai (cấu hình qua system_settings)
    refresh_token_hash   VARCHAR(500),
    -- SHA-256 hash của JWT Refresh Token
    refresh_token_expiry TIMESTAMPTZ,
    must_change_password BOOLEAN      NOT NULL DEFAULT FALSE,
    -- TRUE = buộc đổi mật khẩu lần đăng nhập kế tiếp
    created_at           TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           TIMESTAMPTZ
);
COMMENT ON TABLE  users                      IS 'Tài khoản hệ thống — 5 vai trò';
COMMENT ON COLUMN users.refresh_token_hash   IS 'SHA-256(RefreshToken) — không lưu token gốc';
COMMENT ON COLUMN users.must_change_password IS 'Buộc đổi pass sau lần login tiếp theo';


-- ── B3. face_data  (CORE TABLE) ───────────────────────────────
CREATE TABLE face_data (
    id              SERIAL      PRIMARY KEY,
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
    angle           VARCHAR(10) CHECK (angle IN ('Front','Left','Right','Up','Down')),
    -- Góc chụp để tăng độ bao phủ nhận diện
    quality_score   REAL        NOT NULL DEFAULT 0 CHECK (quality_score BETWEEN 0 AND 1),
    -- 0.0 (xấu) → 1.0 (tốt); reject nếu < 0.6 (cấu hình system_settings)
    brightness      REAL        CHECK (brightness BETWEEN 0 AND 255),
    -- Độ sáng trung bình ảnh
    sharpness       REAL        CHECK (sharpness >= 0),
    -- Độ sắc nét (Laplacian variance)
    face_bbox       JSONB,
    -- Bounding box: {"x":10,"y":20,"w":100,"h":100}

    -- ── Trạng thái & kiểm duyệt ──
    is_active       BOOLEAN     NOT NULL DEFAULT TRUE,
    -- FALSE = vô hiệu hóa (không dùng nhận diện) nhưng giữ lịch sử
    is_verified     BOOLEAN     NOT NULL DEFAULT FALSE,
    -- TRUE = HR/Admin đã xem xét & xác nhận chất lượng
    verified_by     INT         REFERENCES users(id) ON DELETE SET NULL,
    verified_at     TIMESTAMPTZ,
    registered_by   INT         REFERENCES users(id) ON DELETE SET NULL,
    -- Admin tự đăng ký hoặc NV tự chụp qua kiosk
    note            TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ,

    CONSTRAINT uq_face_emp_index UNIQUE (employee_id, image_index)
    -- Mỗi slot (1-5) chỉ có 1 ảnh
);
COMMENT ON TABLE  face_data              IS '★ CORE — Face encoding 128-D cho nhận diện realtime';
COMMENT ON COLUMN face_data.encoding     IS 'float32[128] → byte[512]; dlib / FaceRecognitionDotNet';
COMMENT ON COLUMN face_data.image_index  IS 'Slot 1-5 (tối đa 5 góc/NV)';
COMMENT ON COLUMN face_data.quality_score IS '0→1; reject khi < threshold (system_settings)';
COMMENT ON COLUMN face_data.angle        IS 'Front/Left/Right/Up/Down';
COMMENT ON COLUMN face_data.face_bbox    IS 'JSON {x,y,w,h} — bounding box khuôn mặt trong ảnh';
COMMENT ON COLUMN face_data.is_verified  IS 'HR xác nhận chất lượng ảnh';


-- ── B4. face_registration_logs ────────────────────────────────
-- Ghi lại toàn bộ sự kiện đăng ký / cập nhật / xóa khuôn mặt
CREATE TABLE face_registration_logs (
    id              BIGSERIAL   PRIMARY KEY,
    employee_id     INT         NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    face_data_id    INT         REFERENCES face_data(id) ON DELETE SET NULL,
    action          VARCHAR(20) NOT NULL
                    CHECK (action IN ('Register','Update','Delete','Verify','Deactivate')),
    -- Register   = đăng ký mới
    -- Update     = chụp lại ảnh cho slot đã có
    -- Delete     = xóa vĩnh viễn
    -- Verify     = HR xác nhận chất lượng
    -- Deactivate = vô hiệu hóa (is_active = FALSE)
    image_index     SMALLINT,
    quality_score   REAL,
    -- Snapshot điểm chất lượng tại thời điểm thao tác
    performed_by    INT         REFERENCES users(id) ON DELETE SET NULL,
    reason          TEXT,
    -- Lý do (bắt buộc khi Delete / Deactivate)
    ip_address      VARCHAR(45),
    device_info     TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);
COMMENT ON TABLE face_registration_logs IS 'Audit trail mọi thao tác trên face_data';
COMMENT ON COLUMN face_registration_logs.action IS 'Register/Update/Delete/Verify/Deactivate';


-- ============================================================
--  NHÓM C: THIẾT BỊ & LỊCH LÀM VIỆC
-- ============================================================

-- ── C1. attendance_devices ────────────────────────────────────
CREATE TABLE attendance_devices (
    id              SERIAL       PRIMARY KEY,
    device_code     VARCHAR(50)  NOT NULL UNIQUE,
    device_name     VARCHAR(100) NOT NULL,
    device_type     VARCHAR(20)  NOT NULL DEFAULT 'Camera'
                    CHECK (device_type IN ('Camera','Tablet','Kiosk','Mobile')),
    -- Camera  = camera IP cố định tại cổng/phòng
    -- Tablet  = tablet / kiosk cảm ứng
    -- Kiosk   = máy chuyên dụng chấm công
    -- Mobile  = ứng dụng di động (geofencing)
    location_name   VARCHAR(150),
    -- "Cổng chính - Tầng 1", "Phòng IT - Tầng 3"
    ip_address      VARCHAR(45),
    -- Whitelist IP — chống giả mạo thiết bị nội bộ
    mac_address     VARCHAR(17),
    -- Whitelist MAC
    -- ── GPS / Geofencing (dành cho Mobile) ──
    latitude        DECIMAL(10,8),
    longitude       DECIMAL(11,8),
    radius_meters   INT          DEFAULT 100 CHECK (radius_meters > 0),
    -- Bán kính cho phép check-in (Mobile geofencing)
    -- ── Cấu hình nhận diện ──
    min_confidence  REAL         NOT NULL DEFAULT 0.70 CHECK (min_confidence BETWEEN 0 AND 1),
    -- Override ngưỡng confidence tại thiết bị này
    -- (mặc định lấy từ system_settings nếu NULL)
    camera_url      TEXT,
    -- RTSP URL hoặc HTTP snapshot URL (Camera IP)
    is_online       BOOLEAN      NOT NULL DEFAULT FALSE,
    -- Cập nhật bởi heartbeat job
    last_heartbeat  TIMESTAMPTZ,
    is_active       BOOLEAN      NOT NULL DEFAULT TRUE,
    note            TEXT,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ
);
COMMENT ON TABLE  attendance_devices             IS 'Thiết bị chấm công: Camera/Tablet/Kiosk/Mobile';
COMMENT ON COLUMN attendance_devices.ip_address  IS 'Whitelist IP chống giả mạo';
COMMENT ON COLUMN attendance_devices.min_confidence IS 'Ngưỡng confidence thiết bị (override system_settings)';
COMMENT ON COLUMN attendance_devices.camera_url  IS 'RTSP/HTTP URL stream camera';
COMMENT ON COLUMN attendance_devices.is_online   IS 'Cập nhật bởi heartbeat background job';


-- ── C2. holidays ──────────────────────────────────────────────
CREATE TABLE holidays (
    id              SERIAL       PRIMARY KEY,
    holiday_date    DATE         NOT NULL,
    name            VARCHAR(100) NOT NULL,
    holiday_type    VARCHAR(20)  NOT NULL DEFAULT 'National'
                    CHECK (holiday_type IN ('National','Company','Compensatory')),
    -- National     = Ngày lễ quốc gia (theo luật Việt Nam)
    -- Company      = Nghỉ riêng của công ty
    -- Compensatory = Nghỉ bù khi lễ trùng cuối tuần
    description     TEXT,
    is_recurring    BOOLEAN      NOT NULL DEFAULT FALSE,
    -- TRUE = tự tạo bản ghi năm mới (background job đầu năm)
    year            SMALLINT     NOT NULL DEFAULT EXTRACT(YEAR FROM CURRENT_DATE)::SMALLINT,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_holiday_date_year UNIQUE (holiday_date, year)
);
COMMENT ON COLUMN holidays.is_recurring IS 'TRUE → background job tự clone sang năm mới';


-- ── C3. employee_shift_schedule ───────────────────────────────
CREATE TABLE employee_shift_schedule (
    id          SERIAL      PRIMARY KEY,
    employee_id INT         NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    shift_id    INT         NOT NULL REFERENCES work_shifts(id) ON DELETE RESTRICT,
    work_date   DATE        NOT NULL,
    is_day_off  BOOLEAN     NOT NULL DEFAULT FALSE,
    -- TRUE = ngày nghỉ theo lịch phân công (ROT / ngày nghỉ bù riêng)
    note        TEXT,
    created_by  INT         REFERENCES users(id) ON DELETE SET NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_schedule_emp_date UNIQUE (employee_id, work_date)
    -- 1 NV chỉ có 1 lịch/ngày
);
COMMENT ON TABLE  employee_shift_schedule          IS 'Lịch phân ca chi tiết — ghi đè default_shift_id';
COMMENT ON COLUMN employee_shift_schedule.is_day_off IS 'Ngày nghỉ theo lịch phân công';


-- ── C4. work_calendars ────────────────────────────────────────
CREATE TABLE work_calendars (
    id             SERIAL       PRIMARY KEY,
    name           VARCHAR(100) NOT NULL,
    -- "Hành chính T2-T6", "Vận hành T2-T7", "Nhà máy 3 ca 7 ngày"
    monday         BOOLEAN      NOT NULL DEFAULT TRUE,
    tuesday        BOOLEAN      NOT NULL DEFAULT TRUE,
    wednesday      BOOLEAN      NOT NULL DEFAULT TRUE,
    thursday       BOOLEAN      NOT NULL DEFAULT TRUE,
    friday         BOOLEAN      NOT NULL DEFAULT TRUE,
    saturday       BOOLEAN      NOT NULL DEFAULT FALSE,
    sunday         BOOLEAN      NOT NULL DEFAULT FALSE,
    effective_from DATE         NOT NULL DEFAULT CURRENT_DATE,
    effective_to   DATE,        -- NULL = vô thời hạn
    is_default     BOOLEAN      NOT NULL DEFAULT FALSE,
    -- Trigger đảm bảo chỉ 1 bản ghi is_default=TRUE
    description    TEXT,
    created_at     TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMPTZ
);
COMMENT ON COLUMN work_calendars.is_default IS 'Trigger giữ duy nhất 1 bản ghi TRUE';


-- ============================================================
--  NHÓM D: CHẤM CÔNG
-- ============================================================

-- ── D1. attendance_records ────────────────────────────────────
-- Bảng trung tâm: mỗi nhân viên tối đa 1 bản ghi / ngày
CREATE TABLE attendance_records (
    id                   BIGSERIAL    PRIMARY KEY,
    employee_id          INT          NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    attendance_date      DATE         NOT NULL DEFAULT CURRENT_DATE,
    shift_id             INT          REFERENCES work_shifts(id) ON DELETE SET NULL,
    -- Ca áp dụng hôm đó (business layer: lấy từ schedule → default_shift)

    -- ── Dữ liệu Check-In ──
    check_in             TIMESTAMPTZ,
    check_in_device_id   INT          REFERENCES attendance_devices(id) ON DELETE SET NULL,
    check_in_image_path  TEXT,        -- Ảnh chụp lúc check-in (lưu để xem lại)
    check_in_method      VARCHAR(20)  DEFAULT 'Face'
                         CHECK (check_in_method IN ('Face','Manual','QRCode','NFC','Mobile')),
    check_in_confidence  REAL         CHECK (check_in_confidence  IS NULL OR check_in_confidence  BETWEEN 0 AND 1),
    -- Độ tin cậy khuôn mặt: 0.0 → 1.0
    check_in_latitude    DECIMAL(10,8),
    check_in_longitude   DECIMAL(11,8),
    -- GPS tọa độ lúc check-in (Mobile)

    -- ── Dữ liệu Check-Out ──
    check_out            TIMESTAMPTZ,
    check_out_device_id  INT          REFERENCES attendance_devices(id) ON DELETE SET NULL,
    check_out_image_path TEXT,
    check_out_method     VARCHAR(20)
                         CHECK (check_out_method IN ('Face','Manual','QRCode','NFC','Mobile')),
    check_out_confidence REAL         CHECK (check_out_confidence IS NULL OR check_out_confidence BETWEEN 0 AND 1),
    check_out_latitude   DECIMAL(10,8),
    check_out_longitude  DECIMAL(11,8),

    -- ── Kết quả tính toán (Business Layer → lưu vào DB) ──
    status               VARCHAR(20)  NOT NULL DEFAULT 'NotYet'
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
    is_manual_edit       BOOLEAN      NOT NULL DEFAULT FALSE,
    manual_edit_by       INT          REFERENCES users(id) ON DELETE SET NULL,
    manual_edit_at       TIMESTAMPTZ,
    manual_edit_reason   TEXT,
    -- Bắt buộc khi is_manual_edit = TRUE (check tại app / trigger)
    note                 TEXT,

    created_at           TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           TIMESTAMPTZ,

    CONSTRAINT uq_attendance_emp_date UNIQUE (employee_id, attendance_date),
    CONSTRAINT chk_checkout_after_checkin CHECK (
        check_out IS NULL OR check_in IS NULL OR check_out >= check_in
    ),
    CONSTRAINT chk_manual_reason CHECK (
        is_manual_edit = FALSE
        OR (is_manual_edit = TRUE AND manual_edit_reason IS NOT NULL)
    )
);
COMMENT ON TABLE  attendance_records                   IS '★ CORE — Chấm công chính: 1 dòng/NV/ngày';
COMMENT ON COLUMN attendance_records.status            IS '8 trạng thái: Present/Late/EarlyLeave/LateAndEarly/Absent/Leave/Holiday/DayOff/NotYet';
COMMENT ON COLUMN attendance_records.working_minutes   IS '(check_out - check_in) - break — lưu phút tránh sai số';
COMMENT ON COLUMN attendance_records.check_in_confidence IS 'Face confidence 0→1; < threshold → Suspicious';
COMMENT ON COLUMN attendance_records.is_manual_edit    IS 'TRUE → bắt buộc có manual_edit_reason';


-- ── D2. attendance_logs ───────────────────────────────────────
-- Nhật ký từng lần camera nhận diện / quẹt thẻ
-- Không thể DELETE / UPDATE — append-only audit trail
CREATE TABLE attendance_logs (
    id              BIGSERIAL    PRIMARY KEY,
    attendance_id   BIGINT       REFERENCES attendance_records(id) ON DELETE SET NULL,
    -- NULL = chưa map được vào bản ghi (NV chưa đăng ký face, nhận diện thất bại)
    employee_id     INT          REFERENCES employees(id)          ON DELETE SET NULL,
    device_id       INT          REFERENCES attendance_devices(id) ON DELETE SET NULL,

    log_time        TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    log_type        VARCHAR(20)  NOT NULL
                    CHECK (log_type IN ('CheckIn','CheckOut','Unknown')),
    method          VARCHAR(20)  NOT NULL DEFAULT 'Face'
                    CHECK (method IN ('Face','Manual','QRCode','NFC','Mobile')),

    -- ── Kết quả nhận diện khuôn mặt ──
    matched_face_id  INT         REFERENCES face_data(id) ON DELETE SET NULL,
    -- Face slot nào đã match
    confidence       REAL        CHECK (confidence IS NULL OR confidence BETWEEN 0 AND 1),
    face_distance    REAL        CHECK (face_distance IS NULL OR face_distance >= 0),
    -- Khoảng cách Euclidean: nhỏ hơn = giống hơn (< 0.4 = rất giống)
    image_path       TEXT,       -- Ảnh chụp tại thời điểm nhận diện (evidence)

    -- ── Vị trí ──
    latitude        DECIMAL(10,8),
    longitude       DECIMAL(11,8),
    ip_address      VARCHAR(45),

    -- ── Kết quả xử lý ──
    result          VARCHAR(20)  NOT NULL DEFAULT 'Success'
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
    raw_payload     JSONB,
    -- Dữ liệu thô từ thiết bị gửi lên (debug, không dùng business logic)
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP
    -- KHÔNG có updated_at — append-only
);
COMMENT ON TABLE  attendance_logs               IS '★ CORE — Mọi lần nhận diện: audit trail & anti-fraud';
COMMENT ON COLUMN attendance_logs.matched_face_id IS 'Slot face đã match (1-5)';
COMMENT ON COLUMN attendance_logs.confidence    IS 'Face recognition confidence 0→1';
COMMENT ON COLUMN attendance_logs.face_distance IS 'Euclidean distance; < 0.4 = rất giống';
COMMENT ON COLUMN attendance_logs.result        IS 'Success/Failed/Suspicious/Duplicate/Spoofing/DeviceError';
COMMENT ON COLUMN attendance_logs.raw_payload   IS 'JSONB từ thiết bị — debug only';


-- ── D3. leave_requests ────────────────────────────────────────
-- Đơn nghỉ phép — ảnh hưởng trực tiếp đến status chấm công
CREATE TABLE leave_requests (
    id              SERIAL       PRIMARY KEY,
    employee_id     INT          NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    leave_type      VARCHAR(20)  NOT NULL
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
    start_date      DATE         NOT NULL,
    end_date        DATE         NOT NULL,
    total_days      DECIMAL(5,1) NOT NULL CHECK (total_days > 0),
    -- 0.5 = bán ngày; business layer tính (loại trừ T7, CN, ngày lễ)
    is_half_day     BOOLEAN      NOT NULL DEFAULT FALSE,
    half_day_period VARCHAR(10)  CHECK (half_day_period IN ('Morning','Afternoon')),
    reason          TEXT         NOT NULL,
    document_path   TEXT,
    -- File đính kèm: giấy nghỉ ốm, giấy kết hôn... (PDF/JPG)
    status          VARCHAR(20)  NOT NULL DEFAULT 'Pending'
                    CHECK (status IN ('Pending','Approved','Rejected','Cancelled')),
    approved_by     INT          REFERENCES employees(id) ON DELETE SET NULL,
    approved_at     TIMESTAMPTZ,
    reject_reason   TEXT,
    note            TEXT,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ,

    CONSTRAINT chk_leave_dates   CHECK (end_date >= start_date),
    CONSTRAINT chk_half_day      CHECK (
        is_half_day = FALSE
        OR (start_date = end_date AND half_day_period IS NOT NULL)
    ),
    CONSTRAINT chk_leave_approved CHECK (
        status != 'Approved'
        OR (approved_by IS NOT NULL AND approved_at IS NOT NULL)
    )
);
COMMENT ON TABLE  leave_requests             IS 'Đơn nghỉ phép — khi Approved, chấm công tự chuyển sang Leave';
COMMENT ON COLUMN leave_requests.total_days  IS '0.5 = bán ngày; app tính loại trừ T7/CN/Lễ';
COMMENT ON COLUMN leave_requests.document_path IS 'File đính kèm giấy tờ minh chứng';


-- ============================================================
--  NHÓM E: HỆ THỐNG & KIỂM TOÁN
-- ============================================================

-- ── E1. audit_logs ────────────────────────────────────────────
-- Append-only — KHÔNG bao giờ UPDATE / DELETE
CREATE TABLE audit_logs (
    id          BIGSERIAL    PRIMARY KEY,
    user_id     INT          REFERENCES users(id)     ON DELETE SET NULL,
    employee_id INT          REFERENCES employees(id) ON DELETE SET NULL,
    action      VARCHAR(50)  NOT NULL,
    -- LOGIN, LOGOUT, CREATE, UPDATE, DELETE, APPROVE, REJECT,
    -- FACE_REGISTER, FACE_DELETE, ATTENDANCE_EDIT, EXPORT ...
    table_name  VARCHAR(50),
    record_id   VARCHAR(50),  -- ID bản ghi bị tác động (TEXT để linh hoạt)
    old_values  JSONB,         -- Snapshot trước thay đổi
    new_values  JSONB,         -- Snapshot sau thay đổi
    ip_address  VARCHAR(45),
    user_agent  TEXT,
    description TEXT,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP
);
COMMENT ON TABLE  audit_logs           IS 'Append-only audit trail — không UPDATE/DELETE';
COMMENT ON COLUMN audit_logs.old_values IS 'JSONB snapshot trước khi thay đổi';
COMMENT ON COLUMN audit_logs.new_values IS 'JSONB snapshot sau khi thay đổi';


-- ── E2. system_settings ───────────────────────────────────────
CREATE TABLE system_settings (
    id          SERIAL       PRIMARY KEY,
    key         VARCHAR(100) NOT NULL UNIQUE,
    value       TEXT         NOT NULL,
    value_type  VARCHAR(20)  NOT NULL DEFAULT 'String'
                CHECK (value_type IN ('String','Integer','Decimal','Boolean','Json')),
    category    VARCHAR(50)  NOT NULL DEFAULT 'General'
                CHECK (category IN ('General','FaceRecognition','Attendance','Security','Notification')),
    description TEXT,
    is_editable BOOLEAN      NOT NULL DEFAULT TRUE,
    updated_by  INT          REFERENCES users(id) ON DELETE SET NULL,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMPTZ
);
COMMENT ON TABLE  system_settings IS 'Key-value config — đồng bộ IOptions<T> phía .NET';


-- ============================================================
--  INDEXES
-- ============================================================

-- employees
CREATE INDEX idx_emp_dept      ON employees(department_id);
CREATE INDEX idx_emp_position  ON employees(position_id);
CREATE INDEX idx_emp_shift     ON employees(default_shift_id);
CREATE INDEX idx_emp_manager   ON employees(manager_id);
CREATE INDEX idx_emp_active    ON employees(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_emp_face      ON employees(is_face_registered, is_active);

-- users
CREATE INDEX idx_usr_employee  ON users(employee_id);
CREATE INDEX idx_usr_role      ON users(role);

-- face_data  ← hot table (load vào memory cache)
CREATE INDEX idx_face_emp      ON face_data(employee_id, is_active) WHERE is_active = TRUE;
CREATE INDEX idx_face_verified ON face_data(is_verified) WHERE is_verified = FALSE;

-- face_registration_logs
CREATE INDEX idx_frlog_emp     ON face_registration_logs(employee_id, created_at DESC);
CREATE INDEX idx_frlog_action  ON face_registration_logs(action, created_at DESC);

-- attendance_devices
CREATE INDEX idx_dev_type      ON attendance_devices(device_type, is_active);
CREATE INDEX idx_dev_online    ON attendance_devices(is_online) WHERE is_online = TRUE;

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
CREATE INDEX idx_att_manual    ON attendance_records(is_manual_edit) WHERE is_manual_edit = TRUE;
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
CREATE OR REPLACE FUNCTION fn_set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;

DO $$ DECLARE tbl TEXT; BEGIN
    FOREACH tbl IN ARRAY ARRAY[
        'departments','positions','work_shifts',
        'employees','users','face_data','attendance_devices',
        'work_calendars','attendance_records',
        'leave_requests','system_settings'
    ] LOOP
        EXECUTE format(
            'CREATE TRIGGER trg_%s_upd
             BEFORE UPDATE ON %s
             FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at()',
            tbl, tbl
        );
    END LOOP;
END; $$;


-- T2. Đồng bộ employees.is_face_registered & face_registered_at
CREATE OR REPLACE FUNCTION fn_sync_face_status()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    v_emp INT;
    v_has BOOLEAN;
    v_first TIMESTAMPTZ;
BEGIN
    v_emp := COALESCE(NEW.employee_id, OLD.employee_id);

    SELECT
        EXISTS (SELECT 1 FROM face_data WHERE employee_id = v_emp AND is_active = TRUE),
        MIN(created_at)
    INTO v_has, v_first
    FROM face_data WHERE employee_id = v_emp AND is_active = TRUE;

    UPDATE employees
    SET  is_face_registered = v_has,
         face_registered_at = CASE WHEN v_has THEN COALESCE(face_registered_at, v_first) ELSE NULL END
    WHERE id = v_emp;

    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE TRIGGER trg_face_data_sync
AFTER INSERT OR UPDATE OF is_active OR DELETE ON face_data
FOR EACH ROW EXECUTE FUNCTION fn_sync_face_status();


-- T3. Đồng bộ employees.used_leave_days khi leave_request thay đổi status
CREATE OR REPLACE FUNCTION fn_sync_leave_days()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    -- Cộng khi chuyển → Approved (chỉ loại có nghỉ thực sự, không phải WFH)
    IF OLD.status != 'Approved' AND NEW.status = 'Approved'
       AND NEW.leave_type NOT IN ('WFH') THEN
        UPDATE employees
        SET used_leave_days = used_leave_days + NEW.total_days
        WHERE id = NEW.employee_id;

    -- Hoàn lại khi hủy từ Approved
    ELSIF OLD.status = 'Approved' AND NEW.status IN ('Cancelled','Rejected')
       AND NEW.leave_type NOT IN ('WFH') THEN
        UPDATE employees
        SET used_leave_days = GREATEST(0, used_leave_days - OLD.total_days)
        WHERE id = NEW.employee_id;
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_leave_sync_days
AFTER UPDATE OF status ON leave_requests
FOR EACH ROW EXECUTE FUNCTION fn_sync_leave_days();


-- T4. Chỉ 1 work_calendar is_default = TRUE
CREATE OR REPLACE FUNCTION fn_single_default_calendar()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.is_default THEN
        UPDATE work_calendars SET is_default = FALSE
        WHERE id != NEW.id AND is_default = TRUE;
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_calendar_default
BEFORE INSERT OR UPDATE OF is_default ON work_calendars
FOR EACH ROW EXECUTE FUNCTION fn_single_default_calendar();


-- T5. Tự động ghi face_registration_logs khi face_data thay đổi
CREATE OR REPLACE FUNCTION fn_log_face_change()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO face_registration_logs
            (employee_id, face_data_id, action, image_index, quality_score, performed_by)
        VALUES
            (NEW.employee_id, NEW.id, 'Register', NEW.image_index, NEW.quality_score, NEW.registered_by);

    ELSIF TG_OP = 'UPDATE' THEN
        IF OLD.is_active = TRUE AND NEW.is_active = FALSE THEN
            INSERT INTO face_registration_logs
                (employee_id, face_data_id, action, image_index, quality_score, performed_by)
            VALUES
                (NEW.employee_id, NEW.id, 'Deactivate', NEW.image_index, NEW.quality_score, NEW.registered_by);
        ELSIF OLD.is_verified = FALSE AND NEW.is_verified = TRUE THEN
            INSERT INTO face_registration_logs
                (employee_id, face_data_id, action, image_index, quality_score, performed_by)
            VALUES
                (NEW.employee_id, NEW.id, 'Verify', NEW.image_index, NEW.quality_score, NEW.verified_by);
        ELSIF OLD.encoding != NEW.encoding THEN
            INSERT INTO face_registration_logs
                (employee_id, face_data_id, action, image_index, quality_score, performed_by)
            VALUES
                (NEW.employee_id, NEW.id, 'Update', NEW.image_index, NEW.quality_score, NEW.registered_by);
        END IF;

    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO face_registration_logs
            (employee_id, face_data_id, action, image_index, quality_score)
        VALUES
            (OLD.employee_id, OLD.id, 'Delete', OLD.image_index, OLD.quality_score);
    END IF;
    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE TRIGGER trg_face_audit
AFTER INSERT OR UPDATE OR DELETE ON face_data
FOR EACH ROW EXECUTE FUNCTION fn_log_face_change();


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
    ROUND((a.working_minutes / 60.0)::numeric, 2)     AS working_hours,
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
WHERE e.is_active = TRUE;
COMMENT ON VIEW v_today_attendance IS 'Dashboard hôm nay: COALESCE ca theo attendance→schedule→default';


-- V2. Tổng hợp chấm công theo tháng
CREATE VIEW v_monthly_summary AS
SELECT
    e.id                                                    AS employee_id,
    e.code, e.full_name,
    d.name                                                  AS department_name,
    DATE_TRUNC('month', a.attendance_date)                  AS month,
    COUNT(*)                                                AS total_records,
    COUNT(*) FILTER (WHERE a.status = 'Present')            AS present_days,
    COUNT(*) FILTER (WHERE a.status = 'Late')               AS late_days,
    COUNT(*) FILTER (WHERE a.status = 'EarlyLeave')         AS early_leave_days,
    COUNT(*) FILTER (WHERE a.status = 'LateAndEarly')       AS late_and_early_days,
    COUNT(*) FILTER (WHERE a.status = 'Absent')             AS absent_days,
    COUNT(*) FILTER (WHERE a.status = 'Leave')              AS leave_days,
    COUNT(*) FILTER (WHERE a.status = 'Holiday')            AS holiday_days,
    COUNT(*) FILTER (WHERE a.status = 'DayOff')             AS day_off_days,
    -- Ngày công hợp lệ = có mặt + trễ/sớm + nghỉ phép
    COUNT(*) FILTER (WHERE a.status IN (
        'Present','Late','EarlyLeave','LateAndEarly','Leave'
    ))                                                      AS actual_work_days,
    SUM(a.late_minutes)                                     AS total_late_minutes,
    SUM(a.early_minutes)                                    AS total_early_minutes,
    ROUND((SUM(a.working_minutes) / 60.0)::numeric, 2)      AS total_working_hours,
    COUNT(*) FILTER (WHERE a.is_manual_edit = TRUE)         AS manual_edit_count
FROM employees e
JOIN attendance_records a ON e.id = a.employee_id
LEFT JOIN departments d   ON e.department_id = d.id
WHERE e.is_active = TRUE
GROUP BY e.id, e.code, e.full_name, d.name,
         DATE_TRUNC('month', a.attendance_date);
COMMENT ON VIEW v_monthly_summary IS 'Tổng hợp tháng — nguồn báo cáo chấm công';


-- V3. Nhân viên chưa đăng ký / cần cập nhật Face ID
CREATE VIEW v_face_status AS
SELECT
    e.id, e.code, e.full_name,
    d.name                                          AS department_name,
    e.is_face_registered,
    e.face_registered_at,
    COUNT(fd.id)                                    AS total_faces,
    COUNT(fd.id) FILTER (WHERE fd.is_active = TRUE) AS active_faces,
    COUNT(fd.id) FILTER (WHERE fd.is_verified = TRUE AND fd.is_active = TRUE) AS verified_faces,
    ROUND((AVG(fd.quality_score) FILTER (WHERE fd.is_active = TRUE))::numeric, 3) AS avg_quality,
    MIN(fd.quality_score) FILTER (WHERE fd.is_active = TRUE)           AS min_quality
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
LEFT JOIN face_data fd  ON e.id = fd.employee_id
WHERE e.is_active = TRUE
GROUP BY e.id, e.code, e.full_name, d.name,
         e.is_face_registered, e.face_registered_at;
COMMENT ON VIEW v_face_status IS 'Trạng thái Face ID: số ảnh, chất lượng, xác nhận';


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
    ROUND((a.working_minutes / 60.0)::numeric, 2) AS working_hours,
    a.check_in_confidence,
    a.check_out_confidence,
    CASE
        WHEN a.check_in_confidence  < 0.70  THEN 'Check-in confidence thấp < 0.70'
        WHEN a.check_out_confidence < 0.70  THEN 'Check-out confidence thấp < 0.70'
        WHEN a.late_minutes  > 60           THEN 'Đi muộn > 60 phút'
        WHEN a.early_minutes > 60           THEN 'Về sớm > 60 phút'
        WHEN a.check_out IS NULL
             AND a.check_in IS NOT NULL
             AND CURRENT_TIMESTAMP > (a.attendance_date + INTERVAL '1 day')
                                            THEN 'Quên check-out'
        WHEN a.working_minutes < 240 AND a.status = 'Present'
                                            THEN 'Giờ làm < 4h nhưng status Present'
        WHEN a.is_manual_edit = TRUE        THEN 'Điều chỉnh thủ công'
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
     AND CURRENT_TIMESTAMP > (a.attendance_date + INTERVAL '1 day')) OR
    (a.working_minutes < 240 AND a.status = 'Present') OR
    a.is_manual_edit = TRUE;
COMMENT ON VIEW v_attendance_anomalies IS 'Bất thường cần HR xem xét';


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
    lr.document_path IS NOT NULL AS has_document,
    lr.created_at
FROM leave_requests lr
JOIN employees e    ON lr.employee_id = e.id
LEFT JOIN departments d   ON e.department_id = d.id
LEFT JOIN employees mgr   ON e.manager_id    = mgr.id
WHERE lr.status = 'Pending'
ORDER BY lr.created_at;
COMMENT ON VIEW v_pending_leaves IS 'Đơn nghỉ phép chờ duyệt';


-- V6. Số dư nghỉ phép
CREATE VIEW v_leave_balance AS
SELECT
    e.id, e.code, e.full_name,
    d.name                                   AS department_name,
    e.annual_leave_days,
    e.used_leave_days,
    e.annual_leave_days - e.used_leave_days  AS remaining_days,
    EXTRACT(YEAR FROM CURRENT_DATE)::INT     AS year
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
WHERE e.is_active = TRUE;
COMMENT ON VIEW v_leave_balance IS 'Số dư phép năm của nhân viên';


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
COMMENT ON VIEW v_suspicious_recognition IS 'Các lần nhận diện đáng ngờ — phát hiện gian lận';


-- ============================================================
--  DỮ LIỆU HỆ THỐNG
-- ============================================================

INSERT INTO system_settings (key, value, value_type, category, description) VALUES
-- Face Recognition
('face.confidence_threshold',        '0.70',  'Decimal', 'FaceRecognition', 'Ngưỡng confidence tối thiểu chấp nhận nhận diện (0-1)'),
('face.max_slots_per_employee',      '5',     'Integer', 'FaceRecognition', 'Số ảnh tối đa mỗi nhân viên (slot 1-5)'),
('face.min_quality_score',           '0.60',  'Decimal', 'FaceRecognition', 'Điểm chất lượng ảnh tối thiểu khi đăng ký'),
('face.max_distance',                '0.40',  'Decimal', 'FaceRecognition', 'Khoảng cách Euclidean tối đa (< giá trị này = khớp)'),
('face.anti_spoofing_enabled',       'true',  'Boolean', 'FaceRecognition', 'Bật liveness detection chống ảnh giả/video playback'),
('face.duplicate_window_minutes',    '5',     'Integer', 'FaceRecognition', 'Khoảng thời gian chặn check-in trùng (phút)'),
('face.require_verification',        'true',  'Boolean', 'FaceRecognition', 'Bắt buộc HR xác nhận ảnh trước khi đưa vào nhận diện'),
('face.min_face_size_pixels',        '80',    'Integer', 'FaceRecognition', 'Kích thước khuôn mặt tối thiểu trong ảnh (pixel)'),
-- Attendance
('attendance.auto_absent_hour',      '22',    'Integer', 'Attendance',      'Giờ tự động đánh Absent nếu chưa check-in (22:00)'),
('attendance.allow_mobile_checkin',  'true',  'Boolean', 'Attendance',      'Cho phép chấm công qua mobile app'),
('attendance.geofence_enabled',      'true',  'Boolean', 'Attendance',      'Kiểm tra GPS khi check-in Mobile'),
('attendance.manual_edit_notify',    'true',  'Boolean', 'Attendance',      'Thông báo nhân viên khi HR sửa chấm công'),
-- Security
('security.max_login_attempts',      '5',     'Integer', 'Security',        'Số lần đăng nhập sai trước khi khóa tài khoản'),
('security.lockout_minutes',         '30',    'Integer', 'Security',        'Thời gian khóa tài khoản (phút)'),
('security.jwt_expire_minutes',      '60',    'Integer', 'Security',        'Thời gian hết hạn JWT Access Token (phút)'),
('security.refresh_token_days',      '7',     'Integer', 'Security',        'Thời gian hết hạn Refresh Token (ngày)'),
('security.password_min_length',     '8',     'Integer', 'Security',        'Độ dài mật khẩu tối thiểu'),
-- Notification
('notification.email_enabled',       'true',  'Boolean', 'Notification',    'Gửi email thông báo'),
('notification.late_alert_enabled',  'true',  'Boolean', 'Notification',    'Cảnh báo đi muộn qua email/push'),
('notification.approval_notify',     'true',  'Boolean', 'Notification',    'Thông báo kết quả duyệt đơn');


-- ============================================================
--  DỮ LIỆU MẪU (Seed)
-- ============================================================

-- Phòng ban
INSERT INTO departments (code, name, sort_order) VALUES
('BOD',   'Ban Giám đốc',      1),
('HR',    'Phòng Nhân sự',     2),
('IT',    'Phòng Công nghệ',   3),
('SALES', 'Phòng Kinh doanh',  4),
('ACCT',  'Phòng Kế toán',     5);

-- Chức vụ
INSERT INTO positions (code, name, level) VALUES
('CEO',  'Giám đốc điều hành', 10),
('DIR',  'Trưởng phòng',        7),
('LEAD', 'Trưởng nhóm',         5),
('SR',   'Nhân viên cao cấp',   4),
('JR',   'Nhân viên',           3),
('INT',  'Thực tập sinh',       1);

-- Ca làm việc
INSERT INTO work_shifts
    (code, name, shift_type, start_time, end_time,
     break_minutes, standard_hours, late_threshold, early_threshold, is_overnight, color_code)
VALUES
('MAIN',  'Ca hành chính', 'Fixed',    '08:00', '17:00', 60, 8.0, 15, 15, FALSE, '#4A90D9'),
('MORN',  'Ca sáng',       'Shift',    '06:00', '14:00', 30, 8.0, 10, 10, FALSE, '#F5A623'),
('AFT',   'Ca chiều',      'Shift',    '14:00', '22:00', 30, 8.0, 10, 10, FALSE, '#7ED321'),
('NIGHT', 'Ca đêm',        'Shift',    '22:00', '06:00', 30, 8.0, 10, 10, TRUE,  '#9B59B6'),
('FLEX',  'Ca linh hoạt',  'Flexible', '07:00', '19:00', 60, 8.0,  0,  0, FALSE, '#1ABC9C');

-- Lịch làm tuần
INSERT INTO work_calendars (name, saturday, sunday, is_default) VALUES
('Hành chính T2-T6', FALSE, FALSE, TRUE),
('Vận hành T2-T7',   TRUE,  FALSE, FALSE),
('Sản xuất 7 ngày',  TRUE,  TRUE,  FALSE);

-- Ngày lễ 2026
INSERT INTO holidays (holiday_date, name, holiday_type, is_recurring, year) VALUES
('2026-01-01', 'Tết Dương lịch',             'National', TRUE,  2026),
('2026-02-17', 'Nghỉ Tết Nguyên đán (bù)',   'National', FALSE, 2026),
('2026-02-18', 'Tết Nguyên đán – Mùng 1',   'National', FALSE, 2026),
('2026-02-19', 'Tết Nguyên đán – Mùng 2',   'National', FALSE, 2026),
('2026-02-20', 'Tết Nguyên đán – Mùng 3',   'National', FALSE, 2026),
('2026-04-06', 'Giỗ Tổ Hùng Vương',          'National', FALSE, 2026),
('2026-04-30', 'Ngày Giải phóng miền Nam',   'National', TRUE,  2026),
('2026-05-01', 'Quốc tế Lao động',           'National', TRUE,  2026),
('2026-09-02', 'Quốc khánh',                 'National', TRUE,  2026);

-- Nhân viên mẫu
INSERT INTO employees
    (code, full_name, gender, phone, email, department_id, position_id,
     default_shift_id, employment_type, hire_date, annual_leave_days)
VALUES
('NV001', 'Nguyễn Văn An',   'M', '0901234561', 'an@company.com',     3, 2, 1, 'FullTime', '2022-06-01', 14),
('NV002', 'Trần Thị Bình',   'F', '0901234562', 'binh@company.com',   2, 2, 1, 'FullTime', '2021-01-10', 14),
('NV003', 'Lê Văn Cường',    'M', '0901234563', 'cuong@company.com',  3, 5, 1, 'FullTime', '2023-03-15', 12),
('NV004', 'Phạm Thị Dung',   'F', '0901234564', 'dung@company.com',   4, 4, 1, 'FullTime', '2023-09-01', 12),
('NV005', 'Hoàng Văn Em',    'M', '0901234565', 'em@company.com',     3, 6, 1, 'FullTime', '2025-02-01', 12);

-- Quản lý trực tiếp
UPDATE employees SET manager_id = 1 WHERE id IN (3, 4, 5);

-- Tài khoản (BCrypt hash — thay bằng hash thật trong app)
INSERT INTO users (username, password_hash, role, employee_id, must_change_password) VALUES
('superadmin', '$2a$12$REPLACE_SUPERADMIN_HASH', 'SuperAdmin', NULL,  FALSE),
('hr.binh',    '$2a$12$REPLACE_HR_HASH',          'HR',         2,     FALSE),
('mgr.an',     '$2a$12$REPLACE_MGR_HASH',          'Manager',   1,     FALSE),
('nv.cuong',   '$2a$12$REPLACE_EMP_HASH',          'Employee',  3,     TRUE),
('nv.dung',    '$2a$12$REPLACE_EMP_HASH2',         'Employee',  4,     TRUE);

-- Thiết bị
INSERT INTO attendance_devices
    (device_code, device_name, device_type, location_name, ip_address,
     latitude, longitude, radius_meters, min_confidence, camera_url)
VALUES
('CAM-GATE-01', 'Camera Cổng chính',   'Camera', 'Cổng chính – Tầng 1', '192.168.1.101', 10.77690, 106.70090,  50, 0.72, 'rtsp://192.168.1.101:554/stream'),
('CAM-IT-01',   'Camera Phòng IT',     'Camera', 'Phòng IT – Tầng 3',   '192.168.1.102', 10.77700, 106.70100,  30, 0.70, 'rtsp://192.168.1.102:554/stream'),
('KSK-HR-01',   'Kiosk Phòng HR',      'Kiosk',  'HR – Tầng 2',          '192.168.1.103', 10.77680, 106.70080,  30, 0.70, NULL),
('MOB-APP',     'Mobile Application',  'Mobile', 'Di động / WFH',         NULL,            NULL,     NULL,      100, 0.68, NULL);

-- Bản ghi chấm công mẫu
INSERT INTO attendance_records
    (employee_id, attendance_date, shift_id,
     check_in, check_out, check_in_device_id, check_out_device_id,
     check_in_method, check_out_method,
     check_in_confidence, check_out_confidence,
     status, late_minutes, early_minutes, working_minutes)
VALUES
    (3, CURRENT_DATE, 1,
     CURRENT_DATE + TIME '07:56', CURRENT_DATE + TIME '17:15',
     1, 1, 'Face', 'Face', 0.95, 0.93,
     'Present', 0, 0, 499);
    -- working_minutes = (17:15 - 07:56) - 60 break = 559 - 60 = 499 phút ≈ 8h19m

-- Log nhận diện tương ứng
INSERT INTO attendance_logs
    (attendance_id, employee_id, device_id, log_type, method,
     matched_face_id, confidence, face_distance, result)
VALUES
    (1, 3, 1, 'CheckIn',  'Face', NULL, 0.95, 0.28, 'Success'),
    (1, 3, 1, 'CheckOut', 'Face', NULL, 0.93, 0.31, 'Success');

-- Đơn nghỉ phép mẫu
INSERT INTO leave_requests
    (employee_id, leave_type, start_date, end_date, total_days, reason)
VALUES
    (3, 'Annual', CURRENT_DATE + 10, CURRENT_DATE + 11, 2, 'Nghỉ phép cá nhân');


-- ============================================================
--  GHI CHÚ TÍCH HỢP .NET 8
-- ============================================================
--
-- 1. EF CORE (Npgsql.EntityFrameworkCore.PostgreSQL 8.x)
--    ─ HasColumnType("bytea")    → face_data.encoding
--    ─ HasColumnType("jsonb")    → audit_logs.old_values / new_values
--    ─ HasConversion<string>()   → mọi cột VARCHAR CHECK (enum giả)
--    ─ ValueGeneratedOnAdd()     → SERIAL / BIGSERIAL columns
--
-- 2. FACE RECOGNITION PIPELINE (.NET)
--    ─ Thư viện   : FaceRecognitionDotNet (wrapper dlib)
--                   hoặc Microsoft.ML.OnnxRuntime + ArcFace model
--    ─ Encode     : float[128] → BitConverter.GetBytes() × 128 → byte[512]
--    ─ Decode     : byte[512]  → MemoryMarshal.Cast<byte,float>()
--    ─ So sánh    : Euclidean distance < face.max_distance (system_settings)
--    ─ Cache      : IMemoryCache / IDistributedCache (Redis)
--                   Key: "face_encodings" → List<FaceEncoding>
--                   Invalidate khi face_data INSERT/UPDATE/DELETE
--
-- 3. JWT / AUTH
--    ─ Access Token  : claim {userId, employeeId, role, deptId}, exp = 60 min
--    ─ Refresh Token : SHA-256 hash lưu users.refresh_token_hash
--    ─ Khóa tài khoản: failed_login_count >= security.max_login_attempts
--                      → set locked_until = NOW() + lockout_minutes
--
-- 4. BACKGROUND JOBS (Hangfire hoặc Quartz.NET)
--    ─ Mỗi 22:00  : AutoAbsentJob  — NotYet → Absent cho ngày hôm nay
--    ─ Mỗi 1 phút : DeviceHeartbeat — cập nhật attendance_devices.is_online
--    ─ Đầu năm    : HolidayClone   — nhân bản holidays.is_recurring = TRUE
--    ─ Đầu tháng  : LeaveReset     — reset used_leave_days nếu cần
--
-- 5. SIGNALR (Realtime Dashboard)
--    ─ Hub    : AttendanceHub
--    ─ Groups : "dept_{departmentId}", "all_hr"
--    ─ Broadcast khi: check-in/out mới, đơn mới, bất thường phát hiện
--
-- 6. BUSINESS LOGIC — Tính status chấm công
--    var shiftStart  = shift.StartTime.Add(TimeSpan.FromMinutes(shift.LateThreshold));
--    var shiftEnd    = shift.EndTime.Subtract(TimeSpan.FromMinutes(shift.EarlyThreshold));
--    bool isLate     = checkIn.TimeOfDay  > shiftStart.ToTimeSpan();
--    bool isEarly    = checkOut.TimeOfDay < shiftEnd.ToTimeSpan();
--    status = (isLate, isEarly) switch {
--        (true,  true)  => AttendanceStatus.LateAndEarly,
--        (true,  false) => AttendanceStatus.Late,
--        (false, true)  => AttendanceStatus.EarlyLeave,
--        _              => AttendanceStatus.Present
--    };
--    workingMinutes = (int)(checkOut - checkIn).TotalMinutes - shift.BreakMinutes;
--
-- 7. GEOFENCING (Mobile)
--    double dist = GeoUtils.HaversineMeters(userLat, userLng, dev.Latitude, dev.Longitude);
--    if (dist > device.RadiusMeters) throw new GeofenceViolationException();
--
-- 8. FACE ANTI-SPOOFING
--    ─ Kiểm tra liveness: FaceAntiSpoofing ONNX model (MiniFASNet)
--    ─ Nếu phát hiện ảnh giả → attendance_logs.result = 'Spoofing'
--    ─ Ghi audit_logs.action = 'SPOOFING_DETECTED'
--
-- ============================================================
-- KẾT THÚC SCRIPT
-- ============================================================
