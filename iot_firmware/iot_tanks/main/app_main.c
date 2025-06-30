#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include "esp_wifi.h"
#include "esp_log.h"
#include "esp_system.h"
#include "nvs_flash.h"
#include "esp_event.h"
#include "esp_netif.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "mqtt_client.h"
#include <driver/uart.h>
#include <driver/ledc.h>
#include <driver/adc.h>
#include <esp_adc_cal.h>

#define WIFI_SSID "A55"
//#define WIFI_SSID "LIVE TIM_07A0_2G"

#define WIFI_PASSWORD "2+z=5+s?"
//#define WIFI_PASSWORD "Brunolucas"


#define PUBLISH_INTERVAL 10000 // Tempo em milissegundos para publicar (10 segundos)
#define PUBLISH_TOPIC "test/topic"
#define COMMAND_TOPIC "topic/commands"
#define MODE_TOPIC "mode"
#define PWM_TOPIC "ma/p1"

#define ADC_CHANNEL_1 ADC1_CHANNEL_6 // GPIO34
#define ADC_CHANNEL_2 ADC1_CHANNEL_7 // GPIO35

#define ADC_WIDTH   ADC_WIDTH_BIT_12
#define ADC_ATTEN   ADC_ATTEN_DB_11
#define DEFAULT_VREF 1100

// Parâmetros para cálculo de pressão e nível
#define VS 3.3
#define TOL_P 0.04
#define RHO 997 // Densidade da água em kg/m³
#define G 9.8  // Aceleração da gravidade em m/s²

// Parâmetros da equação de calibração
#define CALIBRATION_SLOPE_1 0.7807
#define CALIBRATION_INTERCEPT_1 -4.1454

#define CALIBRATION_SLOPE_2 0.7007
#define CALIBRATION_INTERCEPT_2 -4.4482

// Configuração da UART
#define UART_NUM UART_NUM_0
#define BUF_SIZE (1024)

// Configuração PWM
#define PWM_FREQ 1000
#define PWM_RES LEDC_TIMER_8_BIT

// Definição dos pinos das válvulas e bombas
#define VALVE1_PIN 23
#define VALVE2_PIN 19
#define PUMP1_PIN 18
#define PUMP2_PIN 32
#define PUMP3_PIN 22
#define MOTOR1_PIN 4
#define MOTOR2_PIN 33



double b0, b1, b2;
// Variável para o nível do tanque
double tank_level_1 = 0.0;
double tank_level_2 = 0.0;


// Variáveis de controle
int mode = 1; // 1: malha fechada (PID), 0: malha aberta
int pwm_value = 0; // Valor do PWM para bomba 1


SemaphoreHandle_t mutex;

void manual_control(const char* command);
void control_valve(int valve_pin, bool open);
void apply_pwm_bomba1(int pwm_value); // Função para aplicar PWM na bomba 1
void initialize_tank();


double Kp = 0.4, Ti = 183, Td = 0.999;
double Ts = 0.2; // Intervalo de amostragem em segundos
double setpoint = 6.0; // Setpoint inicial para o nível do tanque
double integral = 0, prev_error = 0;
double e_prev1 = 0;  // Erro anterior
double u_prev = 0;   // Saída anterior

// Coeficientes do PID discreto
double b0, b1, b2;

// Inicializar os coeficientes do PID
void initialize_pid_coefficients() {
    b0 = Kp * (1 + Ts / (2 * Ti));
    b1 = Kp * (-1 + Ts / (2 * Ti));
    b2 = Kp * (Td / Ts); // Td é 0 neste caso
}

static const char *TAG = "app_tanques";

// Variável global para o cliente MQTT
static esp_mqtt_client_handle_t client = NULL;

// Função para conectar no Wi-Fi
static void wifi_init_sta(void)
{
    esp_netif_init();
    esp_event_loop_create_default();

    esp_netif_create_default_wifi_sta();
    
    wifi_init_config_t cfg = WIFI_INIT_CONFIG_DEFAULT();
    ESP_ERROR_CHECK(esp_wifi_init(&cfg));
    
    wifi_config_t wifi_config = {
        .sta = {
            .ssid = WIFI_SSID,
            .password = WIFI_PASSWORD,
            .threshold.authmode = WIFI_AUTH_WPA2_PSK,
        },
    };

    ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
    ESP_ERROR_CHECK(esp_wifi_set_config(WIFI_IF_STA, &wifi_config));
    ESP_ERROR_CHECK(esp_wifi_start());
    
    ESP_LOGI(TAG, "Conectando ao Wi-Fi...");
    
    // Aguardar conexão
    ESP_ERROR_CHECK(esp_wifi_connect());
}

