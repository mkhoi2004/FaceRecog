import re

path = r'd:\CongNgheDN(CuoiKy)\FaceRecog\FaceIDApp\Database\face_attendance_v3.sql'
with open(path, 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Replace block
# Find the INSERT INTO ATTENDANCE_RECORDS...
start_idx = content.find('INSERT INTO ATTENDANCE_RECORDS')
if start_idx != -1:
    end_idx = content.find(';', start_idx)
    block = content[start_idx:end_idx]
    
    # Let's replace the lines for today's check-out
    # We'll just replace the tuple blocks where DATE('now') is present (excluding DATE('now', '-1 day'))
    # A simple regex to replace check-out fields:
    # 5 fields after Check-in:
    # CHECK_OUT, CHECK_IN_DEVICE_ID, CHECK_OUT_DEVICE_ID, CHECK_IN_METHOD, CHECK_OUT_METHOD, CHECK_IN_CONFIDENCE, CHECK_OUT_CONFIDENCE, STATUS, LATE_MINUTES, EARLY_MINUTES, WORKING_MINUTES
    
    # Actually, it's easier to just replace the whole VALUES block with a cleaned version.
    
    new_values = """VALUES (
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
)"""
    import re
    # We replace from VALUES ( to the end of block
    new_block = re.sub(r'VALUES\s*\(.*', new_values, block, flags=re.DOTALL)
    
    content = content[:start_idx] + new_block + content[end_idx:]

    with open(path, 'w', encoding='utf-8-sig') as f:
        f.write(content)
    print("Done")
