# Checklist Dong Goi FaceIDApp (Release x64)

Cap nhat: 2026-04-23
Pham vi: FaceIDApp WinForms (.NET Framework 4.6.1, x64 only)

## 1) Muc tieu

Checklist nay giup dong goi ban phat hanh an toan, khong thieu dependency, khong mang theo du lieu test/rac runtime.

## 2) Phan loai file: Tu sinh vs Phai tu cung cap

### A. Tu sinh khi build (khong can commit)

- FaceIDApp/bin/
- FaceIDApp/obj/
- src/FaceRecog/bin/
- src/FaceRecog/obj/
- .vs/

Hanh dong:

- Khong commit cac thu muc tren.
- Lay artifact dong goi tu output Release x64.

### B. Tu sinh khi app chay lan dau (khong can ship san)

- face_attendance.db (SQLite runtime DB, tao khi app khoi dong neu chua co)
- img/ (anh runtime)
- face/ (anh da dang ky khuon mat)

Hanh dong:

- Khong copy du lieu test trong cac thu muc runtime vao goi phat hanh.
- Cho phep app tu tao tren may client khi chay lan dau.

### C. Phai tu cung cap (bat buoc)

- models/dlib_face_recognition_resnet_model_v1.dat
- models/shape_predictor_5_face_landmarks.dat
- FaceIDApp/App.config (duoc copy thanh FaceIDApp.exe.config khi build)
- FaceIDApp/Database/face_attendance_v3.sql

Hanh dong:

- Xac nhan du 2 file model truoc khi build.
- Xac nhan config production truoc khi dong goi.

## 3) Pre-Build Checklist (truoc khi build Release)

- [ ] Repo dang o trang thai mong muon (khong co thay doi ngoai y muon).
- [ ] Da chot branch/tag release.
- [ ] Kiem tra platform x64 trong project FaceIDApp.
- [ ] Kiem tra 2 model .dat co ton tai trong thu muc models/.
- [ ] Kiem tra FaceIDApp/App.config dung gia tri production.
- [ ] Kiem tra khong hardcode duong dan Unicode cho model (su dung ModelsDirectoryResolver).

## 4) Build Checklist (Release x64)

Lenh de build:

- dotnet build .\FaceIDApp\FaceIDApp.csproj -c Release -p:Platform=x64 --nologo

Can dat:

- [ ] Build success (exit code = 0).
- [ ] Co thu muc output: FaceIDApp/bin/x64/Release/net461/.
- [ ] Co file chinh: FaceIDApp.exe va FaceIDApp.exe.config.

## 5) Checklist noi dung goi phat hanh

Khuyen nghi an toan nhat: copy NGUYEN thu muc sau vao goi phat hanh:

- FaceIDApp/bin/x64/Release/net461/

Toi thieu phai co:

- [ ] FaceIDApp.exe
- [ ] FaceIDApp.exe.config
- [ ] FaceRecog.dll
- [ ] DlibDotNet.dll
- [ ] DlibDotNetNative.dll
- [ ] DlibDotNetNativeDnn.dll
- [ ] OpenCvSharp.dll
- [ ] OpenCvSharpExtern.dll (neu nam top-level)
- [ ] opencv_videoio_ffmpeg4100_64.dll
- [ ] System.Data.SQLite.dll
- [ ] x64/SQLite.Interop.dll
- [ ] x86/SQLite.Interop.dll
- [ ] dll/x64/OpenCvSharpExtern.dll
- [ ] dll/x64/opencv_videoio_ffmpeg4100_64.dll
- [ ] models/dlib_face_recognition_resnet_model_v1.dat
- [ ] models/shape_predictor_5_face_landmarks.dat
- [ ] Database/face_attendance_v3.sql

Khong nen dong goi:

- [ ] face_attendance.db tu may dev
- [ ] img/ va face/ du lieu test tu may dev
- [ ] *.pdb (neu khong phat hanh ban debug)
- [ ] build_output.txt, test.db, file tam

## 6) Checklist may client (prerequisites)

- [ ] Windows 10/11 64-bit.
- [ ] Microsoft Visual C++ Redistributable 2015-2022 (x64).
- [ ] Webcam duoc he dieu hanh nhan dien.
- [ ] Quyen ghi vao thu muc cai dat hoac data folder.

## 7) Checklist kiem thu smoke test tren may sach

- [ ] Copy goi release vao may sach (khong co source code).
- [ ] Chay FaceIDApp.exe khong loi missing DLL.
- [ ] App tao duoc face_attendance.db khi chay lan dau neu chua co.
- [ ] Dang nhap admin mac dinh (neu DB moi): admin / admin123.
- [ ] Load camera thanh cong.
- [ ] Dang ky duoc 1 khuon mat mau.
- [ ] Cham cong check-in/check-out tao ban ghi thanh cong.
- [ ] Dong/mo lai app, du lieu van con trong SQLite.

## 8) Checklist bao mat truoc khi ban giao

- [ ] Khong de thong tin nhay cam trong App.config (neu co, ma hoa hoac tach secret).
- [ ] Khong log password, khong log face encoding.
- [ ] SQL su dung parameterized query (khong string concatenation).

## 9) Cau truc goi de xuat

- release/
- release/FaceIDApp.exe
- release/FaceIDApp.exe.config
- release/FaceRecog.dll
- release/System.Data.SQLite.dll
- release/DlibDotNet.dll
- release/DlibDotNetNative.dll
- release/DlibDotNetNativeDnn.dll
- release/OpenCvSharp.dll
- release/OpenCvSharpExtern.dll
- release/opencv_videoio_ffmpeg4100_64.dll
- release/x64/SQLite.Interop.dll
- release/x86/SQLite.Interop.dll
- release/dll/x64/OpenCvSharpExtern.dll
- release/dll/x64/opencv_videoio_ffmpeg4100_64.dll
- release/models/dlib_face_recognition_resnet_model_v1.dat
- release/models/shape_predictor_5_face_landmarks.dat
- release/Database/face_attendance_v3.sql

## 10) Lenh kiem nhanh truoc khi zip

- Kiem tra model:
  - dir .\FaceIDApp\bin\x64\Release\net461\models
- Kiem tra SQL:
  - dir .\FaceIDApp\bin\x64\Release\net461\Database
- Kiem tra EXE:
  - dir .\FaceIDApp\bin\x64\Release\net461\FaceIDApp.exe

Neu 3 nhom tren deu co day du, ban co the zip thu muc net461 de ban giao.