// Função de manipulação de eventos MQTT
static void mqtt_event_handler(void *handler_args, esp_event_base_t base, int32_t event_id, void *event_data)
{
    esp_mqtt_event_handle_t event = event_data;
    esp_mqtt_client_handle_t client = event->client;
    int msg_id;

    if (client == NULL) {
        ESP_LOGE(TAG, "Cliente MQTT não foi inicializado");
        return;
    }

    switch ((esp_mqtt_event_id_t)event_id) {
    case MQTT_EVENT_CONNECTED:
        ESP_LOGI(TAG, "MQTT EVENT: Conectado ao broker");
        msg_id = esp_mqtt_client_publish(client, PUBLISH_TOPIC, "Hello from ESP32", 0, 1, 0);
        ESP_LOGI(TAG, "Publicação feita, msg_id=%d", msg_id);
        msg_id = esp_mqtt_client_subscribe(client, COMMAND_TOPIC, 0);
        ESP_LOGI(TAG, "Inscrição no tópico de comandos, msg_id=%d", msg_id);
        msg_id = esp_mqtt_client_subscribe(client, MODE_TOPIC, 0);  // Inscreve no tópico 'mode'
        ESP_LOGI(TAG, "Inscrição no tópico 'mode', msg_id=%d", msg_id);
        msg_id = esp_mqtt_client_subscribe(client, PWM_TOPIC, 0);  // Inscreve no tópico 'ma/p1' para PWM
        ESP_LOGI(TAG, "Inscrição no tópico 'ma/p1', msg_id=%d", msg_id);
        msg_id = esp_mqtt_client_subscribe(client, "ref", 0);  // Inscreve no tópico 'ma/p1' para PWM
        ESP_LOGI(TAG, "Inscrição no tópico 'ref', msg_id=%d", msg_id);        
        break;
    case MQTT_EVENT_DISCONNECTED:
        ESP_LOGI(TAG, "MQTT EVENT: Desconectado do broker");
        break;
    case MQTT_EVENT_DATA:
        ESP_LOGI(TAG, "MQTT EVENT: Dados recebidos");
        printf("TOPIC=%.*s\r\n", event->topic_len, event->topic);
        printf("DATA=%.*s\r\n", event->data_len, event->data);

        if (strncmp(event->topic, "ref", event->topic_len) == 0) {
            // Atualiza o setpoint com o valor recebido
            double new_setpoint = atof(event->data); // Converte a string para um número double
            if (new_setpoint != setpoint){ 
                xSemaphoreTake(mutex, portMAX_DELAY);   // Protege o acesso ao setpoint
                setpoint = new_setpoint;
                xSemaphoreGive(mutex);
            }
            ESP_LOGI(TAG, "Novo setpoint recebido: %.2f", setpoint);
        }

        if (strncmp(event->topic, MODE_TOPIC, event->topic_len) == 0) {
            mode = atoi(event->data);  // Atualiza a variável mode com o valor recebido (0 ou 1)
            ESP_LOGI(TAG, "Modo de operação alterado: %d", mode);
        }

        // Atualiza o valor do PWM para a bomba 1
        if (strncmp(event->topic, PWM_TOPIC, event->topic_len) == 0) {
            // Verificar se a string pode ser convertida corretamente para um valor inteiro (PWM)
            int pwm = atoi(event->data);
            if (pwm >= 0 && pwm <= 255) {
                pwm_value = pwm;  // Converte o valor recebido para int
                ESP_LOGI(TAG, "Novo valor PWM para bomba 1: %d", pwm_value);
            } else {
                ESP_LOGW(TAG, "Valor de PWM inválido recebido: %s", event->data);
            }
        }

        if (strncmp(event->topic, COMMAND_TOPIC, event->topic_len) == 0) {
            manual_control(event->data);  // Chama manual_control com os dados recebidos
        }

        if (strncmp(event->topic, MODE_TOPIC, event->topic_len) == 0) {
            int new_mode = atoi(event->data); // Converte os dados para um inteiro
            if (new_mode == 0 || new_mode == 1) {
                xSemaphoreTake(mutex, portMAX_DELAY);
                mode = new_mode; // Atualiza o modo
                xSemaphoreGive(mutex);
                ESP_LOGI(TAG, "Modo alterado via MQTT para: %d", mode);
            } else {
                ESP_LOGW(TAG, "Valor de modo inválido recebido: %s", event->data);
            }
        }
        break;
    default:
        break;
    }
}

