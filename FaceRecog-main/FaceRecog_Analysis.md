# BÁO CÁO PHÂN TÍCH — FaceRecog / FaceIDApp

> **Hệ thống chấm công nhận diện khuôn mặt**  
> C# WinForms + PostgreSQL | Phân tích hiện trạng · Khoảng trống UI/DB · Kế hoạch chỉnh sửa · Prompts

---

## 1. Tổng quan dự án

| Thành phần | Chi tiết |
|---|---|
| Ngôn ngữ & Runtime | C# .NET Framework (WinForms) — Windows Desktop App |
| Database | PostgreSQL 14+ — schema `face_attendance_v3` |
| ORM / DB Driver | Npgsql (truy vấn SQL thuần, không dùng ORM) |
| Face Recognition | DlibDotNet — ResNet-based face encoding + Euclidean distance matching |
| Camera | `WebcamCaptureService` (OpenCV qua EmguCV hoặc tương đương) |
| Auth | `AuthPasswordHasher` (BCrypt), session qua `AppSession` singleton |
| Project structure | `FaceIDApp/` (main app) + `examples/FaceRecog.WinForms/` (reference impl) |

### 1.1. Cấu trúc file chính

| File | Vai trò |
|---|---|
| `Data/Repository.cs` | ~1,756 dòng — TOÀN BỘ CRUD SQL cho 16 bảng DB |
| `Data/Dtos.cs` | ~362 dòng — DTO mapping đầy đủ tất cả bảng + view DTOs |
| `UserControls/UCDashboard.cs` | ~272 dòng — KPI cards + bảng hôm nay + pending leaves |
| `UserControls/UCAttendance.cs` | ~366 dòng — Camera + Face recognition + Check-in/out |
| `UserControls/UCEmployeeManagement.cs` | ~601 dòng — CRUD nhân viên + lịch sử chấm công tab |
| `UserControls/UCFaceRegistration.cs` | ~258 dòng — Đăng ký 5 ảnh khuôn mặt |
| `UserControls/UCAttendanceReport.cs` | ~289 dòng — Báo cáo tháng + toggle xem attendance_logs |
| `UserControls/UCLeaveManagement.cs` | ~403 dòng — Nghỉ phép CRUD + holidays tab |
| `UserControls/UCCatalog.cs` | ~624 dòng — 6 tabs danh mục: Dept/Position/Shift/Device/Calendar/Schedule |
| `UserControls/UCSettings.cs` | ~597 dòng — 5 tabs: Users/System/Config/Audit/FaceLog |
| `MainForm.cs` | Navigation sidebar với phân quyền Admin/Manager/Employee |

---

## 2. Phân tích DB vs UI — Mức độ tích hợp

DB có **16 bảng + 3 views**. Trạng thái CRUD từng bảng:

| Bảng DB | Repository | UI Screen | CREATE | READ | Ghi chú |
|---|---|---|---|---|---|
| `departments` | ✅ Full CRUD | UCCatalog / Tab Phòng ban | ✅ | ✅ | Inline edit trực tiếp trên grid |
| `positions` | ✅ Full CRUD | UCCatalog / Tab Chức vụ | ✅ | ✅ | Inline edit |
| `work_shifts` | ✅ Full CRUD | UCCatalog / Tab Ca làm việc | ✅ | ✅ | Inline edit |
| `employees` | ✅ Full CRUD | UCEmployeeManagement | ✅ | ✅ | Có thêm lịch sử chấm công tab |
| `face_data` | ✅ Insert+Delete | UCFaceRegistration | ✅ | ✅ | 5 ảnh, 5 góc (Front/Left/Right/Up/Down) |
| `attendance_records` | ✅ CheckIn/Out | UCAttendance + UCReport | ✅ | ✅ | Thiếu manual edit UI |
| `leave_requests` | ✅ Full CRUD | UCLeaveManagement | ✅ | ✅ | Có duyệt/từ chối |
| `holidays` | ✅ Full CRUD | UCLeaveManagement / Tab Ngày lễ | ✅ | ✅ | CRUD đầy đủ |
| `users` | ✅ Full CRUD | UCSettings / Tab Tài khoản | ✅ | ✅ | Có reset mật khẩu |
| `attendance_devices` | ✅ Full CRUD | UCCatalog / Tab Thiết bị | ✅ | ✅ | CRUD đầy đủ |
| `work_calendars` | ✅ Full CRUD | UCCatalog / Tab Lịch làm việc | ✅ | ✅ | CRUD đầy đủ |
| `employee_shift_schedules` | ✅ Upsert+Delete | UCCatalog / Tab Phân ca | ✅ | ✅ | Chỉ xem + thêm/xóa, chưa có bulk assign |
| `attendance_logs` | ✅ Insert+Read | UCAttendanceReport (toggle) | — | ⚠️ Partial | Chỉ xem theo ngày, thiếu filter nâng cao |
| `audit_logs` | ✅ Read-only | UCSettings / Tab Audit | — | ⚠️ Partial | Xem được nhưng thiếu filter theo action/user/date |
| `system_settings` | ✅ Upsert | UCSettings / Tab Config | — | ✅ | Read/write key-value |
| `face_registration_logs` | ✅ Read-only | UCSettings / Tab Face Log | — | ⚠️ Partial | Xem theo nhân viên, thiếu tổng quan |

> ✅ **KẾT LUẬN:** Repository đã **FULL 16/16 bảng** — UI đã kết nối **16/16 bảng**.  
> Đây là bước tiến lớn so với phiên bản trước. Vấn đề còn lại là **chất lượng và UX** của từng màn hình, không phải thiếu chức năng.

---

## 3. Hiện trạng chi tiết từng màn hình

### 3.1. UCDashboard — Trang chủ

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | 4 KPI cards (Tổng NV / Có mặt / Đi trễ / Vắng mặt) · Bảng `v_today_attendance` · Pending leave requests mini-list |
| ❌ Thiếu | Biểu đồ tỷ lệ chấm công theo tuần/tháng · FaceRegistered vs NotRegistered stat · Quick-link tới các màn hình con |
| ⚠️ Bug tiềm ẩn | Cards được tạo động bằng `SetupCards()` nhưng layout chưa responsive khi resize window |

### 3.2. UCAttendance — Chấm công

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | Camera stream · Capture ảnh · Face recognition (DlibDotNet) · Check-in / Check-out · Danh sách hôm nay |
| ❌ Thiếu | Cột 'Số giờ' và 'Ca' trong bảng danh sách · Status 'Chờ xử lý' chưa tự động resolve · Manual check-in (gõ mã NV thay vì camera) |
| ⚠️ Bug tiềm ẩn | `attendance_logs.InsertAttendanceLogAsync()` đã có trong Repository nhưng chưa thấy được gọi sau CheckIn/Out |

### 3.3. UCEmployeeManagement — Quản lý nhân viên

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | CRUD đầy đủ · ComboBox cho Phòng ban/Chức vụ/Ca/Loại hợp đồng · Tab lịch sử chấm công của từng NV · Avatar upload |
| ❌ Thiếu | Form thiếu hiển thị: `ManagerId` (quản lý trực tiếp) · AnnualLeave còn lại (annual - used) · Email · `IdentityCard` chưa validate |
| ⚠️ UX | Detail panel bên phải bị override bằng `ExtendDetailPanel()` — dễ gây lỗi layout khi thêm field mới |

### 3.4. UCFaceRegistration — Đăng ký khuôn mặt

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | 5 ảnh × 5 góc (Front/Left/Right/Up/Down) · Quality score · ComboBox chọn nhân viên · Encoding lưu DB |
| ❌ Thiếu | Không hiển thị `quality_score` từng ảnh sau khi chụp · Không có nút 'Xem lại ảnh đã đăng ký' · `is_verified` flag chưa được set UI |
| ❌ Thiếu | `face_registration_logs` chưa được insert khi đăng ký thành công (Repository có nhưng UC chưa gọi) |

