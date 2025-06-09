-- DROP TABELA DE HISTÓRICO SE EXISTIR
DROP TABLE IF EXISTS iot_project.device_history;

-- CRIAÇÃO DA TABELA DE HISTÓRICO
CREATE TABLE iot_project.device_history (
    device_history_id SERIAL PRIMARY KEY,
    device_id INT,
    user_id INT,
    name VARCHAR NOT NULL,
    type VARCHAR NOT NULL,
    category VARCHAR NOT NULL,
    unit VARCHAR,
    connected_port VARCHAR,
    is_deleted BOOLEAN,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    operation_type VARCHAR(20), -- INSERT, UPDATE, DELETE
    operation_timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    operated_by INT, 
    mqtt_topic VARCHAR, 
    kafka_topic VARCHAR
);

-- ÍNDICES PARA MELHOR DESEMPENHO NAS CONSULTAS
CREATE INDEX idx_device_history_device_id ON iot_project.device_history(device_id);
CREATE INDEX idx_device_history_operation_type ON iot_project.device_history(operation_type);
CREATE INDEX idx_device_history_operation_timestamp ON iot_project.device_history(operation_timestamp DESC);

-- FUNÇÃO DE TRIGGER PARA AUDITORIA
CREATE OR REPLACE FUNCTION iot_project.log_device_history()
RETURNS TRIGGER AS $$
DECLARE
    actor_id INT;
BEGIN
    -- Captura o ID do usuário operador da variável de sessão
    BEGIN
        actor_id := current_setting('iot_project.current_user_id')::INT;
    EXCEPTION
        WHEN OTHERS THEN
            actor_id := NULL;
    END;

    -- TRATAMENTO POR OPERAÇÃO
    IF TG_OP = 'INSERT' THEN
        INSERT INTO iot_project.device_history (
            device_id, user_id, name, type, category, unit,
            connected_port, is_deleted, created_at, updated_at,
            operation_type, operation_timestamp, operated_by
        ) VALUES (
            NEW.device_id, NEW.user_id, NEW.name, NEW.type, NEW.category, NEW.unit,
            NEW.connected_port, NEW.is_deleted, NEW.created_at, NEW.updated_at,
            'INSERT', CURRENT_TIMESTAMP, actor_id
        );

    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO iot_project.device_history (
            device_id, user_id, name, type, category, unit,
            connected_port, is_deleted, created_at, updated_at,
            operation_type, operation_timestamp, operated_by
        ) VALUES (
            NEW.device_id, NEW.user_id, NEW.name, NEW.type, NEW.category, NEW.unit,
            NEW.connected_port, NEW.is_deleted, NEW.created_at, NEW.updated_at,
            'UPDATE', CURRENT_TIMESTAMP, actor_id
        );

    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO iot_project.device_history (
            device_id, user_id, name, type, category, unit,
            connected_port, is_deleted, created_at, updated_at,
            operation_type, operation_timestamp, operated_by
        ) VALUES (
            OLD.device_id, OLD.user_id, OLD.name, OLD.type, OLD.category, OLD.unit,
            OLD.connected_port, OLD.is_deleted, OLD.created_at, OLD.updated_at,
            'DELETE', CURRENT_TIMESTAMP, actor_id
        );
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- CRIAÇÃO DOS TRIGGERS NA TABELA PRINCIPAL iot_project.device
CREATE TRIGGER trg_device_insert
AFTER INSERT ON iot_project.device
FOR EACH ROW
EXECUTE FUNCTION iot_project.log_device_history();

CREATE TRIGGER trg_device_update
AFTER UPDATE ON iot_project.device
FOR EACH ROW
EXECUTE FUNCTION iot_project.log_device_history();

CREATE TRIGGER trg_device_delete
AFTER DELETE ON iot_project.device
FOR EACH ROW
EXECUTE FUNCTION iot_project.log_device_history();