void mode_switch_task(void *pvParameter) {
    int previous_mode = mode; // Variável para armazenar o modo anterior

    while (1) {
        if (mode != previous_mode) { // Verifica se o modo foi alterado
            xSemaphoreTake(mutex, portMAX_DELAY); // Protege o acesso a recursos compartilhados

            if (mode == 0) { // Modo manual
                ESP_LOGI(TAG, "Mudando para o modo manual");

                // Abre as válvulas
                control_valve(VALVE1_PIN, true);
                control_valve(VALVE2_PIN, true);

                // Desliga as bombas
                ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2, 0); // Bomba 1
                ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2);

                ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_3, 0); // Bomba 2
                ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_3);

                ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_4, 0); // Bomba 3
                ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_4);

                // Zera variáveis do PID
                u_prev = 0;
                e_prev1 = 0;

            } else if (mode == 1) { // Modo automático
                ESP_LOGI(TAG, "Mudando para o modo automático");

                // Define o setpoint inicial
                initialize_tank();
                setpoint = 4.0;

                // Zera variáveis do PID
                u_prev = 0;
                e_prev1 = 0;
            }

            previous_mode = mode; // Atualiza o modo anterior

            xSemaphoreGive(mutex);
        }

        vTaskDelay(pdMS_TO_TICKS(500)); // Aguarda antes de verificar novamente
    }
}


// Função para aplicar o PWM na bomba 1
void apply_pwm_bomba1(int pwm_value) {
    // Aplica o valor de PWM à bomba 1, garantindo que o valor esteja entre 0 e 255
    if (pwm_value >= 0 && pwm_value <= 255) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2, pwm_value); // Controla a bomba 1 com o PWM
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2);
    } else {
        ESP_LOGW(TAG, "Valor PWM fora do intervalo (0-255): %d", pwm_value);
    }
}



void initialize_tank() {
    control_valve(VALVE1_PIN, true);
    control_valve(VALVE2_PIN, true);
}

void open_loop_task(void *pvParameter) {
    while (1) {
        if (mode == 1) {  // Se em malha fechada, suspende a tarefa de malha aberta
            vTaskSuspend(NULL);
        }

        apply_pwm_bomba1(pwm_value);  // Aplica o PWM na bomba 1

        vTaskDelay(pdMS_TO_TICKS(200));  // Atraso de 200ms para atualizações do PWM
    }
}


// Função para iniciar o cliente MQTT
static void mqtt_app_start(void)
{
    esp_mqtt_client_config_t mqtt_cfg = {
        .broker.address.uri = "mqtt://test.mosquitto.org",  // Broker público para testes
    };


    // Inicializa o cliente MQTT
    client = esp_mqtt_client_init(&mqtt_cfg);

    if (client == NULL) {
        ESP_LOGE(TAG, "Falha ao inicializar o cliente MQTT");
        return;  // Se a inicialização falhar, retorna
    }

    // Registra o manipulador de eventos MQTT
    esp_mqtt_client_register_event(client, ESP_EVENT_ANY_ID, mqtt_event_handler, NULL);

    // Inicia o cliente MQTT
    esp_mqtt_client_start(client);
    
    ESP_LOGI(TAG, "Cliente MQTT iniciado com sucesso");
}



