#ifndef __DHT11_H
#define __DHT11_H
#include "dwt_stm32_delay.h"

void DHT11_IN(void);
void DHT11_OUT(void);

#define READ_DHT11		HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_3)
#define DHT11_0  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_3, GPIO_PIN_RESET);}	
#define DHT11_1  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_3, GPIO_PIN_SET);}

void DHT11_Init(void); 
unsigned int DHT11_Read(void);
uint8_t getHumidity(void);
uint8_t getTemperature(void);
uint32_t getDHTRaw(void);
#endif