### 3.5. UCAttendanceReport — Báo cáo

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | `v_monthly_summary` view · Toggle giữa báo cáo tháng và attendance_logs · 4 KPI summary cards |
| ❌ Thiếu | Bảng report đang **TRỐNG** (bug: chưa load data khi mở tab) · Không có filter nhân viên/phòng ban · Không có xuất Excel/CSV |
| ❌ Thiếu | `attendance_logs` chỉ hiện theo ngày đơn lẻ, không có filter date range |

### 3.6. UCLeaveManagement — Nghỉ phép & Ngày lễ

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | Danh sách `leave_requests` có filter status · Nút Thêm/Duyệt/Từ chối · Tab quản lý holidays riêng |
| ❌ Thiếu | Chưa có form nộp đơn nghỉ phép cho nhân viên · Thống kê ngày phép còn lại của từng NV · Email thông báo khi duyệt/từ chối |

### 3.7. UCCatalog — Quản lý danh mục

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | 6 tabs: Phòng ban · Chức vụ · Ca làm việc · Thiết bị · Lịch làm việc · Phân ca |
| ❌ Thiếu | Tab Phân ca chưa có bulk assign (gán ca cho nhiều NV cùng lúc) · `work_shifts` thiếu preview 'Giờ vào – Giờ ra' trực quan · Color picker cho ca |
| ⚠️ UX | Designer.cs chỉ có 79 dòng — toàn bộ UI được tạo động bằng code (khó debug, khó edit) |

### 3.8. UCSettings — Cài đặt

| Mục | Chi tiết |
|---|---|
| ✅ Đã có | 5 tabs: Tài khoản · Hệ thống · Cấu hình (system_settings) · Nhật ký (audit_logs) · Face logs |
| ❌ Thiếu | Tab Cấu hình chưa được thiết kế UI nhóm theo category (hiện chỉ là key-value grid) · Audit log thiếu filter theo date/user/table |
| ❌ Thiếu | Tab Hệ thống chỉ hiển thị info tĩnh, chưa kết nối `DatabaseConfig` thật sự để test connection realtime |

---

## 4. Kế hoạch chỉnh sửa — Phân theo ưu tiên

### 🔴 P0 — Sửa ngay (Bug ảnh hưởng chức năng chính)

| # | Màn hình | Vấn đề & Việc cần làm | File cần sửa |
|---|---|---|---|
| 1 | UCAttendanceReport | Báo cáo tháng không load data. **Fix:** gọi `LoadReportAsync()` trong Load event thay vì chỉ setup UI. Thêm DateTimePicker từ-đến + ComboBox NV + phòng ban | `UCAttendanceReport.cs` |
| 2 | UCAttendance | `attendance_logs` không được ghi sau mỗi lần nhận diện. **Fix:** gọi `InsertAttendanceLogAsync()` trong `BtnCheckIn_Click` và `BtnCheckOut_Click` sau khi insert `attendance_record` | `UCAttendance.cs` |
| 3 | UCFaceRegistration | `face_registration_logs` không được insert. **Fix:** gọi `InsertFaceRegistrationLogAsync()` (cần thêm vào Repository) trong `BtnRegister_Click` sau khi lưu `face_data` thành công | `UCFaceRegistration.cs` + `Repository.cs` |

### 🟡 P1 — Hoàn thiện (Chức năng thiếu quan trọng)