// Tarefa para publicar periodicamente no tópico
void mqtt_publish_task(void *pvParameter)
{
    if (client == NULL) {
        ESP_LOGE(TAG, "Cliente MQTT não foi inicializado");
        vTaskDelete(NULL);
        return;
    }

    char msg[50];
    int msg_id;

    while (1) {

        xSemaphoreTake(mutex, portMAX_DELAY);
        double t1 = tank_level_1;
        double t2 = tank_level_2;
        double ref = setpoint;
        xSemaphoreGive(mutex);
        // Publicar uma mensagem no tópico
        snprintf(msg, sizeof(msg), "%.2f", t1);
        msg_id = esp_mqtt_client_publish(client, "t1", msg, 0, 1, 0);
        
        snprintf(msg, sizeof(msg), "%.2f", t2);
        msg_id = esp_mqtt_client_publish(client, "t2", msg, 0, 1, 0);
        
        //ESP_LOGI(TAG, "Publicação feita, msg_id=%d", msg_id);
        
        snprintf(msg, sizeof(msg), "%.2f", setpoint); 
        //msg_id = esp_mqtt_client_publish(client, "ref", msg, 0, 1, 0); 
        
        // Aguardar o intervalo configurado antes de publicar novamente
        vTaskDelay(pdMS_TO_TICKS(200));
    }
}

// Tarefa para escutar o tópico de comandos
void mqtt_subscribe_task(void *pvParameter)
{
    if (client == NULL) {
        ESP_LOGE(TAG, "Cliente MQTT não foi inicializado");
        vTaskDelete(NULL);
        return;
    }

    while (1) {
        // Aguardar por comandos no tópico de comandos
        // As mensagens recebidas no tópico "topic/commands/" serão tratadas no mqtt_event_handler
        vTaskDelay(1000 / portTICK_PERIOD_MS);  // Checar a cada segundo
    }
}

void control_valve(int valve_pin, bool open) {
    int duty_valve = open ? 255 : 0; // 255 para abrir (ou fechar para válvula 2), 0 para fechar (ou abrir para válvula 2)
    int duty_pump = open ? 255 : 0;  // 255 para ligar a bomba, 0 para desligar

    if (valve_pin == VALVE1_PIN) {
        // Controle da válvula 1 e bomba 2 associada
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_0, duty_valve);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_0);

        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_3, duty_pump);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_3);

        ESP_LOGI("VALVE", "Válvula 1 %s e Bomba 2 %s", open ? "aberta" : "fechada", open ? "ligada" : "desligada");
    } else if (valve_pin == VALVE2_PIN) {
        // Controle invertido para a válvula 2 (normalmente aberta)
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_1, open ? 0 : 255);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_1);

        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_4, duty_pump);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_4);

        ESP_LOGI("VALVE", "Válvula 2 %s e Bomba 3 %s", open ? "fechada" : "aberta", open ? "ligada" : "desligada");
    }
}


int calculate_pid(double current_level) {
    // Calcula o erro
    double e = setpoint - current_level;

    // Aplica a equação de diferenças com o termo b2
    double u = u_prev + b0 * e + b1 * e_prev1 + b2 * (e - e_prev1);

    // Saturação da saída (limitada entre 0 e 255)
    int duty = (int)(u * 255);
    if (duty < 0) duty = 0;
    if (duty > 255) duty = 255;

    // Atualiza os estados para a próxima iteração
    e_prev1 = e;
    u_prev = u;

    return duty;
}


// Tarefa do controlador PID
void pid_control_task(void *pvParameter) {
    while (1) {
        if (mode == 0) { // Se em malha aberta, a tarefa do PID fica bloqueada
            vTaskSuspend(NULL);
        }

        xSemaphoreTake(mutex, portMAX_DELAY);

        int duty = calculate_pid(tank_level_1);

        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2, duty); // Controla a bomba 1
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2);

        //ESP_LOGI("PID", "Nível: %.2f cm, Duty: %d", tank_level_1, duty);

        xSemaphoreGive(mutex);

        vTaskDelay(pdMS_TO_TICKS(200));
    }
}




