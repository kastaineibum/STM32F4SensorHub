#ifndef __SCCB_H
#define __SCCB_H
#include "stm32f4xx_hal.h"

#define SCCB_READ_SDA		HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_11)
#define SCCB_ID   			0X60

void SCCB_SDA_IN(void);
void SCCB_SDA_OUT(void);
void SCCB_Init(void);
void SCCB_Start(void);
void SCCB_Stop(void);
void SCCB_No_Ack(void);
uint8_t SCCB_WR_Byte(uint8_t dat);
uint8_t SCCB_RD_Byte(void); 
#endif













