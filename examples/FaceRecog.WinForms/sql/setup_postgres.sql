-- FaceRecog.WinForms PostgreSQL bootstrap script
-- Run this script against the target database to recreate the schema from scratch.

DROP TABLE IF EXISTS attendance_logs;
DROP TABLE IF EXISTS login_logs;
DROP TABLE IF EXISTS detections;
DROP TABLE IF EXISTS images;
DROP TABLE IF EXISTS scan_sessions;
DROP TABLE IF EXISTS app_users;

CREATE TABLE scan_sessions (
    id uuid PRIMARY KEY,
    scan_type varchar(30) NOT NULL,
    source_path text NOT NULL,
    model_name varchar(30) NOT NULL,
    cpu_count integer NULL,
    status varchar(30) NOT NULL DEFAULT 'Completed',
    result_count integer NOT NULL DEFAULT 0,
    started_at timestamptz NOT NULL DEFAULT now(),
    completed_at timestamptz NULL
);

CREATE TABLE images (
    id uuid PRIMARY KEY,
    file_path text NOT NULL UNIQUE,
    file_name text NOT NULL,
    file_extension varchar(16) NOT NULL,
    file_size bigint NULL,
    modified_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE detections (
    id uuid PRIMARY KEY,
    session_id uuid NOT NULL REFERENCES scan_sessions(id) ON DELETE CASCADE,
    image_id uuid NOT NULL REFERENCES images(id) ON DELETE CASCADE,
    top integer NOT NULL,
    "right" integer NOT NULL,
    bottom integer NOT NULL,
    "left" integer NOT NULL,
    confidence numeric(8,5) NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE app_users (
    id uuid PRIMARY KEY,
    username text NOT NULL UNIQUE,
    full_name text NOT NULL,
    role text NOT NULL DEFAULT 'User',
    password_hash text NOT NULL,
    face_encoding_data text NULL,
    face_image_path text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    last_login_at timestamptz NULL
);

CREATE TABLE login_logs (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    logged_in_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE attendance_logs (
    id uuid PRIMARY KEY,
    user_id uuid NULL REFERENCES app_users(id) ON DELETE SET NULL,
    captured_image_path text NULL,
    model_name varchar(30) NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'Present',
    match_distance numeric(10,6) NULL,
    attended_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_images_file_path ON images(file_path);
CREATE INDEX ix_detections_session_id ON detections(session_id);
CREATE INDEX ix_app_users_username ON app_users(username);
CREATE INDEX ix_login_logs_user_id ON login_logs(user_id);
CREATE INDEX ix_login_logs_logged_in_at ON login_logs(logged_in_at);
CREATE INDEX ix_attendance_logs_user_id ON attendance_logs(user_id);
CREATE INDEX ix_attendance_logs_attended_at ON attendance_logs(attended_at);
