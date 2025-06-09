DROP TABLE IF EXISTS iot_project.user_history;
DROP TABLE IF EXISTS iot_project.users;
DROP TABLE IF EXISTS iot_project.user_profile;

-- CREATE TABLE: user_profile
CREATE TABLE iot_project.user_profile (
    user_profile_id SERIAL PRIMARY KEY,
    description VARCHAR(200) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- CREATE TABLE: users
CREATE TABLE iot_project.users (
    user_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100),
    email VARCHAR(150) UNIQUE NOT NULL,
    password_hash VARCHAR(255),
    user_profile_id INT REFERENCES iot_project.user_profile(user_profile_id),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- CREATE INDEXES ON users
CREATE INDEX idx_users_email ON iot_project.users(email);

-- CREATE TABLE: user_history (sem FKs para permitir deletados)
CREATE TABLE iot_project.user_history (
    user_history_id SERIAL PRIMARY KEY,
    user_id INT,
    full_name VARCHAR(100),
    email VARCHAR(150),
    password_hash VARCHAR(255),
    user_profile_id INT,
    is_deleted BOOLEAN,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    operation_type VARCHAR(20), -- INSERT, UPDATE, DELETE
    operation_timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    operated_by INT
);

-- CREATE INDEXES ON user_history
CREATE INDEX idx_user_history_user_id ON iot_project.user_history(user_id);
CREATE INDEX idx_user_history_operation_type ON iot_project.user_history(operation_type);
CREATE INDEX idx_user_history_operation_timestamp ON iot_project.user_history(operation_timestamp DESC);

-- CREATE FUNCTION: log_user_history
CREATE OR REPLACE FUNCTION iot_project.log_user_history()
RETURNS TRIGGER AS $$
DECLARE
    actor_id INT;
BEGIN
    -- Tenta capturar o operador da sessão
    BEGIN
        actor_id := current_setting('iot_project.current_user_id')::INT;
    EXCEPTION
        WHEN OTHERS THEN
            actor_id := NULL;
    END;

    INSERT INTO iot_project.user_history (
        user_id, full_name, email, password_hash,
        user_profile_id, is_deleted, created_at, updated_at,
        operation_type, operation_timestamp, operated_by
    ) VALUES (
        CASE WHEN TG_OP = 'DELETE' THEN OLD.user_id ELSE NEW.user_id END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.full_name ELSE NEW.full_name END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.email ELSE NEW.email END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.password_hash ELSE NEW.password_hash END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.user_profile_id ELSE NEW.user_profile_id END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.is_deleted ELSE NEW.is_deleted END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.created_at ELSE NEW.created_at END,
        CASE WHEN TG_OP = 'DELETE' THEN OLD.updated_at ELSE NEW.updated_at END,
        TG_OP,
        CURRENT_TIMESTAMP,
        actor_id
    );

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- CREATE TRIGGERS ON users
CREATE TRIGGER trg_users_insert
AFTER INSERT ON iot_project.users
FOR EACH ROW
EXECUTE FUNCTION iot_project.log_user_history();

CREATE TRIGGER trg_users_update
AFTER UPDATE ON iot_project.users
FOR EACH ROW
EXECUTE FUNCTION iot_project.log_user_history();

CREATE TRIGGER trg_users_delete
AFTER DELETE ON iot_project.users
FOR EACH ROW
EXECUTE FUNCTION iot_project.log_user_history();




CREATE TABLE iot_project.device (
    device_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES iot_project.users(user_id),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    connected_port VARCHAR,
    name VARCHAR NOT NULL,
    type VARCHAR NOT NULL,
    category VARCHAR NOT NULL,
    unit VARCHAR, 
    mqtt_topic VARCHAR, 
    kafka_topic VARCHAR
);

CREATE INDEX idx_device_user_id ON iot_project.device(user_id);
CREATE INDEX idx_device_type ON iot_project.device(type);
CREATE INDEX idx_device_category ON iot_project.device(category);







