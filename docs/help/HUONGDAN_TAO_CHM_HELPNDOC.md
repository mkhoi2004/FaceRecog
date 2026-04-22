# Huong dan tao file Huongdan.chm bang HelpNDoc

Cap nhat: 2026-04-23

## 1) Thu muc noi dung da duoc chuan bi

Import cac file HTML tai day:

- docs/help/html/

Danh sach file:

- index.html
- 00-gioi-thieu.html
- 01-cai-dat.html
- 02-dang-nhap-va-phan-quyen.html
- 03-cham-cong.html
- 04-quan-ly-nhan-vien.html
- 05-dang-ky-khuon-mat.html
- 06-bao-cao-va-nghi-phep.html
- 07-dong-goi-release.html
- 08-loi-thuong-gap.html

## 2) Thao tac tren man hinh HelpNDoc hien tai

1. Luu project voi ten Huongdan.hnd
   - File > Save As
   - Goi y duong dan: docs/help/Huongdan.hnd

2. Them topic tu file HTML
   - Tren ribbon: Home > Import files
   - Chon tat ca file trong docs/help/html/
   - Sau khi import, HelpNDoc se tao topic cho tung file.

3. Dat default topic
   - Chon topic tuong ung voi index.html
   - Trong Project settings, click Default topic
   - Gan den topic index.

4. Sap xep muc luc
   - Keo tha topic theo thu tu 1 -> 9.
   - Dat ten de doc:
     - Gioi thieu he thong
     - Cai dat va khoi dong lan dau
     - Dang nhap va phan quyen
     - Quy trinh cham cong
     - Quan ly nhan vien
     - Dang ky khuon mat
     - Bao cao va nghi phep
     - Dong goi Release x64
     - Loi thuong gap va cach xu ly

## 3) Cau hinh output CHM

1. Vao Project options.
2. Mo muc Build outputs.
3. Bat Microsoft HTML Help (CHM).
4. Dat output file name: Huongdan.chm
5. Dat output folder:
   - Goi y: docs/help/out/

## 4) Generate

1. Bam Generate help.
2. Sau khi xong, kiem tra file:
   - docs/help/out/Huongdan.chm

## 5) Neu mo CHM bi trang trong

1. Dong file CHM.
2. Chuot phai file Huongdan.chm > Properties.
3. Neu co nut Unblock thi bam Unblock > Apply.
4. Mo lai file.

## 6) Tich hop nhanh voi Visual Studio

1. Visual Studio > Tools > External Tools > Add.
2. Dat:
   - Title: Build Huongdan CHM
   - Command: C:\Program Files\IBE Software\HelpNDoc 10\hnd10.exe
   - Arguments: /cmd build "$(SolutionDir)docs\help\Huongdan.hnd"
   - Initial directory: $(SolutionDir)

Sau do co the build tai lieu ngay trong Visual Studio.