| # | Màn hình | Việc cần làm | File cần sửa |
|---|---|---|---|
| 4 | UCEmployeeManagement | Bổ sung field: `ManagerId` (ComboBox NV khác), Email, IdentityCard validate, hiển thị phép còn lại = `AnnualLeave - UsedLeave` | `UCEmployeeManagement.cs` + `Designer.cs` |
| 5 | UCAttendance | Thêm cột 'Số giờ' (`working_minutes/60`) và 'Ca' (`ShiftName`) vào bảng hôm nay. Thêm nút Manual Check-in (nhập mã NV thay camera) | `UCAttendance.cs` |
| 6 | UCAttendanceReport | Thêm filter: DateRange (từ-đến) + NV + phòng ban. Thêm nút Xuất Excel dùng ClosedXML hoặc NPOI. Attendance logs: thêm filter date range | `UCAttendanceReport.cs` |
| 7 | UCFaceRegistration | Hiển thị `quality_score` sau mỗi ảnh chụp. Thêm nút 'Xem ảnh đã đăng ký' load `face_data` hiện có. Set `is_verified = True` khi Admin xác nhận | `UCFaceRegistration.cs` |
| 8 | UCLeaveManagement | Thêm form nộp đơn nghỉ phép cho nhân viên thường (hiện chỉ Admin mới thêm được). Thêm cột 'Còn lại' trong danh sách | `UCLeaveManagement.cs` |
| 9 | UCCatalog / Phân ca | Thêm tính năng bulk assign: chọn nhiều NV → gán 1 ca cho cả tuần/tháng (loop `UpsertShiftScheduleAsync`) | `UCCatalog.cs` |

### 🟢 P2 — Nâng cao (UX & Nice-to-have)

| # | Màn hình | Việc cần làm | File cần sửa |
|---|---|---|---|
| 10 | UCDashboard | Thêm mini chart (PieChart) tỷ lệ có mặt/đi trễ/vắng. Thêm stat FaceRegistered/NotRegistered. Quick-links tới màn hình chức năng | `UCDashboard.cs` |
| 11 | UCSettings / Audit | Thêm filter cho `audit_logs`: chọn table, action (INSERT/UPDATE/DELETE), date range, username. Pagination | `UCSettings.cs` |
| 12 | UCSettings / Config | Nhóm `system_settings` theo category (Chấm công / Camera / Thông báo / Hệ thống). Thêm validation theo DataType field | `UCSettings.cs` |
| 13 | MainForm | Thêm notification badge trên menu button khi có pending leave requests. Breadcrumb tiêu đề màn hình | `MainForm.cs` |
| 14 | UCCatalog / Ca | Thêm color picker cho `work_shifts.color_code`. Preview 'Ca Sáng: 08:00 – 17:00 (8h)' trực quan | `UCCatalog.cs` |

---

## 5. Prompts cho AI (Cursor / Copilot / Claude)

> **Cách dùng:** Copy từng prompt vào Cursor (Ctrl+K hoặc chat), đính kèm file `.cs` tương ứng.  
> Prompt được viết đủ context để AI không cần hỏi thêm.

---

### PROMPT 1 — Fix báo cáo chấm công (`UCAttendanceReport.cs`)

_Dán vào Cursor chat khi đang mở file `UCAttendanceReport.cs`:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
File: UCAttendanceReport.cs
Database: PostgreSQL via Repository class (Data/Repository.cs)

CURRENT BUG: The report grid (dgvReport) never loads data because LoadReportAsync()
is not called on form Load.

TASK — Fix and enhance UCAttendanceReport:

1. FIX: In the constructor or Load event, call LoadReportAsync()
   with current month as default date range.

2. ADD filter controls above the grid:
   - dtpFrom: DateTimePicker (default: first day of current month)
   - dtpTo: DateTimePicker (default: today)
   - cboEmployee: ComboBox "Tất cả nhân viên" + list from GetEmployeesAsync()
   - cboDepartment: ComboBox "Tất cả phòng ban" + list from GetDepartmentsAsync()
   - btnSearch: Button "Tìm kiếm" → calls LoadReportAsync()
   - btnExport: Button "Xuất Excel" (implement later, add stub)

