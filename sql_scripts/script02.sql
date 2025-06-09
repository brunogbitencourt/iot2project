BEGIN;
SET LOCAL iot_project.current_user_id = '1';

INSERT INTO iot_project.user_profile (description) VALUES ('Administrator');

INSERT INTO iot_project.users (
    full_name, email, password_hash, user_profile_id
) VALUES (
    'João Silva', 'joao.silva@example.com', 'hash123', 1
);
COMMIT;


SELECT * FROM iot_project.user_history ORDER BY operation_timestamp DESC;