void manual_control(const char* command) {
    if (strncmp(command, "v1o", 3) == 0) {
        control_valve(VALVE1_PIN, true);
    } else if (strncmp(command, "v1c", 3) == 0) {
        control_valve(VALVE1_PIN, false);
    } else if (strncmp(command, "v2o", 3) == 0) {
        control_valve(VALVE2_PIN, true);
    } else if (strncmp(command, "v2c", 3) == 0) {
        control_valve(VALVE2_PIN, false);
    } else if (strncmp(command, "p1o", 3) == 0) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2, 255);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2);
        ESP_LOGI("PUMP 1", "Ligada");
    } else if (strncmp(command, "p1c", 3) == 0) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2, 0);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2);
        ESP_LOGI("PUMP 1", "Desligada");
    } else if (strncmp(command, "m1o", 3) == 0) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_5, 255);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_5);
        ESP_LOGI("MOTOR 1", "Ligado");
    } else if (strncmp(command, "m1c", 3) == 0) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_5, 0);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_5);
        ESP_LOGI("MOTOR 1", "Desligado");
    } else if (strncmp(command, "m2o", 3) == 0) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_6, 255);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_6);
        ESP_LOGI("MOTOR 2", "Ligado");
    } else if (strncmp(command, "m2c", 3) == 0) {
        ledc_set_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_6, 0);
        ledc_update_duty(LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_6);
        ESP_LOGI("MOTOR 2", "Desligado");
    } else {
        ESP_LOGW("Manual Control", "Comando não reconhecido: %s", command);
    }
}


#define FILTER_SIZE 5 // Tamanho da janela do filtro de média móvel

void read_sensor_task(void *pvParameter) {
    adc1_config_width(ADC_WIDTH);
    adc1_config_channel_atten(ADC_CHANNEL_1, ADC_ATTEN);
    adc1_config_channel_atten(ADC_CHANNEL_2, ADC_ATTEN);

    esp_adc_cal_characteristics_t *adc_chars = calloc(1, sizeof(esp_adc_cal_characteristics_t));
    esp_adc_cal_characterize(ADC_UNIT_1, ADC_ATTEN, ADC_WIDTH, DEFAULT_VREF, adc_chars);

    // Buffers circulares para o filtro
    double filter_buffer_1[FILTER_SIZE] = {0};
    double filter_buffer_2[FILTER_SIZE] = {0};
    int filter_index_1 = 0, filter_index_2 = 0;

    while (1) {
        int adc_reading_1 = 0, adc_reading_2 = 0;
        int samples = 10;

        // Leitura do ADC para o Sensor 1
        for (int i = 0; i < samples; i++) {
            adc_reading_1 += adc1_get_raw(ADC_CHANNEL_1);
            vTaskDelay(pdMS_TO_TICKS(5));
        }
        adc_reading_1 /= samples;

        // Leitura do ADC para o Sensor 2
        for (int i = 0; i < samples; i++) {
            adc_reading_2 += adc1_get_raw(ADC_CHANNEL_2);
            vTaskDelay(pdMS_TO_TICKS(5));
        }
        adc_reading_2 /= samples;

        // Cálculo do Sensor 1
        uint32_t voltage_1 = esp_adc_cal_raw_to_voltage(adc_reading_1, adc_chars);
        double Vout_1 = voltage_1 / 1000.0;
        double P_1 = (Vout_1 - 0.04 * VS) / (0.09 * VS) + TOL_P;
        double Level_1 = ((P_1 * 1000) / (RHO * G)) * 100;
        double calibrated_level_1 = CALIBRATION_SLOPE_1 * Level_1 + CALIBRATION_INTERCEPT_1;

        // Cálculo do Sensor 2
        uint32_t voltage_2 = esp_adc_cal_raw_to_voltage(adc_reading_2, adc_chars);
        double Vout_2 = voltage_2 / 1000.0;
        double P_2 = (Vout_2 - 0.04 * VS) / (0.09 * VS) + TOL_P;
        double Level_2 = ((P_2 * 1000) / (RHO * G)) * 100;
        double calibrated_level_2 = CALIBRATION_SLOPE_2 * Level_2 + CALIBRATION_INTERCEPT_2;

        // Aplicar filtro de média móvel para calibrated_level_1
        filter_buffer_1[filter_index_1] = calibrated_level_1;
        filter_index_1 = (filter_index_1 + 1) % FILTER_SIZE;
        double filtered_level_1 = 0;
        for (int i = 0; i < FILTER_SIZE; i++) {
            filtered_level_1 += filter_buffer_1[i];
        }
        filtered_level_1 /= FILTER_SIZE;

        // Aplicar filtro de média móvel para calibrated_level_2
        filter_buffer_2[filter_index_2] = calibrated_level_2;
        filter_index_2 = (filter_index_2 + 1) % FILTER_SIZE;
        double filtered_level_2 = 0;
        for (int i = 0; i < FILTER_SIZE; i++) {
            filtered_level_2 += filter_buffer_2[i];
        }
        filtered_level_2 /= FILTER_SIZE;

        xSemaphoreTake(mutex, portMAX_DELAY);
        tank_level_1 = filtered_level_1;
        tank_level_2 = filtered_level_2;
        xSemaphoreGive(mutex);

        //ESP_LOGI(TAG, "Sensor 1 -> ADC: %d, V: %.2f V, P: %.2f kPa, Nível Filtrado: %.2f cm", adc_reading_1, Vout_1, P_1, filtered_level_1);
        //ESP_LOGI(TAG, "Sensor 2 -> ADC: %d, V: %.2f V, P: %.2f kPa, Nível Filtrado: %.2f cm", adc_reading_2, Vout_2, P_2, filtered_level_2);

        vTaskDelay(pdMS_TO_TICKS(200));
    }
}