3. FIX LoadReportAsync():
   Use GetMonthlySummaryAsync(selectedMonth) from Repository.
   Filter client-side by employeeId and departmentName if selected.
   Populate dgvReport with columns:
   STT │ Mã NV │ Họ tên │ Phòng ban │ Tổng ngày │ Đi làm │ Đi trễ │ Vắng │ Tổng giờ

4. For attendance_logs toggle (btnToggle / btnViewLog):
   Change to load logs using GetAttendanceLogsAsync(from, to, limit:500)
   instead of single date. Show in _dgvLog with columns:
   Thời gian │ Mã NV │ Họ tên │ Loại │ Phương thức │ Kết quả │ Lý do thất bại │ Confidence

5. Add 4 summary KPI labels above grid:
   Total working days │ Present days │ Late days │ Absent days
   (sum from filtered MonthlySummaryDto list)

Keep existing style: header BackColor = Color.FromArgb(41, 128, 185),
alternating rows, Segoe UI font. Add to SetupUI(), not new methods.
```

---

### PROMPT 2 — Fix ghi attendance_logs (`UCAttendance.cs`)

_Dán vào Cursor khi đang mở `UCAttendance.cs` + `Repository.cs`:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
Files: UCAttendance.cs, Data/Repository.cs

CURRENT BUG: After successful check-in or check-out via face recognition,
attendance_logs table is never written to.
Repository has InsertAttendanceLogAsync() already implemented.

TASK — Wire attendance logging:

1. In BtnCheckIn_Click, after calling await _repo.CheckInAsync(...)
   and getting back the attendanceId (long), add:

   await AppDatabase.Repository.InsertAttendanceLogAsync(
       attendanceId: attendanceId,
       employeeId: recognizedEmployee.Id,
       deviceId: null,  // no device assigned yet
       logType: "CheckIn",
       method: "Face",
       matchedFaceId: bestMatchFaceId,  // from recognition result
       confidence: recognitionConfidence,
       faceDistance: faceDistance,
       imagePath: _lastCapturedPath,
       result: "Success",
       failReason: null
   );

2. In BtnCheckOut_Click, similar pattern with logType: "CheckOut"

3. In recognition FAILURE case (face not matched), also log:
   await AppDatabase.Repository.InsertAttendanceLogAsync(
       attendanceId: null,
       employeeId: null,
       ...
       result: "Failed",
       failReason: "No face match above threshold"
   );

4. ADD to the today attendance grid (dgvTodayAttendance):
   - Column "Số giờ" showing working_minutes/60.0 formatted as "7.5h"
   - Column "Ca" showing ShiftName from TodayAttendanceDto
   Reorder columns: STT │ Mã NV │ Họ tên │ Ca │ Giờ vào │ Giờ ra │ Số giờ │ Trạng thái

5. ADD Manual Check-in panel (collapsed by default, toggle with small button):
   - TextBox: txtManualCode (enter employee code)
   - ComboBox: cboManualAction (Check-in / Check-out)
   - Button: btnManualSubmit → lookup employee by code, process check-in/out
     with CheckInMethod = "Manual", no face image

Keep all existing camera and face recognition logic unchanged.
```

---

### PROMPT 3 — Hoàn thiện `UCEmployeeManagement.cs`

_Dán vào Cursor khi đang mở `UCEmployeeManagement.cs`:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
File: UCEmployeeManagement.cs (C# WinForms, ~601 lines)
Database: PostgreSQL. EmployeeDto has all fields including:
  ManagerId (int?), Email (string), IdentityCard (string),
  AnnualLeaveDays (decimal), UsedLeaveDays (decimal)

TASK — Enhance employee detail panel (right side panel pnlEmployeeDetail):

1. ADD these missing fields to the form (inside ExtendDetailPanel or
   create new method AddMissingFields()):

   a) cboManager: ComboBox "Quản lý trực tiếp"
      - Populate with GetEmployeesAsync(activeOnly:true)
      - Exclude self (current editing employee)
      - Display: "{Code} - {FullName}"

   b) txtEmail: TextBox "Email" with basic format validation (@)

   c) txtIdentityCard: TextBox "CCCD/CMND" — numeric only, 9 or 12 digits

   d) lblLeaveBalance: Label (read-only)
      Text: "Phép còn lại: {AnnualLeaveDays - UsedLeaveDays} ngày"
      Color: Green if > 0, Red if <= 0

