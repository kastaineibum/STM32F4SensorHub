#ifndef __MODBUS_H
#define __MODBUS_H
#include "dwt_stm32_delay.h"
#include "stm32f4xx_hal.h"

extern UART_HandleTypeDef huart2;
extern uint32_t modbusaddr; //self modbus address

//modbus functions
#define COILREAD 					0x01
#define REGREAD 					0x03
#define COILWRITE 				0x05
#define REGWRITE 					0x06
#define EXCEPTIONREAD 		0x07
#define BROADCASTTEST 		0x08
#define NCOILWRITE 				0x0f
#define NREGWRITE 				0x10
#define NREGREADWRITE 		0x17

//modbus address
#define MOD_ADDR 					0x31 //start address
#define MOD_ADDR2 				0x41 //end address
#define MOD_ADDR3 				0x33 
#define HITACHI_WJ200 		0x11

//modbus baudrate and silent time(microsecond and milisecond)
#define MOD_BAUDRATE 			57600
#define MOD_SILENT_US			11000
#define MOD_SILENT_MS			11

//modbus Thylacine sensors registers address
//baro,elevation,temperature,humidity,pwm1-4(flowmeter etc.)
#define RBARO 					0x01
#define RELEVATION			0x02
#define RTEMPERATURE 		0x03
#define RHUMIDITY 			0x04
#define RPWM1			 			0x05
#define RPWM2			 			0x06
#define RPWM3			 			0x07
#define RPWM4			 			0x08
#define RPITCH			 		0x09
#define RROLL			 			0x0a
#define RYAW			 			0x0b
#define WMOTORFWD			 	0x0c
#define WMOTORRVS			 	0x0d
#define WMOTORINIT			0x0e
#define RDISTANCE				0x0f
#define SENSORMAX			 	0x10

//modbus Thylacine coils address
#define CLASTPINNUM			0xfc
#define CSTATE					0xfd


//modbus Thylacine sensors and coils data sending
void mod_send_data_old(UART_HandleTypeDef *huart,uint32_t addr,uint32_t reg,uint32_t data);
void mod_send_coilstate(UART_HandleTypeDef *huart,uint32_t pinnum,uint32_t pinstate);
void mod_send_othercoilstate(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t pinnum,uint32_t pinstate);
void mod_send_readothercoil(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t pinnum);
void mod_send_readotherreg(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t regaddr);
void mod_send_writeothercoil(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t pinnum,uint32_t pinstate);
void mod_send_getotherreg(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t regaddr,uint32_t sensordata);
void mod_send_baro(UART_HandleTypeDef *huart,uint32_t baro);
void mod_send_elevation(UART_HandleTypeDef *huart,uint32_t elevation);
void mod_send_temperature(UART_HandleTypeDef *huart,uint32_t temperature);
void mod_send_humidity(UART_HandleTypeDef *huart,uint32_t humidity);
void mod_send_pwm1(UART_HandleTypeDef *huart,uint32_t pwm1);
void mod_send_pwm2(UART_HandleTypeDef *huart,uint32_t pwm2);
void mod_send_pwm3(UART_HandleTypeDef *huart,uint32_t pwm3);
void mod_send_pwm4(UART_HandleTypeDef *huart,uint32_t pwm4);
void mod_send_pitch(UART_HandleTypeDef *huart,float pitch);
void mod_send_roll(UART_HandleTypeDef *huart,float roll);
void mod_send_yaw(UART_HandleTypeDef *huart,float yaw);

//modbus Hitachi WJ200 VFD motor control
void mod_set_wj200frequency(UART_HandleTypeDef *huart,uint16_t frequency);
void mod_set_wj200run(UART_HandleTypeDef *huart);
void mod_set_wj200stop(UART_HandleTypeDef *huart);
void mod_set_wj200positiverot(UART_HandleTypeDef *huart);
void mod_set_wj200reverserot(UART_HandleTypeDef *huart);

//modbus read write
void mod_send(UART_HandleTypeDef *huart,uint8_t addr,uint8_t func,uint8_t* data,uint8_t datalength); //send data to modbus
void mod_recv(uint8_t* buff,uint8_t recvlength,uint8_t* addr,uint8_t* func,uint8_t** data); //receive data and parse
uint32_t get_gapcyccnt(void); //get modbus rtu ADU silent gap
uint16_t crc16(uint8_t *ptr,uint8_t len); //crc16 for modbus

#endif