void app_main(void)
{
    ESP_LOGI(TAG, "Iniciando o aplicativo...");
    ESP_ERROR_CHECK(nvs_flash_init());

    initialize_pid_coefficients(); // Inicializa os coeficientes PID


    const uart_config_t uart_config = {
        .baud_rate = 115200,
        .data_bits = UART_DATA_8_BITS,
        .parity = UART_PARITY_DISABLE,
        .stop_bits = UART_STOP_BITS_1,
        .flow_ctrl = UART_HW_FLOWCTRL_DISABLE
    };
    uart_param_config(UART_NUM, &uart_config);
    uart_driver_install(UART_NUM, BUF_SIZE * 2, 0, 0, NULL, 0);

    ledc_timer_config_t ledc_timer = {
        .speed_mode = LEDC_LOW_SPEED_MODE,
        .timer_num = LEDC_TIMER_0,
        .duty_resolution = PWM_RES,
        .freq_hz = PWM_FREQ,
        .clk_cfg = LEDC_AUTO_CLK
    };
    ledc_timer_config(&ledc_timer);

    ledc_channel_config_t ledc_channel1 = { VALVE1_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_0, LEDC_TIMER_0, 0, 0 };
    ledc_channel_config(&ledc_channel1);

    ledc_channel_config_t ledc_channel2 = { VALVE2_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_1, LEDC_TIMER_0, 255, 0 };
    ledc_channel_config(&ledc_channel2);

    ledc_channel_config_t ledc_channel3 = { PUMP1_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_2, LEDC_TIMER_0, 0, 0 };
    ledc_channel_config(&ledc_channel3);

    ledc_channel_config_t ledc_channel4 = { PUMP2_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_3, LEDC_TIMER_0, 0, 0 };
    ledc_channel_config(&ledc_channel4);

    ledc_channel_config_t ledc_channel5 = { PUMP3_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_4, LEDC_TIMER_0, 0, 0 };
    ledc_channel_config(&ledc_channel5);

      // Configuração para os motores
    ledc_channel_config_t ledc_channel6 = { MOTOR1_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_5, LEDC_TIMER_0, 0, 0 };
    ledc_channel_config(&ledc_channel6);

    ledc_channel_config_t ledc_channel7 = { MOTOR2_PIN, LEDC_LOW_SPEED_MODE, LEDC_CHANNEL_6, LEDC_TIMER_0, 0, 0 };
    ledc_channel_config(&ledc_channel7);

    mutex = xSemaphoreCreateMutex();

    initialize_tank();
    
    // Conectar ao Wi-Fi diretamente
    wifi_init_sta();

    vTaskDelay(pdMS_TO_TICKS(2000));

    // Inicia o cliente MQTT
    mqtt_app_start();

    // Cria a tarefa MQTT para publicação periódica
    xTaskCreate(&mqtt_publish_task, "mqtt_publish_task", 4096, NULL, 5, NULL);
    
    // Cria a tarefa MQTT para escutar comandos
    xTaskCreate(&mqtt_subscribe_task, "mqtt_subscribe_task", 4096, NULL, 5, NULL);

    xTaskCreate(&read_sensor_task, "read_sensor_task", 4096, NULL, 5, NULL);

    xTaskCreate(&pid_control_task, "pid_control_task", 4096, NULL, 5, NULL);

    xTaskCreate(&open_loop_task, "open_loop_task", 4096, NULL, 5, NULL);

    xTaskCreate(&mode_switch_task, "mode_switch_task", 2048, NULL, 5, NULL);


    
}