2. In LoadEmployeeToForm(EmployeeDto emp):
   - Set cboManager selected item matching emp.ManagerId
   - Set txtEmail.Text = emp.Email
   - Set txtIdentityCard.Text = emp.IdentityCard
   - Set lblLeaveBalance.Text accordingly

3. In BtnSave_Click, read and validate new fields:
   - Validate email format if not empty
   - Validate identity card length
   - Set emp.ManagerId, emp.Email, emp.IdentityCard

4. In the attendance history tab (dgvAttHistory):
   Load data using GetAttendanceByEmployeeAsync(empId, from:3 months ago, to:today)
   Add columns: Ngày │ Ca │ Giờ vào │ Giờ ra │ Số giờ │ Trạng thái │ Đi trễ (phút)
   Color status cells:
     Present = light green, Late = light orange, Absent = light red

Keep all existing code. Only add what is missing.
Use Segoe UI font, maintain existing color scheme (FromArgb(30, 41, 59) headers).
```

---

### PROMPT 4 — UCFaceRegistration — quality score + registration log

_Dán vào Cursor khi đang mở `UCFaceRegistration.cs`:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
File: UCFaceRegistration.cs
Repository: Data/Repository.cs (has InsertFaceDataAsync, GetFaceDataByEmployeeAsync)

TASK 1 — Show quality score per captured photo:
After BtnCapture_Click successfully captures and encodes a face,
update the corresponding PictureBox caption/label to show quality score.
Add a Label overlay on each capturedPictures[i] PictureBox:
  - If qualityScore >= 0.8: "✓ {score:P0}" in green
  - If qualityScore >= 0.5: "⚠ {score:P0}" in orange
  - If qualityScore < 0.5:  "✗ {score:P0}" in red

TASK 2 — "View registered faces" button:
Add button btnViewRegistered "Xem ảnh đã đăng ký":
On click, load GetFaceDataByEmployeeAsync(selectedEmpId) and show in a new
small Form/Panel with PictureBoxes for each face_data record, showing:
image thumbnail + angle + quality score + created_at

TASK 3 — Write face_registration_logs after successful registration:
In BtnRegister_Click, after all InsertFaceDataAsync() calls succeed,
call a new Repository method. First add to Repository.cs:

  public async Task InsertFaceRegistrationLogAsync(
      int employeeId, string action, int? performedBy, string reason)
  INSERT INTO face_registration_logs
      (employee_id, action, performed_by, reason)
  VALUES (@emp_id, @action, @performed_by, @reason)

Then call it in BtnRegister_Click:
  await AppDatabase.Repository.InsertFaceRegistrationLogAsync(
      selectedEmp.Id, "Register", AppSession.Current.UserId,
      $"Registered {capturedCount} face images");

TASK 4 — Update employees.is_face_registered = TRUE after registration.
Call UpdateEmployeeAsync with emp.IsFaceRegistered = true + emp.FaceRegisteredAt = DateTime.Now

Keep existing camera and encoding logic unchanged.
```

---

### PROMPT 5 — UCCatalog — bulk shift assignment

_Dán vào Cursor khi đang mở `UCCatalog.cs`:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
File: UCCatalog.cs (~624 lines)
Relevant Repository methods:
  UpsertShiftScheduleAsync(int employeeId, DateTime date, int shiftId, bool isOverride, string note)
  GetEmployeesAsync(), GetWorkShiftsAsync()

TASK — Enhance the "Phân ca" (Shift Schedule) tab (BuildShiftScheduleTab):

Currently: only shows a grid of individual schedule records.
Add a "Gán ca hàng loạt" (Bulk Assign) panel above the grid:

UI Controls:
  - listBoxEmployees: CheckedListBox (multi-select)
    Populated with all active employees "{Code} - {FullName}"
    Add "Chọn tất cả" / "Bỏ chọn tất cả" buttons
  - cboShiftBulk: ComboBox — select shift to assign
  - dtpBulkStart: DateTimePicker — start date
  - dtpBulkEnd: DateTimePicker — end date
  - chkSkipWeekends: CheckBox "Bỏ qua cuối tuần" (default: checked)
  - chkSkipHolidays: CheckBox "Bỏ qua ngày lễ" (default: checked)
  - btnBulkAssign: Button "Gán ca" (red background, white text)

Logic for btnBulkAssign_Click:
  var selectedEmployees = listBoxEmployees.CheckedItems (get their IDs)
  var selectedShift = (WorkShiftDto)cboShiftBulk.SelectedItem
  for each date between dtpBulkStart.Value and dtpBulkEnd.Value:
    if chkSkipWeekends: skip Saturday and Sunday
    if chkSkipHolidays: call IsHolidayAsync(date) and skip
    for each selected employee:
      await UpsertShiftScheduleAsync(empId, date, shiftId, true, "Bulk assign")
  Show progress: use a ProgressBar or update a label "Đang xử lý X/Y..."
  After done: reload grid with LoadShiftScheduleAsync()
  Show MessageBox: "Đã gán ca cho X nhân viên, Y ngày thành công"

Keep existing grid and other tabs unchanged.
```

---

### PROMPT 6 — UCSettings — filter audit logs + config nhóm

_Dán vào Cursor khi đang mở `UCSettings.cs`:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
File: UCSettings.cs (~597 lines)
Repository: GetAuditLogsAsync(limit, tableName), GetSystemSettingsAsync(), UpsertSystemSettingAsync()

TASK 1 — Enhance Audit Logs tab (BuildTabAudit):

Add filter panel above dgvAudit:
  - cboAuditTable: ComboBox (existing, filter by table_name)
    ADD options: "employees", "attendance_records", "leave_requests",
                 "face_data", "users", "departments"
  - cboAuditAction: ComboBox "Tất cả hành động" │ "INSERT" │ "UPDATE" │ "DELETE"
  - dtpAuditFrom: DateTimePicker (default: 7 days ago)
  - dtpAuditTo: DateTimePicker (default: today)
  - cboAuditUser: ComboBox "Tất cả người dùng" + list from GetUsersAsync()
  - btnRefreshAudit: refresh with all filters applied

Modify GetAuditLogsAsync() call or filter client-side after loading.
Add columns to dgvAudit:
  Thời gian │ Người dùng │ Hành động │ Bảng │ Record ID │ Thay đổi (old→new summary)

TASK 2 — Enhance Config tab (BuildTabConfig):

Instead of a flat key-value grid, group system_settings by category.
Parse key prefix:
  "work_"   → "Giờ làm việc"
  "camera_" → "Camera"
  "system_" → "Hệ thống"
  others    → "Khác"

Create a TreeView or TabControl with groups.
Within each group, show settings as labeled input controls:
  - Boolean DataType  → CheckBox
  - Integer DataType  → NumericUpDown
  - Time DataType     → DateTimePicker (TimeOnly mode)
  - String DataType   → TextBox

btnSaveConfig should iterate all visible controls and call
UpsertSystemSettingAsync(key, value.ToString()) for each changed value.

Keep all existing tabs (Users, System, FaceLog) unchanged.
```

---

### PROMPT 7 — Tổng thể UI polish (MainForm + tất cả UC)

_Dán vào Cursor khi đang mở `MainForm.cs`, đọc kèm tất cả UC files:_

```
You are working on a C# WinForms attendance system (FaceIDApp).
Files: MainForm.cs + all UserControls/*.cs
This is a desktop HR/attendance app with sidebar navigation.

TASK — UI Polish & UX improvements:

1. MAINFORM — Notification badge on sidebar buttons:
   In MainForm.cs, after loading, check for pending leave requests:
   var pendingCount = (await GetLeaveRequestsAsync(null, "Pending")).Count;
   If pendingCount > 0, add a red circular Label badge with count
   next to the "Nghỉ phép" menu button.
   Refresh badge every 5 minutes using existing timer pattern.

2. ALL GRIDS — Consistent styling:
   Apply this standard to all DataGridView instances:
   - ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59)
   - ColumnHeadersDefaultCellStyle.ForeColor = Color.White
   - ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
   - ColumnHeadersHeight = 36
   - DefaultCellStyle.Font = new Font("Segoe UI", 9.5F)
   - RowTemplate.Height = 30
   - AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252)
   - GridColor = Color.FromArgb(226, 232, 240)
   - SelectionMode = FullRowSelect
   - MultiSelect = false
   Create a static helper: GridStyleHelper.ApplyStandard(DataGridView dgv)

3. STATUS CELLS — Color coding across all grids:
   Create a static helper: GridStyleHelper.ColorStatusCell(DataGridView dgv, int colIndex)
   Status colors:
     "Present" / "Đúng giờ"  → BackColor: FromArgb(212, 237, 218), ForeColor: FromArgb(21, 87, 36)
     "Late" / "Đi trễ"       → BackColor: FromArgb(255, 243, 205), ForeColor: FromArgb(133, 100, 4)
     "Absent" / "Vắng"       → BackColor: FromArgb(248, 215, 218), ForeColor: FromArgb(114, 28, 36)
     "Approved" / "Đã duyệt" → BackColor: FromArgb(212, 237, 218), ForeColor: FromArgb(21, 87, 36)
     "Pending" / "Chờ duyệt" → BackColor: FromArgb(255, 243, 205), ForeColor: FromArgb(133, 100, 4)
     "Rejected" / "Từ chối"  → BackColor: FromArgb(248, 215, 218), ForeColor: FromArgb(114, 28, 36)
   Apply via CellFormatting event.

4. LOADING STATES — Add cursor wait:
   Wrap all async data loads with:
   this.Cursor = Cursors.WaitCursor;
   try { /* load data */ } finally { this.Cursor = Cursors.Default; }

5. ERROR HANDLING — Consistent error display:
   Replace all bare catch blocks with:
   catch (Exception ex) {
     MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo",
       MessageBoxButtons.OK, MessageBoxIcon.Error);
   }

Create GridStyleHelper.cs as a new static class in FaceIDApp namespace.
Apply helpers to all UserControls — do NOT rewrite existing logic.
```

---

## 6. Tóm tắt — Thứ tự thực hiện

| Bước | Prompt | Kết quả | Thời gian ước tính |
|---|---|---|---|
| 1 | PROMPT 1 | Fix báo cáo load data + filter + KPI summary | 30–45 phút |
| 2 | PROMPT 2 | Fix ghi attendance_logs + thêm cột giờ/ca + manual check-in | 30–45 phút |
| 3 | PROMPT 3 | Bổ sung fields NV (manager/email/CCCD) + lịch sử chấm công màu | 45–60 phút |
| 4 | PROMPT 4 | FaceReg: quality score + view ảnh + registration logs | 30–45 phút |
| 5 | PROMPT 5 | Catalog: bulk assign ca cho nhiều nhân viên | 45–60 phút |
| 6 | PROMPT 6 | Settings: filter audit logs + config nhóm theo category | 45–60 phút |
| 7 | PROMPT 7 | UI Polish toàn bộ: consistent grid style + status colors + error handling | 30–45 phút |

> **Tổng thời gian ước tính: 4–6 giờ làm việc**  
> Sau khi hoàn thành 7 bước trên, hệ thống sẽ có đầy đủ chức năng CRUD cho tất cả 16 bảng DB, giao diện nhất quán, và không còn các bug ghi dữ liệu quan trọng bị bỏ sót.
